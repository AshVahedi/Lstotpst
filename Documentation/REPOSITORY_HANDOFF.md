# Repository handoff

Canonical repository: [AshVahedi/Lstotpst](https://github.com/AshVahedi/Lstotpst).
Default branch: **main**. Task 001's tested 0.1.0 foundation is published there.
The completed Tasks 001–004 and Graphify are consolidated without rewritten commits
on **development/integrated-foundation** in [PR #5](https://github.com/AshVahedi/Lstotpst/pull/5).
Task 005's **development/task-005-permanent-progression** is based on that verified
head and has a separate PR targeting it. PRs #1–4 remain open/unmerged. See
[ancestry and integration order](REPOSITORY_CONSOLIDATION.md). No PR/main change
was merged automatically. Unity compilation/generation/tests remain pending.

## Continue development

```sh
git clone https://github.com/AshVahedi/Lstotpst.git
cd Lstotpst
git switch development/task-005-permanent-progression
dotnet restore Tests/LastOutpost.Core.Tests.csproj
dotnet test Tests/LastOutpost.Core.Tests.csproj --no-restore --configuration Release
python3 Tools/validate_project.py
dotnet run --project Tools/UnitySyntaxCheck/UnitySyntaxCheck.csproj --configuration Release -- .
git rev-parse HEAD
```

Use Unity Hub to open this checkout when the editor becomes available, then
follow [the Unity checklist](UNITY_CHECKLIST.md). Install the documented editor
and Android modules rather than assuming the .NET SDK can build a Unity player.
Unity import, scene execution, and Android builds remain unverified.

Use ordinary commits and fast-forward pushes, following any future repository
instructions. Fetch and inspect remote changes before integrating work; never
force-push over someone else's changes. Generated binaries, caches, and test
results are ignored. Source, Unity metadata, documentation, project settings,
and the workflow belong in version control. Never commit credential files.

## Initial integration record

The environment initially had no checkout. An initial authentication failure
required preparing source and tests in a local staging directory without
initializing another repository. On retry, the supplied repository cloned
successfully and was empty: no commits, AGENTS.md, or existing source to preserve.
The 94 source, metadata, configuration, and documentation files were copied into
its actual checkout at `/workspace/Lstotpst`, excluding generated binaries/caches.
No existing user work was overwritten or nested Git repository created.

All 58 independent tests and static validation passed in that checkout. Initial
app and Git publication attempts returned HTTP 403. After the user updated GitHub
authorization, normal Git push succeeded to `main`, preserving foundation commit
`25155c90530e48647c14205300673a59dfae9a98` without rewriting its history.
The remote ref was independently confirmed through GitHub's API, and the push
triggered the real Core gameplay tests workflow. See [verification](VERIFICATION.md).

The earlier source archive and complete Git bundle preserve the initial handoff;
they are no longer needed to access the project. GitHub's `main` branch is the
authoritative source for continued development.

## Task 002 handoff

Historical Task 002 handoff: use its development branch/draft pull request for source review. No automatic
merge or force-push is used. Generate and test the scene in Unity before merging
or promoting to 0.2.0; see [Task 002 integration](TASK002_INTEGRATION.md).
The original source archive/bundle does not contain these development changes.
