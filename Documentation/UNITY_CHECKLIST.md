# Local Unity validation and next implementation

Everything below is **pending**. A passing .NET suite does not check Unity import,
scene execution, touch interaction, Android builds, or physical-device behavior.

## Verify the foundation

- [ ] Clone the repository's `main` branch and review the initial foundation.
- [ ] Install Unity Hub and Unity **6000.3.0f1**, Android Build Support, matching
      Android SDK/NDK Tools, and OpenJDK.
- [ ] Open the project root; let Unity generate its Library and complete package
      resolution. Review generated project-setting changes and packages lock file.
- [ ] Confirm no asset import, assembly definition, compiler, or package errors.
- [ ] Confirm 2D default behavior, portrait orientation, disabled landscape
      autorotation, minimum Android API 26, ARM64 and IL2CPP.
- [ ] Run all EditMode tests in Unity Test Runner; compare against the independent
      suite and preserve real editor test results. No PlayMode tests exist yet.
- [ ] Confirm core assembly has no engine references and test files are excluded
      from a player build by their Editor-only test assembly.
- [ ] Re-run independent .NET tests and the static validator after migrations.

## Recommended next Codex task: first playable scene (Milestone 3)

Work in a Unity-capable environment so integration is actually testable.

1. Add Unity gameplay/presentation assemblies referencing core. Introduce
   ScriptableObject authoring adapters only where needed; keep rules in core.
2. Build one portrait, side-view placeholder scene with orthographic camera,
   an original modular vehicle shape, distinct Basic/Fast/boss shapes, and a
   horizontal industrial route. Use no copied assets.
3. Add an independent configurable route mover: ordinary travel speed, encounter
   slowdown/stop, boss stop until defeat. No manual steering. Keep movement
   separate from targeting/firing and from presentation offsets.
4. Map deterministic core enemy roster members to Unity objects; move living
   enemies toward the platform. Never allow a dead/spawning visual to grant
   another core reward. Track roster membership across delayed spawns if used.
5. Supply current range snapshots and elapsed level time to automatic weapons and
   living enemy attacks. Maintain the unlimited basic gun when missile ammo is
   empty. Represent successful shots with simple tracer/projectile feedback;
   apply damage once according to the core immediate-hit contract.
6. Collect rewards once; refresh level status after all combat in each tick.
   Stop simulation on terminal status, especially simultaneous boss/player death.
7. Add Start, Gameplay, Victory, and Defeat views and a minimal touch-friendly HUD
   for health, Energy, ammo, wave, progress, and the three tactical purchases.
   Show costs and rejection feedback. UI invokes rules and renders results.
8. Write and execute PlayMode tests for travel, stopping at boss, spawn order,
   automatic targeting/fire, callback-safe drops, purchase wiring, simultaneous
   death, restart/reset, and victory/defeat screens. Capture a real gameplay run.

Do not add another level, monetization, saves, permanent shops, or online systems.

## Android validation after the playable milestone

- [ ] Add the playable scene to Build Profiles/Build Settings and select Android.
- [ ] Confirm the target SDK is valid for the installed Unity Android toolchain;
      change the development application identifier if needed.
- [ ] Generate a development APK and record the editor/toolchain versions and log.
- [ ] Install on a physical Android device and confirm portrait behavior, safe
      areas, readable HUD, touch buttons, pause/resume, performance, and restart.
- [ ] Verify victory, defeat, empty-ammo fallback, insufficient funds, capped
      repairs/refills, special strike, boss stop, and duplicate-death callbacks.

No APK build can succeed before the playable scene exists and is in the build list.
