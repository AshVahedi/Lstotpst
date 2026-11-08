# Verification record — Task 002 integration

Version retained at **0.1.0** until Unity verification. This record distinguishes
executed .NET/static checks from authored Unity tests and manual requirements.
The historical Task 001 evidence is preserved in [its record](VERIFICATION_TASK001.md).

## Executed in this environment

- Baseline: all **58 original NUnit cases passed** before implementation, with
  0 failures and 0 skipped (`TestResults/task-002-baseline.trx`).
- Expanded final core/simulation suite: **82 passed, 0 failed, 0 skipped** using
  .NET SDK **8.0.427** on Linux x64, Release configuration, warnings as errors.
  Core compiles against **.NET Standard 2.1**, tests execute in **.NET 8**.
  Actual output: `TestResults/task-002-core.trx` (generated/ignored).
- Static validator: **passed** for 73 Unity assets/folders with unique metadata,
  18 engine-independent core source files, 6 shared NUnit fixtures and 8 assembly
  boundaries. UI does not directly mutate health, ammunition or resource balances.
- SDK Roslyn C# syntax parser: **38 asset sources, 0 syntax errors**.
  This performs no Unity API/type binding, Editor compilation or scene execution.
  No engine stubs or additional NuGet dependencies were used.

| Shared fixture | Executed cases |
| --- | ---: |
| HealthTests | 5 |
| EconomyTests | 17 |
| WeaponTests | 16 |
| ModuleTests | 6 |
| LevelTests | 14 |
| SimulationTests (new) | 24 |
| Total | **82** |

The 24 new cases cover movement modes/frame partitioning, start/travel gating,
spawn positions/notifications, closest automatic fire/interval/range, enemy
approach/attacks/contact rounding, ammo/module fallback, once-only rewards,
wave travel timing, boss stop, victory/defeat/simultaneous death, terminal freeze,
UI command boundary/atomic purchases, restart and a full deterministic run with
real enemy attack rules and tactical ammo purchases. They do not render a scene.
The first expanded run exposed two first-shot test-fixture timing assumptions;
the fixture now stops exactly at the travel boundary before observing combat.

Commands (SDK executable located under /tmp in this environment):

```sh
dotnet test Tests/LastOutpost.Core.Tests.csproj --no-restore --configuration Release --logger "trx;LogFileName=task-002-core.trx" --results-directory TestResults
python3 Tools/validate_project.py
dotnet run --project Tools/UnitySyntaxCheck/UnitySyntaxCheck.csproj --configuration Release -- .
```

The existing sandbox mutex restriction requires the supported escalated execution
mechanism for .NET commands here. Dependencies use the real restored packages.

## Authored, not executed in Unity

| Unity-only fixture | Cases authored | Status |
| --- | ---: | --- |
| GameplayIntegrationTests (PlayMode) | 17 | **Unexecuted** |
| SceneBuilderTests (Editor/EditMode) | 2 | **Unexecuted** |
| Additional Unity-specific total | **19** | **Unexecuted** |

The shared 82 cases have also not run inside Unity; they are .NET passes only.
There are **101 authored cases across all assemblies**, not 101 executed passes.
No Unity tests were marked skipped to fabricate a green result.

PlayMode cases exercise actual objects, pose synchronization, spawning/destruction,
automatic shots, tracer single-damage contract, ammo fallback, rewards/waves/boss,
outcome events, HUD commands, restart/subscriptions, pause and settings.
Editor cases exercise repeat generation, stable GUIDs/tuning, unrelated-root
preservation and build-entry idempotence. Source presence proves none of those
Unity lifecycle/serialization behaviors have worked yet.

## Unity/Android and CI limits

No Unity Editor binary was found on PATH or in expected installation locations.
No Editor asset import/package resolution, Unity API compilation, scene generation,
prefab serialization, PlayMode execution, graphical playthrough, APK generation or
physical-device testing occurred. The generator is source ready for a real Editor;
no generated scene/prefab is committed as if validated. Version stays 0.1.0.

The existing GitHub Actions workflow now runs .NET tests, static boundaries and
syntax-only parsing and uploads actual TRX results. Task 002's remote run must be
observed after branch publication; prior Task 001 green runs do not verify these
changes. The final task response reports the observed branch/PR/CI outcome.
No pretend Unity workflow is provided without an Editor/license-backed runner.

Follow [the Unity checklist](UNITY_CHECKLIST.md) before promoting to 0.2.0.
