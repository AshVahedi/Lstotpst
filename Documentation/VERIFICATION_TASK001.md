# Verification record — 0.1.0

## Executed successfully in this task

- **58 NUnit tests passed, 0 failed, 0 skipped** using the official .NET SDK
  **8.0.427** on Debian 13, Linux x64.
- The core compiled in Release to **.NET Standard 2.1** with warnings treated
  as errors. The shared tests compiled and ran in a **.NET 8** test host.
- Test result file: `TestResults/core-tests.trx` (generated local artifact, ignored
  by Git). GitHub Actions will upload its own real TRX on each future run.

Executed commands (SDK installed under `/tmp`, not included in the project):

```sh
dotnet restore Tests/LastOutpost.Core.Tests.csproj --disable-parallel
dotnet test Tests/LastOutpost.Core.Tests.csproj --no-restore --configuration Release --logger "trx;LogFileName=core-tests.trx" --results-directory TestResults
```

The sandbox could not initialize .NET's `NuGet-Migrations` named mutex. Restore
and tests ran through the supported escalated command mechanism, with temporary
`DOTNET_CLI_HOME` and `NUGET_PACKAGES` directories. Dependencies were restored from
their actual registries. No test stubs or replacement test runner were used.

| Fixture | Executed tests | Coverage |
| --- | ---: | --- |
| HealthTests | 5 | Damage, zero/death, clamped healing, invalid inputs, overflow-safe bounds |
| EconomyTests | 17 | Collection/spending, all purchases, failed-state preservation, caps, cooldown, duplicate rewards, overflow |
| WeaponTests | 16 | Unlimited fallback, missile consumption, intervals, range, closest/tie targeting, invalid input, multi-round ammo |
| ModuleTests | 6 | Attach/detach, capacity, duplicate/null/out-of-range rejection, initial modules |
| LevelTests | 14 | Wave order/gating, boss/victory, defeat/simultaneous death, config immutability, headless replay |

The headless replay test exercises core weapon damage, roster progression,
resource rewards, and ammo purchases through boss defeat twice and compares
deterministic outcomes. It supplies synthetic distances and no enemy movement;
it does **not** demonstrate a playable or balanced level.

## Static project validation

`python3 Tools/validate_project.py` checks unique asset/folder metadata, orphaned
metadata, core source dependency boundaries, and test assembly/package presence.
**Passed:** 43 Unity assets/folders have unique metadata; 13 core source files
have no Unity dependencies; 5 shared NUnit fixtures found. The test assembly is
Editor-only and references the real core. Workflow YAML also parsed successfully
and contains a real `dotnet test` step.

These results must be read alongside the test results; neither check invokes
Unity or establishes that the workflow has run on GitHub.

## GitHub publication

After the user updated repository authorization, normal Git push succeeded to
`main`. GitHub's API confirmed foundation commit
`25155c90530e48647c14205300673a59dfae9a98` on the remote branch. The push triggered
[Core gameplay tests run 42504886370](https://github.com/AshVahedi/Lstotpst/actions/runs/42504886370).
**Completed successfully.** The actual job log reports **58 passed, 0 failed,
0 skipped** in the .NET 8 test host. The metadata/architecture validator also
passed, and the `core-test-results` TRX artifact was uploaded successfully
(artifact ID `13627525162`). This confirms real GitHub-hosted test execution in
addition to the earlier local run. No force-push was used.

The documentation follow-up changes no core source, test source, packages, or
workflow logic. Subsequent push runs are visible on the repository Actions tab;
the linked result above specifically validates the foundation commit.

## Authored but not executed here

- **Unity EditMode execution:** the same 58 test cases are available through the
  Editor-only test assembly; no Unity Test Runner was available.

## Not implemented or not verified

- No Unity Editor installation was found; no Unity asset import, package
  resolution, player compilation, scene execution, or PlayMode testing occurred.
- No scenes, route movement, spawning transforms, projectile visuals, physics,
  input, UI/screens, graphics, or audio are implemented.
- No Android SDK/NDK validation, APK generation, installation, performance test,
  touch test, or physical-device gameplay was performed.
- No gameplay balance claim is made. Enemy movement speeds and combat defaults
  are initial configurable values for the next milestone.

Follow [the Unity checklist](UNITY_CHECKLIST.md) for the next validation step and
[the repository handoff](REPOSITORY_HANDOFF.md) for continued version control.
