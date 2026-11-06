$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$scripts = (Resolve-Path "$PSScriptRoot/..").Path
. "$scripts/Release-PackageFunctions.ps1"
$fixture = Join-Path ([IO.Path]::GetTempPath()) ('publish-test-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
$checks = 0
function Reject([scriptblock]$Action) {
    $rejected = $false
    try { & $Action } catch { $rejected = $true }
    if (-not $rejected) { throw 'Expected invalid release input to be rejected.' }
    $script:checks++
}
function Write-Json($Value, [string]$Path) { $Value | ConvertTo-Json -Depth 10 | Set-Content $Path -Encoding utf8NoBOM }
function New-Package([string]$Path, [string]$Content = 'binary', [switch]$Signed) {
    $absolutePath = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Path)
    $zip = [IO.Compression.ZipFile]::Open($absolutePath, [IO.Compression.ZipArchiveMode]::Create)
    try {
        $entries = @{
            'Example.nuspec' = "<package><metadata><id>Example</id><version>2.0.0</version><repository url='https://github.com/owner/library' commit='$script:commit'/></metadata></package>"
            'lib/net10.0/Example.dll' = $Content
        }
        if ($Signed) { $entries['.signature.p7s'] = 'registry signature' }
        foreach ($entry in $entries.GetEnumerator()) {
            $writer = [IO.StreamWriter]::new($zip.CreateEntry($entry.Key).Open())
            try { $writer.Write($entry.Value) } finally { $writer.Dispose() }
        }
    } finally { $zip.Dispose() }
}
Push-Location $fixture
try {
    git init --quiet
    git -c user.name=Fixture -c user.email=fixture@example.test commit --allow-empty --quiet -m fixture
    if ($LASTEXITCODE -ne 0) { throw 'Cannot create fixture commit.' }
    $script:commit = git rev-parse HEAD
    '<Project><PropertyGroup><PackageId>Example</PackageId><PackageVersion>2.0.0</PackageVersion><AssemblyVersion>2.0.0</AssemblyVersion><FileVersion>2.0.0</FileVersion></PropertyGroup></Project>' | Set-Content library.csproj
    "## Release notes`n* Version 2.0.0 - Today`n    - Feature 1`n      - Nested detail`n    - Feature 2`n`n* Version 1.0.0 - Before`n    - Old" | Set-Content README.md
    $config = @{ baseBranch = 'master'; sdk = '10.0.401'; packages = @(
        @{ key = 'other'; project = 'missing.csproj' },
        @{ key = 'example'; packageId = 'Example'; project = 'library.csproj'; readme = 'README.md'; testProjects = @(); tagPrefix = 'Example.' }
    ) }
    Write-Json $config config.json
    $event = @{ action = 'closed'; pull_request = @{ number = 7; merged = $true; merge_commit_sha = $commit; head = @{ ref = 'release/example/2.0.0'; repo = @{ full_name = 'owner/library' } }; base = @{ ref = 'master'; repo = @{ full_name = 'owner/library' } } } }
    function Plan { Write-Json $event event.json; & "$scripts/Get-PublishPlan.ps1" -EventPath event.json -Repository owner/library -ConfigPath config.json -OutputDirectory output }
    Plan
    $plan = Get-Content output/manifest.json -Raw | ConvertFrom-Json
    if ($plan.packageId -ne 'Example' -or $plan.baseBranch -ne 'master' -or $plan.integrationTests -or -not $plan.publishToGitHubPackages) { throw 'Package selection/defaults failed.' }
    if ((Get-Content output/release-notes.md -Raw).TrimEnd() -cne "- Feature 1`n  - Nested detail`n- Feature 2") { throw 'Nested release notes changed.' }
    $checks += 2
    $event.pull_request.merged = $false; Reject { Plan }; $event.pull_request.merged = $true
    $event.pull_request.head.repo.full_name = 'fork/library'; Reject { Plan }; $event.pull_request.head.repo.full_name = 'owner/library'
    $event.pull_request.base.ref = 'main'; Reject { Plan }; $event.pull_request.base.ref = 'master'
    $event.pull_request.head.ref = 'release/example/2.0.1'; Reject { Plan }; $event.pull_request.head.ref = 'release/example/2.0.0'
    $event.pull_request.merge_commit_sha = 'a' * 40; Reject { Plan }; $event.pull_request.merge_commit_sha = $commit
    New-Package output/Example.2.0.0.nupkg
    New-Package output/Example.2.0.0.snupkg
    $plan | Add-Member files @(@('Example.2.0.0.nupkg', 'Example.2.0.0.snupkg', 'release-notes.md') | ForEach-Object { @{ name = $_; sha256 = (Get-FileHash "output/$_").Hash } })
    Write-Json $plan output/manifest.json
    & "$scripts/Publish-Release.ps1" -ArtifactDirectory output -Repository owner/library -Commit $commit -PullRequest 7 -ValidateOnly
    $checks++
    # Recovery validates original metadata as well as package identity/checksums.
    # Only the GitHub HTTP transport is mocked; artifact checks remain real.
    $script:recoveryRun = @{ event = 'pull_request'; path = '.github/workflows/publish-release.yml'; display_title = 'Publish release PR #7'; status = 'completed'; conclusion = 'failure'; head_sha = 'pr-head'; head_branch = 'release/example/2.0.0' }
    $script:recoveryPr = @{ merged = $true; merge_commit_sha = $commit; head = @{ sha = 'pr-head'; ref = 'release/example/2.0.0'; repo = @{ full_name = 'owner/library' } }; base = @{ ref = 'master'; repo = @{ full_name = 'owner/library' } } }
    $script:recoveryArtifact = @{ expired = $false; workflow_run = @{ id = 100 }; name = 'release-pr-7-attempt-1' }
    function Invoke-RestMethod([string]$Uri, [string]$Method, [hashtable]$Headers, [switch]$SkipHttpErrorCheck, [string]$StatusCodeVariable) {
        Set-Variable -Name $StatusCodeVariable -Value 200 -Scope 1
        if ($Uri.EndsWith('/actions/runs/100')) { return $recoveryRun }
        if ($Uri.EndsWith('/pulls/7')) { return $recoveryPr }
        if ($Uri.EndsWith('/actions/artifacts/200')) { return $recoveryArtifact }
        throw 'Unexpected recovery metadata request.'
    }
    $oldOutput = $env:GITHUB_OUTPUT
    $oldSummary = $env:GITHUB_STEP_SUMMARY
    try {
        $env:GITHUB_OUTPUT = Join-Path $fixture 'recovery-output.txt'
        $env:GITHUB_STEP_SUMMARY = Join-Path $fixture 'recovery-summary.md'
        function Recover { & "$scripts/Get-RecoveryPlan.ps1" -Repository owner/library -OriginalRunId 100 -ArtifactId 200 -ArtifactDirectory output }
        Recover
        if (-not (Get-Content $env:GITHUB_OUTPUT -Raw).Contains("commit=$commit")) { throw 'Recovery lost the original source commit.' }
        $checks++
        $script:recoveryRun.head_sha = 'unrelated'; Reject { Recover }; $script:recoveryRun.head_sha = 'pr-head'
        $script:recoveryRun.status = 'in_progress'; Reject { Recover }; $script:recoveryRun.status = 'completed'
        $script:recoveryArtifact.workflow_run.id = 999; Reject { Recover }; $script:recoveryArtifact.workflow_run.id = 100
        $script:recoveryArtifact.expired = $true; Reject { Recover }; $script:recoveryArtifact.expired = $false
        $script:recoveryArtifact.name = 'publication-test-results-1'; Reject { Recover }; $script:recoveryArtifact.name = 'release-pr-7-attempt-1'
    } finally { $env:GITHUB_OUTPUT = $oldOutput; $env:GITHUB_STEP_SUMMARY = $oldSummary }
    Reject { Assert-ReleaseArtifacts output owner/library $commit 8 }
    Reject { Assert-ReleaseArtifacts output owner/other $commit 7 }
    Add-Content output/release-notes.md 'unexpected change'
    Reject { Assert-ReleaseArtifacts output owner/library $commit 7 }
    New-Package signed.nupkg -Signed
    Assert-ArchivePayloadEqual output/Example.2.0.0.nupkg signed.nupkg
    $checks++
    New-Package changed.nupkg -Content 'different binary'
    Reject { Assert-ArchivePayloadEqual output/Example.2.0.0.nupkg changed.nupkg }
    $plan.files[0].sha256 = '0' * 64
    Write-Json $plan output/manifest.json
    Reject { Assert-ReleaseArtifacts output owner/library $commit 7 }
    # Retry discovery must find unpublished drafts; the by-tag endpoint does not.
    function Invoke-ReleaseApi([string]$Path, [switch]$AllowMissing) {
        if ($Path.Contains('/releases/tags/')) { return $null }
        return [pscustomobject]@{ id = 123; tag_name = 'Example.2.0.0'; draft = $true }
    }
    $draft = Find-ReleaseByTag owner/library Example.2.0.0
    if ($draft.id -ne 123 -or -not $draft.draft) { throw 'Failed to locate existing draft for retry.' }
    if ($null -ne (Find-ReleaseByTag owner/library Example.3.0.0)) { throw 'Selected another release tag.' }
    $checks += 2
    Write-Host "$checks publishing checks passed. No network or publication performed."
} finally { Pop-Location }
