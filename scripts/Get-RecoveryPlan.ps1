[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Repository,
    [Parameter(Mandatory)][long]$OriginalRunId,
    [Parameter(Mandatory)][long]$ArtifactId,
    [Parameter(Mandatory)][string]$ArtifactDirectory
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. "$PSScriptRoot/Release-PackageFunctions.ps1"
$run = Invoke-ReleaseApi "repos/$Repository/actions/runs/$OriginalRunId"
if ($run.event -ne 'pull_request' -or $run.path -ne '.github/workflows/publish-release.yml' -or $run.display_title -notmatch '^Publish release PR #(?<number>[1-9][0-9]*)$') {
    throw 'Recovery requires an original merge-triggered publish-release.yml run.'
}
$number = [int]$Matches.number
if ($run.status -ne 'completed' -or $run.conclusion -eq 'success') { throw 'Only a completed, unsuccessful original run may be recovered.' }
$pr = Invoke-ReleaseApi "repos/$Repository/pulls/$number"
if (-not $pr.merged -or $pr.head.repo.full_name -cne $Repository -or $pr.base.repo.full_name -cne $Repository -or $pr.base.ref -notin @('main', 'master')) {
    throw 'Original PR must be merged from this repository into main/master.'
}
if ($run.head_sha -cne $pr.merge_commit_sha -and ($run.head_sha -cne $pr.head.sha -or $run.head_branch -cne $pr.head.ref)) {
    throw 'Original run does not match the merged PR or its final head.'
}
$artifact = Invoke-ReleaseApi "repos/$Repository/actions/artifacts/$ArtifactId"
if ($artifact.expired -or $artifact.workflow_run.id -ne $OriginalRunId -or $artifact.name -notmatch "^release-pr-$number-attempt-[1-9][0-9]*$") {
    throw 'Artifact must be an unexpired release package artifact from the original run.'
}
$manifest = Assert-ReleaseArtifacts $ArtifactDirectory $Repository $pr.merge_commit_sha $number
if ($manifest.sdk -notmatch '^\d+\.\d+\.\d+$') { throw 'Invalid SDK version in artifact manifest.' }
if ($pr.head.ref -cne "release/$($manifest.package)/$($manifest.version)") { throw 'Artifact package/version differs from the merged preparation PR.' }
"sdk=$($manifest.sdk)" >> $env:GITHUB_OUTPUT
"commit=$($pr.merge_commit_sha)" >> $env:GITHUB_OUTPUT
"pull_request=$number" >> $env:GITHUB_OUTPUT
"artifact_id=$ArtifactId" >> $env:GITHUB_OUTPUT
"Recover $($manifest.packageId) $($manifest.version) from PR #$number, source commit $($pr.merge_commit_sha), original run $OriginalRunId, artifact $ArtifactId. No packages will be rebuilt." >> $env:GITHUB_STEP_SUMMARY
