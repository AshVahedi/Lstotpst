# Last Outpost

Original Unity 2D combat survival prototype for portrait Android. A modular
armored platform advances automatically through **Industrial Wasteland**, fights
three deterministic waves and a final boss, and spends tactical Energy on
missiles, repairs, and a special strike. The basic gun never needs ammunition.

**Current version: 0.1.0. Task 006 progression/session/UI/recovery integration is
implemented and independently tested.** Unity
compilation, generated scenes, and gameplay remain unverified. The version is
retained until the Editor validation gate for 0.2.0 is met. There is no claim of a
validated playable build or Android APK. No Unity Editor is installed here.

## Run the prototype locally

1. Check out `development/task-006-unity-progression` from
   [AshVahedi/Lstotpst](https://github.com/AshVahedi/Lstotpst).
2. Open this repository root using **Unity 6.3 LTS, 6000.3.0f1**. Allow package
   resolution/import and inspect the Console for compiler/import errors.
3. Choose **Last Outpost → Generate Level 1 Prototype**. This creates the settings,
   placeholder art, four prefabs, camera, HUD, and scene, registers it in Build
   Settings, and opens `Assets/LastOutpostGenerated/Scenes/IndustrialWasteland.unity`.
4. Set Game View to **540 × 960** (9:16 portrait), enter Play Mode, and press
   **UPGRADES** to inspect permanent progression, then **START LEVEL 1**. Combat
   is automatic; three tactical buttons spend Energy. Results offer **BETWEEN-RUN MENU**.
5. Run all EditMode and PlayMode tests in **Window → General → Test Runner**.
   Complete [the Unity checklist](Documentation/UNITY_CHECKLIST.md) before
   considering this a verified playable milestone.

Follow [the exact first-launch procedure](Documentation/UNITY_FIRST_LAUNCH.md)
and [Task 003 readiness audit](Documentation/UNITY_READINESS_REPORT.md).
PRs #1–4 remain unmerged. The verified `development/integrated-foundation` contains
all their ancestry; [PR #5](https://github.com/AshVahedi/Lstotpst/pull/5) proposes
consolidating it into main. [PR #6](https://github.com/AshVahedi/Lstotpst/pull/6) adds Task 005 on that foundation.
Task 006 branches from its complete `b3b835b…` head and targets the Task 005 branch. See
[ancestry and review order](Documentation/REPOSITORY_CONSOLIDATION.md).
No Unity compilation, scene execution or APK build has run.

The generator is designed to be repeatable: stable output paths/GUIDs, one owned
scene root, no duplicate build entries, and preserved settings tuning/unrelated
scene roots. Its four Editor tests (including save-failure rollback) require
Unity and **have not run here**.
Rerunning regenerates the owned placeholder prefabs/root; do not put manual art
edits there. The menu asks to save modified scenes first. See
[setup details](Documentation/TASK002_INTEGRATION.md) for batch commands and ownership.

**No hand-authored `.unity` or `.prefab` files are supplied as if validated.**
Generation must actually run in Unity. Review and commit generated assets, `.meta`
files, imported ProjectSettings, and `Packages/packages-lock.json` after validation.

## Configuration and play rules

Tune `Assets/LastOutpostGenerated/Configuration/PrototypeSettings.asset` in the
Inspector. Settings construct existing validated core configuration; edits apply
when a fresh run is created, through the between-run menu. Speeds/ranges use world
units, timing uses seconds, and health/damage/Energy/ammunition use integer units.

| Purchase | Initial Energy | Effect |
| --- | ---: | --- |
| Missiles | 30 | Up to 20 ammunition, base capacity 100 + upgrades |
| Repair | 50 | Up to 25 health, base maximum 100 + upgrades |
| Special strike | 80 | 120 damage to the closest live target within 10 units |

Costs, effects, enemy stats, wave counts, weapon intervals/ranges, spawn offsets,
travel speed/delay, and ordinary encounter slowdown are configurable. Boss and
terminal movement always stop. There are exactly three ordinary waves and one
boss. Full health/ammo, insufficient funds, or an invalid special target causes
no charge. Partial refills/repairs cost the full listed price. Death cannot revive.

Combat uses **immediate-hit damage with a visual tracer**, not physical projectiles.
The core applies damage once; tracer lifetime/destruction cannot cause damage.
Enemy detection uses the authoritative one-dimensional route roster and range,
without physics queries or collision callbacks. Only living enemies attack.

## Tests supported here

```sh
dotnet restore Tests/LastOutpost.Core.Tests.csproj
dotnet test Tests/LastOutpost.Core.Tests.csproj --no-restore --configuration Release --logger "trx;LogFileName=core-tests.trx" --results-directory TestResults
python3 Tools/validate_project.py
python3 -m unittest discover -s Tools/tests -v
python3 Tools/validate_assemblies.py --json TestResults/unity-static-audit.json
dotnet run --project Tools/UnitySyntaxCheck/UnitySyntaxCheck.csproj --configuration Release -- .
```

**274 independent tests passed, 0 failed, 0 skipped**: 228 NUnit gameplay/progression
cases, 39 Python validator/CI-helper tests and seven Graphify tests. The original
246-case baseline was rerun first; Task 006 adds 28 integration/recovery cases.
Core compiles to .NET Standard 2.1 and executes in .NET 8, including real temporary
file save/recovery checks. This is not a Unity scene test.

The [cloud verification report](Documentation/CLOUD_UNITY_VERIFICATION.md)
documents the exact available Unity images, licensing setup and dedicated
`unity-verification.yml` workflow. **CLOUD VERIFICATION BLOCKED**: no actual Unity
execution is claimed. Required steps fail on absent activation or missing results;
Android is optional and gated on all Editor stages. Configure secrets only in
GitHub Settings. The [actual Unity CI run](https://github.com/AshVahedi/Lstotpst/actions/runs/42522941291)
failed activation preflight and published infrastructure evidence; Editor and
Android jobs were skipped before Unity execution. Independent core/Graphify CI
passed. Task 006 does not manually retry Unity licensing; independent tests need
no Unity account. Earlier task evidence remains in its historical reports.

**27 additional Unity-specific tests are authored and unexecuted:** 23 PlayMode
and 4 Editor generator cases. The shared 228 have also not run inside Unity.
Metadata/layer validation passes. All 50 asset C# sources parse without syntax
errors, using Roslyn from the installed SDK. Syntax parsing performs **no Unity
API/type binding or Editor compilation**, and uses no Unity stubs.
GitHub Actions executes the supported .NET/static checks and preserves actual TRX
results. There is no pretend Unity-success job. See [verification](Documentation/VERIFICATION.md).

## Permanent progression

The pure C# `ProgressionService` awards **500 Credits per confirmed Level 1 victory**,
with a durable run-ID ledger preventing duplicate payment across save/load. Defeat,
abandonment and restart award zero. Credits persist separately from tactical Energy.

Four configurable upgrades cover +10 HP, +3 percentage points armor, +0.05 weapon
multiplier and +20 ammo capacity per level, each capped at level 10. Next costs use
exact rational `ceil(base × growth^currentLevel)` calculations. Transactions save
the whole profile before publishing new balance/levels; failures/conflicts block
unsafe retries. Memory/file stores and versioned JSON support tested persistence.

See [PERMANENT_PROGRESSION.md](Documentation/PERMANENT_PROGRESSION.md) for prices,
effects, schema, recovery and integration examples. The Unity session now adopts the registered
`ProgressionRunSession.Run` with a 100-round base and fixed run-start upgrades.
A persistent host, thin between-run upgrade controls and explicit backed-up save
recovery are implemented; Unity execution is still pending. See
[Unity progression integration](Documentation/UNITY_PROGRESSION_INTEGRATION.md).

## Architecture and layout

```text
Assets/
  Scripts/
    Core/Simulation/       GameRun, route, agents, combat coordination (plain C#)
    Core/Progression/      Credits, upgrade catalog/snapshots, run ledger, JSON and stores
    Core/...               Task 001 health, economy, weapons, modules, level rules
    Levels/                OutpostSession, persistent ProgressionHost, settings
    Platform/              PlatformMotor transform adapter
    Enemies/               EnemyActor and EnemySpawner
    Presentation/          Tracers, camera follow, health bars
    UI/                    Start/upgrades/recovery/HUD/results via IMGUI
  Editor/                  Reproducible scene/prefab/settings generator
  Tests/EditMode/          Shared NUnit core/simulation tests
  Tests/PlayMode/          Actual Unity object/lifecycle/HUD tests (unexecuted)
  Tests/Editor/            Generator idempotence tests (unexecuted)
  LastOutpostGenerated/    Produced by Unity only; absent until generation runs
Packages/ ProjectSettings/ Unity toolchain and portrait Android intent
Tests/                     Independent .NET library and NUnit runner
Tools/                     Metadata/assembly validators, tests and C# syntax-only tool
Documentation/             Architecture, integration contract, verification, checklist
.github/workflows/         Executed .NET/static CI, no unlicensed Unity runner
```

Dependencies: existing Unity Test Framework **1.4.5**, plus explicit built-in Unity
**IMGUI** and **Image Conversion** modules **1.0.0** for the HUD/editor PNG generation.
No asset-store assets, gameplay libraries, or extra NuGet packages were added.
The syntax tool uses the compiler already bundled in .NET SDK **8.0.4xx**.
Android intent remains portrait, API 26 minimum, ARM64/IL2CPP. Install Unity's
Android Build Support with matching SDK/NDK/OpenJDK before attempting a build.

See [architecture](Documentation/ARCHITECTURE.md),
[Task 002 integration](Documentation/TASK002_INTEGRATION.md), and
[repository handoff](Documentation/REPOSITORY_HANDOFF.md).

Next task: execute the Unity import, generation, both test suites and progression
playthrough, then validate Android persistence/device behavior when available.
Promote to 0.2.0 only after its validation gate is met.

## Queryable architecture map for Codex

Development-only [Graphify setup and query guide](Documentation/GRAPHIFY.md) registers
its official project Codex skill, provides local incremental updates, and publishes
fresh architecture artifacts from `.github/workflows/graphify.yml`.
The committed [report](graphify-out/GRAPH_REPORT.md), [JSON graph](graphify-out/graph.json)
and downloadable [visualization](graphify-out/graph.html) provide a persistent map.
Run `python3 Tools/Graphify/setup.py` once, activate `.venv-graphify`, then use
`graphify explain "GameRun.Tick()"` before changing important systems.
