# Task 004 — cloud Unity verification

Date: **2026-11-08**. Gameplay version remains **0.1.0**.

## A. Environment

The Task 004 branch is `development/task-004-cloud-unity-verification`, based on
Task 003 commit `64e7475a591a1fab9d89572c931c03b16cf5727f`. PRs #1, #2 and #3 were
checked before implementation: all are open/unmerged. Dependencies are
Task 002 → Graphify → Task 003 → Task 004. This PR targets the Task 003 branch;
no outstanding PR is merged automatically.

| Item | Investigated/prepared configuration |
| --- | --- |
| Current workspace | Debian 13, no Unity/Hub/adb; about 6.5 GB free |
| Docker | Managed daemon 29.9.2/vfs responds; no Unity image pulled |
| GitHub runner | `ubuntu-24.04`, separate Editor and optional Android jobs |
| Editor | Exact **6000.3.0f1 / Unity 6.3 LTS**, unchanged |
| Installation | Existing GameCI container images, pinned by digest; no custom Unity installer |
| Actions | Unity Test Runner 4.4.0 and Unity Builder 6.0.0, pinned by commit SHA |
| GameCI CLI | **v0.1.72**, explicitly pinned instead of default latest |
| Licensing | Personal account, supported license file, Pro serial, or floating server; activation still required |
| Android modules | Exact Android image exists; module/toolchain execution remains unverified |

Docker Hub tag API checks actually returned these Linux amd64 images:

| Tag | Compressed size | Manifest digest |
| --- | ---: | --- |
| `unityci/editor:ubuntu-6000.3.0f1-base-3` | 5,151,635,130 bytes | `sha256:676dbc8866563961e5e3bc95e9356e282e4712e414092ab7e52e43f64576b6cd` |
| `unityci/editor:ubuntu-6000.3.0f1-linux-il2cpp-3` | 5,584,011,570 bytes | `sha256:54aaff42fb294bc2f6e206d2f96930b2dfe50dfb2656cccf014523b39b026c91` |
| `unityci/editor:ubuntu-6000.3.0f1-android-3` | 8,591,412,212 bytes | `sha256:34801f9cfeaa674b39db9984dd433ae25347d62d7ed6ed547acb48dfa89f291c` |

The `linux-mono` tag returned 404, so it was not substituted. Editor CI uses the
Linux IL2CPP image, including the Linux module needed by GameCI's native test
target and generation action. Upstream CLI source documents the base image as
insufficient for its test flow. This is supporting upstream evidence, not a
successful Last Outpost test. No alternate Editor version was selected.

## B. Feasibility

GitHub-hosted Linux runners with Docker and valid Unity activation can run the
documented GameCI batch-mode flow. Required operations are configured in
`.github/workflows/unity-verification.yml`:

1. Check the exact project version and activation configuration. Failure returns
   nonzero, publishes infrastructure JSON and prevents all Unity jobs.
2. Import/compile in Unity and execute EditMode, then require actual NUnit XML
   containing at least **86 executed, passing cases**, with no failures/skips.
3. Run the existing `LastOutpost.Editor.PrototypeSceneBuilder.GenerateBatch`
   through GameCI's custom method support. It calls the same generator as the
   menu; exceptions fail batch execution. Task 003 Undo rollback is unchanged.
4. Validate generated GUID references/package lock/build-scene registration and
   archive generated assets, metadata, imported settings and lockfile.
5. Execute PlayMode and require at least **17 executed, passing cases**.
6. Optionally build Android only after all required Editor stages succeed.

These jobs are **prepared, not yet proven by a licensed run**. The local
preflight actually failed with `CLOUD VERIFICATION BLOCKED`: no Unity activation
configuration is bound to this cloud environment. Repository secret-name listing
returned **403 Resource not accessible by integration**; that does not prove
repository secrets are absent. The workflow's own preflight determines whether
secrets are available to that run. The observed GitHub run will be recorded below.

### Storage, time and cost

The current workspace cannot practically hold the compressed Android image,
expanded image and Unity import/build outputs. No image was pulled here merely
to reach a known activation blocker. Container images need considerably more
space when expanded; the compressed sizes above are not installed sizes.

GitHub jobs use separate disposable runners so the Linux and Android images do
not accumulate on one machine. The Android job removes only the redundant
preinstalled **host** SDK at `/usr/local/lib/android`; it uses the container's
Unity-matched SDK/NDK/OpenJDK. It does not delete project files. Available disk
space is logged; disk exhaustion remains an infrastructure failure to investigate.
Only `Library` is cached, with distinct Editor/Android/version/content keys;
licenses, home folders and activation state are never cached or archived.

Editor and Android jobs each time out after 60 minutes. Initial multi-GB pulls,
package import and IL2CPP compilation can take many minutes; no measured Unity
duration exists for this project. Public-repository standard runners are generally
free; private-repository usage, larger/self-hosted runners, cache/artifact storage
and plan limits can incur charges. Check GitHub billing/usage before repeated
runs; no paid runner/service or larger plan was provisioned. Artifacts expire
after **7 days**. PR path filters avoid Unity jobs on documentation-only edits;
Android is opt-in and Unity runs are serialized to limit seat contention.

## C. Compilation

**Unity Editor has not executed. Compilation is UNEXECUTED, not passed or failed.**
No demonstrated Unity compiler defect exists to fix. No C# source was changed,
excluded from compilation, stubbed or suppressed in this task. The project still
contains `Assets`, `Packages`, `ProjectSettings`, the selected version/manifest,
eight validated asmdefs, real Unity tests and the batch-capable Editor generator.
The independent core compiles in .NET; all 38 asset C# files still pass syntax
parsing, without Unity API/type binding.

## D. Tests

Fresh independent results: **128 passed, 0 failed, 0 skipped**:

| Independent suite | Passed |
| --- | ---: |
| Gameplay NUnit (.NET, actual core) | 82 |
| Existing Graphify CLI integration | 7 |
| Task 003 assembly/GUID validator | 22 |
| Task 004 CI evidence/licensing/configuration helper | 17 |

The 17 new tests exercise credential-presence gates without printing values,
version refusal, executed NUnit leaf counts, discovery-only/failed/skipped/setup
failure rejection, generated-reference/build-scene checks, allowlisted archives,
Android settings and CI-only SDK pinning. Their XML/assets are **synthetic test
fixtures**, not Unity-generated evidence or Unity test passes.

**Actual Unity EditMode: 0 executed; no Unity pass/fail/skip counts available.**
**Actual Unity PlayMode: 0 executed; no Unity pass/fail/skip counts available.**
The authored inventory remains 86 EditMode (82 shared + four Editor generator
cases) and 17 PlayMode. There are 21 additional Unity-only cases unexecuted; the
82 shared cases have also not run inside Unity. A skipped CI job is not a skipped
test result. GameCI's headless Editor flow supplies its supported display setup;
headless passes would still not validate real touch, rendering quality or device
performance. Coverage instrumentation is disabled without removing tests or
adding a coverage package. Actionlint **1.7.12** validates the workflow; it does
not execute GameCI, activate Unity or prove Unity compatibility.

## E. Scene generation

**UNEXECUTED.** No real generated scene/prefab/settings/normalized ProjectSettings
or package lockfile has been produced. The existing menu and batch entry point
are reused without changes. The four Editor cases include repeat generation,
saved-reference/GUID preservation, build-list idempotence and both save-failure
rollback paths. They remain unexecuted until a licensed Editor run.

On success, the generated-project ZIP includes the generated root's `.meta`,
Level 1 scene, four prefabs, settings, placeholder art and their metadata,
`Packages/packages-lock.json` and imported ProjectSettings. It excludes Library,
activation directories, license files, keystores and tool caches. Review its
contents/diff before importing or committing; no workflow auto-commits assets.
Missing/broken references, a missing lockfile or duplicate/disabled scene entry
fails publication. Static reference checks do not replace Unity serialization tests.

## F. Android build

**UNEXECUTED. No APK was generated or device-tested.** Android is downstream of
successful Unity import/compilation, EditMode, generation and PlayMode gates.
Prepared configuration: portrait, `com.lastoutpost.prototype`, 0.1.0/code 1,
minimum API 26, ARM64, IL2CPP, `BuildOptions.Development`, APK, default development
signing; no keystore/signing secrets or store upload.

GameCI's pinned SDK bootstrap reads `AndroidTargetSdkVersion` before Unity starts
and otherwise attempts to install `platforms;android-0` for the project's automatic
setting. The helper validates existing Android intent and changes **only the
disposable Android CI checkout** to target **API 35**. The build input also selects
`AndroidApiLevel35`. The committed target remains automatic; no gameplay or
production policy changes are made. SDK installation/build logs must confirm this
works with the image. API 35 is a reproducible prototype choice, not a claim of
current store submission compliance. Unexpected settings/custom signing fail
validation rather than being silently replaced.

## G. Blockers and supported licensing setup

This environment has no Unity credentials or activated installation. The GitHub
integration cannot list/manage Actions secrets. **Do not send any credential,
license file or serial in chat, source, issue/PR text or logs.** A repository
administrator must configure the appropriate secrets directly at:

`AshVahedi/Lstotpst → Settings → Secrets and variables → Actions → New repository secret`

Choose one method appropriate to the account/seat, with permission to use it in CI:

| Method | GitHub Actions secrets |
| --- | --- |
| Personal account (pinned CLI licensing-client support) | `UNITY_EMAIL`, `UNITY_PASSWORD` |
| Pro serial | `UNITY_EMAIL`, `UNITY_PASSWORD`, `UNITY_SERIAL` |
| Existing valid supported `.ulf` file | `UNITY_LICENSE` (entire file contents); account credentials if required by that license's documented activation flow |
| Existing reachable floating license server | `UNITY_LICENSE_SERVER` (passed as GameCI `unityLicensingServer`) |

Do not configure conflicting file/serial/server methods. Secret presence is not
successful authentication or seat assignment. Personal-account login can be
blocked by authentication/2FA/account policies or seat availability; use Unity's
supported account/support process, not a bypass or disabled security control.
The current website's older Personal `.ulf` instructions are not treated as proof
that manual offline Personal activation is available. The pinned maintained CLI
supports Unity's licensing-client Personal account path; acceptance for this
account/exact Editor remains untested. A valid compatible entitlement is required.
GameCI handles activation and return; cancellation/crashes may still need seat
recovery through the account/administrator. Do not reuse production signing keys.

Supporting sources inspected:

- [Official GameCI activation guide source](https://github.com/game-ci/documentation/blob/main/docs/03-github/02-activation.mdx)
- [Pinned CLI activation implementation](https://github.com/game-ci/cli/blob/v0.1.72/dist/platforms/ubuntu/steps/activate.sh)
- [GameCI test runner documentation](https://github.com/game-ci/documentation/blob/main/docs/03-github/03-test-runner.mdx)
- [GameCI builder documentation](https://github.com/game-ci/documentation/blob/main/docs/03-github/04-builder.mdx)
- [Exact Docker Hub tags](https://hub.docker.com/r/unityci/editor/tags?name=6000.3.0f1)

The website returned HTTP 403 to this environment; its official GitHub sources
were read instead. No alternative unofficial activation service was used.

## H. Reproduction and artifacts

1. Check out this Task 004 branch or its reviewed successor. Configure only the
   supported license secrets in GitHub Settings, never in repository files.
2. Open/update the Task 004 PR with a relevant source/configuration change, or
   rerun the failed preflight/job after adding secrets. Fork PRs do not receive
   repository secrets and will be blocked. The workflow uses `pull_request`, not
   `pull_request_target`, read-only repository permissions, and the exact PR head.
3. Once the workflow exists on the default branch through the separately reviewed
   integration sequence, **Actions → Unity cloud verification → Run workflow**
   allows selecting the integrated branch and the **build_android** checkbox.
   GitHub's manual-dispatch discovery requires the workflow on the default branch;
   this task does not merge PRs merely to expose that button.
4. To request Android during PR verification before that, set the non-secret
   repository **Actions variable** `UNITY_BUILD_ANDROID` to `true`, then rerun the
   workflow. Leave it unset/false for Editor-only verification. After a failed
   prerequisite, use **Re-run all jobs** so downstream jobs are reevaluated.
5. Open the actual run and inspect failing step/job logs first. An activation,
   image pull, disk, SDK or registry failure is infrastructure; compiler
   diagnostics and NUnit failures are separate evidence. Never reclassify a
   failed required step as optional. No `continue-on-error` is used.

Expected artifact names (suffix is the checked-out **source SHA**, not a speculative
merge commit):

| Artifact | Contents / production condition |
| --- | --- |
| `unity-infrastructure-<sha>` | Safe preflight JSON; attempted even when activation configuration blocks |
| `unity-editor-evidence-<sha>` | Actual Unity test XML/logs, helper summaries, package/import logs when produced; attempted on Editor failure |
| `unity-generated-project-<sha>` | `generated-project.zip`, only after required Editor stages pass |
| `unity-android-evidence-<sha>` | Android configuration/logs when produced; attempted on Android failure |
| `last-outpost-android-dev-<sha>` | Nonempty APK plus source/version/configuration/SHA256 provenance; only after successful build |

Use the run's **Artifacts** section to download; retention is seven days. Full
GameCI/compiler/build console output is in **job logs → Download log archive**,
even when no Editor file log exists. No whole workspace/home/activation log
directory is uploaded. Only real generated files are published; absent outputs
are not fabricated. Downloading the APK does not mean it ran on a device.

Local helper/static reproduction (not Unity execution):

```sh
python3 -m unittest discover -s Tools/tests -v
python3 Tools/validate_project.py
python3 Tools/validate_assemblies.py --json TestResults/task-004-static.json
dotnet test Tests/LastOutpost.Core.Tests.csproj --configuration Release
.venv-graphify/bin/python -m unittest discover -s Tools/Graphify/tests -v
.venv-graphify/bin/python Tools/Graphify/update.py --check
```

Graphify remains pinned/development-only and was incrementally refreshed after
tool/workflow edits. No C# dependency correction was needed; no Unity/gameplay
source or balance changed. Existing workflows continue to run independent tests.

### Observed GitHub execution

Pending the first Task 004 PR run at initial configuration commit. This section
will be updated with the observed run, blockers and artifact names; configured
steps are not counted as executed Unity operations.

## I. Verification classification

**CLOUD VERIFICATION BLOCKED.** The current environment cannot execute Unity
reliably without supported activation and sufficient storage. The prepared
GitHub runner must still prove activation and every Unity operation; static and
independent test passes do not close those gaps. If Editor stages later pass
without Android, classify that actual run as **PARTIAL CLOUD VERIFICATION**.
Use **FULL CLOUD VERIFICATION** only after compilation, scene generation, required
Unity tests and Android development build actually succeed.

Even full cloud verification leaves physical-device installation, touch controls,
safe areas/aspect ratios, rendering, pause/resume, performance, memory and both
gameplay outcomes to validate. Follow [UNITY_FIRST_LAUNCH.md](UNITY_FIRST_LAUNCH.md)
and [UNITY_CHECKLIST.md](UNITY_CHECKLIST.md). Keep version 0.1.0 until evidence
justifies a separate milestone decision.
