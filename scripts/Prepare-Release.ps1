[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Version,
    [Parameter(Mandatory)][string]$Notes,
    [string]$Package = '',
    [string]$OverrideReason = '',
    [string]$ConfigPath = '.github/release-config.json',
    [Parameter(Mandatory)][string]$BodyPath,
    [string]$ResultPath = '',
    [switch]$ValidateOnly
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Get-StableVersion([string]$Value) {
    if ($Value -cnotmatch '^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$') {
        throw "Expected a stable major.minor.patch version, got '$Value'. Prereleases are not supported by this pilot."
    }
    $parsed = [version]$Value
    if ($parsed.Major -gt 65534 -or $parsed.Minor -gt 65534 -or $parsed.Build -gt 65534) {
        throw 'Version components must fit the assembly version fields (0..65534).'
    }
    return $parsed
}

# One package per run: packages in the same repository may version independently.
$config = Get-Content -LiteralPath $ConfigPath -Raw | ConvertFrom-Json
if ($config.baseBranch -notin @('main', 'master')) { throw 'baseBranch must be main or master.' }
$packages = @($config.packages)
$keys = @($packages | ForEach-Object { $_.key })
if (@($keys | Sort-Object -Unique).Count -ne $keys.Count) { throw 'Package keys must be unique.' }
foreach ($key in $keys) {
    if ($key -cnotmatch '^[a-z0-9][a-z0-9._-]*$') { throw "Invalid package key '$key'." }
}
if ([string]::IsNullOrWhiteSpace($Package) -and $packages.Count -eq 1) { $Package = $packages[0].key }
$selected = @($packages | Where-Object { $_.key -ceq $Package })
if ($selected.Count -ne 1) { throw "Select one package using its key. Available: $($keys -join ', ')." }
$releasePackage = $selected[0]
$projectPath = (Resolve-Path -LiteralPath $releasePackage.project).Path
$readmePath = (Resolve-Path -LiteralPath $releasePackage.readme).Path
foreach ($testProject in @($releasePackage.testProjects)) {
    if (-not (Test-Path -LiteralPath $testProject -PathType Leaf)) { throw "Missing test project: $testProject" }
}
$projectText = [IO.File]::ReadAllText($projectPath)
$readmeText = [IO.File]::ReadAllText($readmePath)
$project = [xml]$projectText
$packageIdNodes = @($project.SelectNodes('/Project/PropertyGroup/PackageId'))
if ($packageIdNodes.Count -ne 1 -or $packageIdNodes[0].InnerText -cne $releasePackage.packageId) {
    throw 'Configured packageId must match the selected project PackageId.'
}
foreach ($field in @('PackageVersion', 'AssemblyVersion', 'FileVersion')) {
    if (@($project.SelectNodes("/Project/PropertyGroup/$field")).Count -ne 1) {
        throw "Expected exactly one $field element."
    }
}
$currentText = $project.SelectSingleNode('/Project/PropertyGroup/PackageVersion').InnerText
$current = Get-StableVersion $currentText
$next = Get-StableVersion $Version
if ($next -le $current) { throw "New version $Version must be greater than $currentText, even with an override." }
$normalVersions = @(
    "$($current.Major).$($current.Minor).$($current.Build + 1)",
    "$($current.Major).$($current.Minor + 1).0"
)
$majorRelease = $next.Minor -eq 0 -and $next.Build -eq 0
$requiresApproval = $majorRelease -or $Version -notin $normalVersions
$approvalType = if ($majorRelease) { 'Major release' } elseif ($requiresApproval) { 'Unusual version progression' } else { 'Standard version progression' }
if ($requiresApproval -and [string]::IsNullOrWhiteSpace($OverrideReason)) {
    throw "$approvalType $currentText -> $Version requires a reason and explicit workflow approval. Supply override_reason."
}
if ([string]::IsNullOrWhiteSpace($Notes)) { throw 'Release notes cannot be empty.' }

# Notes are Markdown: preserve nesting, blank lines, bullet style, and literal pipes.
# Normalize newlines only; literal backslash-n is not interpreted as a newline.
$notesMarkdown = ($Notes -replace '\r\n?', "`n").Trim([char[]]"`n")
$heading = [regex]::new('(?m)^## Release notes\r?$')
if ($heading.Matches($readmeText).Count -ne 1) { throw 'Expected exactly one README release notes heading.' }
if ($readmeText -match "(?m)^\* Version $([regex]::Escape($Version))\s+-") {
    throw "README already contains release $Version."
}
# Preserve project formatting rather than serializing the whole XML document.
foreach ($field in @('PackageVersion', 'AssemblyVersion', 'FileVersion')) {
    $pattern = [regex]::new("(<$field>)[^<]*(</$field>)")
    if ($pattern.Matches($projectText).Count -ne 1) { throw "Unsupported formatting for $field." }
    $projectText = $pattern.Replace($projectText, [System.Text.RegularExpressions.MatchEvaluator]{
        param($match)
        $match.Groups[1].Value + $Version + $match.Groups[2].Value
    })
}
$newline = if ($readmeText.Contains("`r`n")) { "`r`n" } else { "`n" }
$date = [TimeZoneInfo]::ConvertTimeBySystemTimeZoneId([DateTimeOffset]::UtcNow, 'Europe/Amsterdam').ToString('dd MMM yyyy', [Globalization.CultureInfo]::InvariantCulture)
$entry = "* Version $Version - $date$newline"
$entry += (($notesMarkdown -split "`n" | ForEach-Object { if ($_.Length) { "    $_" } else { '' } }) -join $newline)
$readmeText = $heading.Replace($readmeText, [System.Text.RegularExpressions.MatchEvaluator]{
    param($match)
    $match.Value.TrimEnd("`r") + $newline + $entry + $newline
}, 1)
$review = if ($requiresApproval) {
    "**$approvalType approval required:** $currentText -> $Version`n`nReason: $OverrideReason`n`nThe workflow must pass the protected version-approval environment before creating this PR."
} else { "Standard version progression: $currentText -> $Version" }
$body = @"
## Release $($releasePackage.packageId) $Version

$review

### Release notes

$notesMarkdown

### Review before merging

"@
$result = [ordered]@{
    package = $Package
    package_id = $releasePackage.packageId
    previous_version = $currentText
    version = $Version
    base_branch = $config.baseBranch
    branch = "release/$Package/$Version"
    tag = "$($releasePackage.tagPrefix)$Version"
    sdk = $config.sdk
    project = $releasePackage.project
    test_projects = @($releasePackage.testProjects)
    readme = $releasePackage.readme
    requires_approval = $requiresApproval
}
$encoding = [Text.UTF8Encoding]::new($false)
# Finish validation before modifying repository files. In CI, preview and result
# paths point outside the repository, including during validation-only runs.
if (-not $ValidateOnly) {
    [IO.File]::WriteAllText($projectPath, $projectText, $encoding)
    [IO.File]::WriteAllText($readmePath, $readmeText, $encoding)
}
[IO.File]::WriteAllText($BodyPath, $body, $encoding)
if ($ResultPath) { [IO.File]::WriteAllText($ResultPath, ($result | ConvertTo-Json -Depth 5), $encoding) }
Write-Host "Validated $($releasePackage.packageId) $currentText -> $Version; additional approval: $requiresApproval"
