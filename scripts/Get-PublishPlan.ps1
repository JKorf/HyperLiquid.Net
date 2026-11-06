[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$EventPath,
    [Parameter(Mandatory)][string]$Repository,
    [Parameter(Mandatory)][string]$OutputDirectory,
    [string]$ConfigPath = '.github/release-config.json'
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$event = Get-Content -LiteralPath $EventPath -Raw | ConvertFrom-Json
$pr = $event.pull_request
$config = Get-Content -LiteralPath $ConfigPath -Raw | ConvertFrom-Json
if ($event.action -ne 'closed' -or -not $pr.merged) { throw 'Only merged preparation PRs may publish.' }
if ($pr.head.repo.full_name -cne $Repository -or $pr.base.repo.full_name -cne $Repository) { throw 'The PR must originate in this repository.' }
if ($config.baseBranch -notin @('main', 'master') -or $pr.base.ref -cne $config.baseBranch) { throw 'PR target does not match the configured release branch.' }
if ($pr.merge_commit_sha -cnotmatch '^[0-9a-f]{40}$') { throw 'Invalid merged commit SHA.' }
if ($pr.head.ref -cnotmatch '^release/(?<key>[a-z0-9][a-z0-9._-]*)/(?<version>(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*))$') {
    throw 'Expected release/<package-key>/<major.minor.patch>.'
}
$key = $Matches.key
$version = $Matches.version
$packages = @($config.packages | Where-Object { $_.key -ceq $key })
if ($packages.Count -ne 1) { throw 'The release branch must select exactly one configured package.' }
$package = $packages[0]
if ($package.packageId -cnotmatch '^[A-Za-z0-9_.-]+$') { throw 'Invalid package ID.' }
$project = [xml](Get-Content -LiteralPath $package.project -Raw)
foreach ($field in @('PackageId', 'PackageVersion', 'AssemblyVersion', 'FileVersion')) {
    $nodes = @($project.SelectNodes("/Project/PropertyGroup/$field"))
    if ($nodes.Count -ne 1) { throw "Expected exactly one $field in the package project." }
    $expected = if ($field -eq 'PackageId') { $package.packageId } else { $version }
    if ($nodes[0].InnerText -cne $expected) { throw "$field does not match the preparation PR." }
}
if ([string]::IsNullOrWhiteSpace($package.tagPrefix)) { throw 'tagPrefix is required.' }
$tag = "$($package.tagPrefix)$version"
git check-ref-format "refs/tags/$tag"
if ($LASTEXITCODE -ne 0) { throw 'Invalid release tag.' }
$head = git rev-parse HEAD
if ($LASTEXITCODE -ne 0 -or $head -cne $pr.merge_commit_sha) { throw 'Checkout is not the exact merged PR commit.' }
foreach ($path in @($package.project, $package.readme) + @($package.testProjects)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Missing release input: $path" }
}
$readme = Get-Content -LiteralPath $package.readme -Raw
$pattern = '(?ms)^\* Version ' + [regex]::Escape($version) + ' - [^\r\n]*\r?\n(?<notes>.*?)(?=^\* Version |\z)'
$entries = [regex]::Matches($readme, $pattern)
if ($entries.Count -ne 1) { throw 'Expected exactly one README release entry for the requested version.' }
# Remove only the README version-list indentation, keeping nested Markdown intact.
$notes = [regex]::Replace($entries[0].Groups['notes'].Value, '(?m)^    ', '').TrimEnd()
if ([string]::IsNullOrWhiteSpace($notes)) { throw 'Release notes cannot be empty.' }
$integration = $false
if ($package.PSObject.Properties.Name -contains 'releaseIntegrationTests') { $integration = [bool]$package.releaseIntegrationTests }
$githubPackages = $true
if ($package.PSObject.Properties.Name -contains 'publishToGitHubPackages') { $githubPackages = [bool]$package.publishToGitHubPackages }
$plan = [ordered]@{
    repository = $Repository; pullRequest = [int]$pr.number; commit = $pr.merge_commit_sha
    package = $key; packageId = $package.packageId; version = $version; tag = $tag
    project = $package.project; testProjects = @($package.testProjects); sdk = $config.sdk
    baseBranch = $config.baseBranch; integrationTests = $integration; publishToGitHubPackages = $githubPackages
    packageFile = "$($package.packageId).$version.nupkg"; symbolFile = "$($package.packageId).$version.snupkg"
}
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$OutputDirectory = (Resolve-Path -LiteralPath $OutputDirectory).Path
$encoding = [Text.UTF8Encoding]::new($false)
[IO.File]::WriteAllText((Join-Path $OutputDirectory 'release-notes.md'), $notes + "`n", $encoding)
[IO.File]::WriteAllText((Join-Path $OutputDirectory 'manifest.json'), ($plan | ConvertTo-Json -Depth 6), $encoding)
Write-Host "Validated $($package.packageId) $version from merged PR #$($pr.number), commit $($pr.merge_commit_sha)."
