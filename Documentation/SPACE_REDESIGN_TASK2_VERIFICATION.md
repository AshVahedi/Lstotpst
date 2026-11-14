# Space redesign Task 2 verification

Date: **2026-11-14**. Installed Editor: **Unity 6000.6.7f1**.
Branch: `development/space-independent-combat`, based on verified Task 1 commit
`0f0f435ca1d54b79c022b6c5b66d61b03423b33c`. Its PR targets
`development/space-redesign-foundation` (#10), preserving the earlier dependency
chain without merging it. Design, policies and exact manual instructions are in
[SPACE_REDESIGN_COMBAT.md](SPACE_REDESIGN_COMBAT.md).

## Resulting prototype

The existing space scene now uses automatic mounted projectiles, individual weapon
clocks, local directional cones and a staggered four-direction encounter. Its
default hull laser, equipped cannon and equipped missile use distinct ranges,
cadences, damage and speeds. A definition can also supply twin/triple emitters;
shared definitions never share timers. Configured side placement determines
outward facing. Ship steering, manual aiming and manual firing are absent.

The optional space path reuses the existing wave/boss roster, armored Health,
immutable equipment/stat snapshots, progression driver and reward owner. It does
not spend or award Energy or consume ammunition. The legacy scene still uses
its original direct-hit/energy/ammunition system. No schema or inventory IDs change.
Space projectiles have swept core collision; Unity objects only present results.

## Baseline and final executed results

The unchanged Task 1 baseline was rerun before implementation: **369 independent**
(323 shared .NET, 39 Python, seven Graphify), **350 EditMode** and **34 permanent
PlayMode** cases passed, with zero failures/skips. Task 1's previously reported
35th CLI case was disposable and is not part of its permanent inventory.

| Final suite / check | Passed | Failed | Skipped |
| --- | ---: | ---: | ---: |
| Shared NUnit / .NET 8 | 370 | 0 | 0 |
| Independent Python helper regressions | 39 | 0 | 0 |
| Pinned Graphify integration | 7 | 0 | 0 |
| **Independent total** | **416** | **0** | **0** |
| Complete Unity EditMode | **401** | **0** | **0** |
| Complete Unity PlayMode | **42** | **0** | **0** |

No earlier cases were disabled, filtered out of complete runs or silently weakened.
The shared fixture adds 47 cases; Editor adds four generator cases; PlayMode adds
seven adapter cases and one committed actual-scene/file-store smoke case. The smoke
is **included in the 42**, not an additional uncommitted case. Existing metadata,
reference-integrity, scene cancellation/rollback and all persistence tests remain.

Final safe generation, complete EditMode and complete PlayMode processes exited
zero. The final Unity logs contain **zero C# compiler errors or warnings** and no
unexpected runtime exception. A nonfatal licensing message remains: access token
unavailable for refresh; the same logs then resolve entitlements/update the cached
license and execute successfully. This is not a cloud activation claim.

The .NET runner emitted its existing `VSTEST_ARTIFACTS_DIRECTORY` fallback advisory;
the requested TRX was written and all 370 cases passed. No package/SDK upgrade was
introduced to silence that advisory. Syntax-only parsing passed for 76 asset C#
files; the actual Unity runs separately establish API/type compilation.

The publishable selection validates **141 Unity assets/folders**, 35 independent
Core source files, 14 shared NUnit fixtures and eight assembly definitions with
zero assembly errors. The raw workspace validates 146 assets/folders because five
ignored local workspace/test-export items remain excluded from publication.

The pinned incremental Graphify snapshot is fresh for 94 code inputs / 77 C# files
(including its tooling input): 2,011 nodes and 6,287 edges. Graphify is static code
evidence; scene references and runtime success come from their own audits/tests.

## Actual native scene and save smoke

The committed PlayMode smoke preinstalls a ProgressionHost with an injected file
store in a unique checked temporary directory. The generated scene resolves that
owner rather than opening the real profile. Teardown unloads only its own scenes,
destroys its own host and removes only that unique test directory.

With actual committed scene tuning and isolated initial **700 Credits**:

- Automatic simulation completes three waves and the boss (four encounters,
  14 arrivals). All front/left/right/rear directions and all three weapon types
  operate. Launch coordinates correspond to actual mount geometry.
- The boss emits **one hostile projectile** during the default winning run.
  Victory commits **500 Credits once**, giving 1,200. A later step does not pay again.
- A 200-Credit health upgrade leaves **1,000 Credits**. Unequipping the optional
  missile removes its emitter and module; the next run has two emitters and
  **110 maximum health**. Existing owned IDs are retained.
- Disabling those transient emitters lets ordinary contact/boss combat produce
  **Defeat**, with **zero additional reward**. Both terminal outcomes clear active
  projectiles. Core regressions separately verify hostile projectile damage,
  aggregate contacts and defeat priority on simultaneous boss/ship death.
- After unloading space and recreating the owner, the actual legacy scene reloads
  the same file **without rewriting it on load**: 1,000 Credits, health upgrade and
  missile selection persist. Legacy +X movement and the null space path remain.
  The final file is schema v2 with the same four owned IDs.

No real player save is used as a writable fixture. Atomic writes, unsupported-save
preservation, malformed-save recovery and stale-writer protection remain the
existing implementation and pass their retained regressions.

## Issues found and corrected

1. The first PlayMode integration run passed 40/41. The actual-scene smoke caught
   legacy CombatFeedback clearing descendants that belonged to the new reusable
   projectile pool on menu/restart, producing MissingReferenceException. Space
   presentation now has a separate sibling hierarchy. The generator assertion
   checks ownership separation, and the same enabled smoke verifies repeated runs.
2. Initial native renders showed side weapons could kill arrivals offscreen. Full
   camera entry is now an explicit acquisition condition. Offscreen waiting never
   spends cooldown, and range/cone checks still apply after entry. A shared
   regression covers that boundary. The space-only boss range changes from 3 to 6
   so it visibly returns fire; no other copied encounter tuning changes.
3. Muzzle position now follows travel before the first encounter as well as during
   combat; a regression checks the world position against route distance.
4. A new test initially referenced Collider2D directly, but the runtime project
   intentionally lacks Physics2D. The assertion now inspects component types for
   collider presence without adding an unnecessary engine package. The production
   swept collision model remains the sole damage authority.

No logged exception was suppressed and no failing fixture was disabled. Initial
failure logs/XML remain local alongside final evidence.

## Rendering and manual limits

Four actual Direct3D11 Camera.Render images at **540x960** were inspected: ordinary
combat, a side arrival, the boss in frame, and hostile boss fire. They show the
centered hull, connected horizontal modules, side enemy sprites, muzzle/projectile
positions, enemy/ship health bars and boss projectiles. A not-yet-entered arrival
can remain partly offscreen; it is not targetable until fully entered.

These world renders do **not** capture IMGUI/clicks, human camera-smoothness
judgment, Android safe areas or touch interaction. **Task 2 manual visual acceptance
is pending**, not replaced by automated results. The Editor-only disarm menu in the
design document provides a repeatable defeat check without editing assets or
directly assigning health/outcomes/rewards.

## Publication and preservation

All original legacy generated files and metadata remain SHA-256 identical. Space
prefabs/art/geometry and the scene GUID are retained. Exactly four generated files
change: the scene, its existing prototype tuning (boss range only), and the new
SpaceCombatSettings asset/metadata. Both existing build scene entries remain
unchanged; no new package or unrelated ProjectSettings edit is needed.

The tested native files were copied back only after source identity, complete
results, original-file hashes and the absence of the real Editor lock were checked.
The existing stash `073fe26483c196d18c4a4bad1d3a1a5b98ad94ba` and unrelated dirty
PlayerSettings file are hash-preserved and excluded. Raw XML/TRX/log/render evidence,
CLI drivers/projects, local scenes/exports and temporary saves are excluded.

Final XML SHA-256:

| Result | SHA-256 |
| --- | --- |
| EditMode 401/401 | `8da87c37bf568b25d60439b2f6b683ac95cb0980c97237fadeb07ef5f2aae0e5` |
| PlayMode 42/42 including native smoke | `e7d83c126c402c983b4a6b2db4e6c96533c1105613cd99a6dd37159b52b14482` |

Core and Graphify CI remain enabled. The existing cloud-Unity exclusion extends
only to this exact 6000.6 branch; the cloud pipeline still targets 6000.3 and is
intentionally skipped. No cloud Unity success, Android build, PR merge, force-push,
stash restoration/deletion or Computer Use occurred.

## Task 3 readiness

The combat foundation has no known blocking automated failure: independent emitters,
mount-relative targeting, bounded arrivals, swept collisions, wave/boss outcomes,
safe generation and compatible progression are available for later environments.
Manual visual/playability acceptance remains the next checkpoint. Task 3 will
still need explicit sector/mission configuration and reward policy, obstacle-aware
movement/collision choices and environment presentation. Straight-line missiles,
shared hull health, simple approach/ram AI, legacy-only reactor/ammunition effects,
basic graphics/IMGUI and untested device performance remain limitations. No
planetary/sector system or other Task 3 implementation is included here.
