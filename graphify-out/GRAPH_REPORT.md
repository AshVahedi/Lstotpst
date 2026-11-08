# Graph Report - Lstotpst  (2026-11-08)

## Corpus Check
- 61 files · ~45,509 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 26 file(s) not represented in the graph (top: (none) 16, .asmdef 8, .xml 1)
- WARNING: 26 of 107 files seen (24%) have no supported extractor and are missing from this graph entirely. It may look complete while representing only part of the codebase.

## Summary
- 1138 nodes · 3270 edges · 59 communities (38 shown, 21 thin omitted)
- Extraction: 72% EXTRACTED · 28% INFERRED · 0% AMBIGUOUS · INFERRED: 924 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `a8799756`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- PrototypeSceneBuilder.BuildScene
- LastOutpost.Core.Simulation
- system
- with
- GameRun
- PrototypeSettings.Build
- OutpostSession
- PlatformState
- steps
- LevelProgression
- Guard.NonNegative
- LastOutpost.Core.Tests.csproj
- manifest.json
- update.py
- with
- EconomyTests
- ProgressionTests
- ProgressionService
- StoreCommitResult
- UnityCiTests
- ProgressionConfiguration
- AssemblyAuditTests
- steps
- unity-verification.yml
- Target
- build_android
- android
- env
- editor
- WeaponConfig
- ProgressionData.cs
- Health
- ProgressionResult
- EnemyState
- ProgressionSnapshot
- ProgressionData
- SceneBuilderTests.cs
- ShotEvent
- PlatformState
- ProgressionTests.cs
- ProgressionLoadError
- RunStatus
- PrototypeContent
- LevelState
- Program.cs

## God Nodes (most connected - your core abstractions)
1. `GameRun` - 58 edges
2. `ProgressionService` - 47 edges
3. `ProgressionData` - 43 edges
4. `ProgressionTests` - 43 edges
5. `Health` - 42 edges
6. `PrototypeContent` - 39 edges
7. `Target` - 35 edges
8. `MemoryProgressionStore` - 34 edges
9. `ProgressionPersistenceTests` - 33 edges
10. `OutpostSession` - 32 edges

## Surprising Connections (you probably didn't know these)
- `Target` --references--> `Health`  [EXTRACTED]
  Assets/Scripts/Core/Combat/Targeting.cs → Assets/Scripts/Core/Health.cs
- `EnemyState` --references--> `Health`  [EXTRACTED]
  Assets/Scripts/Core/Enemies/EnemyState.cs → Assets/Scripts/Core/Health.cs
- `LevelProgression` --references--> `Health`  [EXTRACTED]
  Assets/Scripts/Core/Levels/LevelProgression.cs → Assets/Scripts/Core/Health.cs
- `PlatformState` --references--> `Health`  [EXTRACTED]
  Assets/Scripts/Core/Platform/PlatformState.cs → Assets/Scripts/Core/Health.cs
- `LevelTests` --references--> `Health`  [EXTRACTED]
  Assets/Tests/EditMode/LevelTests.cs → Assets/Scripts/Core/Health.cs

## Import Cycles
- None detected.

## Communities (59 total, 21 thin omitted)

### Community 0 - "PrototypeSceneBuilder.BuildScene"
Cohesion: 0.07
Nodes (5): PrototypeSceneBuilder, CameraFollow, CombatFeedback, ShotTrace, SceneBuilderTests

### Community 1 - "LastOutpost.Core.Simulation"
Cohesion: 0.20
Nodes (6): GeneratedLevelRoot, LastOutpost.Presentation, LastOutpost.UI, LastOutpost.Tests.PlayMode, LastOutpost.Gameplay, LastOutpost.Core.Simulation

### Community 2 - "system"
Cohesion: 0.17
Nodes (8): LastOutpost.Tests, LastOutpost.Core.Combat, LastOutpost.Core.Configuration, LastOutpost.Core.Modules, LastOutpost.Core, LastOutpost.Core.Economy, LastOutpost.Core.Levels, LastOutpost.Core.Enemies

### Community 3 - "with"
Cohesion: 0.06
Nodes (33): env, runs-on, steps, timeout-minutes, GRAPHIFY_BACKEND, GRAPHIFY_MAX_WORKERS, GRAPHIFY_OUT, GRAPHIFY_QUERY_LOG_DISABLE (+25 more)

### Community 4 - "GameRun"
Cohesion: 0.06
Nodes (29): GameRun, Configuration, ElapsedSeconds, Enemies, Level, Platform, Route, RunId (+21 more)

### Community 5 - "PrototypeSettings.Build"
Cohesion: 0.07
Nodes (31): EnemyDefinition, Attack, EnergyDrop, Id, IsBoss, MaximumHealth, MovementSpeed, LevelDefinition (+23 more)

### Community 6 - "OutpostSession"
Cohesion: 0.05
Nodes (22): PurchaseResult, CoolingDown, InsufficientEnergy, NoEffect, PlatformDestroyed, RunNotActive, Succeeded, EnemyActor (+14 more)

### Community 7 - "PlatformState"
Cohesion: 0.18
Nodes (14): ModuleDefinition, Id, Kind, ModuleKind, Armor, BasicGun, MissileLauncher, Reactor (+6 more)

### Community 8 - "steps"
Cohesion: 0.09
Nodes (21): runs-on, steps, timeout-minutes, jobs, core-tests, name, on, pull_request (+13 more)

### Community 9 - "LevelProgression"
Cohesion: 0.14
Nodes (9): LevelProgression, ActiveEnemies, BossActive, CurrentWave, Definition, Progress, RemainingEnemies, State (+1 more)

### Community 11 - "LastOutpost.Core.Tests.csproj"
Cohesion: 0.28
Nodes (6): netstandard2.1, Microsoft.NET.Test.Sdk (17.11.1), NUnit (3.14.0), NUnit3TestAdapter (4.6.0), Microsoft.NET.Sdk, net8.0

### Community 12 - "manifest.json"
Cohesion: 0.36
Nodes (7): dependencies, com.unity.modules.imageconversion@1.0.0, com.unity.modules.imgui@1.0.0, com.unity.test-framework@1.4.5, com.unity.modules.imageconversion, com.unity.modules.imgui, com.unity.test-framework

### Community 13 - "update.py"
Cohesion: 0.06
Nodes (19): main(), GraphifyWorkflowTests, command(), detect_code(), ensure_version(), main(), tool_environment(), update() (+11 more)

### Community 14 - "with"
Cohesion: 0.07
Nodes (29): with, allowDirtyBuild, androidExportType, androidTargetSdkVersion, androidVersionCode, artifactsPath, buildMethod, buildName (+21 more)

### Community 15 - "EconomyTests"
Cohesion: 0.17
Nodes (8): ResourceType, Energy, ResourceWallet, TacticalPurchases, Config, NextSpecialTimeSeconds, SpecialAttack, EconomyTests

### Community 17 - "ProgressionService"
Cohesion: 0.19
Nodes (11): ProgressionService, Configuration, RequiresReload, FileProgressionStore, HasPendingWrite, PendingPath, SavePath, IProgressionStore (+3 more)

### Community 18 - "StoreCommitResult"
Cohesion: 0.18
Nodes (3): StoreCommitResult, Committed, Conflict

### Community 20 - "ProgressionConfiguration"
Cohesion: 0.07
Nodes (28): IPlatformStatsProvider, PlatformStats, AmmunitionCapacity, DamageReduction, MaximumHealth, ModuleSlots, LevelRewardDefinition, LevelId (+20 more)

### Community 22 - "steps"
Cohesion: 0.24
Nodes (14): steps, steps, UNITY_EMAIL, UNITY_LICENSE, UNITY_LICENSE_SERVER, UNITY_PASSWORD, UNITY_SERIAL, steps (+6 more)

### Community 23 - "unity-verification.yml"
Cohesion: 0.17
Nodes (11): concurrency, cancel-in-progress, group, name, runs-on, timeout-minutes, jobs, infrastructure (+3 more)

### Community 24 - "Target"
Cohesion: 0.18
Nodes (12): Ammunition, Capacity, Count, Target, Distance, Health, Id, Weapon (+4 more)

### Community 25 - "build_android"
Cohesion: 0.22
Nodes (9): default, description, type, build_android, on, pull_request, workflow_dispatch, paths (+1 more)

### Community 26 - "android"
Cohesion: 0.33
Nodes (6): if, name, needs, runs-on, timeout-minutes, android

### Community 27 - "env"
Cohesion: 0.33
Nodes (6): env, ANDROID_IMAGE, EDITOR_IMAGE, GAME_CI_CLI_VERSION, OUTPOST_SOURCE_SHA, UNITY_VERSION

### Community 28 - "editor"
Cohesion: 0.40
Nodes (5): name, needs, runs-on, timeout-minutes, editor

### Community 29 - "WeaponConfig"
Cohesion: 0.13
Nodes (15): ClosestTargetStrategy, ITargetingStrategy, WeaponConfig, AmmunitionPerShot, AttackIntervalSeconds, Damage, Id, Range (+7 more)

### Community 30 - "ProgressionData.cs"
Cohesion: 0.13
Nodes (9): SavedRunOutcome, Abandoned, Active, Defeat, Victory, GuidRunIdGenerator, IRunIdGenerator, FixedId (+1 more)

### Community 31 - "Health"
Cohesion: 0.25
Nodes (7): Health, Current, DamageReduction, IsDead, Maximum, Missing, HealthTests

### Community 32 - "ProgressionResult"
Cohesion: 0.09
Nodes (19): ProgressionResult, AlreadyProcessed, Conflict, InsufficientCredits, InvalidRun, InvalidRunConfiguration, InvalidUpgrade, LevelLocked (+11 more)

### Community 33 - "EnemyState"
Cohesion: 0.20
Nodes (10): EnemyState, Attack, Definition, DropCollected, Health, Id, EnemyAgent, DefeatReported (+2 more)

### Community 34 - "ProgressionSnapshot"
Cohesion: 0.13
Nodes (8): ProgressionSnapshot, CompletedLevelIds, Credits, HighestUnlockedLevel, RegisteredRuns, Revision, RewardedRuns, Snapshot

### Community 35 - "ProgressionData"
Cohesion: 0.13
Nodes (9): IProgressionCodec, JsonProgressionCodec, ProgressionData, ProgressionLoadException, Reason, RunRecordData, SchemaHeader, UpgradeLevelsData (+1 more)

### Community 37 - "ShotEvent"
Cohesion: 0.20
Nodes (10): ShotEvent, FromX, Kind, TargetEnemyId, ToX, ShotKind, BasicGun, Enemy (+2 more)

### Community 38 - "PlatformState"
Cohesion: 0.25
Nodes (5): PlatformState, AdvancedAmmunition, Health, Modules, Resources

### Community 40 - "ProgressionLoadError"
Cohesion: 0.33
Nodes (6): ProgressionLoadError, Corrupt, InterruptedWrite, InvalidValues, StorageUnavailable, UnsupportedSchema

### Community 41 - "RunStatus"
Cohesion: 0.29
Nodes (5): RunStatus, Defeat, Playing, StartScreen, Victory

### Community 42 - "PrototypeContent"
Cohesion: 0.13
Nodes (14): PrototypeContent, BasicEnemy, BasicGun, Boss, FastEnemy, LevelOne, MissileLauncher, Purchases (+6 more)

### Community 43 - "LevelState"
Cohesion: 0.33
Nodes (6): LevelState, BetweenEncounters, Defeat, EncounterActive, Ready, Victory

## Knowledge Gaps
- **208 isolated node(s):** `Maximum`, `Current`, `IsDead`, `Missing`, `DamageReduction` (+203 more)
  These have ≤1 connection - possible missing edges. (Counts symbols only; 383 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **21 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `GameRun` connect `GameRun` to `ProgressionResult`, `PrototypeSceneBuilder.BuildScene`, `PlatformState`, `ProgressionTests.cs`, `OutpostSession`, `RunStatus`, `LevelProgression`, `ProgressionTests`?**
  _High betweenness centrality (0.086) - this node is a cross-community bridge._
- **Are the 13 inferred relationships involving `GameRun` (e.g. with `ProgressionService.CreateRun()` and `OutpostSession.ResetRun()`) actually correct?**
  _`GameRun` has 13 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Maximum`, `Current`, `IsDead` to the rest of the system?**
  _208 weakly-connected nodes found - possible missing edges._
- **Should `PrototypeSceneBuilder.BuildScene` be split into smaller, more focused modules?**
  _Cohesion score 0.06512890094979647 - nodes in this community are weakly interconnected._
- **Why does `EnemySpawner` connect `OutpostSession` to `PrototypeSceneBuilder.BuildScene`, `LastOutpost.Core.Simulation`, `GameRun`, `EconomyTests`?**
  _High betweenness centrality (0.034) - this node is a cross-community bridge._
- **Are the 25 inferred relationships involving `ProgressionService` (e.g. with `ProgressionPersistenceTests.AnotherInterruptedFirstWriteBlocksAnAlreadyLoadedServiceWithoutAnUnhandledException()` and `ProgressionPersistenceTests.CompareAndSwapRejectsStaleWritersWithoutLosingAnotherPurchase()`) actually correct?**
  _`ProgressionService` has 25 INFERRED edges - model-reasoned connections that need verification._
- **Should `with` be split into smaller, more focused modules?**
  _Cohesion score 0.058823529411764705 - nodes in this community are weakly interconnected._