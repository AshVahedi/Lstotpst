# Modular equipment — Task 007

Game version remains **0.1.0**, Unity target **6000.3.0f1**. Core/loadout/persistence
behavior is independently tested. Unity compilation, scene generation, UI rendering,
PlayMode execution and Android storage remain unverified. No licensing retry occurred.

## Prerequisite and branch

Task 006 was verified at `8e7cbd1197c6fde998e4fa5fdcbb91038c4d0838`:
[PR #7](https://github.com/AshVahedi/Lstotpst/pull/7) was open/unmerged, both independent
CI workflows passed, and all **274 baseline independent tests** were rerun here.
Its source-triggered Unity preflight had failed; Editor/Android jobs were skipped.
Task 007 branches from that exact complete head on
`development/task-007-modular-equipment`, targeting the Task 006 branch. No earlier
PR was merged/closed, no patch reapplied and no default-branch/history rewrite occurred.

## Configuration, slots and catalog

`EquipmentConfiguration` composes an immutable `EquipmentCatalog` and ordered slot
definitions. Neither gameplay nor menu code assumes four slots. Each `EquipmentSlot`
has a stable ID, typed category, allowed equipment categories, enabled/required
flags, optional installation-size capacity and default item ID. An optional
platform-wide total capacity uses the same units. Add slots by adding definitions;
the tests exercise one-slot and five-slot platforms.

| Initial slot ID | Slot category | Accepted category | Required | Default |
| --- | --- | --- | --- | --- |
| `primary` | PrimaryWeapon | BasicWeapon | Yes | `basic-cannon` |
| `secondary` | SecondaryWeapon | AdvancedWeapon | No | `missile-launcher` |
| `defense` | Defense | DefensiveModule | No | `armor-plating` |
| `utility` | Utility | SupportModule | No | `reactor-booster` |

Initial slots have no capacity budget; items consume one configurable capacity
unit each. Required-disabled slots and invalid/unplayable defaults are rejected.
Equipment and slots both participate in compatibility via `EquipmentSlot.Accepts`.

Each equipment definition owns an ID, display name, category, compatible slot
categories, description, typed existing mechanism, modifiers, installation size,
maximum simultaneous installations and default availability. An optional immutable
weapon configuration can override template tuning while retaining the existing
firing mechanism. With no override, current Inspector weapon tuning is reused.
Definitions copy their input lists and reject nulls, duplicate IDs, invalid enum
values and incompatible weapon/ammunition semantics. Names never select behavior.

| Initial item | Existing mechanism | Initial effect |
| --- | --- | --- |
| Basic Cannon | BasicGun | Existing basic configuration, unlimited ammunition |
| Missile Launcher | MissileLauncher | Existing advanced configuration, limited ammunition |
| Armor Plating | Armor | **+0.02 damage reduction** before the existing final armor cap |
| Reactor Booster | Reactor | **+1 tactical Energy per defeated enemy** |

The two new equipment effects are provisional tuning in
`EquipmentConfiguration.Prototype()`. They do not change Credits, tactical purchase
prices or permanent upgrade values. The illustrative +20 HP module in numerical
tests is **test-only**, not the initial Armor Plating configuration. Default item
installation limits are one. Support/defense definitions can explicitly allow
duplicates; each installed rack instance uses its unique slot ID.

The current combat engine supports one basic and one advanced firing channel.
Validation rejects extra weapon installations with `WeaponLimit`; more support
slots require no loadout rewrite, while additional weapon channels would require
a separate combat feature. No new weapon type, attack or projectile behavior is added.

## Definition, ownership and installation

The catalog defines items. `EquipmentProfileData.OwnedEquipmentIds` records
ownership. Its `Installations` record slot/item IDs separately. All four initial
types are owned on a new profile and after v1 migration. Installing/removing an
item changes neither ownership nor Credits. Existing v2 profiles do not gain items
merely because a later catalog marks them available by default. No acquisition,
equipment pricing, rarity, crafting or random drops are implemented.

`LoadoutSnapshot` is an immutable deep copy for queries and validation; mutable
serializer DTOs are not exposed through `ProgressionSnapshot`.

## Operations and validation

`ProgressionService.Equip`, `Replace`, `Unequip`, `RestoreDefaultLoadout`, and
`ValidateLoadout` use `LoadoutRules`. `ChangeLoadout` exposes the same operations
through the application controller. Results are `LoadoutResult` enum values,
including UnknownSlot/Equipment, NotOwned, Incompatible, DisabledSlot,
DuplicateInstallation, CapacityExceeded, RequiredEquipment, WeaponLimit and
storage/recovery failures. Ordinary invalid actions do not throw.

Equip requires an empty slot; Replace requires an occupied slot. Reinstalling the
same item reports AlreadyInstalled. An enabled required slot must remain filled,
and **at least one unlimited basic weapon** remains mandatory even if the layout
does not mark its primary slot required. The optional missile can be removed;
basic firing continues and ammunition purchases return NoEffect without charging
when the advanced channel is absent. Special strikes retain existing behavior.

Rules validate a candidate copy before saving. Failed validation returns no
candidate and changes no state/revision. Successful changes use the existing
whole-profile CAS/save-before-publication transaction. Failed/uncertain saves or
stale writers block unsafe edits until explicit reload; a failed edit is never
automatically replayed. A live owned `ProgressionRunSession` also blocks service
equipment changes; the controller permits edits only between runs. Completion,
abandonment or disposal releases its lease. Disposing is not combat resume or
permission to keep driving a detached simulation.

Restore Defaults replaces all assignments explicitly but retains ownership,
including unknown owned IDs. It fails if required default items are not owned;
it does not invent an acquisition or grant missing equipment. The UI asks for
confirmation before replacing assignments.

## Modifier calculation and immutable runs

`LoadoutRunBuilder` is the composition boundary:

1. Use the original combat template and authoritative progression base stats.
2. Snapshot permanent upgrade effects through `PermanentStatsProvider`.
3. Resolve selected definitions in configured slot order and apply equipment.
4. Check overflow, clamp armor to `ProgressionConfiguration.MaximumDamageReduction`
   (initially 0.30), and round weapon damage once, nearest integer with half ties
   away from zero. HP/ammo/Energy increments must be whole nonnegative units.
5. Build a new `RunConfiguration` and immutable resolved `RunLoadout` before
   persisting the unique run registration and exposing the run.

Typed `EquipmentStat` values cover MaximumHealth, DamageReduction,
WeaponDamageMultiplier, AmmunitionCapacity and EnergyPerDefeat. HP, rounds, armor
fractions and Energy bonuses **add**. Weapon modifier factors **multiply** the
permanent multiplier, then raw weapon damage is rounded only at the end. A factor
must be positive; negative effects and invalid definitions are rejected. A final
nonpositive weapon damage or unrepresentable value rejects run initialization.

Examples: 100 HP + health level 3 (+30) + a test-configured +20 HP module = **150 HP**.
Basic damage 8 × permanent 1.15 × module 1.25 = 11.5, rounded to **12**, not a
twice-rounded 11. Base armor 0 + level-2 armor 0.06 + initial plating 0.02 = **0.08**.
At the permanent 0.30 cap, plating still yields 0.30. No negative damage is possible;
incoming damage retains existing upward rounding after armor.

Ammo capacity is validated and initial ammo clamps to it. Final rack capacity comes
from the configured slot count, superseding the legacy two-slot template count.
The registered run's rack is frozen against direct attach/detach, and its snapshot
retains immutable definitions rather than profile DTOs. Subsequent loadout or
permanent transactions cannot mutate it. Building from an already equipped run
configuration is rejected to prevent double application. Raw legacy core templates
retain their existing mutable two-module test seam; production uses the registered
equipment path.

Reactor rewards use `EnemyState.TryCollectDrop(wallet, bonus)` and its existing
exactly-once flag. Bonus addition is checked before crediting; the builder checks
each enemy reward for overflow before registration. Credits and special-attack
logic are unaffected. No UI/destruction callback grants a second drop.

## Schema-v2 persistence and migration

`ProgressionData.CurrentSchema` is **2**. The exact required v1 fields are read
through `LegacyProgressionData`; migration copies Credits, revision, upgrades,
unlock/completion information and the full reward ledger unchanged, then adds
default ownership/loadout. Invalid legacy values fail validation and preserve the
source. Missing required v2 equipment data is corruption, not a v1 fallback.
Unknown/future schemas are rejected before profile decoding.

V2 adds this required field to the existing profile:

```json
"equipment": {
  "ownedEquipmentIds": ["basic-cannon", "missile-launcher", "armor-plating", "reactor-booster"],
  "installations": [
    {"slotId": "primary", "equipmentId": "basic-cannon"},
    {"slotId": "secondary", "equipmentId": "missile-launcher"},
    {"slotId": "defense", "equipmentId": "armor-plating"},
    {"slotId": "utility", "equipmentId": "reactor-booster"}
  ]
}
```

Loading v1 is read-only: the original raw v1 payload remains the CAS comparison
until the first successful transaction atomically writes a complete v2 document.
There is no separate migration write that could partially update Credits/ledger.
Injected write/promotion failures preserve the old primary; a sibling pending v2
is not automatically promoted during load. Conflicts require reload. Durable old
reward IDs remain paid after migration and cannot earn again.

**The filename remains `progression-v1.json` under Application.persistentDataPath**
to find existing Task 006 saves. Its header, not its historical filename, selects
the schema. Do not rename/delete it or create an independent save file. Existing
atomic replacement, cooperative locks, raw backups and reset-token recovery remain
in use. Confirmed new profiles now encode v2. Older Task 006 code will reject v2
as unsupported; there is no downgrade migration.

Structural validation rejects null/duplicate ownership IDs and slot assignments,
empty IDs and malformed DTOs. Unknown/removed item and slot IDs are **preserved**.
They are shown by the menu and invalid installations prevent Start. Other valid
progression transactions retain those IDs. Explicit replacement or confirmed
Restore Defaults can replace assignments; unknown owned IDs remain recorded.
Do not silently filter them out on load/save. Missing ownership may require a
future acquisition/repair policy; Restore Defaults does not grant it.

## Unity integration and remaining verification

The existing persistent `ProgressionHost`, `OutpostSession` and safe-area IMGUI HUD
are retained. Start adds **LOADOUT** beside UPGRADES. The scrollable panel lists
configured slots, installed item, compatible item descriptions/availability,
install/replace/unequip, status errors and confirmed Restore Defaults. All controls
read immutable menu models and invoke controller commands; the UI never edits
serializer DTOs. Active/paused runs and recovery block edits. No scene-object-name
lookup, second progression manager, new runtime package or Canvas shop is added.

In Unity 6000.3.0f1, import the existing project and run **Last Outpost → Generate
Level 1 Prototype**, then open
`Assets/LastOutpostGenerated/Scenes/IndustrialWasteland.unity`. The existing generator
already supplies the host/session/HUD references; no new serialized reference is
required. Generate twice and check idempotency. No scene/prefab generation occurred
here. Placeholder mounted weapon graphics do not yet change with loadout selection;
combat channels and menu state do. No new visual equipment system is claimed.

## Verification and limitations

The original 274 independent baseline cases were rerun. Final execution is **331
passed, 0 failed, 0 skipped**: 285 NUnit cases (57 new equipment cases), 39 Python
validator/helpers and seven Graphify tests. Existing cases were retained; schema
fixtures now use current/future version 2/3, lethal fixtures account for plating,
and two unexecuted Unity Energy expectations include the reactor bonus.

Three additional Unity PlayMode cases are authored for HUD equipment commands,
host v1 migration and unknown-gear repair. Expected Unity inventory is **289
EditMode** (285 shared + four generator) and **26 PlayMode**. All remain unexecuted
in Unity. Static checks validate 95 assets/folders, 30 Core sources, 12 shared
fixtures and eight assemblies. All 58 asset C# sources pass SDK syntax parsing;
this is not Unity API binding or compilation.

Graphify uses the existing pinned 0.9.104 tooling. It covers 75 code inputs and
59 C# files (58 Assets plus the syntax tool); source hashes and separate assembly
validation check boundaries. The composition direction is Progression -> Modules/
Simulation; Modules and Simulation do not depend on the profile service. No import
cycles or mapped Core-to-Unity dependencies were detected. Graph counts are
snapshot-specific; see `graphify-out/GRAPH_REPORT.md` and CI artifacts. Assembly
definitions/linker metadata are among 26 unsupported graph inputs and are checked
separately. No unrelated framework or reverse Unity dependency was introduced.

Android IL2CPP/AOT serializer types were added to the existing `link.xml`, but
actual stripping, file locking, atomic replacement, pause/kill/relaunch and touch
scrolling still require Unity/device validation. The existing no-combat-resume,
unsaved-outcome interruption, growing ledger/backups and client-side tampering
limitations remain. No APK, device compatibility or verified playable build is
claimed. The recommended next task is actual Unity import/generation, both suites,
loadout playthrough and Android save/migration verification when access is available.
