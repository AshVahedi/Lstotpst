# Unity progression integration — Task 006

Version remains **0.1.0**. The integration code is implemented; **Unity compilation,
scene generation, UI rendering, PlayMode execution and Android persistence have not
been verified**. No Unity activation was retried. See [first launch](UNITY_FIRST_LAUNCH.md).

## Progression lifecycle

`ProgressionController` is a plain C# application-lifetime coordinator over the
existing `ProgressionService` and `ProgressionRunSession`. It loads once before
enabling Start/Upgrades. Missing data creates the existing zero-Credit, level-1
profile without writing until the first transaction. Explicit recovery reloads
are the only subsequent reads requested by the controller.

```text
Load -> Start <-> Upgrades -> register run -> Playing
                                           -> Victory/Defeat -> Start
Any unconfirmed progression transaction -> Recovery -> reload/retry
```

Commands are synchronous on the gameplay thread. Busy and notification guards
reject overlapping/reentrant initialization, purchases and run creation; commands
are never queued. UI events do not own profile state. Temporary Energy remains in
the run wallet and is never saved as Credits.

## Unity adapters and ownership

`ProgressionHost` supplies `Application.persistentDataPath` and owns one controller.
Its early Awake detaches its dedicated object and uses `DontDestroyOnLoad`. A
Unity-only registry rejects duplicate hosts before they open storage. No Core
singleton or scene-object-name lookup is required. Keep unrelated components off
the dedicated host object because a duplicate host object is destroyed as a unit.

`OutpostSession` binds that owner, adopts its registered run and forwards existing
run-created/finished events to the existing presentation. It no longer constructs
an independent `GameRun`. A host driver lease releases the previous session's
bindings before a replacement scene drives the same controller. Only the current
driver advances FixedUpdate. Disable/destroy disconnects subscriptions and views;
scene rebinding neither reloads the profile nor completes the run again.

Production scenes use the serialized host reference. Explicit controller/store
injection supports tests without touching the player's files. Unconfigured or
failed initialization blocks gameplay rather than falling back to an unsaved run.

## Run initialization and upgrade snapshot

`StartRun(settings.Build())` delegates to `ProgressionService.CreateRun`, which
persists a unique registration before exposing `ProgressionRunSession.Run`.
`PermanentStatsProvider` applies a fresh saved-level snapshot to a copy of the
template. No base asset is mutated and no modifier is stacked onto an old run.
Health starts at the upgraded maximum, ammo is constrained by the upgraded
capacity, and armor/weapon scaling retain Task 005 rounding rules.

The authoritative catalog is `ProgressionConfiguration.Prototype()`:

| Upgrade | Base | Increment per level | Maximum level | Initial price / growth |
| --- | --- | --- | ---: | --- |
| Health | 100 HP | 10 HP | 10 | 200 / 1.5 |
| Armor | 0% | 3 percentage points, 30% ceiling | 10 | 250 / 1.5 |
| Weapon damage | 1.00x | 0.05x | 10 | 300 / 1.6 |
| Ammo capacity | 100 rounds | 20 rounds | 10 | 150 / 1.4 |

`EffectAt` shares these definitions between stats and menu previews. Prices remain
exact rational `ceil(base × growth^currentLevel)` Credits. Unity Inspector tuning
still supplies combat/wave settings; permanent base HP/armor/ammo/scaling come
from this catalog. In particular the progression path now uses Task 005's 100-round
base, superseding the raw legacy template's 60-round capacity without changing the
catalog or tactical prices. Purchases are disabled during a run; new runs use
the current saved levels. Level 1 is the only playable level even when the existing
completion metadata unlocks ordinal 2.

## Victory reward commit

The existing GameRun boss/platform victory checks and latched Finished event feed
`ProgressionRunSession`. It subscribes before the controller, so reward processing
precedes results notification. `RunReceipt` exposes the durable ledger outcome;
`AlreadyProcessed` alone is not treated as proof of a paid victory. Saved Level 1
victory earns 500 Credits; defeat, abandonment and restart earn zero.

The controller retains the same run and ID if saving is uncertain. Recovery stops
simulation and blocks new runs/purchases. Reload reconciles storage; Retry Saving
uses the same idempotent outcome transaction. The UI shows a saved reward only
with terminal receipt evidence and labels stale Credit snapshots as last known.
Reopening results or rebinding a scene performs no reward transaction. A terminal
run returns to Start only after its outcome is confirmed; abandoning a live run
also persists its zero-reward outcome before detaching it.

Run IDs and schema-v1 ledger deduplication are unchanged. Combat resume remains
unsupported: process termination before an outcome commits can lose that unsaved
reward. Keep the live recovery session open to retry. A committed reward survives
relaunch; no Active ledger entry is automatically promoted to victory.

## Storage and lifecycle

The path is `Path.Combine(Application.persistentDataPath, "progression-v1.json")`.
The thin host validates the directory; Core uses the existing file store/JSON
codec. Whole-profile compare-and-swap transactions retain sibling `.pending`
flush, atomic `File.Replace`/first-save `File.Move`, and cooperative `.lock`.
Schema and serialized DTO fields remain **version 1**; no migration is required.
No PlayerPrefs mirror or second save implementation exists.

Critical transactions save immediately, before publishing profile changes.
Application pause freezes simulation/commands; quit is not a required save point.
Repeated enable/configure operations unsubscribe before binding. Disposable menu
bindings disconnect destroyed/disabled HUDs. The application host survives scene
replacement, but does not support combat restoration after process loss.

## Recovery controls

| Condition | Behavior / action |
| --- | --- |
| Missing save | Valid initial in-memory profile; normal Start allowed |
| Corrupt, invalid, unsupported schema or pending-only file | Preserve files, block Start/purchases, explain error; Retry Loading or explicitly request a new profile |
| Stale writer | Never overwrite newer profile; reload required; failed purchase is not automatically repeated |
| Failed/uncertain write | Freeze progression; reload/reconcile, then retry the same run outcome when safe |
| Valid primary plus stale pending | Existing policy keeps primary authoritative; next explicit transaction may replace pending |

`REQUEST NEW PROFILE...` only prepares confirmation. Confirm backs up raw primary
and pending bytes into a sibling `progression-v1.json.recovery-<GUID>/`, flushes the
backup files, then uses the existing atomic promotion path for a fresh profile.
Cancel changes no storage. Pending reward loss is stated in the confirmation.
The optional `IProgressionRecoveryStore` extension avoids adding recovery to every
store implementation. Memory tests retain backup payloads equivalently.

A token hashes raw primary/pending contents, existence and lengths under the
cooperative lock; changing either after confirmation is requested rejects reset
without replacement. The token detects changes, **not malicious tampering**.
Backup or promotion failure blocks success; original/backup evidence is retained.
A lost acknowledgment is reconciled against the intended replacement. If a later
reload confirms the reset and the old run receipt is absent, that explicitly
discarded run is detached instead of replaying its reward. Backups are not pruned
automatically or restored through a blind overwrite button; inspect them and use
an explicit supported repair before Retry Loading. Storage locks only coordinate
cooperating writers.

## Minimal UI

The existing safe-area IMGUI HUD adds Start, Upgrades, results and Recovery states
within one scene. Start displays Credits/unlocked ordinal and opens Upgrades.
Four scrollable rows display level/cap, current/next effect, price or MAXIMUM.
Insufficient funds, maximum level, loading, transactions, active runs and recovery
disable purchasing. Commands delegate to the controller and refresh immutable
views from its authoritative snapshot. Results offer BETWEEN-RUN MENU. Live combat
retains the three Energy purchases and adds ABANDON TO MENU. No graphical shop
assets, new UI package or extra scene is required.

## Scene generation and Unity tests

Outside Play Mode run **Last Outpost → Generate Level 1 Prototype**, then open
`Assets/LastOutpostGenerated/Scenes/IndustrialWasteland.unity`. The generator adds
one dedicated host child and assigns the session reference, preserving existing
owned-root replacement, stable prefab paths and Undo/save-failure handling. Repeat
generation to verify idempotency. No scenes/prefabs or resolved package lockfile
have been generated here. Existing generated scenes must be regenerated.

The expected Unity inventory is **232 EditMode** (228 shared + four generator)
and **23 PlayMode**. Six new PlayMode cases cover host/load/path setup, HUD binding,
scene replacement/one driver, enable/disable subscriptions, saved victory results
and confirmed corruption recovery. The existing 17 PlayMode cases now inject a
memory progression owner; the four Editor tests also check the host reference.
All of these Unity executions remain pending, including shared cases in Unity.
Use an isolated Test Runner scene; fixtures refuse to modify an existing host.

Independent verification executed **274 tests, 274 passed, 0 failed, 0 skipped**:
228 NUnit in .NET 8, 39 Python validator/helpers, seven Graphify tests. The original
246-case baseline was rerun before changes. New cases exercise load-before-start,
run snapshots, Energy separation, receipts/reload deduplication, failed commits,
view availability, reentrancy, unsubscription, raw-byte backups, stale confirmation
and lost reset acknowledgments. Eight assemblies validate without cycles; all 50
asset C# files pass SDK syntax parsing. This does not bind Unity APIs.

## Android risks and next verification

IL2CPP/AOT reflection serialization retains the existing DTO `link.xml`; the new
controller/views add no serialized DTOs. Unity's framework serializer, SHA256,
locking, flush/replace behavior and writable persistent path still need an actual
ARM64 build/device test. There is no directory fsync/power-loss guarantee. Unsupported
atomic replacement fails closed. Synchronous bounded saves/backups may stall a
frame; measure on a device before considering asynchronous transactions. The
ledger and recovery backups grow and need a future explicit retention/migration
policy. No cloud synchronization or anti-cheat security is claimed.

When Unity is available, import/compile, generate twice, run both Unity suites,
exercise purchases and both outcomes, replace/reload the scene, relaunch to check
Credits, test recovery with **copied test saves**, and test pause/kill/relaunch and
portrait scrolling on Android. Editor domain/scene-reload-disabled options are
also unverified. The next task should validate this integration in Unity and on
Android rather than extend content.

## Graphify dependency review

The existing pinned 0.9.104 installation generated **1,351 nodes and 3,979
relationships**, covering 67 code inputs / 51 C# files (50 Assets plus the syntax
tool). Source hashes validate as fresh. The source graph shows
`PrototypeHud -> ProgressionMenuBinding -> ProgressionController -> ProgressionService`
and identifies ProgressionService/ProgressionController, OutpostSession and
GameRun as connected hubs. Review their event ordering when changing lifecycle
code. No import-edge cycle or mapped Core-to-Unity dependency was detected; the
eight-assembly validator independently passed. Graphify's 26 unsupported inputs
include assembly definitions/linker metadata, which are validated separately.
No new runtime package or reverse Core dependency was introduced. AST graph edges
do not prove scene serialization, lifecycle execution or complete absence of
semantic cycles. Portable graph artifacts remain in `graphify-out/` with native
hooks and existing CI publication preserved.
