# Repository handoff

## Repository and integration

Canonical repository: [AshVahedi/Lstotpst](https://github.com/AshVahedi/Lstotpst).
Default branch: **main**.

The environment initially had no checkout, and the first clone failed because
GitHub authentication was unavailable. Source and tests were prepared in a local
staging directory without initializing another Git repository.

On retry, the connected GitHub app reported repository access with push permission,
and cloning the supplied URL succeeded. The repository was empty: no commits,
AGENTS.md, existing source, or project conventions were present. The 94 source,
metadata, configuration, and documentation files were copied into its actual
checkout at `/workspace/Lstotpst`; generated binaries and test caches were excluded.
No existing user work was overwritten, and no nested Git repository was created.

The independent tests and static validator were rerun from this checkout before
the publication attempt. Although repository metadata reports the connected
account's push permission, the app's actual Contents API write was rejected:

```text
GitHub API error 403: Resource not accessible by integration
```

The attempted initial README write created no remote commit. A complete local
foundation commit is prepared on `main` using the configured Git author identity.
Normal Git push was attempted and also rejected:

```text
remote: Permission to AshVahedi/Lstotpst.git denied to AshVahedi.
fatal: unable to access 'https://github.com/AshVahedi/Lstotpst.git/': The requested URL returned error: 403
```

The remote repository remains empty. No force-push was attempted. The complete
local commit is preserved in the task's `LastOutpost-0.1.0.bundle` artifact; its
hash is reported in the task response. GitHub Actions has not run.

To resolve app publication access, grant the connected GitHub app access to this
repository with Contents and Workflows write permissions, or configure normal
authenticated Git push in the environment. Do not send credentials in chat.

## Continue development

Download the Git bundle from the task response to preserve the local commit:

```sh
git clone -b main LastOutpost-0.1.0.bundle Lstotpst
cd Lstotpst
git remote set-url origin https://github.com/AshVahedi/Lstotpst.git
dotnet restore Tests/LastOutpost.Core.Tests.csproj
dotnet test Tests/LastOutpost.Core.Tests.csproj --no-restore --configuration Release
python3 Tools/validate_project.py
git rev-parse HEAD
```

Once authenticated write access is fixed, publish with `git push -u origin main`.
If another commit appears on the remote meanwhile, fetch and inspect it first;
integrate it without overwriting work or force-pushing. After publication, a
normal clone of the GitHub URL will contain the project, and the workflow can run.

Use Unity Hub to open this checkout when the editor becomes available, then
follow [the Unity checklist](UNITY_CHECKLIST.md). Install the documented editor
and Android modules rather than assuming the .NET SDK can build a Unity player.

Generated .NET binaries, caches, and TestResults are intentionally ignored.
Source, Unity metadata, documentation, project settings, and the workflow belong
in version control. Do not copy another checkout's `.git` directory or commit
credential files. The original staging archive represents the earlier local
handoff. Use the actual checkout and its local commit until publication succeeds;
afterward the repository's `main` branch is authoritative for continued work.
