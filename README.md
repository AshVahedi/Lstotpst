# Last Outpost

Version **0.1.0** — tested core foundation for an original portrait Android
combat survival game built with Unity 2D and C#.

A modular armored vehicle will advance automatically through hostile territory.
Its basic gun always has unlimited ammunition. Energy earned from enemies buys
missiles, emergency repairs, and a special strike during a level. The initial
level is **Industrial Wasteland**: Basic wave, Fast wave, Mixed wave, then boss.

**This version implements the core foundation planned for Milestones 1–2;
it is not yet a playable Unity prototype.**
No scenes, movement controllers, visuals, UI, or APK
are included. No Unity Editor is installed in the development environment, so
Unity integration is deliberately deferred under the task's milestone gate.

## Setup

- Intended editor: **Unity 6.3 LTS, 6000.3.0f1**. Install with Unity Hub and the
  Android Build Support, SDK/NDK Tools, and OpenJDK modules.
- Unity dependency: **Unity Test Framework 1.4.5**. Its registry metadata declares
  Unity 2019.4 as its minimum; Unity resolves its transitive dependencies on import.
  No asset-store assets or third-party gameplay libraries are required.
- Independent test runner: **.NET SDK 8.0.4xx**; `global.json` allows the latest
  patch in that feature band. The core itself targets **.NET Standard 2.1** and
  uses C# 8 or earlier syntax. .NET 8 is only a test host, not an Android runtime.
- Android intent: portrait, minimum API 26, ARM64, IL2CPP; target API automatic
  for the installed Unity toolchain. Inspect these settings in Unity before builds.
  `com.lastoutpost.prototype` is a development application identifier.

Open this directory through Unity Hub. Allow package resolution and asset import,
then use **Window → General → Test Runner → EditMode → Run All**. The project
contains no startup scene yet; see [the Unity checklist](Documentation/UNITY_CHECKLIST.md).
The first import may fill in additional ProjectSettings defaults and create
`Packages/packages-lock.json`; review and commit those through the real checkout.

## Test without Unity

```sh
dotnet restore Tests/LastOutpost.Core.Tests.csproj
dotnet test Tests/LastOutpost.Core.Tests.csproj --no-restore --configuration Release --logger "trx;LogFileName=core-tests.trx" --results-directory TestResults
python3 Tools/validate_project.py
```

The .NET project compiles the **same core and NUnit sources** used by Unity; it
does not copy rules into a separate implementation or use Unity API stubs.
GitHub Actions runs these commands for pushes and pull requests and uploads the
actual TRX results. It does not claim Unity or Android validation.
See [verification results and limitations](Documentation/VERIFICATION.md).

## Layout

```text
Assets/
  Scripts/
    Core/                 Plain C# assembly; health, economy, combat, enemies,
                          modules, platform stats, level progression, defaults
    Combat/ Enemies/      Reserved Unity gameplay adapters
    Platform/ Progression/ Levels/
    UI/                   Reserved presentation layer
  Tests/EditMode/         Shared NUnit core tests and Unity test assembly
  Tests/PlayMode/         Reserved for future Unity integration tests
  Scenes/ Prefabs/ ScriptableObjects/ Art/ Audio/  Reserved integration folders
Packages/                 Unity package manifest
ProjectSettings/          Intended editor, 2D defaults, portrait Android settings
Tests/                    Independent .NET Standard library/.NET 8 test projects
Tools/                    Static project/metadata validation
Documentation/            Decisions, verification, integration checklist
.github/workflows/        Real .NET core test workflow
```

All rule implementations live under `Assets/Scripts/Core/` so one Unity assembly
can explicitly disable engine references. The sibling script folders are reserved
for adapters, not additional copies of those rules. All asset and folder `.meta`
files have stable identifiers.

## Core use

```csharp
var content = new LastOutpost.Core.Configuration.PrototypeContent();
var platform = content.CreatePlatform();
var level = new LastOutpost.Core.Levels.LevelProgression(content.LevelOne, platform.Health);
var gun = new LastOutpost.Core.Combat.Weapon(content.BasicGun);
var missiles = new LastOutpost.Core.Combat.Weapon(content.MissileLauncher, platform.AdvancedAmmunition);
var purchases = new LastOutpost.Core.Economy.TacticalPurchases(platform, content.Purchases, content.SpecialAttack);
level.TryBeginNextEncounter();
```

An eventual Unity adapter supplies target distance snapshots and elapsed level
time, calls weapon rules, collects defeated enemies' drops, then calls
`level.Refresh()` **after all combat for the tick**. Start the next encounter only
when travel logic requests it. Render impacts and resource feedback after the
corresponding rule succeeds. Freeze combat after a terminal level result.

## Gameplay defaults

| Purchase | Energy | Effect |
| --- | ---: | --- |
| Ammunition | 30 | Add up to 20 missiles, capacity 60 |
| Emergency repair | 50 | Restore up to 25 health, maximum 100 |
| Special strike | 80 | 120 damage to the closest live target within 10 units |

All values are constructor configuration; initial defaults are in
`PrototypeContent`. Full health, full ammunition, or no special target rejects a
purchase without charging. Partial refills/repairs cost the listed full price.
Destroyed platforms cannot purchase or revive. Special cooldown is supported
and defaults to zero. Tactical Energy is reset with a new platform/run; future
permanent Credits, a shop, and persistence are not implemented.

## Development status

The foundation is published to the default `main` branch of
[AshVahedi/Lstotpst](https://github.com/AshVahedi/Lstotpst). GitHub authorization
resolved the initial publication blocker. The original staging tree was integrated
without overwriting existing project files or initializing a nested repository.
Use the GitHub repository for continued development. See
[repository handoff](Documentation/REPOSITORY_HANDOFF.md) for setup details and
[verification](Documentation/VERIFICATION.md) for actual local and CI results.

Recommended next task: in a Unity-capable environment, implement one playable
scene with independent route movement, spawn adapters, automatic combat,
placeholder side-view graphics, minimal HUD/screens, and PlayMode tests.
