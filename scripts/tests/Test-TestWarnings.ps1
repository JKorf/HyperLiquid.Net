$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$script = Join-Path $PSScriptRoot '../Add-TestWarnings.ps1'
$fixture = Join-Path ([IO.Path]::GetTempPath()) ('test-warning-report-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $fixture | Out-Null
$bodyPath = Join-Path $fixture 'body.md'
$summaryPath = Join-Path $fixture 'summary.md'
$results = Join-Path $fixture 'results'
New-Item -ItemType Directory -Path $results | Out-Null
$originalBody = "## Release notes`n`n- Feature 1`n  - Additional info"
$xml = @'
<test-run><test-suite>
  <test-case fullname="Aster.TestFuturesExchangeData" result="Warning">
    <assertions>
      <assertion result="Warning"><message>Missing property `assetType`: crypto</message><stack-trace>Stack trace</stack-trace></assertion>
      <assertion result="Warning"><message>Missing property `assetType`: stable_coin</message></assertion>
      <assertion result="Warning"><message>Missing property `assetType`: stock</message></assertion>
      <assertion result="Warning"><message>Missing property `assetType`: other &lt;/pre&gt;&lt;script&gt;</message></assertion>
    </assertions>
  </test-case>
  <test-case fullname="OrdinarySkipped" result="Skipped"><reason><message>Not enabled</message></reason></test-case>
  <test-case fullname="OrdinaryPassed" result="Passed"><output>Warning in arbitrary console text</output></test-case>
</test-suite></test-run>
'@
[IO.File]::WriteAllText((Join-Path $results 'nunit.xml'), $xml)
[IO.File]::WriteAllText($bodyPath, $originalBody)
& $script -ResultsDirectory $results -BodyPath $bodyPath -SummaryPath $summaryPath -RunUrl 'https://github.com/example/library/actions/runs/123' -ExpectResults
$summary = [IO.File]::ReadAllText($summaryPath)
$body = [IO.File]::ReadAllText($bodyPath)
foreach ($expected in @('4 warning(s) in 1 test result(s)', 'Aster.TestFuturesExchangeData', 'stable_coin', 'snapshot', 'actions/runs/123', '&lt;/pre&gt;&lt;script&gt;')) {
    if (-not $summary.Contains($expected)) { throw "Missing $expected" }
}
if ($summary.Contains('OrdinarySkipped') -or $summary.Contains('OrdinaryPassed') -or $summary.Contains('<script>')) { throw 'False positive or unescaped test data.' }
if (-not $body.StartsWith($originalBody)) { throw 'Release notes were modified.' }

# Older result shapes carry one combined message rather than assertion nodes.
[IO.File]::WriteAllText((Join-Path $results 'nunit.xml'), '<test-run><test-case name="LegacyWarning" result="Passed" label="Warning"><failure><message>Combined warning message</message></failure></test-case></test-run>')
[IO.File]::WriteAllText($bodyPath, $originalBody)
& $script -ResultsDirectory $results -BodyPath $bodyPath -SummaryPath $summaryPath -ExpectResults
if (-not ([IO.File]::ReadAllText($summaryPath)).Contains('Combined warning message')) { throw 'Warning label fallback failed.' }

# Multiple reports (projects/target frameworks) are all included.
[IO.File]::WriteAllText((Join-Path $results 'second.xml'), $xml)
[IO.File]::WriteAllText($bodyPath, $originalBody)
& $script -ResultsDirectory $results -BodyPath $bodyPath -SummaryPath $summaryPath -ExpectResults
if (-not ([IO.File]::ReadAllText($summaryPath)).Contains('5 warning(s) in 2 test result(s)')) { throw 'Multiple report aggregation failed.' }

# Large messages are omitted from the PR, with a pointer to the full reports.
$largeResults = Join-Path $fixture 'large'
New-Item -ItemType Directory -Path $largeResults | Out-Null
[IO.File]::WriteAllText((Join-Path $largeResults 'large.xml'), ('<test-run><test-case name="Large" result="Warning"><failure><message>' + ('x' * 70000) + '</message></failure></test-case></test-run>'))
[IO.File]::WriteAllText($bodyPath, $originalBody)
& $script -ResultsDirectory $largeResults -BodyPath $bodyPath -SummaryPath $summaryPath -ExpectResults
$body = [IO.File]::ReadAllText($bodyPath)
if ($body.Length -gt 60000 -or -not $body.Contains('omitted')) { throw 'Report truncation failed.' }

$cleanResults = Join-Path $fixture 'clean'
New-Item -ItemType Directory -Path $cleanResults | Out-Null
[IO.File]::WriteAllText((Join-Path $cleanResults 'clean.xml'), '<test-run><test-case name="Passed" result="Passed" /></test-run>')
[IO.File]::WriteAllText($bodyPath, $originalBody)
& $script -ResultsDirectory $cleanResults -BodyPath $bodyPath -SummaryPath $summaryPath -ExpectResults
if (-not ([IO.File]::ReadAllText($summaryPath)).Contains('No NUnit warnings')) { throw 'Clean run reporting failed.' }
& $script -ResultsDirectory $cleanResults -BodyPath $bodyPath -SummaryPath $summaryPath -Stage Publication -ExpectResults
$publication = [IO.File]::ReadAllText($summaryPath)
if (-not $publication.Contains('Publication test warnings') -or $publication.Contains('preparation')) { throw 'Final publication report used preparation wording.' }

$emptyResults = Join-Path $fixture 'empty'
New-Item -ItemType Directory -Path $emptyResults | Out-Null
[IO.File]::WriteAllText($bodyPath, $originalBody)
& $script -ResultsDirectory $emptyResults -BodyPath $bodyPath -SummaryPath $summaryPath
if (-not ([IO.File]::ReadAllText($summaryPath)).Contains('no tests ran')) { throw 'No-tests reporting failed.' }
$failed = $false
try { & $script -ResultsDirectory $emptyResults -BodyPath $bodyPath -SummaryPath $summaryPath -ExpectResults } catch { $failed = $true }
if (-not $failed) { throw 'Missing reports were silently treated as no warnings.' }
Write-Host "Passed warning-report regression checks. Fixture: $fixture"
