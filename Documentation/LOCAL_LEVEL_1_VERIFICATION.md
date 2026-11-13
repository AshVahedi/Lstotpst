# Local Level 1 integration verification

Automated verification began on 2026-11-12 with Unity **6000.6.7f1**; the user
accepted the manual visual checkpoint on 2026-11-13. Branch:
`development/local-unity-6000.6-verification`, C# verification baseline
`be9e437f3e5b2c141d0f58e41a012e4a486318ba` (PR #9).
Publication scope: generated Level 1 assets, build scene entry and this report;
no production C# changes.

## Generation and asset inspection

The user invoked **Last Outpost > Generate Level 1 Prototype** in the real Editor.
The output folder was absent beforehand; no existing prototype assets were
overwritten. The generator produced eight primary assets and their metadata:

- `Assets/LastOutpostGenerated/Scenes/IndustrialWasteland.unity`
- `Assets/LastOutpostGenerated/Prefabs/Platform.prefab`
- `Assets/LastOutpostGenerated/Prefabs/Basic.prefab`
- `Assets/LastOutpostGenerated/Prefabs/Fast.prefab`
- `Assets/LastOutpostGenerated/Prefabs/Boss.prefab`
- `Assets/LastOutpostGenerated/Configuration/PrototypeSettings.asset`
- `Assets/LastOutpostGenerated/Art/Placeholder.mat`
- `Assets/LastOutpostGenerated/Art/Placeholder.png`

The scene occurs exactly once, enabled, in EditorBuildSettings with a matching
scene GUID. Forty read-only serialized structure/reference checks passed. These
checked the session/settings/host links, automatic ticking, platform prefab and
both mounted weapon placeholders, three enemy prefab actor references and health
bars, HUD, combat feedback, camera target, initialized tuning, wave counts and
single-sprite placeholder import. The scene contains a single generated root and
one enabled orthographic MainCamera.

Equipment is supplied by the existing ProgressionHost ->
ProgressionConfiguration.Prototype -> EquipmentConfiguration.Prototype path.
The default four owned items are Basic Cannon, Missile Launcher, Armor Plating
and Reactor Booster. There is no separate generated equipment asset or Canvas:
the existing HUD uses IMGUI. Placeholder weapon graphics do not change with
equipment selection; functional selection is tested separately.

The publishable project selection passed metadata validation for **108
assets/folders**, eight assembly definitions and package consistency, with zero
audit errors. A raw workspace scan initially encountered an ignored Test Runner
XML export without imported metadata; that local export was preserved and omitted
from the publishable selection. No production asset metadata is missing. The
generated file scan found no absolute user paths or credential patterns.

## Executed tests

| Run | Passed | Failed | Skipped |
| --- | ---: | ---: | ---: |
| Real-project Editor EditMode export after generation | 302 | 0 | 0 |
| Real-project Editor PlayMode export | 26 | 0 | 0 |
| Isolated-copy command-line EditMode rerun | 302 | 0 | 0 |
| Isolated-copy command-line PlayMode rerun | 26 | 0 | 0 |
| Additional generated-scene command-line smoke test | 1 | 0 | 0 |
| Independent .NET NUnit rerun | 291 | 0 | 0 |
| Independent Python helper rerun | 39 | 0 | 0 |
| Pinned Graphify integration rerun | 7 | 0 | 0 |

The independent total remains **337 passed**. All existing tests remained enabled.
The generated-scene smoke test is an additional disposable verification harness,
not a replacement for any of the 26 existing PlayMode cases. No production C# or
test sources were changed, and no gameplay fix was necessary in this checkpoint.

The command-line runs used the installed Editor with `-batchmode -nographics
-runTests`, separate EditMode/PlayMode invocations, explicit XML/log paths, and no
Computer Use. The copied project was verified against the original. Only its
company name was intentionally overridden to give Application.persistentDataPath
a unique test directory. The additional harness was then placed only in that
ignored copy and selected with `-testFilter`. The original scenes and player
profile were never used as writable test fixtures. All three Unity processes
exited with code zero and produced passing executed-result XML.

### Actual generated-scene runtime check

The additional test loaded the copied persistent IndustrialWasteland scene,
retained its actual serialized tuning, host, HUD and views, and used the existing
controlled-step API with automatic ticking disabled for deterministic stepping.
It issued ordinary ammunition/repair commands without changing balance values:

- Platform movement and instantiated enemy views were observed programmatically.
- Automatic combat fired **44 basic shots and 14 missiles**.
- **One ammunition purchase and two repairs** succeeded using earned Energy.
- All three waves and the fourth/boss encounter were reached; the run won.
- Victory committed **500 Credits**; HUD rebinding did not pay again.
- A maximum-health upgrade cost **200 Credits**, leaving **300**.
- Removing the required primary weapon was rejected; removing the utility item
  succeeded and persisted in schema v2.
- Unloading the scene, destroying the test's persistent host, and reloading the
  same saved scene loaded the real test file: 300 Credits, empty utility slot,
  110 maximum health for the next run, and zero reactor Energy bonus.
- Defeat was triggered through the existing damage API and gave no extra Credits.

This was headless behavioral verification, not visual inspection. It does not
prove IMGUI layout, sprite/tracer appearance, camera framing or input usability.
The test's 300-Credit profile is isolated; those Credits were not added to the
user's real profile. Existing cases also passed for corrupt-save recovery,
v1-to-v2 migration, host lifecycle, pause, special strikes and save-safe rewards.

## Preservation and diagnostics

All 22 inventoried generated/build files remained byte-identical through both
Editor suites and all command-line runs. The existing real schema-v2 player save
was backed up locally and remained byte-identical. The stash remains
`073fe26483c196d18c4a4bad1d3a1a5b98ad94ba`. No PR was merged, history rewritten,
stash restored, Android build started, or Task 008 work performed.

All three command-line logs contain zero C# compiler error/warning diagnostics.
Each log contains one nonfatal Unity licensing access-token refresh message;
cached licensing allowed execution and every suite completed successfully. No credentials
were inspected or changed to address that message.

Graphify freshness passed: 76 code inputs, 59 C# files, 1,652 nodes and 4,890 edges.
The snapshot remains current because production source did not change; it does
not prove serialized references or runtime behavior.

Local ignored evidence is under
`TestResults/local-unity-6000.6/level-1-integration/`: raw exports, command-line
XML/logs, file hashes, reference inspections, independent results, the disposable
project/harness and player-save backup. Command-line XML SHA-256 values:

| XML | SHA-256 |
| --- | --- |
| EditMode | `29ddae6a50c43ef83627519f78e1264a07d53c0208138ff12575e60e10c06408` |
| PlayMode | `1e1d901a756f025a29f67c71d3a8c0fc7030e25bc0ef4d496ed94e85c9dfa324` |
| Generated-scene smoke | `1fdb557d509672a1602089da85fb067200f6a49ad679b0eaea5edfaf86d7b1b8` |

## Git review and remaining checkpoint

The reviewed commit selection is the generated folder and all its metadata,
EditorBuildSettings, and this report, following the manual acceptance below.
Unity also rewrote whitespace and repopulated the unused PS4 passcode field in
ProjectSettings.asset after the previous milestone. Its only semantic difference
from HEAD is that generated field; it is not an intended gameplay/settings change
and must be excluded or sanitized before a future commit. It was left untouched
in this checkpoint to preserve the user's current working state. Raw test exports,
local saves, CLI copies and caches must remain excluded.

### Manual visual/gameplay checklist

1. Open the real checkout with **Unity 6000.6.7f1**, not the ignored command-line
   copy under TestResults. Leave Play Mode and save your current work first.
2. Choose **File > Open Scene**, select
   `Assets/LastOutpostGenerated/Scenes/IndustrialWasteland.unity`, and open it
   normally, not additively. In Hierarchy, confirm IndustrialWasteland is the only
   user scene. LocalVerificationWorkspace has another Main Camera; do not leave
   that workspace loaded for this check. Save unrelated edits if prompted rather
   than discarding them. Do not regenerate anything.
3. Open **Window > General > Game**. In its aspect/resolution dropdown, add a
   **Fixed Resolution** preset named `Portrait 540x960`, width **540**, height
   **960**, and select it. A 9:16 aspect preset alone is not a fixed pixel size.
   Use Maximize On Play if needed to see it comfortably. PlayerSettings defaults
   do not automatically set the Editor Game view resolution.
4. Open **Window > General > Console**, clear old messages, then enter Play Mode
   with the normal Play button. Confirm the start menu, Credits, UPGRADES and
   LOADOUT controls are readable and clickable. Record the existing balance;
   the isolated automated test's Credits were not added to this profile.
5. Open UPGRADES and check all four rows, previews, prices and BACK. Open LOADOUT,
   scroll through all slots to its bottom controls, and check text and button
   spacing. Primary removal should be unavailable. If changing an optional item,
   record and restore your prior choice unless you want to keep the change;
   these commands persist immediately. Do not use a profile reset for this test.
6. Return to the menu and click **START LEVEL 1**. Movement, targeting and firing
   are automatic; no movement/fire keys are required. Observe the teal platform,
   scrolling scenery, health bars, red basic enemies, yellow fast enemies and
   cyan/yellow/red shot feedback. Watch for clipping, unreadable text, overlapping
   HUD elements, camera jitter or attacks that are difficult to see.
7. Check the top Health/Energy, Missiles, Wave and progress displays and the bottom
   tactical buttons. Verify insufficient-funds feedback. With the default tuning,
   ammunition costs 30 Energy, repair costs 50 and special strike costs 80. Buying
   ammunition after earning 30 Energy and repairing when needed is a viable
   route used in the automated scene test. Observe missile and special feedback
   when those purchases are available. Credits must remain separate from Energy.
8. Observe three waves, then the purple boss. Check the VICTORY overlay, saved
   reward text and BETWEEN-RUN MENU button. Compare the balance with the amount
   recorded before the run: a saved victory adds 500 Credits once. Returning to
   the menu must not pay again. Check that combat/movement stop and existing
   tracers disappear shortly after the outcome.
9. On a separate run, avoiding tactical purchases may lead to defeat. If it does,
   inspect OUTPOST LOST and confirm no victory reward. Do not edit health or save
   data just to force a loss. If defeat is not reached, report its visual check
   as unobserved rather than passed.
10. With sufficient Credits, buy an upgrade and check the balance and next-run
    preview. Stop and reenter Play Mode; confirm committed Credits, upgrade and
    optional equipment choice persist. This checks profile transactions, not
    combat resume. Inspect Console for new errors/warnings and leave Play Mode
    before reporting results. If SAVE RECOVERY appears, preserve its message and
    stop; do not confirm a new profile as a shortcut.

Report which screens you observed, any clipped/overlapping elements or unreadable
controls, any blocking gameplay issue, and new Console messages. Mark untested
items explicitly. The user's subsequent manual acceptance is recorded below.

## Portrait/world inspection - 2026-11-13

The branch/head/stash and generated asset inventory were rechecked. All 40 static
reference/structure checks passed again. The scene remains self-contained and
enabled exactly once in the build list. No production source or generated asset
was changed for this inspection.

A permitted hidden Unity batch process with Direct3D 11 rendered the copied
scene's actual camera into **540 x 960** textures. The isolated copy used a unique
company name for test saves, and the harness existed only under ignored
TestResults. Its final executed PlayMode render test passed **1/1**, with process
exit code zero. This is additional render evidence, not another execution of all
302 EditMode/26 PlayMode tests. Those earlier complete passing results remain
unchanged. Seven native camera images were inspected: before a run, each of the
four encounters, a current basic shot, and the world after victory.

**Observed in camera images:** platform, mounted placeholders and green health
bar are visible; basic/fast/boss sprites render with their expected colors and
shapes; the boss fits in frame; a cyan current-shot tracer is visible; the road
and scenery scroll; the post-victory world has no lingering tracer after its
normal lifetime. These are world-only images. Camera.Render does not capture the
IMGUI HUD, result overlay, upgrade/loadout UI or actual mouse interaction.
Those visual checks have not passed yet.

An initial accelerated capture retained old real-time tracers while advancing the
model faster than wall-clock time. The final harness waits for the production
0.12-second tracer lifetime before clean world captures and separately captures
a newly emitted shot. No gameplay/presentation source was changed to achieve
this. Initial images were retained locally rather than treated as a product bug.

### Findings and remaining presentation risks

- **Portrait configuration:** PlayerSettings has 540 x 960 defaults and Portrait
  orientation; the HUD scales from a 540-unit logical width. The Game view preset
  must still be selected manually as described above.
- **Camera math:** orthographic size 6.5 and 540/960 aspect give a 7.3125-unit
  horizontal span. With the 2.1-unit lead, visible X relative to the platform is
  approximately -1.556 to +5.756. The platform fits with a left margin; actual
  rendered images confirm its visibility. Combat sits toward the lower part of
  the image with substantial headroom. Judge that framing with the real HUD.
- **Right-edge arrivals:** later enemies in a wave start partially/outside the
  right edge; this is visible in the captures and follows the 4.4-unit spawn
  distance plus 0.6-unit spacing. The 6/7-unit basic/missile ranges also extend
  beyond the visible forward edge. Inspect whether offscreen firing or crowded
  enemy silhouettes are confusing; this has not been declared a blocker.
- **HUD bounds from code:** at full 540 x 960 safe area, the combat HUD occupies
  y=12..176, abandon y=184..224, feedback y=702..736 and purchases y=742..926.
  The result panel occupies approximately y=365..585. Between-run scroll content
  ends before the message/start controls. No obvious fixed-rectangle collision
  was established at this size, but font wrapping, contrast and scroll usability
  require the actual manual UI check.
- **Prototype feedback limitations:** tracers are resolved hitscan lines rather
  than moving projectiles, and mounted weapon blocks are not animated. Removing
  equipment does not alter the placeholder mounted artwork. Effective player
  stats come from progression/loadout snapshots; an unupgraded profile uses its
  100-ammunition base rather than the legacy 60 in the scene tuning asset.
- **No automatic technical blocker established:** native references, scene load,
  behavior and camera rendering work in the verified scope. This is not a visual
  acceptance of the complete game. Camera smoothness, all IMGUI screens and
  human interaction remain pending.

Render evidence remains local under
`TestResults/local-unity-6000.6/level-1-visual/`; it is test output and is excluded
from the proposed commit. The intended commit still contains only the generated
scene/prefabs/settings/art and all metadata, EditorBuildSettings and this report.
The unrelated ProjectSettings.asset rewrite, local workspace, caches, exports,
isolated saves and harnesses remain excluded.

## Manual acceptance and publication checkpoint - 2026-11-13

After receiving the manual checklist, the user reported **"everything seems
fine"** and instructed continuation and a push to GitHub. No blocking visual or
gameplay problem or new Console message was reported. This satisfies the manual
checkpoint for publishing these prototype assets. It is a user-reported result,
not independent observation of the Editor HUD by the agent; detailed per-screen
results and a separate defeat-screen observation were not supplied.

The publication selection contains **23 files**: the eight primary generated
assets, their file/folder metadata, EditorBuildSettings and this report. The
unrelated PlayerSettings whitespace/generated-console-field rewrite is excluded
and preserved locally. Test exports, camera captures, caches, isolated projects,
harnesses, saved profiles and the existing stash remain outside the commit.

The native generated asset bytes are retained from the tested/accepted scene.
No gameplay, tuning or camera asset was changed after the automated and manual
checks. Final reference/metadata, assembly, path/credential and Graphify checks
are repeated for publication; the full suites are not rerun merely for a report
update. PR #9 continues to target Task 007, preserving the existing dependency
chain. Android/device testing and target-platform save migration remain pending.
