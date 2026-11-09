# Version

**0.1.0** — foundation, Unity integration source and independently tested permanent
progression and Unity-facing lifecycle/UI/recovery wiring; actual Unity/Android
validation remains pending. Task 007 adds modular equipment and schema-v2 saves.

Task 002 adds runtime adapters, settings, automatic route/combat orchestration,
spawning, placeholder presentation, minimal UI, and reproducible Editor generation.
82 independent NUnit tests pass; 21 additional Unity-specific tests are authored
but unexecuted. Unity compilation, generated scenes/prefabs, and APK/device testing
have not run. Retain 0.1.0 until those integration gates justify 0.2.0.

Task 003 readiness audit retains this version: **READY WITH KNOWN LIMITATIONS**.
111 independent tests pass (82 gameplay, 7 Graphify, 22 static-validator cases).
See Documentation/UNITY_READINESS_REPORT.md and UNITY_FIRST_LAUNCH.md for evidence
and remaining Unity/Android gates. This is not a verified playable release.

Task 004 prepares licensed cloud Unity verification, with **128 independent
passes** and **CLOUD VERIFICATION BLOCKED** until activation/execution succeeds.
No Unity/gameplay code, balance or gameplay version was changed. See
Documentation/CLOUD_UNITY_VERIFICATION.md for current evidence and setup.

Task 005 retains 0.1.0 under the existing Unity verification gate. It adds separate
Credits, four permanent upgrades, durable run rewards, versioned JSON/memory/file
persistence and a Unity-neutral session boundary. **246 independent tests pass**
(200 NUnit + 39 Python validators/helpers + seven Graphify). The 200 shared tests
and 21 additional Unity-only cases have not executed in Unity. See
Documentation/PERMANENT_PROGRESSION.md and REPOSITORY_CONSOLIDATION.md.

Task 006 retains 0.1.0. Application-lifetime progression ownership, registered-run
adoption, between-run upgrades, saved reward results and confirmed backed-up
recovery are implemented. **274 independent tests pass** (228 C# + 39 helpers +
seven Graphify); 27 additional Unity-only cases remain unexecuted (four Editor,
23 PlayMode), as do the shared cases inside Unity. No Editor/Android claim is made.
See Documentation/UNITY_PROGRESSION_INTEGRATION.md.

Task 007 retains 0.1.0. Typed modular loadouts, immutable run equipment, minimal
Unity loadout controls and explicit schema-v1-to-v2 migration are implemented.
**331 independent tests pass** (285 C# + 39 helpers + seven Graphify). Three new
PlayMode cases bring the pending Unity inventory to 289 EditMode / 26 PlayMode;
no Unity compilation, generation or Android/device execution is claimed. See
Documentation/MODULAR_EQUIPMENT.md.
