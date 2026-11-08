# First Unity launch and Android development APK

Task 003, 2026-11-08. **These are pending execution steps, not a record of a
successful Unity import or build.** See [the audit](UNITY_READINESS_REPORT.md).
Task 004 adds [cloud verification configuration and licensing instructions](CLOUD_UNITY_VERIFICATION.md);
no cloud Unity operation is claimed as successful.

## 1. Get the integrated source

```sh
git clone https://github.com/AshVahedi/Lstotpst.git
cd Lstotpst
git switch development/task-005-permanent-progression
git status
```

At Task 004 inspection PRs #1, #2 and #3 are open and unmerged. `main` does not contain the
full integration. The audit branch starts from `development/graphify-workflow`
(`51443e57142b02247a10550f21754955f7a65858`), which contains Tasks 001–002 and Graphify.
After review/merge, use the branch that includes this audit; do not assume `main`
has it. Keep existing local edits and do not reset/force-push over them.
Task 004 starts from the Task 003 head `64e7475a591a1fab9d89572c931c03b16cf5727f`.
Task 005 uses the verified `development/integrated-foundation` at Task 004's
`c6440ba…` head. See [consolidation](REPOSITORY_CONSOLIDATION.md) and
[progression integration requirements](PERMANENT_PROGRESSION.md). The generator
still creates the legacy prototype; permanent progression is not wired to its UI.

Optional independent checks require .NET SDK 8.0.4xx and Python 3.12:

```sh
dotnet restore Tests/LastOutpost.Core.Tests.csproj
dotnet test Tests/LastOutpost.Core.Tests.csproj --no-restore --configuration Release
python3 Tools/validate_project.py
python3 -m unittest discover -s Tools/tests -v
python3 Tools/validate_assemblies.py --json TestResults/unity-static-audit.json
```

Graphify is optional for running Unity. For dependency queries, follow
[its existing setup](GRAPHIFY.md); do not install it into `Assets` or Unity packages.

## 2. Install the matching Editor and Android tools

In Unity Hub, install **Unity 6.3 LTS, 6000.3.0f1**, matching
`ProjectSettings/ProjectVersion.txt`. Use the Unity release archive if Hub does
not list that exact release. Add **Android Build Support**, its **Android SDK &
NDK Tools**, and **OpenJDK** submodules. Use Hub's matching tools rather than
arbitrary system Java/NDK installations. Activate an appropriate Unity license.
No production signing credentials are needed for a development APK.

Choose **Projects → Add → Add project from disk**, selecting the repository
root containing `Assets`, `Packages`, and `ProjectSettings`. Open it with that
Editor. Do not create a new project over this checkout. If upgrading to a newer
6000.3 LTS patch is necessary, do so in a separate reviewed change and rerun all
verification; this audit did not validate an upgrade.

## 3. Import and resolve packages

Wait for package resolution, asset import and script compilation to finish.
Open **Window → General → Console** and resolve errors before proceeding.
**Window → Package Management → Package Manager** should show Unity Test
Framework **1.4.5**; the manifest also requests built-in IMGUI and Image Conversion
modules **1.0.0**. Core uses .NET Standard 2.1; keep that API compatibility level
in Android Player Settings. No URP, Input System or Canvas/EventSystem is required.

`Packages/packages-lock.json` is deliberately absent until real resolution.
Review Unity's generated lockfile and completed/migrated ProjectSettings defaults
and commit them after verification. Do not manufacture a lockfile or ignore
package/import failures. The committed ProjectSettings express intent; confirm
their interpreted values in the Editor UI.

## 4. Generate and inspect Level 1

Exit Play Mode, save unrelated scene changes, then choose
**Last Outpost → Generate Level 1 Prototype**. The menu opens:

`Assets/LastOutpostGenerated/Scenes/IndustrialWasteland.unity`

Expected outputs below `Assets/LastOutpostGenerated/`:

| Folder | Generated content |
| --- | --- |
| `Configuration` | `PrototypeSettings.asset`, editable tuning |
| `Art` | `Placeholder.png` sprite and `Placeholder.mat` built-in sprite material |
| `Prefabs` | `Platform.prefab`, `Basic.prefab`, `Fast.prefab`, `Boss.prefab` |
| `Scenes` | `IndustrialWasteland.unity` |

The scene has one `GeneratedLevelRoot`, session/settings reference, platform/motor,
enemy view root/spawner with three prefab references, feedback root/material,
HUD, orthographic camera following the platform and placeholder scenery.
The platform has a health bar; enemy prefabs bind their own health bars.
There is no physical projectile prefab: immediate-hit damage is authoritative
in Core, and `ShotTrace` is a temporary visual LineRenderer only.
Start/victory/defeat are states of the single IMGUI HUD, not separate scenes.
Wave/resource managers are composed inside `GameRun`, not extra scene components.

Run the menu again. Confirm one owned root, four prefabs, stable GUIDs/settings,
preserved unrelated roots, and one enabled scene-list entry. Owned prefab/root
content is regenerated: keep manual art outside those owned outputs. A failed
scene save is intended to restore the old hierarchy; the new Undo rollback tests
must run before relying on that behavior. Asset generation is not a multi-file
disk transaction; use Git to review asset changes after any failure.

Optional batch generation from the root (Bash; `UNITY_EDITOR` is the full path
to a licensed Editor executable, e.g. Hub's Editor/Unity on Linux):

```sh
"$UNITY_EDITOR" -batchmode -nographics -quit -projectPath "$PWD" -executeMethod LastOutpost.Editor.PrototypeSceneBuilder.GenerateBatch -logFile TestResults/unity-generation.log
```

Create `TestResults` first if needed. On PowerShell invoke the executable with
`& $env:UNITY_EDITOR`, and replace `$PWD` with `(Get-Location).Path`.

## 5. Play and inspect the Console

Open the generated scene if it is not already active. Set Game View to
**540 × 960** / 9:16 portrait. Enter Play Mode, press **START RUN**, and check:

- Automatic horizontal travel; slowdown during ordinary encounters; stop for
  boss and terminal states. Camera follows the platform.
- Basic/red, fast/yellow and boss/purple enemies approach, attack and disappear
  once. Hit tracers render; basic fire continues when missiles reach zero.
- Health, Energy, ammo, wave and progress update; three waves precede the boss.
- Purchases cost 30 Energy for up to 20 ammo, 50 for up to 25 health, and 80 for
  a special strike by default. Unaffordable/no-effect commands do not charge.
- Boss death while alive yields **VICTORY**; zero platform health yields
  **OUTPOST LOST**. **RUN AGAIN** resets inventories, clock, position and views.

Repeat at different portrait aspect ratios. Clear Console before each run,
disable Collapse when investigating repeated errors, and preserve exception
stacks/logs. Any compile error, missing script/reference, unexpected duplicate
reward, or terminal-state failure blocks acceptance. A passing .NET suite alone
does not validate this scene.

## 6. Execute actual Unity tests

Outside Play Mode, choose **Window → General → Test Runner**:

1. **EditMode → Run All**: expected **204 cases**, comprising 200 shared core cases
   and four Editor generator cases (two existing and two save-failure cases).
2. **PlayMode → Run All**: expected **17 integration cases**.
3. Export/save actual XML results and Editor logs. Inspect any discrepancy in
   discovery/counts; do not substitute .NET results for Unity results.

Batch equivalents (do not add `-quit` to these test commands):

```sh
mkdir -p TestResults
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$PWD" -runTests -testPlatform EditMode -testResults TestResults/unity-editmode.xml -logFile TestResults/unity-editmode.log
"$UNITY_EDITOR" -batchmode -nographics -projectPath "$PWD" -runTests -testPlatform PlayMode -testResults TestResults/unity-playmode.xml -logFile TestResults/unity-playmode.log
```

Generator tests own and clean `Assets/LastOutpostGeneratorTestOutput`. If that
folder already exists, they deliberately fail rather than overwrite it. Inspect
its contents before manually removing leftovers. No Unity tests have run in
the audit environment; expected counts are source-derived, not executed passes.

## 7. Configure Android and build a development APK

Open **File → Build Profiles** (Unity 6), **Add Build Profile → Android**, add/select
the profile, then **Switch Platform**. In its Scene List use the global scene
list containing the enabled generated scene, or explicitly include that scene
if the profile overrides the global list. The generator registers the global
list; an overriding profile must be checked separately.

Open **Edit → Project Settings → Player → Android** (or Player Settings from
the Build Profiles window). Verify the imported values:

| Setting | Prototype intent |
| --- | --- |
| Company / Product | LastOutpost / Last Outpost |
| Version / Bundle version code | 0.1.0 / 1 |
| Application identifier | `com.lastoutpost.prototype` (development identifier) |
| Resolution and Presentation → Default Orientation | Portrait; no landscape autorotation |
| Other Settings → Minimum API Level | Android 8.0 / API 26 |
| Other Settings → Target API Level | Automatic / highest installed supported API |
| Scripting Backend / Target Architectures | IL2CPP / ARM64 |
| API Compatibility Level | .NET Standard 2.1 |

Keep built-in rendering; do not add an unrelated render pipeline. Under
**Edit → Preferences → External Tools** (macOS: **Unity → Settings/Preferences →
External Tools**), prefer the Android SDK, NDK and JDK installed with Unity.
Check the Console's actual toolchain errors if modules are missing. The project
has no reflection-driven gameplay registration or dynamic code generation, but
IL2CPP stripping/AOT compatibility is still unverified until this build runs.

Enable **Development Build**, leave **Build App Bundle (Google Play)** off to
produce an APK, and optionally enable Script Debugging. Use default development
signing; do not add keystores/secrets. Choose **Build**, save as
`Builds/Android/LastOutpost-dev.apk`, and retain the build log. Build outputs are
ignored by Git. Target API automatic is not a Play Store compliance guarantee.

## 8. Install on a physical Android device

Use an ARM64 device running Android 8.0 or later. Enable developer options/USB
debugging, connect USB and approve the device's debugging prompt. With the SDK's
`platform-tools` on PATH (or its full `adb` path):

```sh
adb devices
adb install -r Builds/Android/LastOutpost-dev.apk
adb shell monkey -p com.lastoutpost.prototype 1
adb logcat -s Unity
```

Alternatively use **Build And Run** with that device selected. Test actual touch
controls, notched safe areas, narrow/tall portrait screens, pause/background/resume,
restarts, all purchases, three-wave/boss order, both outcomes, performance and
memory. Save logs, device model/API, screenshots and reproducible failures.

## Troubleshooting and acceptance

- **No Last Outpost menu / tests missing:** first fix Console compilation/package
  errors. Confirm the integrated branch, Editor-only asmdefs, Test Framework 1.4.5
  and installed Editor version. Do not move Editor code into runtime assemblies.
- **Blank scene / no HUD:** open the generated scene; confirm its enabled root,
  serialized session/settings reference and orthographic Main Camera. An empty
  default scene is not the prototype. Generation never ran in this cloud audit.
- **Pink or missing placeholders:** inspect sprite import/material references and
  `Sprites/Default` availability. This project uses built-in rendering, not URP.
- **Repeated generation/save error:** stop Play Mode, inspect Console/permissions,
  confirm old content remains, and inspect Git asset changes. Do not delete
  unrelated scene roots or manually invent YAML references.
- **Android/Gradle/IL2CPP error:** verify Hub's matching SDK/NDK/OpenJDK, ARM64,
  installed target API and scene list. Preserve the first real error/build log.
- **ADB unauthorized / installation failure:** approve USB debugging, select the
  correct device and inspect the actual adb message (ABI, API, storage/signature).
  Removing an existing app can delete its data; it is not a routine build step.
- **Clipped/tiny UI:** capture device aspect ratio and safe area. HUD uses
  width-scaled IMGUI within `Screen.safeArea`; physical touch/readability is pending.

Complete [the detailed checklist](UNITY_CHECKLIST.md), review and commit generated
assets with `.meta` files, lockfile and imported settings, and attach real Unity
test/build/device evidence. Retain **0.1.0** until the 0.2.0 validation gate is met.
