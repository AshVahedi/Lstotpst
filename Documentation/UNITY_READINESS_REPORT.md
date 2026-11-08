# Task 003 — Unity readiness audit

Audit date: **2026-11-08**. Game version remains **0.1.0**.

## A. Executive summary

**READY WITH KNOWN LIMITATIONS.** The repository has a coherent Unity source
project and a tested pure C# simulation. No confirmed C# compilation blocker was
found through static review. This is readiness to attempt a real Unity import,
not a claim of a working imported project, scene, APK or device playthrough.

The cloud environment has .NET SDK 8.0.427 and the existing Graphify 0.9.104
installation. No Unity Editor was found on PATH or in the expected installation
locations, and no adb was found. Java alone is not a verified Android toolchain.
Unity package resolution, API compilation, asset serialization, Unity tests,
IL2CPP, APK creation and physical-device behavior remain unexecuted.

### Repository and integration base

Git/remote and GitHub PR metadata were checked before edits:

| Ref | Audited head | Contents/status |
| --- | --- | --- |
| `main` | `7002134dea9e4ca81d313b3c71c6b6821def948b` | Task 001 foundation/documentation; incomplete integration base |
| `development/task-002-unity-integration` | `c7c08455c4a4e7c185f37f1aad544d108efbd8b2` | Tasks 001–002 |
| `development/graphify-workflow` | `51443e57142b02247a10550f21754955f7a65858` | Tasks 001–002 plus Graphify |

[PR #1](https://github.com/AshVahedi/Lstotpst/pull/1) and
[PR #2](https://github.com/AshVahedi/Lstotpst/pull/2) were **open and unmerged**;
#2 targets the Task 002 branch. A speculative PR merge SHA does not mean merged.
Git ancestry checks confirmed the Task 001 implementation and Task 002 head are
ancestors of the Graphify head. The audit branch,
`development/task-003-unity-readiness-audit`, starts from that Graphify head and
targets `development/graphify-workflow`. No earlier PR was merged and no history
was overwritten. Review this as a stacked change; integrate the preceding PRs
in order when separately authorized.

## B. Verified components

### Project structure and package intent

- `Assets`, `Packages`, and `ProjectSettings` are present. The selected Editor is
  **Unity 6000.3.0f1 / Unity 6.3 LTS** from `ProjectVersion.txt`. No revision hash
  was invented. This exact Editor was not installed/executed here.
- `Packages/manifest.json` is valid JSON and requests Test Framework **1.4.5**,
  built-in IMGUI **1.0.0**, and Image Conversion **1.0.0**. No unrequested gameplay
  packages, runtime Graphify, render pipeline or asset-store dependency was added.
  `packages-lock.json` is absent; resolved/transitive versions are not verified.
- Project settings express 2D defaults, text serialization/visible metadata,
  540×960 portrait, development ID `com.lastoutpost.prototype`, version 0.1.0,
  minimum Android API 26, automatic target API, IL2CPP and ARM64. These are minimal
  serialized settings: Editor interpretation/default migration must be checked.
- Existing metadata validation passes for **73 assets/folders**, with unique
  GUIDs/no missing or orphan metadata. No generated metadata was fabricated.
  Core has **18 source files** without Unity references and compiles independently.

### Assemblies and namespaces

Eight `.asmdef` files and zero `.asmref` files were audited independently of
Graphify, including name/GUID resolution, cycles, platform restrictions and
runtime/Editor/test boundaries. All current references resolve to project
assemblies. No assembly cycle or unavailable explicit package assembly was found.

| Assembly | References | Restriction |
| --- | --- | --- |
| `LastOutpost.Core` | None | `noEngineReferences: true` |
| `LastOutpost.Gameplay` | Core | Runtime |
| `LastOutpost.Presentation` | Core, Gameplay | Runtime |
| `LastOutpost.UI` | Core, Gameplay | Runtime |
| `LastOutpost.Editor` | Core, Gameplay, Presentation, UI | Editor only |
| `LastOutpost.Core.EditModeTests` | Core | Editor, test assembly |
| `LastOutpost.EditorTests` | Core, Gameplay, Presentation, UI, Editor | Editor, test assembly |
| `LastOutpost.PlayModeTests` | Core, Gameplay, Presentation, UI | Unity test build rules |

The test assemblies use `optionalUnityReferences: ["TestAssemblies"]`, with
`autoReferenced: false`. Unity's public `CustomScriptAssembly` loader still
migrates this legacy field into the runner/NUnit references and
`UNITY_INCLUDE_TESTS`; it is not evidence of a broken reference. Actual test
discovery/player exclusion requires the chosen Editor/package combination.
[Loader source inspected](https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/Scripting/ScriptCompilation/CustomScriptAssembly.cs)
is supporting static evidence from upstream, not a compile against 6000.3.0f1.

Runtime sources do not import UnityEditor; Core imports no Unity APIs. Folder
assembly ownership was reviewed, including nested Core/Presentation/UI asmdefs
under the Gameplay root and Editor/test assemblies outside it. The graph's
62 class-like asset declarations had no duplicate namespace/name entries;
direct source review and syntax parsing found no obvious duplicate definitions.
This inventory does not replace symbol binding against Unity assemblies.

### Scene generation and references

There are **no committed `.unity`, `.prefab`, generated settings `.asset`, sprite
or material outputs**. Consequently there are no generated scene/prefab GUID
references to validate yet, and `EditorBuildSettings.m_Scenes` is empty.
This is a source scaffold, not an Editor-imported and serialized prototype.

`PrototypeSceneBuilder.Generate` was traced through settings validation, sprite
import, material creation, prefab saving and scene construction. It is expected
to create exactly four prefabs and the single scene at
`Assets/LastOutpostGenerated/Scenes/IndustrialWasteland.unity`:

| Requirement | Source wiring |
| --- | --- |
| Platform | Instantiated platform prefab; motor targets platform transform |
| Basic/Fast/boss | Three actor prefabs assigned to spawner; health bars configured |
| Core level/resources | Session creates GameRun with validated settings, level and wallet |
| HUD and outcome views | One PrototypeHud session reference; start/playing/victory/defeat branches |
| Camera | Orthographic Main Camera, size 6.5; follow target, X lead 2.1 |
| Firing feedback | CombatFeedback has session/material/effect root; temporary visual traces |

There is deliberately no physical projectile prefab, Canvas, EventSystem or
separate menu scene. The documented mechanism is immediate-hit core damage with
visual-only traces, and the UI uses the built-in IMGUI module. Resource/wave
rules are composed in the session's `GameRun`, not separate manager objects.

Generation uses stable paths, preserves the existing settings asset, rebuilds
only roots marked `GeneratedLevelRoot`, preserves unrelated roots and registers
one enabled global build entry without removing unrelated entries. Existing
tests reopen the saved scene to check actual serialized references and stable
GUIDs, but those tests have not executed. The new rollback protects scene
hierarchy replacement on save failure; prefab/settings writes are not a single
atomic disk transaction. Run the menu twice and execute the Editor tests locally.

### Actual gameplay execution path, reviewed at source level

1. Scene settings feed `OutpostSession.Awake → ResetRun → PrototypeSettings.Build
   → GameRun`. Configuration builds before replacing an existing run. HUD
   **START RUN** calls `BeginRun → GameRun.Start`; start-screen ticks are inert.
2. Session `FixedUpdate → StepSimulation(Time.fixedDeltaTime) → GameRun.Tick`
   drives the pure model. Route modes cover travel, encounter slowdown, boss
   stop and terminal stop. `PlatformMotor.LateUpdate` copies route X; the later
   camera update follows the rendered platform. There is no steering dependency.
3. `GameRun.BeginEncounter` uses the level's ordered roster after its travel
   delay. `EnemySpawned → EnemySpawner.Spawn → EnemyActor.Bind` supplies views
   for authoritative agents. Subscribe-plus-bind handles session initialization
   order and re-enable; prefab references and route origins are explicitly set.
4. Agents approach; `RunCombat.Tick → Weapon.TryFire → ClosestTargetStrategy`
   checks live targets, range and attack time. Basic fire uses unlimited ammo;
   missile fire checks the mounted module and consumes advanced ammunition.
   Enemy attacks use their configured schedules/ranges. Dead targets cannot
   fire or receive another lethal-shot reward.
5. Core damage is applied once before `ShotFired`. `CombatFeedback` only renders
   the already-resolved shot; `ShotTrace` lifetime/destruction cannot damage.
   `ResolveDefeats → TryCollectDrop` grants each reward once and notifies the
   spawner, which disables/destroys the view. `OnDestroy` never grants Energy.
6. Level refresh waits for living roster completion, then three ordinary waves
   precede one boss. Boss death yields victory only while the platform lives;
   platform death wins over simultaneous boss death. `Finish` latches the result,
   stops route/combat and publishes once.
7. HUD polls authoritative health/Energy/ammo/wave/progress. Purchase methods
   call session commands, then GameRun/TacticalPurchases validation. The UI cannot
   mutate wallets/health/ammo directly. Initial costs/effects remain configurable
   (30/+20 ammo, 50/+25 health, 80/special). Paused/inactive/terminal state, funds,
   capacity, dead platform, target and optional cooldown rules are enforced below
   presentation. Restart replaces the run and rebinds views/events.

The independent simulation tests exercise this model path, including a complete
default run with enemy attacks and purchases. MonoBehaviour callback order,
serialized references, rendering and actual touches remain Unity-only checks.

### Graphify findings

Used the existing project venv/pinned **graphifyy 0.9.104**, scoped explain/path/
affected queries and native incremental update. No reinstall, model backend or
Unity dependency was introduced. Updated artifacts contain **750 nodes**, **2,144
relationships**, **27 communities**, **52 code inputs** and all **39 C# files**
(38 asset sources plus the standalone syntax checker). Freshness validation
compares native content hashes, not just the graph's parent-commit build stamp.

- High-degree hubs: `GameRun` (48), `PrototypeContent` (39), `Health` (39),
  `Target` (35), `OutpostSession` (32), `Weapon` (25). Changes here can propagate
  through multiple gameplay systems; explain/affected queries identify tests
  and adapters to review before changes.
- The qualified undirected path from PrototypeHud to TacticalPurchases is
  `PrototypeHud → OutpostSession → GameRun → TacticalPurchases`, three EXTRACTED
  reference hops. Direct source tracing confirms command routing.
- No import cycles are reported, and no mapped outgoing Core-to-Unity-adapter
  dependency was found. Separate assembly validation confirms the allowed
  direction; neither result proves all dynamic execution relationships.
- Low-degree class candidates reviewed: `IPlatformStatsProvider` (3) is the
  future permanent-stat seam already consumed by configuration; `GeneratedLevelRoot`
  (2) is used by the generator for ownership. They are not deletion candidates.
  Low-degree property/enum nodes do not establish unused code. No classes were
  removed based on a graph heuristic.
- **25 unsupported inputs**, including all eight asmdefs, remain explicitly
  outside Graphify's analysis. Snapshot links include inferred relationships;
  no assembly or serialization correctness was inferred from them. HTML still
  requires its documented vis-network CDN; local JSON/report queries work offline.

The generated [report](../graphify-out/GRAPH_REPORT.md),
[JSON](../graphify-out/graph.json), [HTML](../graphify-out/graph.html) and native
manifest are preserved. The existing graph artifact workflow remains code-only.

## C. Identified issues

| Severity | Finding | Disposition |
| --- | --- | --- |
| Critical | No confirmed compilation/launch defect found by available checks | Unity compilation remains unverified; absence of a finding is not proof |
| High | No serialized gameplay scene/prefabs/settings and no enabled build scene | Intentional first-launch gate; run generator in real Unity before play/build |
| High | Checking out main or assuming PR #2 merged omits completed integration | Correct base verified; dedicated stacked branch and first-launch instructions |
| Medium | Generator removed old roots before a potentially failing save, then removed replacement in catch | Scoped Undo rollback added; two Unity regression cases authored, unexecuted |
| Medium | Previous static assembly checker could miss cycles, asmref/GUID, platform and test leakage errors | Independent validator and 22 real mutation tests added to CI |
| Low | No resolved package lock; minimal settings have not been normalized by Editor | Explicit import/commit checklist; no synthetic metadata or lockfile |
| Low | Graph query short names can become ambiguous after updates | Documented qualified path command, rerun successfully |

## D. Corrections implemented

- Generator now places only the completed root replacement/removal in a dedicated
  Undo group, restores the previous hierarchy on failed/thrown save, and leaves
  build-scene registration after successful persistence. Removed a redundant
  same-scene move after selecting the active scene. An optional Editor save
  callback lets automation exercise both failure paths without disk corruption.
- Added stdlib-only `Tools/validate_assemblies.py` and fixtures that actually mutate
  references, GUIDs, graph cycles, platform restrictions, package manifests/locks
  and serialized references. The script returns nonzero on errors and writes
  machine-readable JSON evidence. It inspects project-local GUIDs; future package
  asset GUIDs need resolved package-cache/Editor review. Non-text assets are flagged
  as uninspected, not counted as validated references.
- Core CI runs those tests and the audit and publishes JSON alongside real TRX.
  Graph CI also triggers on `.asmref` edits. Neither workflow claims Unity tests.
- Updated README, checklist, version status and this first-launch/audit record;
  refreshed existing Graphify artifacts without changing gameplay rules/version.

## E. Remaining risks and Android readiness

The project is **not yet verified ready to build an APK**. No production signing
was configured. Editor import must confirm module/package availability, asmdef
test discovery/exclusion, all Unity API signatures, scene serialization and built-in
shader availability. Generated asset GUIDs and internal serialized fileIDs cannot
be checked before generation. The new rollback uses actual Unity Undo APIs and
requires its Editor tests; it has only passed C# syntax parsing here.

Android intent is portrait/API 26/automatic target/IL2CPP/ARM64. Confirm these in
Player Settings, use Hub's matching Android modules, select an Android Build
Profile and include the generated scene. Profile scene-list overrides must be
checked separately from the global scene list. Verify .NET Standard 2.1 API
compatibility, actual target SDK installation, AOT/stripping, shader inclusion and
Gradle packaging through a real build. Automatic target API is not a store-policy
claim. No Android SDK/NDK/license, IL2CPP output, APK or device was validated here.

HUD uses `Screen.safeArea`, a width-scaled 540-unit layout, and IMGUI buttons;
no mouse-only gameplay input is required. Touch size/readability, portrait aspect
ratios, notches, frame rate, memory and pause/resume need real-device verification.
Minimal art/balance and source-level initialization checks cannot predict those
results. Do not promote the game to 0.2.0 until actual validation justifies it.

## F. Test results

Fresh runs, not the historical 89-test claim:

| Suite | Executed | Passed | Failed | Skipped |
| --- | ---: | ---: | ---: | ---: |
| Shared gameplay NUnit, .NET 8 Release / Core netstandard2.1 | 82 | 82 | 0 | 0 |
| Existing Graphify CLI integration tests | 7 | 7 | 0 | 0 |
| New independent assembly/GUID validator tests | 22 | 22 | 0 | 0 |
| **Total unique automated cases** | **111** | **111** | **0** | **0** |

The baseline 82 and 7 were also run before changes; repeat runs are not additional
unique cases. The metadata/layer check and independent assembly audit both pass
(8 definitions, 0 asmrefs, 0 errors). Roslyn parses **38 asset C# sources, 0 syntax
errors**, with **no Unity API/type resolution**. Graph freshness/artifact validation
passes. No tests were marked skipped to disguise missing Unity execution.

Reproduce from the repository root:

```sh
dotnet restore Tests/LastOutpost.Core.Tests.csproj
dotnet test Tests/LastOutpost.Core.Tests.csproj --no-restore --configuration Release --logger "trx;LogFileName=task-003-core.trx" --results-directory TestResults
python3 Tools/validate_project.py
python3 -m unittest discover -s Tools/tests -v
python3 Tools/validate_assemblies.py --json TestResults/unity-static-audit.json
dotnet run --project Tools/UnitySyntaxCheck/UnitySyntaxCheck.csproj --configuration Release -- .
.venv-graphify/bin/python -m unittest discover -s Tools/Graphify/tests -v
.venv-graphify/bin/python Tools/Graphify/update.py --check
```

Actual local TRX/JSON are generated under ignored `TestResults/`; CI uploads fresh
results. The cloud SDK is installed under `/tmp/last-outpost-dotnet`, with temporary
CLI/NuGet paths. Its sandbox mutex restriction required the approved execution
mechanism; real SDK/packages were used, without Unity stubs.

**Unity execution counts: zero.** There are **21 additional Unity-only cases**
unexecuted: 17 PlayMode and four Editor cases (two existing plus two new rollback
cases). The shared 82 also have not run inside Unity, so the expected Unity Test
Runner inventory is **103 cases: 86 EditMode, 17 PlayMode**. These are not skipped
or passing Unity results. Unity graphical tests, APK build and device tests are
pending. CI run URLs/status for the pushed revision are recorded on the audit PR;
local results do not stand in for observed GitHub Actions execution.

## G. First-launch checklist

- [ ] Clone the integrated audit branch; install 6000.3.0f1 and Android modules.
- [ ] Import/resolve packages; zero Console compile/import errors; review lock/settings.
- [ ] Generate Level 1 twice; inspect assets, references, owned roots and build list.
- [ ] Execute actual Unity EditMode and PlayMode tests; preserve XML and logs.
- [ ] Play all purchase, wave, boss, defeat, victory, restart and pause paths.
- [ ] Check safe areas/aspect ratios; verify Android profile/player/toolchain settings.
- [ ] Build a development APK and test ARM64 hardware with touch and log capture.
- [ ] Commit verified generated assets/metas/lock/settings; rerun CI after fixes.

Exact commands, menu paths and troubleshooting are in
[UNITY_FIRST_LAUNCH.md](UNITY_FIRST_LAUNCH.md), with the detailed
[manual gameplay checklist](UNITY_CHECKLIST.md).

## H. Readiness verdict

**READY WITH KNOWN LIMITATIONS** for first Unity testing. The independent model
and static structure checks pass, and no confirmed unresolved source blocker was
found. Scene generation, package resolution, Unity compilation/tests and Android
build/device behavior are required acceptance gates and remain unverified.
The next task is actual Editor validation and fixes based on its logs, followed
by a development APK/device pass—not additional gameplay features.
