# Last Outpost

Original Unity 2D combat survival prototype for portrait Android. A modular
armored platform advances automatically through **Industrial Wasteland**, fights
three deterministic waves and a final boss, and spends tactical Energy on
missiles, repairs, and a special strike. The basic gun never needs ammunition.

**Current version: 0.1.0. Task 002 integration source is implemented; Unity
compilation, generated scenes, and gameplay remain unverified.** The version is
retained until the Editor validation gate for 0.2.0 is met. There is no claim of a
validated playable build or Android APK. No Unity Editor is installed here.

## Run the prototype locally

1. Check out `development/task-002-unity-integration` from
   [AshVahedi/Lstotpst](https://github.com/AshVahedi/Lstotpst).
2. Open this repository root using **Unity 6.3 LTS, 6000.3.0f1**. Allow package
   resolution/import and inspect the Console for compiler/import errors.
3. Choose **Last Outpost → Generate Level 1 Prototype**. This creates the settings,
   placeholder art, four prefabs, camera, HUD, and scene, registers it in Build
   Settings, and opens `Assets/LastOutpostGenerated/Scenes/IndustrialWasteland.unity`.
4. Set Game View to **540 × 960** (9:16 portrait), enter Play Mode, and press
   **START RUN**. Movement, targeting, and combat are automatic; use the three
   purchase buttons to spend earned Energy. Victory/defeat offers **RUN AGAIN**.
5. Run all EditMode and PlayMode tests in **Window → General → Test Runner**.
   Complete [the Unity checklist](Documentation/UNITY_CHECKLIST.md) before
   considering this a verified playable milestone.

The generator is designed to be repeatable: stable output paths/GUIDs, one owned
scene root, no duplicate build entries, and preserved settings tuning/unrelated
scene roots. Its two idempotence tests require Unity and **have not run here**.
Rerunning regenerates the owned placeholder prefabs/root; do not put manual art
edits there. The menu asks to save modified scenes first. See
[setup details](Documentation/TASK002_INTEGRATION.md) for batch commands and ownership.

**No hand-authored `.unity` or `.prefab` files are supplied as if validated.**
Generation must actually run in Unity. Review and commit generated assets, `.meta`
files, imported ProjectSettings, and `Packages/packages-lock.json` after validation.

## Configuration and play rules

Tune `Assets/LastOutpostGenerated/Configuration/PrototypeSettings.asset` in the
Inspector. Settings construct existing validated core configuration; edits apply
when a fresh run is created, including **RUN AGAIN**. Speeds/ranges use world
units, timing uses seconds, and health/damage/Energy/ammunition use integer units.

| Purchase | Initial Energy | Effect |
| --- | ---: | --- |
| Missiles | 30 | Up to 20 ammunition, capacity 60 |
| Repair | 50 | Up to 25 health, maximum 100 |
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
dotnet run --project Tools/UnitySyntaxCheck/UnitySyntaxCheck.csproj --configuration Release -- .
```

**82 NUnit tests passed, 0 failed, 0 skipped**: the original 58 plus 24 simulation
integration cases. They compile the actual pure C# source to .NET Standard 2.1
and execute in .NET 8, including a complete deterministic run with enemy attacks
and tactical ammunition purchases. This is not a scene test.

**19 additional Unity-specific tests are authored and unexecuted:** 17 PlayMode
and 2 Editor generator tests. The shared 82 have also not run inside Unity.
Metadata/layer validation passes. All 38 asset C# sources parse without syntax
errors, using Roslyn from the installed SDK. Syntax parsing performs **no Unity
API/type binding or Editor compilation**, and uses no Unity stubs.
GitHub Actions executes the supported .NET/static checks and preserves actual TRX
results. There is no pretend Unity-success job. See [verification](Documentation/VERIFICATION.md).

## Architecture and layout

```text
Assets/
  Scripts/
    Core/Simulation/       GameRun, route, agents, combat coordination (plain C#)
    Core/...               Task 001 health, economy, weapons, modules, level rules
    Levels/                OutpostSession, serialized settings, generated-root marker
    Platform/              PlatformMotor transform adapter
    Enemies/               EnemyActor and EnemySpawner
    Presentation/          Tracers, camera follow, health bars
    UI/                    Start/HUD/purchases/victory/defeat via IMGUI
  Editor/                  Reproducible scene/prefab/settings generator
  Tests/EditMode/          Shared NUnit core/simulation tests
  Tests/PlayMode/          Actual Unity object/lifecycle/HUD tests (unexecuted)
  Tests/Editor/            Generator idempotence tests (unexecuted)
  LastOutpostGenerated/    Produced by Unity only; absent until generation runs
Packages/ ProjectSettings/ Unity toolchain and portrait Android intent
Tests/                     Independent .NET library and NUnit runner
Tools/                     Metadata/architecture validator and C# syntax-only tool
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

Next task: validate import, generator, all Unity tests, portrait gameplay and
Android development build in a Unity-capable environment; fix observed issues,
commit generated assets, and promote to 0.2.0 only after that validation.
