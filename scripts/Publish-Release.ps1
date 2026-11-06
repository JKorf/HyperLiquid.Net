[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ArtifactDirectory,
    [Parameter(Mandatory)][string]$Repository,
    [Parameter(Mandatory)][string]$Commit,
    [Parameter(Mandatory)][int]$PullRequest,
    [switch]$ValidateOnly
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. "$PSScriptRoot/Release-PackageFunctions.ps1"
$manifest = Assert-ReleaseArtifacts $ArtifactDirectory $Repository $Commit $PullRequest
if ($ValidateOnly) { Write-Host 'Release artifact identity, metadata and checksums verified.'; return }
if ([string]::IsNullOrWhiteSpace($env:GH_TOKEN) -or [string]::IsNullOrWhiteSpace($env:NUGET_API_KEY)) { throw 'Publishing credentials are required.' }
$package = (Resolve-Path -LiteralPath (Join-Path $ArtifactDirectory $manifest.packageFile)).Path
$symbols = (Resolve-Path -LiteralPath (Join-Path $ArtifactDirectory $manifest.symbolFile)).Path
$tag = [Uri]::EscapeDataString($manifest.tag)
$owner = $Repository.Split('/')[0]
$temp = Join-Path ([IO.Path]::GetTempPath()) ('release-publish-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temp | Out-Null
$sources = @(@{ Name = 'NuGet.org'; Url = 'https://api.nuget.org/v3/index.json'; Key = $env:NUGET_API_KEY; Headers = @{} })
if ($manifest.publishToGitHubPackages) {
    # Git tags/releases may need an App token with Workflows: write. Keep the
    # registry on GITHUB_TOKEN, which has this repository's Packages permission.
    $packageToken = if ([string]::IsNullOrWhiteSpace($env:GITHUB_PACKAGES_TOKEN)) { $env:GH_TOKEN } else { $env:GITHUB_PACKAGES_TOKEN }
    $basic = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes("${owner}:$packageToken"))
    $sources += @{ Name = 'GitHub Packages'; Url = "https://nuget.pkg.github.com/$owner/index.json"; Key = $packageToken; Headers = @{ Authorization = "Basic $basic" } }
}

# Preflight every registry before creating a tag or uploading anything. A retry
# may reuse an existing version only if all package entries match the approved file.
foreach ($source in $sources) {
    $download = Join-Path $temp ($source.Name.Replace(' ', '-') + '.nupkg')
    $source.Exists = Get-RemotePackage $source.Url $manifest.packageId $manifest.version $source.Headers $download
    if ($source.Exists) { Assert-ArchivePayloadEqual $package $download }
}

$reference = Invoke-ReleaseApi "repos/$Repository/git/ref/tags/$tag" -AllowMissing
if ($null -ne $reference) {
    $object = $reference.object
    $depth = 0
    while ($object.type -eq 'tag' -and $depth++ -lt 8) { $object = (Invoke-ReleaseApi "repos/$Repository/git/tags/$($object.sha)").object }
    if ($object.type -ne 'commit' -or $object.sha -cne $Commit) { throw 'Existing release tag points to a different commit.' }
} else {
    Invoke-ReleaseApi "repos/$Repository/git/refs" 'POST' @{ ref = "refs/tags/$($manifest.tag)"; sha = $Commit } | Out-Null
}

# A draft and its original assets provide a durable record for recovery. Existing
# assets must match; never replace files on a published release.
$release = Find-ReleaseByTag $Repository $manifest.tag
if ($null -eq $release) {
    $release = Invoke-ReleaseApi "repos/$Repository/releases" 'POST' @{
        tag_name = $manifest.tag; target_commitish = $Commit; name = "$($manifest.packageId) $($manifest.version)"
        body = (Get-Content -LiteralPath (Join-Path $ArtifactDirectory 'release-notes.md') -Raw)
        draft = $true; prerelease = $false
    }
}
$assetNames = @($manifest.packageFile, $manifest.symbolFile, 'manifest.json', 'release-notes.md')
foreach ($name in $assetNames) {
    $local = Join-Path $ArtifactDirectory $name
    $asset = @($release.assets | Where-Object { $_.name -ceq $name })
    if ($asset.Count -gt 1) { throw "Duplicate release asset: $name" }
    if ($asset.Count -eq 1) {
        $downloadDir = Join-Path $temp ('asset-' + [guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $downloadDir | Out-Null
        gh release download $manifest.tag --repo $Repository --pattern $name --dir $downloadDir
        if ($LASTEXITCODE -ne 0) { throw "Cannot verify existing release asset $name" }
        if ((Get-FileHash -LiteralPath (Join-Path $downloadDir $name)).Hash -cne (Get-FileHash -LiteralPath $local).Hash) { throw "Existing release asset differs: $name. Re-run failed jobs using the original artifacts." }
    } else {
        if (-not $release.draft) { throw "Published release is missing asset $name; refusing to modify it." }
        gh release upload $manifest.tag $local --repo $Repository
        if ($LASTEXITCODE -ne 0) { throw "Failed to upload release asset $name" }
    }
}

foreach ($source in $sources) {
    if (-not $source.Exists) {
        dotnet nuget push $package --source $source.Url --api-key $source.Key --no-symbols --timeout 300
        if ($LASTEXITCODE -ne 0) {
            # Handle an ambiguous response or concurrent duplicate only after
            # reading back and checking the exact package payload.
            $download = Join-Path $temp 'push-recovery.nupkg'
            if (-not (Get-RemotePackage $source.Url $manifest.packageId $manifest.version $source.Headers $download)) { throw "Package push failed: $($source.Name)" }
            Assert-ArchivePayloadEqual $package $download
        }
    }
}
dotnet nuget push $symbols --source https://api.nuget.org/v3/index.json --api-key $env:NUGET_API_KEY --skip-duplicate --timeout 300
if ($LASTEXITCODE -ne 0) { throw 'Symbol package publishing failed.' }

# Registry acceptance completes publication; NuGet indexing proceeds separately.
if ($release.draft) { $release = Invoke-ReleaseApi "repos/$Repository/releases/$($release.id)" 'PATCH' @{ draft = $false } }
Write-Host "Published $($manifest.packageId) $($manifest.version): $($release.html_url)"
if ($env:GITHUB_STEP_SUMMARY) {
    "Published release: $($release.html_url)`n`nNuGet: https://www.nuget.org/packages/$($manifest.packageId)/$($manifest.version)" >> $env:GITHUB_STEP_SUMMARY
}
