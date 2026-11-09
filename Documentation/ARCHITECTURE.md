# Architecture and decisions

## Scope and toolchain

Task 001 established the engine-independent foundation. Task 002 adds a run
coordinator, Unity adapters, presentation/UI and reproducible Editor generation;
see [its integration contract](TASK002_INTEGRATION.md). Unity compilation and scene
execution remain unverified, so version stays 0.1.0 pending the 0.2.0 gate.
Task 005 adds independently tested permanent progression and a Unity-neutral run
boundary; see [PERMANENT_PROGRESSION.md](PERMANENT_PROGRESSION.md). The verified
base/review sequence is in [REPOSITORY_CONSOLIDATION.md](REPOSITORY_CONSOLIDATION.md).
The official .NET 8.0.427 SDK under `/tmp/last-outpost-dotnet` validates core rules
targeting .NET Standard 2.1; .NET 8 is the test host, not the Android runtime.

Intended editor is Unity **6000.3.0f1 (6.3 LTS)**, confirmed through the official
Unity release API. Its reported release date is 2025-12-03 and stream is `LTS`.
Unity Test Framework **1.4.5** was confirmed through Unity's package registry.
Registry metadata declares minimum Unity 2019.4 and dependencies on
`com.unity.ext.nunit` 2.0.3, `com.unity.modules.imgui` 1.0.0, and
`com.unity.modules.jsonserialize` 1.0.0. Package resolution itself has **not** run
in Unity. Do not infer editor compatibility validation from registry metadata.

The minimal hand-authored ProjectSettings files express intent. Unity may migrate
or fill in their serialization on first import. Portrait, 2D defaults, Android
API/architecture/backend settings must be confirmed through the actual editor.
No Android SDK, NDK, JDK, signing configuration, or scene build entry is bundled.

## Layers

1. **Core rules:** `Assets/Scripts/Core/`, assembly `LastOutpost.Core` with
   `noEngineReferences: true`. No Unity types, frame timing, rendering, UI, file
   I/O in business rules, or external game libraries. The plain .NET
   `FileProgressionStore` is the isolated I/O adapter behind `IProgressionStore`.
   Immutable configuration is separate from mutable per-run/profile state.
   The .NET build consumes these same source files.
2. **Unity gameplay:** `Levels` owns OutpostSession/PrototypeSettings,
   `Platform` owns PlatformMotor, and `Enemies` owns actor/spawner adapters.
   The Gameplay assembly references only Core. FixedUpdate advances GameRun,
   whose independent RouteMotion chooses speed from travel/encounter/boss modes.
3. **Presentation:** Presentation and UI assemblies reference Gameplay/Core.
   Tracers, health bars, camera follow and IMGUI views observe authoritative results.
   Animation/collision callbacks never cause damage, rewards or purchase effects.
   The Editor-only generator creates original placeholder art, prefabs and a scene.

Core subfolders cover Combat, Economy, Enemies, Modules, Platform, Levels, and
Configuration, Simulation for Task 002 composition, and Progression for Task 005.
This keeps every engine-independent rule and its .NET storage adapters
rule inside one engine-free assembly without per-subsystem assembly plumbing.
Core requires no singleton. The Unity host uses a lifetime-only registry to reject
duplicate profile owners; session simulation uses an explicit driver lease.

## Health, ammunition, and economy contracts

- All counts and damage are integer units. Optional armor applies decimal
  `ceil(incoming × (1 - reduction))` before damage clamps at zero, healing at
  maximum, and healing cannot revive. Invalid negative values throw explicitly.
- An ammunition capacity of `null` has no configured upper bound (still limited
  by `int.MaxValue`). Zero capacity is valid. Refills clamp; consumption either
  succeeds entirely or changes nothing.
- `ResourceWallet` addresses balances by `ResourceType`; only `Energy` exists.
  Adding a future tactical type extends that enum. Permanent Credits stay in the
  separate Progression profile. Integer overflow rejects collection
  before mutation rather than wrapping balances negative.
- `TacticalPurchases` validates effects and eligibility, then spends and applies
  the effect synchronously with no callbacks between the two. It is atomic in
  the intended **single gameplay thread** model; it is not a concurrent database.
  UI/background tasks must dispatch to that thread. No events execute halfway
  through commits. Callers emit presentation events after returned success.
- Full inventory/full health/no special target returns `NoEffect`, with no cost.
  Partial refills and repairs charge the full listed price. Insufficient funds,
  cooldowns, and destroyed platform status leave all gameplay state unchanged.
- Special strike is deterministic **single-target** damage, closest in range,
  not an area explosion. Range, cost, damage, and cooldown are configurable.
  Cooldown defaults to zero. Charges/inventory limits can be added later.
- Enemy reward ownership belongs to each `EnemyState`, not a UI callback or a
  global string-ID cache. `TryCollectDrop` rejects live/previously collected
  enemies, even if called with another wallet. Overflow does not mark a reward
  consumed. New enemies must use new state instances; pooled visuals must not
  reuse a dead enemy's state for a fresh spawn.

## Combat and modules

`WeaponConfig` supplies damage, attack interval (seconds), range (world units),
ammo cost per shot, and `ITargetingStrategy`. `ClosestTargetStrategy` ignores dead
or out-of-range targets and resolves equal distances by lowest stable ID.
Target IDs must be unique within a target list. `LevelProgression` supplies unique
enemy IDs within a run. Different targeting policies can implement the interface.

`Weapon.TryFire` receives distance snapshots and elapsed **level** time in seconds.
Use one consistent clock, stop it while paused, and recreate weapons when a run
restarts. The first shot is available immediately. No ammo or damage changes occur
on a failed shot. One call fires at most once; missed frames do not produce
catch-up bursts. Damage is applied immediately on success. A future projectile
can visualize that result; it must not apply a second damage event on impact.

The basic weapon's zero ammo cost makes it independent of missile inventory.
Keep that fallback active in the eventual platform adapter. `ModuleRack` models
installation only: fixed slot capacity, unique module instance IDs, explicit
attach/detach results. The only installed prototype modules are basic gun and
missile launcher. Future module kind names reserve vocabulary, not functionality.
RunCombat gates missile fire on an installed missile module and keeps the basic
fallback active even if modules are removed. Mounted placeholder visuals are
static; dynamic visual attachment/removal remains future work.

Enemy definitions contain health, movement speed, attack configuration, Energy
drop, and boss identity. Enemy state owns a weapon timer and health. Unity adapters
must only run attacks/movement for living actors and must stop all combat after
the level's terminal outcome. Range snapshots must come from authoritative
positions, not presentation offsets.

## Deterministic level progression

Only Level 1 exists:

| Encounter | Spawns (in deterministic order) |
| --- | --- |
| Wave 1 | 3 Basic |
| Wave 2 | 4 Fast |
| Wave 3 | 3 Basic, then 3 Fast |
| Final | 1 Wasteland Siege Engine boss |

`TryBeginNextEncounter` creates a complete core roster. GameRun immediately
creates all agents and EnemySpawner mirrors them; no staggered hidden spawning is
used. It refuses to advance while any roster member lives, independent of visuals.
The caller controls time/distance between encounters. Boss definitions can only
occupy the final encounter; the prototype enforces one boss there.

`Refresh()` evaluates remaining enemies, progress, and outcome. Call it after
**all combat in a tick**, before presenting state or making the next spawn
decision. Progress is encounter-weighted: each of the four encounters contributes
one quarter, with deaths supplying fractional progress within an encounter.
Zero before start, one when the boss is killed. Full enemy progress can coexist
with defeat on a simultaneous death; progress is not the victory condition.

State transitions:

```text
Ready -> EncounterActive -> BetweenEncounters -> EncounterActive ... -> Victory
   \             \                 \                  \
    +-------------+-----------------+------------------+------------> Defeat
```

Platform death takes priority over boss death within the same evaluation tick.
Victory requires the final boss dead and platform alive. Terminal results are
latched; the caller must freeze gameplay at that point. Starting another level
creates fresh state, rather than resetting timers or mutating an old roster.

## Permanent progression

`PermanentStatsProvider` implements the existing `IPlatformStatsProvider` seam and
creates fixed health/ammo/armor plus basic/advanced weapon configurations from an
immutable progression snapshot. Base configuration is not mutated; existing runs
do not observe later profile purchases. PrototypeContent/legacy Unity defaults
remain available independently. Progression's configured ammo base is 100.

`ProgressionService` owns validated Credits/levels and durable run registrations.
Only registered, actually victorious GameRun state can commit a reward. A whole
profile copy contains balance, levels, outcome ledger, completion/unlock metadata
and revision; compare-and-swap persistence precedes publication. Storage conflicts
or uncertain outcomes require explicit reconciliation rather than blind retries.
The service never touches tactical ResourceWallet. Exact rational prices use only
framework decimal/BigInteger types, with one final ceiling to integer Credits.

`IProgressionStore` separates these rules from memory/file storage;
`IProgressionCodec` isolates versioned JSON. File writes flush a sibling pending
file then atomically replace, with no delete-and-copy fallback. Loaded malformed,
inconsistent or unsupported schemas are reported/preserved. The codec is the
future migration dispatch point; no legacy migrations were invented. Linker
preservation prepares reflection DTOs for IL2CPP, which remains unverified.

Dependency direction is Progression → Simulation/Platform/Combat and storage
interfaces, never Core → Unity. `ProgressionRunSession` connects the terminal
event to reward processing without making presentation authoritative. Task 006 now binds the same registered
run through `ProgressionController`, a plain application coordinator, and
`ProgressionHost`, a persistent Unity path/lifetime adapter. The existing HUD
uses immutable menu views and disposable subscriptions. Recovery extends the
same store with a confirmation token and raw backups before atomic replacement;
there is no second economy, serializer or runtime dependency. See
[Unity progression integration](UNITY_PROGRESSION_INTEGRATION.md).

## Testing and dependencies

`Tests/LastOutpost.Core.csproj` compiles the pure assets to .NET Standard 2.1.
`Tests/LastOutpost.Core.Tests.csproj` runs the shared tests with .NET 8, NUnit
3.14.0, NUnit3TestAdapter 4.6.0, and Microsoft.NET.Test.Sdk 17.11.1. Tests use NUnit
APIs available to Unity's bundled NUnit. The two projects have distinct intermediate
output directories to prevent restore collisions. Build warnings fail CI.

CI runs actual tests and the static metadata/core-boundary validator. TRX upload
runs even after failure and errors if no result file exists; it does not hide a
test failure. A dedicated Unity workflow exists, with the known activation gate
still blocked; it was not manually retried during Task 005. Package locks and Unity migration output are deferred
until an actual editor import rather than fabricating a resolved dependency graph.

## Task 002 validation boundaries

The 58 original shared tests remain unchanged; 24 simulation cases bring the
independent suite to 82 passing tests. Seventeen PlayMode and two Editor generator
cases are authored but require Unity. Eight assembly definitions separate Core,
Gameplay, Presentation, UI, Editor and the three test assemblies. CI additionally
checks those boundaries and parses all 38 asset source files for syntax using
the SDK's Roslyn binaries. This does not bind Unity APIs or prove compilation.

The Editor generator owns Assets/LastOutpostGenerated and roots explicitly marked
GeneratedLevelRoot. It reuses paths/GUIDs, preserves saved settings and unrelated
roots, and registers one build scene. No serialized scenes/prefabs were generated
or validated here. Follow [the checklist](UNITY_CHECKLIST.md) in a licensed Editor.

## Task 005 validation boundaries

The original 82 C# cases remain unchanged. Two new shared fixtures add 118 cases
for progression (74) and persistence (44), producing 200 NUnit passes. With the
existing 39 Python validator/helper and seven Graphify cases, 246 independent
cases pass. Static validation checks 81 assets/folders, 22 Core sources, eight
shared fixtures and eight assemblies; syntax parsing covers 44 asset C# files.
These are .NET/static results, not Unity API compilation. Expected Unity tests
are now 204 EditMode (200 shared + four Editor) and 17 PlayMode, all unexecuted.

## Task 006 validation boundaries

All 246 baseline independent cases were rerun. The 28 new session/UI/recovery
cases bring the C# suite to 228; with 39 helper and seven Graphify cases, **274
pass with no failures/skips**. Static checks cover 87 assets/folders, 24 Core
sources, ten shared fixtures and eight acyclic assemblies. All 50 asset C# files
parse without syntax errors. No Unity API binding or compilation has run.

Unity's expected inventory is 232 EditMode (228 shared + four generator) and
23 PlayMode; six progression PlayMode cases are new and unexecuted. Generator
host references are checked in authored Editor tests, not by actual generation.
Graphify remains AST-only evidence; assembly/lifecycle correctness is checked
separately. No core-to-Unity dependency is introduced.

## Task 007 equipment boundary

`EquipmentConfiguration` owns ordered slot definitions and the immutable catalog.
Definition, owned IDs and installed slot/item IDs are separate. `LoadoutRules`
validates and copies candidate edits; `ProgressionService` persists through the
existing whole-profile CAS path, never an equipment-specific save file. Unknown
IDs remain in snapshots/JSON and invalid installations block Start.

`LoadoutRunBuilder` composes permanent snapshots and typed equipment effects,
rounding weapon damage once at the end and applying the existing armor ceiling.
`RunConfiguration` carries immutable `RunLoadout` definitions; its installed rack
is frozen. Simulation depends on that module snapshot, not the profile service.
Legacy raw templates retain existing module-rack tests. The HUD uses compatible
options and enum command results from thin menu models. No new assembly or Unity
runtime package was added. See [MODULAR_EQUIPMENT.md](MODULAR_EQUIPMENT.md).

Schema-v2 adds ownership/installation data. An exact required-field v1 DTO migrates
progression and the ledger in memory; only a successful later transaction writes
v2 against the unchanged raw v1 CAS token. Keep the historical save filename.
New serializer DTO types are preserved in the existing linker XML.

Task 007 verification: 331 independent passes (285 C#, 39 helpers, seven Graphify),
95 metadata assets/folders, 30 Core sources, 12 shared fixtures and eight acyclic
assemblies. All 58 asset C# files pass syntax parsing only. Expected Unity tests
are 289 EditMode and 26 PlayMode, all still unexecuted inside Unity.
