# Changelog

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
