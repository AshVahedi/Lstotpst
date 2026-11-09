# Unity and Android validation checklist — Task 003

All Unity/Android items are **pending**, even where implementation source exists.
The environment has .NET, but no Unity Editor installation was found.

## Import and generation

- [ ] Check out `development/task-007-modular-equipment` or its reviewed successor.
      PRs #1–2 were unmerged at audit time; see [exact setup](UNITY_FIRST_LAUNCH.md).
      Task 004's successor includes [cloud CI setup](CLOUD_UNITY_VERIFICATION.md);
      PR #3 also remained unmerged at its start. CI configuration is not execution.
- [ ] Install Unity 6000.3.0f1 through Hub; add Android Build Support, matching
      SDK/NDK Tools and OpenJDK when preparing device builds.
- [ ] Open the existing project root and wait for complete import/package resolution.
- [ ] Resolve any real Console compiler, API, assembly or asset-import errors.
- [ ] Confirm 2D defaults, portrait orientation/disabled landscape autorotation,
      API 26 minimum, ARM64 and IL2CPP. Do not infer these from hand-authored YAML alone.
- [ ] Run **Last Outpost → Generate Level 1 Prototype**. Confirm one scene,
      one marked root, four prefabs, one settings asset, camera, motor, spawner,
      tracers and HUD references, plus one enabled build entry.
- [ ] Run generation a second time; confirm no duplicate roots, actors, build
      entries or assets, stable GUIDs and preserved settings tuning/unrelated roots.
- [ ] Review and commit generated assets/.meta, imported ProjectSettings defaults
      and Packages/packages-lock.json. No serialized scene/prefab was validated here.

## Actual Unity tests

- [ ] Run shared core/simulation/progression **EditMode** cases (285 authored cases).
- [ ] Run **Editor generator tests** (4 cases, including failed/thrown save rollback),
      which create/clean their own output. EditMode total should be 289.
- [ ] Run **PlayMode** integration tests (26 cases, including three equipment adapter cases). These use real Unity objects,
      controlled session stepping, destruction frames and HUD command entry points.
- [ ] Preserve the real Unity XML/log files. Investigate failures rather than
      counting .NET passes or syntax parsing as Unity passes.

Optional CLI equivalents, with UNITY_EDITOR set to a licensed Editor executable:

```sh
mkdir -p TestResults
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$PWD" -runTests -testPlatform EditMode -testResults TestResults/unity-editmode.xml -logFile TestResults/unity-editmode.log
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$PWD" -runTests -testPlatform PlayMode -testResults TestResults/unity-playmode.xml -logFile TestResults/unity-playmode.log
```

## Manual portrait gameplay

- [ ] Open the generated IndustrialWasteland scene, set Game View to 540 × 960,
      enter Play Mode and inspect UPGRADES/LOADOUT, then press START LEVEL 1. Confirm no simulation on the start screen.
- [ ] Confirm automatic horizontal route movement, ordinary encounter slowdown,
      boss stop and terminal stop, independent of render frame rate.
- [ ] Confirm Basic/Fast/boss shapes, health bars, approach and distinct attack timing.
- [ ] Confirm closest-live targeting, attack intervals, one damage application per
      visual tracer and continued basic fire at zero missile ammo.
- [ ] Defeat enemies; confirm each reward and destroyed view occurs once.
- [ ] Exercise all purchase buttons with sufficient and insufficient Energy,
      full health/ammo, capped partial effects, no special target and destroyed platform.
- [ ] Confirm exactly three waves then boss, no premature wave advancement,
      travel delays, victory and defeat views and once-only finish notifications.
- [ ] Restart from both outcomes; verify fresh health, Energy, ammo, timers,
      position, enemies and event subscriptions. Pause/resume the application.
- [ ] Purchase each permanent upgrade, confirm run snapshots do not stack, and
      verify ammo capacity/armor/scaling/health from the authoritative catalog.
- [ ] Confirm saved victory earns 500 Credits once, defeat earns zero, returning
      to menu/rebinding does not pay again, and relaunch preserves committed state.
- [ ] With copied test saves, exercise corrupt/future/pending-only recovery, stale
      writers, failed reward saves, explicit reset confirmation and preserved backups.
- [ ] Verify owned/compatible slots, primary removal rejection, optional equipment
      removal/replacement, plating/reactor effects, and immutable active-run gear.
- [ ] Test copied v1 saves migrating without changing Credits/ledger, v2 reload,
      unknown equipment/slot preservation, stale writers and atomic migration failure.
- [ ] Replace/reload the scene; confirm one persistent host and one ticking driver.
- [ ] Inspect HUD safe-area/readability and touch targets across portrait sizes.
- [ ] Record a real playthrough. No graphical/gameplay quality or balance claim
      is supported by the current headless tests alone.

## Android gate and version

- [ ] Select Android Build Profile and confirm the generated scene is included.
- [ ] Verify installed SDK/NDK/JDK/target API compatibility, development identifier
      and portrait device settings; generate a development APK and preserve logs.
- [ ] Install on a physical device and check touch, safe areas, pause/resume,
      victory/defeat/restart, empty-ammo fallback, performance and memory.
- [ ] Rerun .NET/static CI after fixes; review generated source/asset changes.
- [ ] Promote VERSION.md, bundleVersion and changelog to 0.2.0 only when the
      integration milestone is verified; keep unverified checks explicitly pending.

Recommended next task is validation and fixes, not another level or feature set.
Do not add monetization, multiplayer, additional shop features or unrelated changes.
