# Changelog

## Unreleased — Task 007 modular equipment (0.1.0 retained)

- Verified Task 006 PR #7/head 8e7cbd1 and reran its 274-case baseline before
  branching; preserved the dependent PR stack without merging or rewriting history.
- Added typed equipment/catalog/slots, separate ownership/installation, atomic
  loadout edits, compatibility/required/duplicate/capacity checks and defaults.
- Reused basic/missile combat and the module rack; added +2 percentage point
  Armor Plating within the existing armor cap and +1 Energy-per-defeat Reactor
  Booster. Existing upgrade values, Credits and tactical prices are unchanged.
- Applied base/permanent/equipment modifiers before final rounding/clamping into
  immutable run snapshots; frozen installed racks and preserved unlimited basic fire.
- Added schema-v2 ownership/loadout persistence, explicit read-only v1 migration,
  unknown-ID retention, existing CAS/atomic recovery and IL2CPP DTO preservation.
- Added minimal thin loadout controls and three pending Unity PlayMode cases.
  All 331 independent tests pass (285 C# + 39 helpers + seven Graphify); 57 cases
  are new. Existing schema/death fixtures were adapted without dropping cases.
- Updated graph, static checks, documentation and future Unity test thresholds.
  Unity compilation/generation/tests and Android/device behavior remain unverified.

## Unreleased — Task 006 Unity progression integration (0.1.0 retained)

- Based the dedicated branch on Task 005 b3b835b; verified PRs #1–6 remain open
  and dependent. No automatic merge, rewritten history or main changes.
- Added a testable application progression coordinator, immutable upgrade/menu
  views and a single persistent Unity host with safe scene-session rebinding.
- Adopted the registered run, fixed upgrade snapshot and durable outcome receipts;
  pending reward saves block unsafe transitions instead of appearing committed.
- Added minimal between-run upgrades, saved-Credit results and explicit recovery:
  reload/retry, confirmation-bound reset, raw-byte backups and atomic promotion.
- Preserved schema-v1, catalog values, Energy transactions and existing tests.
  Added 28 independent integration/recovery cases and six pending PlayMode cases.
- Verified 274 independent passes (228 C# + 39 helpers + seven Graphify), static
  assembly boundaries and syntax. Updated graph and scene-generator host wiring.
- Unity compilation/generation, 27 additional Unity-only cases and Android/device
  persistence remain unexecuted. No Unity activation retry or version increase.

## Unreleased — Task 005 permanent progression (version retained at 0.1.0)

- Published the clean Tasks 001–004/Graphify ancestry as `development/integrated-foundation`
  in review PR #5, without merging/closing earlier PRs or changing main. Independently
  verified its 128-test baseline in an isolated worktree.
- Added permanent Credits, configurable 500-Credit Level 1 victory rewards, durable
  unique run registration/outcome deduplication, completion/unlock metadata and a
  Unity-neutral run lifecycle boundary; kept Energy separate and transient.
- Added four permanent upgrades, exact rational ceiling prices, validated catalogs,
  fixed run-start stats, capped armor, deterministic weapon scaling and ammo capacity.
- Added versioned framework-JSON persistence, memory/file compare-and-swap stores,
  atomic replacement, pending-write handling and explicit recovery/error states.
- Preserved all 82 original gameplay tests and added 118 progression/persistence
  cases: 200 NUnit passes plus 46 Python cases, 246 independent passes total.
- Updated graph artifacts, architecture, integration/recovery documentation, linker
  preservation and future Unity EditMode minimum. No graphical shop/scenes were added.
- Unity/Android compilation, serialization/AOT and actual session wiring remain
  unverified; no manual Unity activation retry was requested by Task 005.

## Unreleased — Task 004 cloud verification configuration

- Verified the unmerged PR stack and based Task 004 on the Task 003 integration
  head; kept gameplay version 0.1.0 and all Unity/gameplay source unchanged.
- Verified exact 6000.3.0f1 Linux/Android GameCI image availability; pinned images,
  actions and CLI. Added activation preflight, sequential EditMode/generation/
  PlayMode stages, optional gated ARM64/IL2CPP development APK and evidence uploads.
- Reused the existing batch generator and rollback; added 17 independently tested
  evidence/licensing/configuration checks. All 128 independent tests pass.
- Documented secret setup, storage/time/cost assumptions, CI-only Android API 35
  workaround for SDK bootstrap, artifact retention and remaining device checks.
- Unity compilation/tests, generated assets/package lock and APK remain unexecuted;
  classification CLOUD VERIFICATION BLOCKED until licensed CI execution succeeds.
- Observed the real PR workflow fail activation preflight and upload infrastructure
  JSON; downstream Unity/Android jobs were skipped. Both independent CI workflows
  passed on push and PR; recorded run/artifact links and the required secret setup.

## Unreleased — Task 003 readiness audit (version retained at 0.1.0)

- Verified Tasks 001–002/Graphify ancestry and open, unmerged PRs #1–2; based the
  dedicated audit branch on `development/graphify-workflow` without merging them.
- Audited eight assembly definitions, initialization, generator wiring, hitscan
  combat/purchases/outcomes and portrait Android settings; documented unverified
  package resolution, generated assets, Unity APIs/tests and APK/device gates.
- Added generator scene-replacement rollback for failed/thrown saves and two
  authored Unity Editor regression cases; Unity execution remains unavailable.
- Added independent assembly/GUID/platform/cycle validation, 22 mutation tests,
  CI evidence artifact and `.asmref` graph-workflow triggers.
- Reran 82 gameplay and 7 Graphify tests: 111 independent tests passed including
  the new 22. All 38 asset C# sources parse; this is not Unity compilation.
- Refreshed Graphify artifacts; added exact first-launch/Android instructions and
  readiness report. Verdict: READY WITH KNOWN LIMITATIONS. No gameplay features added.

## Unreleased — Graphify development workflow

- Added pinned graphifyy 0.9.104 in a development-only Python environment, official
  project-scoped Codex skill/registration and native incremental Git hook setup.
- Generated and committed the queryable JSON, interactive HTML and architecture
  report with portable native update state; excluded Unity outputs and tool caches.
- Added a read-only, code-only GitHub Actions graph/artifact workflow and seven real
  Python integration tests for builds, freshness, edits, deletions and exclusions.
- Documented query-first Codex usage, inferred/static graph limits, unsupported
  assembly definitions, CDN visualization dependency and no-op Codex Desktop hook.
- Kept gameplay source, Unity packages and game version unchanged.

## Unreleased — Task 002 integration (version retained at 0.1.0)

- Added a plain C# run coordinator composing the existing rules, configurable
  automatic route motion, deterministic spawn positions, approach/attack logic,
  ordered combat, once-only rewards and terminal notifications, and run commands.
- Added Unity session, settings, platform transform, enemy actor/spawner adapters,
  visual-only hit tracers, camera follow, actor health bars, and minimal IMGUI
  start/HUD/purchases/victory/defeat views.
- Added an idempotent Editor generator for one Level 1 scene, four prefabs,
  placeholder art, settings, and build-scene registration. Generation is pending.
- Preserved all original 58 tests; added 24 independent simulation integration
  tests (82 passing total), 17 authored PlayMode tests and 2 Editor generator tests.
- Added SDK-bundled Roslyn syntax-only validation and assembly/UI-boundary checks
  to the real .NET CI workflow. Added only built-in IMGUI/Image Conversion modules.
- Fixed contact-range rounding at nonzero route positions so enemies reach attack
  range reliably; kept basic fallback active when advanced modules/ammo are absent.
- Unity compilation, generator execution, Unity tests, manual play and Android
  packaging remain unverified; no 0.2.0 release claim is made.

## 0.1.0 — 2026-11-08

- Established a Unity 6.3 LTS project scaffold with portrait Android intent,
  engine-independent core assembly, stable metadata, and test assembly.
- Added validated health, Energy wallet, ammunition, atomic tactical purchases,
  automatic weapon timing and targeting, special strike, and module slots.
- Added Basic/Fast/boss definitions, exactly-once enemy rewards, and deterministic
  Industrial Wasteland progression with boss gating, victory, and defeat.
- Added a future permanent starting-stats provider seam without Credits, saves,
  or a permanent upgrade shop.
- Added shared NUnit tests, independent .NET projects, static project validation,
  and a GitHub Actions workflow that executes tests and preserves results.
- Documented architecture, integration contracts, verification limits, and next work.
- Unity scene integration deferred because no Unity Editor is available.
- Integrated the foundation into the supplied repository's initially empty `main`
  checkout after GitHub access became available.
- Published the tested foundation to GitHub `main` after repository authorization
  was updated; preserved the original foundation commit without force-pushing.
