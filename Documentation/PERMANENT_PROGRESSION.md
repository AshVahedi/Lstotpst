# Permanent progression — Task 005

Game version remains **0.1.0**. These rules and storage adapters execute in the
independent C# tests. Task 006 now connects the Unity session and minimal upgrade/
recovery controls in source; actual Unity execution remains pending. See
[Unity integration](UNITY_PROGRESSION_INTEGRATION.md). No new level, premium
currency or save server is added.

## Economies and public entry points

**Energy** stays in the existing per-run `ResourceWallet`, paying only for tactical
ammo, repair and special strikes. A new run creates a fresh wallet. **Credits**
are nonnegative signed 64-bit integer units in the permanent profile. They buy
permanent upgrades only. There is no conversion, shared currency enum, real-money
purchase or automatic carry of Energy into the save.

`ProgressionService` composes existing platform, weapon and `GameRun` rules:

| API | Purpose |
| --- | --- |
| `Snapshot` | Immutable Credits, levels, unlock/completion metadata and ledger counts |
| `Configuration.Get(kind)` / `TryGetPrice(kind, out price)` | Upgrade definition and current next price; false for unknown/capped upgrade |
| `BuyUpgrade(kind)` | Validate, persist and then publish an upgrade transaction |
| `CreateRun(template, out session)` | Calculate fixed stats, persist unique run registration, then expose the run |
| `CompleteRun(run)` / `session.RetryCompletion()` | Validate confirmed outcome and idempotently persist the reward |
| `session.Abandon()` | Durably close an active run with zero Credits |
| `Reload()` / `RequiresReload` | Explicitly reconcile stale or uncertain storage state |

`UpgradeKind` is a stable typed identifier: MaximumHealth=1, Armor=2,
WeaponDamage=3, AmmunitionCapacity=4. Invalid enum values are rejected. The existing
Energy wallet cannot make a durable whole-profile transaction, so it is not reused
as a second, independently mutable Credit wallet. Existing health/combat/level
rules and `IPlatformStatsProvider` are reused instead.

## Rewards, run identity and completion

The sole configured level is still `level-1`, Industrial Wasteland:

| Outcome | Initial Credits |
| --- | ---: |
| Confirmed Level 1 victory | 500 |
| Defeat, restart, abandonment | 0 |

`LevelRewardDefinition` configures the level identifier/ordinal and reward. It
supports future definitions without adding playable content. Level 1 victory
records completion and sets `HighestUnlockedLevel` to 2; **no Level 2 is created**
and an unknown level template is still rejected.

Each `GameRun` receives `Guid.NewGuid().ToString("N")` by default: 32 lowercase
hexadecimal characters, with no separators. Empty/noncanonical GUIDs are rejected.
`IRunIdGenerator` makes service registration testable without making combat random.
The service generates once, rejects collision with **any** stored run identifier,
and persists an Active ledger record before returning a usable session. A failed
registration returns no session. Do not manufacture an ID in a victory screen.

The ledger stores ID, level ID/ordinal, snapshotted victory entitlement and outcome.
The stored entitlement is used even if a later balance configuration changes the
reward. `CompleteRun` requires a registered ID/level and actual `GameRun.Victory`,
`LevelProgression.Victory`, and a living platform. Defeat does not satisfy those
conditions; simultaneous boss/platform death remains defeat. UI assertions or
calling a reward method with an arbitrary string do not establish victory.

Victory changes Credits, ledger outcome, completed-level metadata, unlock and
revision in **one** saved document. A repeated callback, reopened results view,
stale service or reloaded profile cannot pay that run again. A genuinely new,
registered and completed run can earn another 500. Ledger IDs/outcomes are retained
durably; an in-memory flag is not the deduplication mechanism. Outcome wire values
are Active=0, Victory=1, Defeat=2, Abandoned=3 and must not be renumbered.

`ProgressionRunSession` subscribes to the actual run's `Finished` event. Inspect
`LastCompletionResult`; after storage recovery, retry completion on the **same
run**, rather than starting combat again. `Dispose` disconnects the event and
does not silently abandon or save. Before restarting/leaving an active run, call
`Abandon`, check the result, then dispose and register a new run. An abandoned run
cannot later pay even if its old simulation reaches victory. A pending victory
cannot be converted to abandonment to hide a failed reward save.

Run registration is durable, but active combat and an **unsaved** victory are not
resumable across process loss. If the app dies before an outcome transaction
commits, its Active record grants no automatic reward. Keep the live session to
retry a failed completion while the app runs. This provides atomic, at-most-once
payment with explicit failures, not guaranteed delivery through every power loss.

## Upgrade effects and costs

`ProgressionConfiguration.Prototype()` defines these provisional values:

| Upgrade | Base | Per level | Cap | Base cost | Growth |
| --- | --- | --- | ---: | ---: | ---: |
| Maximum Health | 100 HP | +10 HP | 10 | 200 | 1.5 |
| Armor | 0% reduction | +3 percentage points | 10 | 250 | 1.5 |
| Weapon Damage | multiplier 1.00 | +0.05 | 10 | 300 | 1.6 |
| Ammunition Capacity | 100 rounds | +20 rounds | 10 | 150 | 1.4 |

Each definition's base cost, decimal growth, effect and maximum level is
configurable. `ProgressionConfiguration` owns the base platform stats, base weapon
multiplier, armor ceiling (initially 0.30) and reward catalog. It validates all four
categories, unique definitions/level ordinals and representable maximum effects.

For current upgrade level `n` starting at zero:

```text
next cost = ceil(C_base × r^n)
HP = baseHP + healthLevel × healthIncrease
ammo capacity = baseCapacity + ammoLevel × ammoIncrease
reduction = min(configured ceiling, baseReduction + armorLevel × armorIncrease)
weapon multiplier = baseMultiplier + damageLevel × multiplierIncrease
```

Costs use `decimal` configuration decomposed into an exact `BigInteger` fraction;
integer powers and one final ceiling determine the price. There is no `double`,
`Math.Pow`, repeated intermediate rounding or floating currency balance. Example
Armor prices at levels 0..3 are **250, 375, 563, 844**. At level 9 the next prices
are Health **7,689**, Armor **9,611**, Damage **20,616**, Ammo **3,100**. At the cap
there is no next purchase. Zero costs and constant growth 1 are supported;
negative costs/effects, growth below 1 and limits outside 1..1000 are rejected.
HP/ammo effect steps must be whole units. Every purchasable price is checked
against `long.MaxValue` at catalog construction; an overflowing catalog is refused
before it can debit a profile. Invalid catalogs throw explicitly; no save is reset.

Incoming integer damage becomes `ceil(damage × (1 - reduction))`, then clamps to
remaining health. Thus 30% armor turns 10 damage into **7**, 4 into **3**, 1 into
**1**, and zero remains zero. Reduction is validated within [0,1] and capped by
configuration. Damage/healing cannot make health negative or revive a dead actor.
Enemy health defaults to zero armor, preserving existing enemy rules.

Basic/advanced weapon damage is multiplied and rounded to the nearest integer,
with exact halves **away from zero**. Damage level 4 gives **1.20**, turning the
existing 8/30 damage into **10/36**. The tactical special strike is unchanged.
At maximum defaults, HP=200, ammo capacity=300, reduction=30%, multiplier=1.50
and basic/missile damage=12/45.

`PermanentStatsProvider` snapshots a profile through `IPlatformStatsProvider` and
constructs a **new** `RunConfiguration`; it never mutates the base template.
Configured progression base stats are authoritative, while the template supplies
level, weapons, route, spawns and tactical rules. Raw legacy template settings
still declare 60 ammo capacity; Task 006 now explicitly adopts the progression
boundary and its authoritative 100-round base in Unity. Existing initial ammo clamps
to the resulting capacity. Purchases made after a run was initialized affect
subsequent runs only; armor and both weapons stay consistent for its whole life.
The service permits profile transactions during a run, but Unity should expose
the eventual shop between runs. Always apply to an original base template, not
an already upgraded configuration. Cross-catalog snapshots above the destination
cap are rejected.

## Atomic transactions and failure handling

Purchases reject unknown IDs, capped levels and insufficient Credits before
writing. A candidate profile is copied, price deducted and one level added, then
validated/serialized with an incremented checked revision. Memory is published
only after `IProgressionStore.Commit(expectedDocument, replacement)` succeeds.
That compare-and-swap operation prevents a stale service overwriting another
purchase/reward. Snapshot readers and commands are serialized by the service lock.

Failure results are explicit:

| Result | State/recovery |
| --- | --- |
| InvalidUpgrade / MaximumLevel / InsufficientCredits | No profile or save mutation |
| Overflow | No debit/level/reward mutation; checked Credit/revision/weapon bounds |
| Conflict | Another writer changed the save; keep prior snapshot, block writes, Reload |
| PersistenceFailed | Candidate not committed or serialization failed; no charge; inspect RequiresReload |
| PersistenceUncertain | Authoritative result cannot be read; block writes until Reload |
| ReloadRequired | No further transaction attempted until successful Reload |
| AlreadyProcessed | Terminal ledger entry already handled; no repeated reward/write |

An I/O error may occur **after** an atomic replacement succeeded (e.g. lost
acknowledgment). The service reads once to reconcile: an exact candidate document
means success and publishes both new balance/level; the exact previous document
means failure. Any unreadable/different state is uncertain. It never refunds or
retries blindly. On uncertainty/conflict, `RequiresReload` blocks further writes.
If Reload fails, it retains the last known snapshot and leaves writes blocked.

## Storage adapters and schema

`IProgressionStore` supports file paths, Unity-supplied paths and test memory;
`MemoryProgressionStore` performs a locked in-memory compare-and-swap. The Core
has no Unity dependency. File I/O is confined to the plain .NET file adapter;
business rules depend on the interface. JSON uses the framework's
`DataContractJsonSerializer`, without adding a package or framework dependency.

Initial schema example (field order is immaterial):

```json
{
  "schemaVersion": 1,
  "revision": 0,
  "credits": 0,
  "upgradeLevels": { "health": 0, "armor": 0, "weaponDamage": 0, "ammunitionCapacity": 0 },
  "highestUnlockedLevel": 1,
  "completedLevelIds": [],
  "runs": []
}
```

A run entry has `runId`, `levelId`, `levelNumber`, `victoryCredits` and numeric
`outcome`. **No Energy, health-in-combat or tactical ammo inventory is serialized.**
Negative/out-of-range balances, revisions, upgrade levels, malformed/duplicate
IDs, invalid outcomes, inconsistent completion/unlock metadata and missing
required members are rejected. Required-member duplicates, fractional/overflow
Credit values, trailing documents, invalid file bytes and excessive nesting are
covered by tests. JSON is bounded to 8 Mi characters, 64 nesting levels and
100,000 serializer graph items; writes use UTF-8. Hitting a bound fails explicitly.

`JsonProgressionCodec` reads the required schema header before decoding v1 fields.
Missing saves create a valid zero-Credit profile in memory; no blank JSON file is
written until a successful transaction. Unsupported schemas (including future
versions) raise `UnsupportedSchema` and preserve the original. There is no invented
v0 migration. Future versions should dispatch by schema here, transform/validate a
copy and persist through the same atomic store contract with explicit migration
tests. Do not deserialize an unknown version as v1 or silently reset it.

`FileProgressionStore` writes a sibling `.pending`, flushes file contents to disk,
then uses `File.Replace` for an existing primary or `File.Move` on first save.
There is no delete-and-copy fallback. A sibling `.lock`, held with `FileShare.None`,
coordinates cooperating instances/processes; it is retained to avoid inode races.

| Files found | Load/recovery behavior |
| --- | --- |
| No primary or pending | Valid initial profile; lock file may be created |
| Valid primary only | Load/validate it |
| Valid primary plus pending | Primary remains authoritative; expose HasPendingWrite; explicit new transaction may overwrite stale pending |
| Pending only | InterruptedWrite; preserve it, block automatic initialization; explicit user/operator recovery required |
| Corrupt/future/invalid primary | Report typed error; leave original and pending unchanged |
| Storage unavailable/lock held | Report failure/uncertainty; no partial transaction |

For pending-only recovery, first back up files, inspect/validate the candidate,
and explicitly choose recovery or a fresh profile; do not automatically promote
an uncommitted purchase or delete evidence. Task 006 adds explicit confirmed backup/reset and recovery controls; see the
[integration guide](UNITY_PROGRESSION_INTEGRATION.md). On unsupported filesystems/runtimes, atomic promotion throws and
writes fail closed; implement another store only if it honors the same contract.
Linux/.NET tests exercise actual file replacement and injected interruption
before promotion, not a real power cut. File flush plus atomic namespace change
is not a promise of power-loss durability on all devices; directory metadata
fsync is not supplied. Android/Mono/IL2CPP file locking/replacement still require
target validation.

## Unity integration boundary

Task 006 supplies `ProgressionHost` at `Application.persistentDataPath`,
`ProgressionController` over the existing service/session, and thin IMGUI
between-run/recovery controls. `OutpostSession` adopts the registered run and
uses a run-start snapshot; it no longer constructs an unregistered alternative.
Receipts distinguish saved victory from duplicate/abandoned outcomes. Recovery
blocks Start/purchases and retains the current failed outcome for explicit retry.
The optional recovery store backs up raw files and binds reset confirmation to
unchanged storage. These adapters are authored, not verified in Unity. See
[full lifecycle and remaining checks](UNITY_PROGRESSION_INTEGRATION.md).

Unity's Inspector does not serialize `decimal` directly. A future settings adapter
should parse invariant-culture decimal strings or use an exact integer-ratio
representation for cost growth; do not introduce float-based price calculations
or truncate the 64-bit Credit balance into PlayerPrefs integers.

`link.xml` preserves the reflected DTOs/schema codec for Unity's managed linker.
This is preparation, not proof of IL2CPP support: test actual JSON round trips,
stripping/AOT, file operations and restart/reward failures in Unity/Android before
release. The new shared NUnit cases will be discovered by the existing test
assembly; cloud EditMode's expected minimum is updated accordingly. No Unity
activation workflow was manually retried during Task 005.

## Verification and limitations

- Clean consolidation base: **128 independent passes** in an isolated worktree.
- Task 005: **200 NUnit gameplay passes** (82 unchanged original cases plus
  74 progression and 44 persistence cases), plus 39 Python validator/helper and
  seven Graphify cases: **246 passed, 0 failed/skipped** after final verification.
- Metadata/assembly checks, SDK syntax parsing and graph freshness are recorded
  on the Task 005 PR. Existing CI globs already include new test files; no new
  NUnit dependency or Unity license is required by independent tests.
- **Unity execution is still unverified**: 200 shared cases have not run in Unity;
  four Editor and 17 PlayMode cases remain additional unexecuted cases. Expected
  Unity inventory is 204 EditMode + 17 PlayMode. No generated scene or APK claim.
- Client-owned saves are editable/rollbackable/deletable. Validation and run
  ledgers prevent malformed state and normal duplicate callbacks; they provide
  no cryptographic anti-cheat, cloud reconciliation or trusted server validation.
- The run ledger is intentionally retained for deduplication and grows with runs;
  bounded saves can eventually reach capacity. Future compaction/migration must
  retain deduplication evidence instead of dropping old paid IDs. No save format
  migration, combat resume, production recovery UI or complete shop exists yet.

The refreshed Graphify snapshot covers **61 code inputs and all 45 C# files**.
ProgressionService and GameRun are highly connected hubs; the generated report
records the current node/edge counts and source-commit provenance. Native Git
hooks refresh the map after source commits; the portable snapshot follows in an
ordinary data/documentation commit, validated by actual source hashes. Its
import-cycle report is empty; the separate eight-assembly
validator also finds no cycles. Mapped Core-to-Unity-adapter references remain
empty. Progression composes the existing simulation rather than adding a reverse
dependency from simulation to a Unity service. Graph links are source evidence
and include inferred edges, not proof of runtime wiring. The linker XML and
assembly definitions remain among 26 unsupported graph inputs and were inspected
separately.

See [REPOSITORY_CONSOLIDATION.md](REPOSITORY_CONSOLIDATION.md) for ancestry and
review order. The figures in this Task 005 report are historical. Task 006 reruns
all baseline cases and adds 28: **274 total passes**. Unity's current expected
inventory is **232 EditMode + 23 PlayMode**, all pending Unity execution. Next work
is actual Editor/Android verification of the now-authored lifecycle integration.
