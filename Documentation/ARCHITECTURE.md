# Architecture and decisions

## Scope and toolchain

The initial task prioritizes Milestones 1–2 and permits gameplay integration only
with meaningful validation. This environment had neither Unity nor a C# toolchain.
An official .NET 8.0.427 SDK was installed in `/tmp/last-outpost-dotnet`, outside the
project. Core rules compile to .NET Standard 2.1; .NET 8 runs the tests only.
Unity integration is deferred. There is no scene that can be played yet.

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
   I/O, or external game libraries. Immutable configuration is separate from
   mutable per-run state. The .NET build consumes these same source files.
2. **Unity gameplay (reserved):** sibling `Combat`, `Enemies`, `Platform`,
   `Progression`, `Levels` folders. Future MonoBehaviours will own transforms,
   spawning, routing, lifecycle, and adapters to core objects. Movement must use
   a separate configurable route component, with travel/encounter/boss modes.
3. **Presentation (reserved):** `UI`, `Art`, `Audio`, scenes and prefabs.
   Successful core operations drive presentation. Animation/collision callbacks
   must not decide whether authoritative damage or purchases occur.

Core subfolders cover Combat, Economy, Enemies, Modules, Platform, Levels, and
Configuration. This adjustment keeps every pure rule inside one engine-free
assembly without per-subsystem assembly plumbing. No singleton is required.

## Health, ammunition, and economy contracts

- All counts and damage are integer units. Damage clamps at zero, healing at
  maximum, and healing cannot revive. Invalid negative values throw explicitly.
- An ammunition capacity of `null` has no configured upper bound (still limited
  by `int.MaxValue`). Zero capacity is valid. Refills clamp; consumption either
  succeeds entirely or changes nothing.
- `ResourceWallet` addresses balances by `ResourceType`; only `Energy` exists.
  Adding a future tactical type extends that enum. Permanent Credits must stay
  in a separate future progression system. Integer overflow rejects collection
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
Dynamic attachment-to-Unity-object/weapon binding is deferred; it must preserve
the basic fallback even while advanced modules are removed.

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

`TryBeginNextEncounter` creates a complete core roster. Future Unity spawn timing
must keep every member represented and cannot advance based only on currently
visible enemies. It refuses to advance while any roster member lives.
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

## Permanent progression seam

`IPlatformStatsProvider` supplies validated starting health, ammo capacity, and
module slots through `PlatformStats`. `PrototypeContent` implements the baseline
provider. A future permanent upgrade service can implement that interface.
No persistent currency, save format, upgrade purchase logic, or shop is present.
Weapon damage/armor/special progression should extend this starting-stat model
when their requirements are specified, rather than mixing them into Energy.

## Testing and dependencies

`Tests/LastOutpost.Core.csproj` compiles the pure assets to .NET Standard 2.1.
`Tests/LastOutpost.Core.Tests.csproj` runs the shared tests with .NET 8, NUnit
3.14.0, NUnit3TestAdapter 4.6.0, and Microsoft.NET.Test.Sdk 17.11.1. Tests use NUnit
APIs available to Unity's bundled NUnit. The two projects have distinct intermediate
output directories to prevent restore collisions. Build warnings fail CI.

CI runs actual tests and the static metadata/core-boundary validator. TRX upload
runs even after failure and errors if no result file exists; it does not hide a
test failure. Unity jobs are deliberately absent until an editor/license-backed
runner can execute them. Package locks and Unity migration output are deferred
until an actual editor import rather than fabricating a resolved dependency graph.
