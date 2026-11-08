# Unity and Android validation checklist — Task 003

All Unity/Android items are **pending**, even where implementation source exists.
The environment has .NET, but no Unity Editor installation was found.

## Import and generation

- [ ] Check out `development/task-003-unity-readiness-audit` or its reviewed successor.
      PRs #1–2 were unmerged at audit time; see [exact setup](UNITY_FIRST_LAUNCH.md).
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

- [ ] Run shared core/simulation **EditMode** cases (82 authored cases).
- [ ] Run **Editor generator tests** (4 cases, including failed/thrown save rollback),
      which create/clean their own output. EditMode total should be 86.
- [ ] Run **PlayMode** integration tests (17 cases). These use real Unity objects,
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
      enter Play Mode and press START RUN. Confirm no simulation on the start screen.
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
Do not add monetization, multiplayer, permanent shops or unrelated project changes.
