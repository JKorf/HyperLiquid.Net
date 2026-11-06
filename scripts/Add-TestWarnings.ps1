[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ResultsDirectory,
    [Parameter(Mandatory)][string]$BodyPath,
    [Parameter(Mandatory)][string]$SummaryPath,
    [string]$RunUrl = '',
    [ValidateSet('Preparation', 'Publication')][string]$Stage = 'Preparation',
    [switch]$ExpectResults
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$files = @(Get-ChildItem -LiteralPath $ResultsDirectory -Filter '*.xml' -Recurse -File)
$warnings = [Collections.Generic.List[object]]::new()
$reportCount = 0
foreach ($file in $files) {
    $xml = [xml][IO.File]::ReadAllText($file.FullName)
    if ($xml.DocumentElement.LocalName -ne 'test-run') { continue }
    $reportCount++
    foreach ($test in $xml.SelectNodes('//test-case')) {
        $assertions = @($test.SelectNodes("assertions/assertion[@result='Warning']"))
        if ($assertions.Count -eq 0 -and $test.GetAttribute('result') -ne 'Warning' -and $test.GetAttribute('label') -ne 'Warning') {
            continue
        }
        $name = $test.GetAttribute('fullname')
        if (-not $name) { $name = $test.GetAttribute('name') }
        $messages = @($assertions | ForEach-Object {
            $message = $_.SelectSingleNode('message')
            if ($null -ne $message) { $message.InnerText }
        })
        if ($messages.Count -eq 0) {
            $message = $test.SelectSingleNode('failure/message | reason/message')
            $messages = @($(if ($null -ne $message) { $message.InnerText } else { 'NUnit reported a warning without a message.' }))
        }
        $warnings.Add(@{ Name = $name; Messages = $messages })
    }
}
if ($ExpectResults -and $reportCount -eq 0) {
    throw 'No NUnit XML results were produced; cannot determine whether tests reported warnings.'
}

$count = 0
foreach ($test in $warnings) { $count += $test.Messages.Count }
$header = "### $Stage test warnings`n`n"
if ($reportCount -eq 0) {
    $header += "No test projects were configured; no tests ran.`n"
} elseif ($count -eq 0) {
    $header += "No NUnit warnings were reported in this $($Stage.ToLowerInvariant()) run.`n"
} else {
    $header += "$count warning(s) in $($warnings.Count) test result(s). Warnings do not block release $($Stage.ToLowerInvariant()).`n"
}
if ($Stage -eq 'Preparation') {
    $header += "`nThis is a snapshot of the preparation tests. Later commits to the release branch require fresh PR checks; this section is not automatically refreshed.`n"
} else {
    $header += "`nThis is a snapshot of the final publication tests for this run.`n"
}
if ($RunUrl) { $header += "`n[$Stage run and test reports]($RunUrl)`n" }

# Keep space for release notes and GitHub's PR body limit. Full XML remains in the
# workflow artifacts. Escape test data so it cannot break the Markdown/HTML layout.
$body = [IO.File]::ReadAllText($BodyPath)
$budget = [Math]::Max(0, [Math]::Min(24000, 60000 - $body.Length - $header.Length))
$details = ''
$shown = 0
foreach ($test in $warnings) {
    $name = [Net.WebUtility]::HtmlEncode($test.Name)
    $text = ($test.Messages | ForEach-Object { $_ }) -join "`n`n"
    $text = [Net.WebUtility]::HtmlEncode($text)
    $section = "`n<strong>$name</strong>`n<pre>$text</pre>`n"
    if ($details.Length + $section.Length -gt $budget) { break }
    $details += $section
    $shown++
}
$report = $header
if ($details) { $report += "`n<details>`n<summary>Warning details</summary>`n$details`n</details>`n" }
if ($shown -lt $warnings.Count) { $report += "`nAdditional warning details were omitted to fit the PR description. See the NUnit XML artifacts in the preparation run.`n" }
$encoding = [Text.UTF8Encoding]::new($false)
[IO.File]::WriteAllText($SummaryPath, $report, $encoding)
[IO.File]::WriteAllText($BodyPath, "$body`n`n$report", $encoding)
Write-Host "Recorded $count NUnit warning(s) from $reportCount result file(s)."
