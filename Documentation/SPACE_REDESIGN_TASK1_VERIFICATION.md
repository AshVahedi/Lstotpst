# Space redesign Task 1 verification

Date: 2026-11-13. Unity **6000.6.7f1**. Branch
`development/space-redesign-foundation`, based on verified commit
`4e2f53bba166718d353b0eb80efb25ae5a79ea01`. Its PR targets
`development/local-unity-6000.6-verification` (PR #9); earlier dependencies remain
unmerged. Design and architectural boundaries are in
[SPACE_REDESIGN_FOUNDATION.md](SPACE_REDESIGN_FOUNDATION.md).

## Delivered

- Separate `Assets/SpacePrototypeGenerated/Scenes/SpaceFoundation.unity` with
  ShipCore/Basic/Fast/Boss prefabs, existing prototype tuning, new ship geometry
  settings, placeholder sprite/material, star field and all metadata.
- Automatic +Y route projection shared by ship, enemy and shot presentation.
  The camera follows the hull without steering or a manual movement component.
- Four existing equipment slots mapped to two horizontal steps per side, with
  immutable geometry, deterministic non-overlap checks, symmetric/asymmetric
  bounds, connected rails and read-only runtime module presentation.
- Explicit portrait framing based on configured width/margins and actual aspect,
  rather than pixels or equipment-driven zoom. Default ship width is 5.2 units
  inside a 9-unit-wide reference view; the hull remains the camera anchor.
- Shared safe generation with saved-scene preflight, dirty-target cancellation,
  failure rollback and owned-output cleanup. Legacy aliases cannot be space output.
- Minimal HUD title/intro adaptation. Inventory, Credits, upgrades, save format,
  combat cadence, Energy and enemy AI remain owned by their existing systems.

## Executed results

| Suite/check | Passed | Failed | Skipped |
| --- | ---: | ---: | ---: |
| Independent shared NUnit / .NET 8 | 323 | 0 | 0 |
| Independent Python validator/CI helpers | 39 | 0 | 0 |
| Pinned Graphify integration tests | 7 | 0 | 0 |
| **Independent total** | **369** | **0** | **0** |
| Complete Unity EditMode | 350 | 0 | 0 |
| Final Unity PlayMode CLI, including native scene test | 35 | 0 | 0 |

The original **337 independent cases** are preserved: 32 new shared cases bring
the independent total to 369. EditMode contains 323 shared cases, the original 11
generator cases and 16 new space-generator cases. The versioned PlayMode inventory
is **34** (original 26 plus eight new space cases). The final 35-case command-line
run additionally includes one disposable real-scene/file-store compatibility test;
it is not a committed extra fixture. No existing assertion/test was disabled.

An initial 32-case PlayMode run had one new failure: unavailable modules were
hidden but the ship-view validation diagnostic was null. An unbraced inner slot
check captured an `else` intended for profile validation. Explicit branching fixed
both that omission and the risk of flagging a valid empty optional bay. Stronger
regressions now cover empty bays, corrupt profiles, retained unknown gear and an
unbound runtime view. Runtime missing profiles never substitute Editor preview
equipment. The final complete suite and EditMode rerun passed against the corrected
source; initial failure evidence is retained locally.

Generation, final EditMode and final PlayMode processes exited with code zero.
The final Unity logs have zero C# compiler error/warning diagnostics. Any cached
license token-refresh message is infrastructure output, not a successful cloud
activation or a C# compilation problem. Syntax-only parsing also passed for all
67 asset C# files; unlike the Unity runs, it does not establish API binding.

The publishable project selection validates **131 Unity assets/folders**, 31
engine-independent Core source files, 13 shared NUnit fixtures and eight assembly
boundaries. The raw workspace also contains ignored local scenes/test exports;
those are excluded from the publication audit and commit.

## Native scene and persistence evidence

The disposable project used a unique company name to isolate persistentDataPath.
Controlled schema-v2 data started with 700 Credits. The actual saved space scene
loaded with four modules and exactly one active generated mount hierarchy. Its
camera centered the hull at the 9/16 reference aspect. A 200-Credit health upgrade
and utility unequip left 500 Credits and three modules. Automatic progression
moved the ship along +Y with 110 health and no reactor bonus after unequipping it.

After unloading space and destroying its persistent host, the actual saved
IndustrialWasteland scene loaded the same file without rewriting it: 500 Credits,
the upgrade and selection were retained. Its default title and +X movement were
unchanged. Schema remained v2, with four owned items and three installations.
No layout state was added to the save and no migration was introduced. Separate
shared cases verify that resolving geometry does not change encoded profile bytes.

Direct3D11 Camera.Render produced actual 540x960 world images for symmetric,
asymmetric and forward-flight states. The images were inspected: the core remains
centered, modules are separated and connected, the optional module disappears,
and frontal enemies appear along +Y. These images do not capture IMGUI, human
input, camera smoothness or Android safe-area behavior. New-space manual visual
acceptance is not claimed or substituted with automated evidence.

## Preservation and publication

All legacy generated files, including IndustrialWasteland and its metadata, were
SHA-256 checked against the starting baseline and remained byte-identical. Its
build entry is retained; the space scene is appended once and enabled. New space
assets were generated through the shared safe workflow in the isolated project,
then copied into a previously absent new output directory. The real player save
was unchanged by asset copy; no real profile was opened as a writable test fixture.

The existing stash remains
`073fe26483c196d18c4a4bad1d3a1a5b98ad94ba`. The unrelated local PlayerSettings
rewrite is hash-preserved and excluded. Caches, raw XML/log/image evidence,
disposable drivers/projects and controlled saved profiles remain under ignored
`TestResults/space-redesign-task1/` and are not publication candidates.

The intended implementation commit contains source, tests, new space assets,
metadata, design/report, the added build entry and the exact-branch cloud guard.
The portable Graphify snapshot is recorded separately after that commit. Graphify
is code-only static evidence; it does not validate Unity assets or prove test runs.
Final native source-result hashes (raw evidence remains local):

| XML | SHA-256 |
| --- | --- |
| EditMode | `dfde1bc6abf07ac27fbc5ae324b35837bcf6c1d4d8be7db11babb21bd98535bd` |
| PlayMode, including native compatibility | `77e55fa575a915a2a9ff2bfecc981bdbbab7cd26e392adba9244ffeb73649396` |

No PR merge, force-push, stash restoration/deletion, Android build, new weapon
system or exploration implementation occurred.

## Ready for Task 2

1. `HorizontalShipLayout`, `ShipAttachmentPoint`, `ShipBounds` and stable slot IDs:
   deterministic horizontal topology and validation without inventory duplication.
2. `ShipAssemblyView.GetMount` / `IsInstalled`: actual mount transforms and
   equipment-driven visuals, with safe invalid-profile behavior.
3. `RouteProjection` / `RouteOrientation`: explicit +Y space and default +X legacy
   coordinate adapters across movement, enemy views, traces and camera following.
4. `SpaceShipSettings` / `ShipCameraFraming`: validated geometry and portrait policy.
5. `SpaceSceneBuilder`: separate reproducible output through the safe transaction.
6. Existing progression owner, immutable run equipment snapshots and schema-v2
   persistence for the next combat adapter.

Task 2 must supply actual 2D target positions, firing arcs and mounted weapon
behavior deliberately. Task 1 still projects the legacy front-only Level 1 combat
harness, retains encounter/boss movement modes, and fires from the logical hull
center. Energy removal, independent weapon channels/cooldowns, multidirectional
enemies, sectors/planets, full HUD redesign and Android/device checks remain pending.
