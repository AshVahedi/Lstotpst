# Task 002 integration contract

Status: source implementation prepared; **not compiled or executed in Unity**.
Version remains 0.1.0 pending validation for 0.2.0. Work is isolated to
`development/task-002-unity-integration`; the original project and all Task 001
rules/tests are preserved. No Bubble Shooter files or other repositories changed.

## Composition and authority

- `Core/Simulation/GameRun` coordinates the existing `Weapon`, `Health`,
  `TacticalPurchases`, `EnemyState`, `ModuleRack`, and `LevelProgression` models.
  It adds run lifecycle, route/spawn positions, events and command gating; it does
  not reimplement damage, resource validation, weapon timing or wave rules.
- `RouteMotion` is independent of combat and integrates world-units/second along
  positive X. Modes: Idle (zero), Travel (configured), Encounter (configured),
  Boss (zero), Stopped (zero). Delays before/between encounters are travel seconds.
- `EnemyAgent` approaches the platform without passing attack range. A tiny named
  contact inset avoids rounding placing a stopped enemy just outside that range.
- `RunCombat` builds distance snapshots from the authoritative roster/positions,
  uses the existing targeting/weapon rules, then runs living enemies' attacks.
  Basic fire is always available, even when the rack lacks its placeholder gun.
  Missiles require an installed missile module and sufficient ammunition.
- `PrototypeSettings` serializes the authoring values and constructs validated
  immutable configuration. New settings assets seed enemy/weapon tuning from
  Task 001's `PrototypeContent`. Existing settings are preserved by generation.
- `OutpostSession` owns one run; `FixedUpdate` supplies scaled `fixedDeltaTime`.
  `PlatformMotor` and `EnemyActor` synchronize transforms from that model.
  `EnemySpawner` maps stable IDs to Unity instances and removes defeated views.
- Presentation and UI depend on gameplay/core; gameplay does not depend on them.
  HUD methods call session commands, which call the existing purchase service.
  OnGUI only draws/dispatches input; it never directly spends, heals, or damages.

Runtime assemblies: Core (no engine references), Gameplay (Core), Presentation
(Core + Gameplay), UI (Core + Gameplay). The generator is Editor-only. Shared
EditMode, PlayMode, and Editor generator tests have separate test assemblies.

## Tick and command ordering

1. Ignore StartScreen, terminal runs and zero delta. Reject invalid delta.
2. Resolve any externally pending deaths and refresh core outcome before moving.
3. Consume the configured travel delay, then create the entire next core roster.
   Spawn views in exactly that roster order. No randomness or delayed hidden spawns.
4. Set encounter/boss movement mode, advance the route, approach with living enemies.
5. Fire the basic gun, then an eligible missile launcher, using closest-live-target
   selection. Surviving enemies attack the platform. Dead actors never attack.
6. Award defeated enemies through `EnemyState.TryCollectDrop`; notify removal once.
7. Refresh the existing level rules after combat. Ordinary completion starts travel
   delay, boss death with a living platform yields victory, platform death yields
   defeat. Simultaneous external boss/platform death prioritizes defeat.
8. Latch the result, stop the route, emit one terminal event and freeze simulation.

Special commands use the same current distance snapshots and purchase rules,
then resolve deaths/outcome immediately. StartScreen/terminal commands return
`RunNotActive`. Full inventories/health, lack of targets, insufficient funds,
optional cooldown and death validation stay in the existing core service.

Events are synchronous notifications after the relevant operation. Observers
must not reenter run commands while a tick/event dispatch is in progress. Current
Unity observers only synchronize/spawn/destroy views or show feedback.

The model is authoritative on one horizontal route, not a physics simulation.
Transform edits/collider callbacks cannot move or damage core actors. FixedUpdate
makes movement/combat independent of rendered frame rate; one existing weapon
call still produces at most one shot. No catch-up firing burst is added.
Target lists/arrays are reused, but immutable Target snapshots allocate per living
actor per fixed tick (maximum six ordinary enemies by default). Profile before
changing Task 001's snapshot API; no unmeasured performance guarantee is made.

## Immediate-hit tracer mechanism

`Weapon.TryFire` / special purchase applies damage **once, immediately**.
`ShotEvent` carries only kind and positions for already-resolved shots.
`CombatFeedback` draws a `ShotTrace` using a short-lived LineRenderer (default
0.12 unscaled seconds). There is no projectile collider, impact callback or
second damage command. Destruction of a trace/enemy view grants no resources.
This deliberately follows Task 001's immediate-damage contract.

All ordinary waves are 3 Basic, 4 Fast, then 3 Basic + 3 Fast by default, followed
by one boss. Counts are configurable, but the Unity settings always build exactly
three ordinary encounters and one final boss. The whole roster exists before
completion checks; destroying a view cannot clear a living roster member.

## Lifecycle and UI

The start screen has no active simulation. START RUN begins travel. Application
pause freezes simulation and rejects UI commands; restart creates fresh model,
health, resources, ammo, IDs and weapon timers, then rebinds observers. Spawner and
feedback unsubscribe old models and clear old views/effects. Disabling/re-enabling
spawning can reconstruct still-living views without awarding another reward.

The HUD uses built-in IMGUI to avoid extra UI/input packages. It scales a
540-unit-wide layout inside Screen.safeArea, supports portrait purchase buttons,
and displays health, Energy, missiles, wave and level progress. View state follows
RunStatus for start, play, victory and defeat. Button methods are also the entry
points exercised by authored PlayMode tests. No automated touch interaction has
been performed. The camera follows X after platform synchronization; backdrop
silhouettes show horizontal advancement. The platform has separate visible basic
and missile placeholder parts. Dynamic visual module swapping is future work.

## Reproducible generation and ownership

Use **Last Outpost → Generate Level 1 Prototype** in Unity 6000.3.0f1.
The menu asks to save modified scenes before generation/opening. Output:

```text
Assets/LastOutpostGenerated/
  Art/Placeholder.png, Placeholder.mat
  Configuration/PrototypeSettings.asset
  Prefabs/Platform.prefab, Basic.prefab, Fast.prefab, Boss.prefab
  Scenes/IndustrialWasteland.unity
```

Generated content uses only an original white square sprite, colored rectangular
or diamond shapes, health bars and mounted weapon blocks. Built-in Sprites/Default
must exist; no external art is downloaded. The material is serialized into the
generated assets so its shader is referenced by a future player build.

The generator reuses paths (and importer/prefab GUIDs), preserves existing tuning,
replaces only scene roots with GeneratedLevelRoot, preserves unrelated roots and
build entries, and registers the gameplay scene once. Prefabs and the marked root
are owned outputs and may be replaced: make manual customization elsewhere.
Configuration is validated before scene replacement. This design is covered by
two authored Editor tests, **not demonstrated by an actual Unity run here**.

The default output folder is dedicated to this prototype. Tests use a separate
`Assets/LastOutpostGeneratorTestOutput` folder and refuse a preexisting folder.
They restore build settings and remove only their own output. Generated assets
are not ignored: once real Editor validation succeeds, inspect and commit them
with all metadata and package/settings migrations.

Batch generation example (requires an installed, licensed Editor):

```sh
mkdir -p TestResults
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$PWD" -executeMethod LastOutpost.Editor.PrototypeSceneBuilder.GenerateBatch -quit -logFile TestResults/unity-generate.log
```

No serialized scene or prefab has been fabricated or validated in this environment.
There is intentionally no Android build job until generation/import and Unity
checks actually pass. Android intent remains portrait, ARM64, IL2CPP, API 26+.

## Verification and next gate

82 independent NUnit cases pass; the 58 original cases are unchanged. All 38 asset
sources pass C# syntax parsing only. Metadata and assembly/UI boundaries pass.
17 PlayMode tests and 2 Editor generator tests are authored but unexecuted; shared
EditMode tests have not been run inside Unity either. No Unity API binding,
Editor/player compilation, scene execution, APK or physical-device test occurred.
See [verification](VERIFICATION.md) for evidence and [the checklist](UNITY_CHECKLIST.md)
for the next task. Promote to 0.2.0 after those gates; add no new features first.
