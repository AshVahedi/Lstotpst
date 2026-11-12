# Local Unity 6000.6 verification

Started 2026-11-12. **Unity 6000.6.7f1 compilation/package resolution, all 302 Unity
EditMode tests (291 shared + 11 generator), and all 337 independent tests pass.
The initial persistence and generator failures are resolved. Persistent Level 1
generation, PlayMode and Android verification remain pending.** This local verification
uses the installed Unity 6000.6.7f1 at the user's request; the original Task 007
baseline targets 6000.3.0f1. This document records the local exception without
changing the historical Cloud verification reports or CI's original target.

## Preserved baseline

- Branch: `development/local-unity-6000.6-verification`.
- Base: `420337486fad15ad1c0527b251a27dc52f0f8a39` (complete Task 007 ancestry).
- Previous local work remains in the named stash
  `local-unity-6000.6.7f1-before-task007-2026-11-12`, commit
  `073fe26483c196d18c4a4bad1d3a1a5b98ad94ba`. It has not been applied or dropped.
- No PR merge, gameplay-rule edit, scene generation, or prefab generation is part
  of this verification. The persistence classification correction is described below.
- Ignored local evidence: `TestResults/local-unity-6000.6/`. The preparation
  baseline records SHA-256 hashes for 179 files under Assets, Packages and
  ProjectSettings, plus copies of the original version, manifest and player settings.

## Preparation changes

The only manual Unity configuration change before opening was `ProjectSettings/ProjectVersion.txt`:
`6000.3.0f1` became `6000.6.7f1`. Unity subsequently added its revision. The installed
executable reports `6000.6.7f1_b22767f04f04`.

The package manifest and all eight assembly definitions are unchanged before
import. The manifest requests Test Framework 1.4.5 and built-in IMGUI/Image
Conversion 1.0.0. Test assemblies retain their existing TestAssemblies references;
Core retains noEngineReferences and its existing assembly boundaries.

The previous ignored Library cache contains Test Framework 1.8.0 from the earlier
main-branch import. This is not evidence of successful Task 007 package resolution.
The old lockfile/settings remain in the stash and were not restored.

## Executed checks before import

- Metadata/layer validation: passed; 95 assets/folders, 30 engine-independent Core
  source files, 12 shared NUnit fixtures, eight assembly boundaries.
- Assembly/GUID audit: passed; eight asmdefs, zero asmrefs, zero errors. Reports
  expected notes for the absent package lockfile and absent generated scenes.
- Independent Python helper tests: 39 passed, zero failures/skips, initially using
  an existing Python 3.11.5 interpreter (these helpers use the standard library).
- Independent C# tests: initially blocked because global.json requires 8.0.4xx
  and the system lists only 10.0.403. Installed SDK 8.0.428 in ignored `.dotnet/`
  and restored the existing NuGet dependencies into ignored `.nuget/`; all 285
  tests passed, zero failures/skips. global.json and test dependencies are unchanged.
- Installed Python 3.12.16 under ignored `.dotnet/python/` and the frozen Graphify
  requirements in ignored `.venv-graphify/`, without updating global packages or
  installing Git hooks. All seven Graphify integration tests passed, zero failures/skips.

All **331 independent tests passed locally** (285 + 39 + 7). The C# results are
in `local-core-tests.trx`; Python helper output is in `python-helper-tests.txt`.

## First import and change record

The user opened this checkout with Unity 6000.6.7f1. Package resolution completed
and Unity entered Safe Mode on the initial compiler errors. Source inspection
and reflection over the actual resolved NUnit assembly confirmed:

- The Unity NUnit fork omits NonParallelizableAttribute. Removed that unsupported
  annotation from ProgressionAdapterTests; its isolation setup/teardown remains.
- A negated Does.Contain expression accepts strings in that fork, so enum
  assertions failed compilation. Replaced the negative collection assertions with
  Has.No.Member in both SimulationTests and GameplayIntegrationTests. The existing
  missile fallback regression cases retain the same checks; no test was disabled.

Recompilation completed with a successful build in Editor.log, all eight project
assemblies present, and zero compiler errors in the Console. Unity exited Safe
Mode and opened its normal workspace. Four CS0618 deprecation warnings remain in
ProgressionAdapterTests for FindObjectsSortMode and its FindObjectsByType overloads.
Gameplay sources and all assembly definitions are unchanged.

Unity resolved Test Framework **1.8.0** as a built-in package, with built-in NUnit
2.1.0, IMGUI 1.0.0, Image Conversion 1.0.0 and JSON Serialize 1.0.0. It initially
left the requested 1.4.5 manifest declaration unchanged. The local assembly audit
correctly flagged that manifest/lock mismatch. Updated only the Test Framework
manifest declaration to the actual 1.8.0, after observing resolution; Unity then
resolved again and the assembly/package audit passed with zero errors.

Unity expanded ProjectSettings.asset from serializedVersion 28 to 30, added the
Editor revision to ProjectVersion.txt, wrote **19 additional ProjectSettings
files**, and created Packages/packages-lock.json. No serialized Assets files or
metadata were changed by import. The three test-source fixes are manual edits.
The exact tracked patch, new settings/lockfile copies, original-file hashes,
changed-file inventory and assembly timestamps are stored in:

- `post-import-project.patch`
- `post-import-git-status.txt`
- `post-import-change-record.json`
- `editor-generated/`
- `post-import-static-audit.json`

Graphify's pinned native update and freshness check passed after the test/package
changes. A whole-tree `git diff --check` reports trailing spaces in Unity-written
empty YAML fields in ProjectSettings.asset; those generated settings were preserved.

## Initial Unity failures (resolved by the follow-up below)

Selected only LastOutpost.Core.EditModeTests in the real Test Runner, leaving the
four Editor generator tests unrun. **285 executed: 282 passed, three failed, zero
skipped/inconclusive.** The exported results are preserved as
`unity-core-editmode-initial.xml` in the local evidence directory.

All three failures are existing cases of
`ProgressionPersistenceTests.InvalidStoredCreditValuesAreRejectedWithoutReplacingTheFile`,
using `1.5`, `true` and `9223372036854775808` as Credits values. Unity's Mono
DataContractJsonSerializer wraps the XmlException/format/overflow error in
TargetInvocationException. JsonProgressionCodec.Decode at ProgressionData.cs:248
does not normalize that wrapper in its current catch filter, so the expected
ProgressionLoadException is not returned. The same cases pass under .NET 8.

This was an observed runtime save-error handling difference, not a compiler error.
At the first compile-only checkpoint, no persistence implementation was changed.
The initial failure XML remains available alongside the passing follow-up results.

PlayMode tests and the four Editor generator tests are **not run**, not passing
or skipped. Level 1 generation, manual play, Android build and device verification
remain pending. The existing stash and PRs are untouched.

## Persistence classification correction — 2026-11-12

The native exception chains were recorded by the same shared test cases in each
runtime, rather than inferred from the passing .NET result:

| Runtime | Cause retained inside ProgressionLoadException after the fix |
| --- | --- |
| .NET 8.0 / SDK 8.0.428 | SerializationException → XmlException → OverflowException or FormatException |
| Unity 6000.6.7f1 Mono | TargetInvocationException → XmlException → OverflowException or FormatException |

The XML reader rejects malformed Int64 Credits in both runtimes. Unity's serializer
invokes the numeric reader through reflection and retains its invocation wrapper;
.NET 8 reports a SerializationException. The previous decoder filter recognized
SerializationException, XmlException and FormatException, but not that wrapper.
Consequently Unity bypassed both the domain load exception and the controller's
normal recovery path. **Production error classification was incomplete; the tests'
expectation of ProgressionLoadException was correct.**

The only production change is JsonProgressionCodec.IsMalformedJson and its use in
the existing catch filter. It walks TargetInvocationException wrappers only, then
recognizes the same three existing parsing-error types. The thrown Corrupt load
exception retains the original wrapper/inner chain for diagnostics. It does not
blanket-catch reflection errors, inspect arbitrary inner exception chains, classify
I/O errors as corruption, or catch the domain UnsupportedSchema/InvalidValues
exceptions. Missing-save detection, DTO validation/quotas, migration, file stores,
backup/reset confirmation, atomic promotion and compare-and-swap are unchanged.

The existing four invalid-Credits cases now also assert the correct load reason:
the malformed numeric values are Corrupt, while the parsed negative value remains
InvalidValues. They check file preservation and record the native exception chains.
Six new shared regression cases cover three malformed Credits values through both
v1 migration and v2 decoding. Each checks startup/retry enters Recovery, never
exposes a default Credits balance, blocks runs/upgrades, requires reset confirmation,
and leaves primary and pending bytes unchanged without creating a backup/reset.
All test cases remain enabled. Existing unsupported-schema, corrupt/pending-only,
recovery-token conflict, atomic-write failure and stale-writer cases also passed.

The four deprecation warnings came from two host-counting calls using
FindObjectsSortMode and the deprecated FindObjectsByType overloads. A test-only
FindHosts helper uses the replacement overload behind UNITY_6000_6_OR_NEWER and
retains the original overload for older Editors. Host counts and inclusion of
inactive objects are unchanged; no test depends on order. This follows the
[Unity 6.6 API guidance](https://docs.unity.com/en-us/engine/6000.6/script-reference/unityengine/findobjectssortmode).
The new branch compiled in the installed Editor. The older-Editor branch was
preserved but was not executed with an older Editor in this session.

### Executed follow-up results

| Check | Passed | Failed | Skipped |
| --- | ---: | ---: | ---: |
| Independent NUnit / .NET 8 | 291 | 0 | 0 |
| Python validator/CI helpers / Python 3.12.16 | 39 | 0 | 0 |
| Graphify integration tests / pinned 0.9.104 | 7 | 0 | 0 |
| **Independent total: original 331 + six regressions** | **337** | **0** | **0** |
| Actual Unity shared EditMode | 291 | 0 | 0 |

Unity discovered 295 EditMode cases; only the 291-case shared assembly was selected.
The four generator cases were not executed, as explicitly requested for this pass.
No PlayMode test, scene/prefab generator, Android build or device test was run.
The final Console showed **zero errors and zero warnings** after successful
recompilation. Metadata/layer validation, the eight-assembly/package audit and
Graphify refresh/freshness checks passed.

Evidence is under `TestResults/local-unity-6000.6/persistence-fix/`:

- `core-tests.trx` — complete .NET run, including native parser diagnostics.
- `unity-core-editmode.xml` — exported actual Unity run and native parser diagnostics.
- `python-helper-tests.txt`, `graphify-tests.txt` — complete independent helper output.
- `static-audit.json`, `source-changes.patch` — audit and source-change evidence.
- `verification-summary.json`, `runtime-exception-comparison.json` — counts and native causes extracted from those result files.

**The persistence checkpoint passed and was ready for generator testing.** This means compilation,
package resolution and the shared persistence/core tests have passed on 6000.6.7f1;
it is not a claim that the generator, PlayMode gameplay or Android build has passed.
Generated output folders are still absent. The existing stash is preserved and no
PR was merged or restored. Task 008 was not started.

## Generator checkpoint — blocked, 2026-11-12

Executed only LastOutpost.EditorTests in the actual Unity 6000.6.7f1 EditMode
runner: **four executed, zero passed, four failed, zero skipped/inconclusive**.
No generator or gameplay code was changed and no assertion was disabled.

All four fail with the same runtime exception:

```text
System.InvalidOperationException:
Cannot create a new scene additively with an untitled scene unsaved.
```

The stack points to PrototypeSceneBuilder.BuildScene at
`Assets/Editor/PrototypeSceneBuilder.cs:165`, where a previously nonexistent
output scene is created using EditorSceneManager.NewScene with Additive mode.
The Editor has an Untitled scene open. The fixture does not establish a saved
workspace scene, and the generator has no early guard for that precondition.
This failure occurs before the idempotence/reference/rollback assertions can
validate their intended behavior, so generator compatibility is **not verified**.

Affected cases:

- BuildSceneRegistrationIsIdempotentAndPreservesExistingEntries.
- FailedSaveRestoresPreviousLoadedHierarchyAndLeavesDiskUnchanged(false).
- FailedSaveRestoresPreviousLoadedHierarchyAndLeavesDiskUnchanged(true).
- RegenerationPreservesSerializedReferencesSettingsGuidsAndUnrelatedRoots.

The generator wrote placeholder settings/art/four prefabs inside its dedicated
fixture folder before failing to create the scene. Fixture teardown removed its
owned temporary output. Both Assets/LastOutpostGeneratorTestOutput and
Assets/LastOutpostGenerated are absent afterward. The EditorBuildSettings asset
matches its captured pre-run bytes and still has an empty scene list. No Level 1
scene, persistent prototype prefab, or production configuration was generated.

Stopped at this blocker as explicitly requested. Full EditMode, PlayMode and
manual gameplay were not started in this pass. The previously executed 337
independent and 291 shared Unity passes remain the latest results for those
suites; they were not rerun or counted as current generator passes. No new source
compilation was required; the prior zero-error/zero-warning compilation result
is unchanged. This run reported runtime test failures, not compiler errors.

Evidence: `TestResults/local-unity-6000.6/scene-checkpoint/generator-initial.xml`,
with the four complete failure messages/stacks; the directory also contains the
pre-run build settings and Git inventory. No new generated assets are ready to
commit. Existing branch changes, the previous stash, schema-v2 persistence and
PRs are preserved. Android and Task 008 were not started.

Next checkpoint: resolve the saved-workspace precondition without discarding an
untitled scene or silently saving user work, and ensure the generator detects it
before asset writes; then rerun these four tests before generating Level 1.

## Scene-safety correction (subsequently verified below)

Computer Use is disabled for this project by user instruction. Future Editor UI
steps are manual; code edits, terminal tests, logs and exported XML remain usable.

Implemented changes in PrototypeSceneBuilder.cs:

- Check all loaded scenes for an unnamed path before any folders/imports/asset writes;
  require the user to save Untitled scenes manually, without saving or closing them.
- Require explicit confirmation before replacing an already loaded, dirty generated
  scene. Cancellation returns without changing hierarchy, files or build settings.
- Open the generated scene additively from the menu, preserving other open scenes.
- Save only the generator settings asset rather than globally saving dirty assets.
- Remove only a newly owned output tree after a failed first generation; never
  delete preexisting output. Retain the existing hierarchy rollback on save failure.

SceneBuilderTests.cs retains the original four reference/rollback cases and adds
seven cases for unnamed scenes, multiple saved scenes, unsaved edits, cancellation,
and first-save failure cleanup. The fixture tracks scene ownership and restores
the previous scene order, active scene and dirty flags without closing user scenes.

An initial 11-case rerun stopped in fixture setup: Unity Test Framework 1.8 creates
an unnamed bootstrap scene even when the user's workspace is saved. The subsequent
correction uses IPrebuildSetup/IPostBuildCleanup to save and clean only that
framework-owned bootstrap. It rejects unexpected scene count, path, dirty state or
root content. Unity's own runner restores the user's saved scene setup afterward.
The user manually saved LocalVerificationWorkspace.unity; the generator did not
save that scene. The bootstrap correction subsequently passed the targeted run.

Latest terminal evidence under `TestResults/local-unity-6000.6/generator-scene-safety/`:
291 NUnit + 39 Python helper + seven Graphify cases passed (337 total, zero
failures/skips). The previously pending Graphify run completed successfully.
Metadata/assembly validation passed, and the native Graphify snapshot was refreshed.
The Editor log records a successful compilation after the bootstrap change; that
is compilation evidence, not a passing Unity test run.

## Generator scene-safety results - 2026-11-12 17:58 UTC

The exported Unity result confirms **11 passed, zero failed, zero skipped** in
Unity 6000.6.7f1. This includes all four original reference-integrity, idempotence
and rollback cases plus seven new scene-safety cases. The run began at
17:57:53 UTC and ended at 17:58:00 UTC. Evidence was copied from the user's export
at Assets/Scenes/TestResults_20261112_125809.xml to
`TestResults/local-unity-6000.6/generator-scene-safety/unity-generator.xml`.
The source export and its Unity metadata remain untouched; they are verification
evidence, not generated gameplay assets to commit.

Root cause: Unity refuses additive creation of a normal scene while an unnamed
scene is open. The generator previously assumed a saved starting scene and only
reached this failure after writing assets. The original test fixture inherited
Unity Test Framework's unnamed bootstrap scene, so it also needed explicit
runner-owned workspace preparation. The fix guards production generation before
writes and independently prepares only the framework-owned bootstrap for tests.

Post-run checks passed: project metadata, all eight assembly definitions, and
Graphify freshness (77 code inputs, 59 C# files). Both test-owned folders and their
metadata were cleaned up. Assets/LastOutpostGenerated remains absent; build scenes
remain empty. No persistent Level 1 scene or prefabs have been generated. There
are no C# error/warning diagnostics in the inspected end-of-run Editor log; the
previous successful compilation remains the latest compilation evidence.

## Full EditMode verification complete - 2026-11-12 18:00 UTC

The full exported result confirms **302 passed, zero failed, zero skipped,
zero inconclusive**: LastOutpost.Core.EditModeTests.dll 291/291 and
LastOutpost.EditorTests.dll 11/11. The run began at 18:00:47 UTC and ended at
18:00:51 UTC (4.7912324 seconds). This rerun includes the four original generator
tests and all seven added regression cases; no tests were disabled or weakened.

The user's export at Assets/Scenes/TestResults_20261112_130101.xml was copied to
`TestResults/local-unity-6000.6/generator-scene-safety/unity-editmode-full.xml`.
The evidence directory contains both Unity XML exports, the 291-pass independent
NUnit TRX, 39-pass Python helper log, seven-pass Graphify log, assembly audit and
verification-summary.json with the exported XML hashes. All **337 independent
tests passed** in the already executed scene-safety verification run; these were
checked against the stored results rather than rerun after this Editor export.

Final checks passed again after the full Unity run: project metadata validation,
eight assembly definitions with zero errors, and Graphify freshness. The current
Editor log contains zero C# compiler error/warning diagnostics. Both temporary
test folders and their .meta files are absent. The production generated output is
also absent and the build scene list remains empty, as expected before Level 1
generation. No new compilation or test failures remain in the verified scope.

Files changed for this correction: Assets/Editor/PrototypeSceneBuilder.cs,
Assets/Tests/Editor/SceneBuilderTests.cs, and the portable graphify-out snapshot.
AGENTS.md records the user's permanent Computer Use prohibition; this report
records the evidence. Earlier persistence/import fixes remain in the working tree.
The user's LocalVerificationWorkspace scene and exported XML/.meta files were
preserved. The XML exports under Assets/Scenes are evidence, not gameplay content
to include in a commit. No commit, stash restoration or PR merge was performed.

The scene-safety checkpoint is complete: Level 1 generation is now ready for a
separate manual Editor checkpoint. Save every Untitled scene manually first; a
modified loaded generated scene requires explicit confirmation before replacement.
Other scenes remain open and are not automatically saved. Existing generated
output, if present at that future checkpoint, must be inspected before invoking
the command. Do not generate persistent Level 1, enter PlayMode, build Android,
restore the stash, or merge PRs in this pass. Computer Use remains disabled.

## GitHub milestone publication

The verification PR extends Task 007 directly: its review base is
`development/task-007-modular-equipment` at
`420337486fad15ad1c0527b251a27dc52f0f8a39`. Task 007 PR #8 still targets Task 006;
earlier PRs remain untouched. There is no merge, default-branch modification,
history rewrite or stash restoration in this publication.

Included configuration is the resolved package manifest/lock, Editor version,
expanded PlayerSettings and 14 portable settings assets: audio, physics (3D/2D),
graphics, input, memory defaults, navigation, presets, quality, tags/layers, time,
disabled Unity services, VFX defaults and visible-metadata version control. A
semantic comparison confirms every preexisting PlayerSettings value is retained
except the serialized format version (28 to 30). Empty YAML field whitespace was
normalized and an unused Unity-generated PS4 passcode was cleared before staging;
the original settings remain in ignored local evidence. This last packaging-only
sanitization was not followed by another Unity suite run; C# sources are unchanged
from the 302-case passing run. No console target build is claimed.

Narrow ignore rules preserve but exclude the local workspace scene and its
metadata, Test Runner XML exports and metadata, Package Manager UI preferences,
Project Auditor defaults, default scene-template preferences, and unused cluster,
multiplayer and optional physics-core settings. Existing cache/build/SDK/NuGet,
UserSettings and TestResults exclusions remain in place. No persistent generated
Level 1 assets are present. Raw XML/TRX/log evidence stays local; this report and
the XML hashes below preserve the reviewed milestone without machine paths.

| Export | Passed / total | SHA-256 |
| --- | ---: | --- |
| Targeted generator | 11 / 11 | `a2dac0a599fee352db67c1e653d2229f1ce17b201f00aa72fa393038bccc9a6b` |
| Full Unity EditMode | 302 / 302 | `d865fb52938fd6624b8a41f8473788ef3fbf9d2d94e1d23e23244da927f1a75d` |

Publication checks were executed again: **291 .NET NUnit + 39 Python helpers +
seven Graphify = 337 independent passes**, zero failures/skips. Project metadata,
eight assembly definitions, package consistency and syntax parsing of all 58 asset
C# files passed. Syntax parsing alone does not bind Unity APIs. Graphify is
refreshed separately after the implementation/configuration commit so its portable
snapshot records that source revision. Local publication evidence is under
`TestResults/local-unity-6000.6/publication/`.

The cloud Unity workflow remains pinned to 6000.3. Its activation prerequisite is
skipped only for pull requests whose head is this verification branch; dependent
Editor/Android jobs therefore also skip. This prevents the existing automatic
generation/PlayMode stages from exceeding the current checkpoint and avoids a
6000.3 import of this 6000.6 project. Other PRs and explicit manual dispatches keep
their existing behavior. Independent CI remains enabled. Cloud Unity validation
must not be reported as passed; CI target alignment and later Unity stages require
a separate checkpoint. No workflow was manually dispatched for this publication.
