# Release preparation

The preparation workflow builds and tests the selected package before opening a
release PR. It writes native NUnit XML and TRX reports, retains them as workflow
artifacts for 14 days, and adds a warning summary to the PR description.

Warnings from Assert.Warn are mapped to Passed for VSTest, so they do not block
preparation. Actual failures still make dotnet test fail and prevent creation of
the release PR. The summary reads NUnit warning assertions directly rather than
guessing from console output or treating every skipped test as a warning.

Each warning test is shown with its messages in an expandable section. Large
reports are shortened in the PR; full NUnit XML is available through the linked
preparation run. Warning details are preparation metadata, not release notes, and
are not inserted into the package README.

## Editing the prepared release

Make normal commits on the generated release branch and push them. The existing
PR updates automatically; no new preparation run is needed for code fixes,
documentation changes, or release-note edits that keep the same release version.
For example, from the repository directory:

```powershell
git fetch origin
git switch --track origin/release/aster/4.6.1
# Edit the files, including README release notes if needed.
git add <changed-files>
git commit -m "Adjust release preparation"
git push
```

If the branch already exists locally, use git switch release/aster/4.6.1 instead.
Update the PR description's release notes too when changing the README notes.
The warning section explicitly describes the initial preparation run; it does
not automatically refresh when later commits are pushed. Check the new ordinary
PR CI results before merging. Ordinary CI currently does not rerun optional
integration tests, so rerun those explicitly after relevant fixes.

If the requested version or selected package changes, create a fresh preparation
run so validation and any required version approval run again. Close the previous
release PR if superseded. Do not edit version fields to bypass the approval gate.
The preparation workflow intentionally refuses to overwrite an existing release
branch. Keep its version unchanged when adjusting that branch.

The publication stage builds and tests the final merged commit; the initial
preparation test report cannot validate later edits. See
[publishing setup and recovery](../docs/publishing.md).

## Regression tests

```powershell
./scripts/tests/Test-TestWarnings.ps1
./scripts/tests/Test-PublishRelease.ps1
./scripts/tests/Test-ReleaseHttp.ps1
```
