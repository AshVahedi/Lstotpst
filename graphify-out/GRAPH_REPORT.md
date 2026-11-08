# Graph Report - Lstotpst  (2026-11-08)

## Corpus Check
- 61 files · ~45,509 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 26 file(s) not represented in the graph (top: (none) 16, .asmdef 8, .xml 1)
- WARNING: 26 of 107 files seen (24%) have no supported extractor and are missing from this graph entirely. It may look complete while representing only part of the codebase.

## Summary
- 1140 nodes · 3189 edges · 44 communities (37 shown, 7 thin omitted)
- Extraction: 74% EXTRACTED · 26% INFERRED · 0% AMBIGUOUS · INFERRED: 845 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `c6440ba2`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- PrototypeSceneBuilder.BuildScene
- OutpostSession
- system
- with
- GameRun
- PrototypeSettings.Build
- GameplayIntegrationTests
- PlatformState
- steps
- LevelProgression
- Guard.NonNegative
- LastOutpost.Core.Tests.csproj
- manifest.json
- update.py
- with
- EnemySpawner
- Test
- PurchaseResult
- StoreCommitResult
- UnityCiTests
- ProgressionConfiguration
- AssemblyAuditTests
- steps
- unity-verification.yml
- PrototypeContent
- build_android
- android
- env
- editor
- WeaponConfig
- UpgradeKind
- UpgradeDefinition
- ProgressionResult
- RunConfiguration
- ProgressionSnapshot
- ProgressionData.Validate
- SpecialAttackConfig
- PrototypeHud
- PrototypeSettings
- PlatformStats
- ProgressionLoadError
- RunStatus
- TacticalPurchaseConfig
- PlatformMotor

## God Nodes (most connected - your core abstractions)
1. `GameRun` - 50 edges
2. `ProgressionService` - 47 edges
3. `ProgressionTests` - 43 edges
4. `PrototypeContent` - 39 edges
5. `Health` - 38 edges
6. `Target` - 35 edges
7. `ProgressionPersistenceTests` - 33 edges
8. `OutpostSession` - 32 edges
9. `with` - 31 edges
10. `GameplayIntegrationTests` - 31 edges

## Surprising Connections (you probably didn't know these)
- `CombatFeedback` --references--> `GameRun`  [EXTRACTED]
  Assets/Scripts/Presentation/CombatFeedback.cs → Assets/Scripts/Core/Simulation/GameRun.cs
- `CombatFeedback` --references--> `OutpostSession`  [EXTRACTED]
  Assets/Scripts/Presentation/CombatFeedback.cs → Assets/Scripts/Levels/OutpostSession.cs
- `TacticalPurchases` --references--> `PlatformState`  [EXTRACTED]
  Assets/Scripts/Core/Economy/TacticalPurchases.cs → Assets/Scripts/Core/Platform/PlatformState.cs
- `EconomyTests` --references--> `PlatformState`  [EXTRACTED]
  Assets/Tests/EditMode/EconomyTests.cs → Assets/Scripts/Core/Platform/PlatformState.cs
- `LevelProgression` --references--> `EnemyState`  [EXTRACTED]
  Assets/Scripts/Core/Levels/LevelProgression.cs → Assets/Scripts/Core/Enemies/EnemyState.cs

## Import Cycles
- None detected.

## Communities (44 total, 7 thin omitted)

### Community 0 - "PrototypeSceneBuilder.BuildScene"
Cohesion: 0.07
Nodes (5): PrototypeSceneBuilder, CameraFollow, CombatFeedback, ShotTrace, SceneBuilderTests

### Community 1 - "OutpostSession"
Cohesion: 0.15
Nodes (6): GeneratedLevelRoot, OutpostSession, CanCommand, IsPaused, Run, HealthBarView

### Community 2 - "system"
Cohesion: 0.05
Nodes (33): SavedRunOutcome, Abandoned, Active, Defeat, Victory, ShotEvent, FromX, Kind (+25 more)

### Community 3 - "with"
Cohesion: 0.06
Nodes (33): env, runs-on, steps, timeout-minutes, GRAPHIFY_BACKEND, GRAPHIFY_MAX_WORKERS, GRAPHIFY_OUT, GRAPHIFY_QUERY_LOG_DISABLE (+25 more)

### Community 4 - "GameRun"
Cohesion: 0.07
Nodes (28): GameRun, Configuration, ElapsedSeconds, Enemies, Level, Platform, Route, RunId (+20 more)

### Community 5 - "PrototypeSettings.Build"
Cohesion: 0.20
Nodes (12): LevelDefinition, DisplayName, Encounters, Id, SpawnGroup, Count, Enemy, WaveDefinition (+4 more)

### Community 7 - "PlatformState"
Cohesion: 0.09
Nodes (19): ModuleDefinition, Id, Kind, ModuleKind, Armor, BasicGun, MissileLauncher, Reactor (+11 more)

### Community 8 - "steps"
Cohesion: 0.09
Nodes (21): runs-on, steps, timeout-minutes, jobs, core-tests, name, on, pull_request (+13 more)

### Community 9 - "LevelProgression"
Cohesion: 0.12
Nodes (15): LevelProgression, ActiveEnemies, BossActive, CurrentWave, Definition, Progress, RemainingEnemies, State (+7 more)

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

### Community 15 - "EnemySpawner"
Cohesion: 0.22
Nodes (4): EnemyActor, Agent, EnemySpawner, ActiveCount

### Community 16 - "Test"
Cohesion: 0.05
Nodes (24): PermanentStatsProvider, WeaponMultiplier, IProgressionCodec, JsonProgressionCodec, ProgressionData, RunRecordData, SchemaHeader, GuidRunIdGenerator (+16 more)

### Community 17 - "PurchaseResult"
Cohesion: 0.17
Nodes (7): PurchaseResult, CoolingDown, InsufficientEnergy, NoEffect, PlatformDestroyed, RunNotActive, Succeeded

### Community 18 - "StoreCommitResult"
Cohesion: 0.20
Nodes (7): FileProgressionStore, HasPendingWrite, PendingPath, SavePath, StoreCommitResult, Committed, Conflict

### Community 20 - "ProgressionConfiguration"
Cohesion: 0.23
Nodes (9): LevelRewardDefinition, LevelId, LevelNumber, VictoryCredits, ProgressionConfiguration, BasePlatform, BaseWeaponMultiplier, MaximumDamageReduction (+1 more)

### Community 22 - "steps"
Cohesion: 0.24
Nodes (14): steps, steps, UNITY_EMAIL, UNITY_LICENSE, UNITY_LICENSE_SERVER, UNITY_PASSWORD, UNITY_SERIAL, steps (+6 more)

### Community 23 - "unity-verification.yml"
Cohesion: 0.17
Nodes (11): concurrency, cancel-in-progress, group, name, runs-on, timeout-minutes, jobs, infrastructure (+3 more)

### Community 24 - "PrototypeContent"
Cohesion: 0.06
Nodes (49): Ammunition, Capacity, Count, ClosestTargetStrategy, ITargetingStrategy, Target, Distance, Health (+41 more)

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
Cohesion: 0.14
Nodes (14): WeaponConfig, AmmunitionPerShot, AttackIntervalSeconds, Damage, Id, Range, Targeting, EnemyDefinition (+6 more)

### Community 30 - "UpgradeKind"
Cohesion: 0.29
Nodes (5): UpgradeKind, AmmunitionCapacity, Armor, MaximumHealth, WeaponDamage

### Community 31 - "UpgradeDefinition"
Cohesion: 0.27
Nodes (6): UpgradeDefinition, BaseCost, GrowthFactor, IncreasePerLevel, Kind, MaximumLevel

### Community 32 - "ProgressionResult"
Cohesion: 0.12
Nodes (16): ProgressionResult, AlreadyProcessed, Conflict, InsufficientCredits, InvalidRun, InvalidRunConfiguration, InvalidUpgrade, LevelLocked (+8 more)

### Community 33 - "RunConfiguration"
Cohesion: 0.18
Nodes (10): RunConfiguration, BasicGun, InitialAmmunition, Level, MissileLauncher, Platform, Purchases, Route (+2 more)

### Community 34 - "ProgressionSnapshot"
Cohesion: 0.18
Nodes (8): ProgressionSnapshot, CompletedLevelIds, Credits, HighestUnlockedLevel, RegisteredRuns, Revision, RewardedRuns, UpgradeLevelsData

### Community 36 - "SpecialAttackConfig"
Cohesion: 0.33
Nodes (6): SpecialAttackConfig, CooldownSeconds, Damage, EnergyCost, Range, Targeting

### Community 38 - "PrototypeSettings"
Cohesion: 0.36
Nodes (3): EnemyTuning, PrototypeSettings, WeaponTuning

### Community 39 - "PlatformStats"
Cohesion: 0.29
Nodes (6): IPlatformStatsProvider, PlatformStats, AmmunitionCapacity, DamageReduction, MaximumHealth, ModuleSlots

### Community 40 - "ProgressionLoadError"
Cohesion: 0.33
Nodes (6): ProgressionLoadError, Corrupt, InterruptedWrite, InvalidValues, StorageUnavailable, UnsupportedSchema

### Community 41 - "RunStatus"
Cohesion: 0.33
Nodes (5): RunStatus, Defeat, Playing, StartScreen, Victory

### Community 42 - "TacticalPurchaseConfig"
Cohesion: 0.40
Nodes (5): TacticalPurchaseConfig, AmmunitionAmount, AmmunitionCost, RepairCost, RepairHealth

## Knowledge Gaps
- **209 isolated node(s):** `Succeeded`, `InvalidUpgrade`, `MaximumLevel`, `InsufficientCredits`, `Overflow` (+204 more)
  These have ≤1 connection - possible missing edges. (Counts symbols only; 381 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **7 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `GameRun` connect `GameRun` to `PrototypeSceneBuilder.BuildScene`, `OutpostSession`, `system`, `ProgressionData.Validate`, `PrototypeHud`, `PlatformState`, `RunStatus`, `LevelProgression`, `EnemySpawner`, `Test`, `PrototypeContent`?**
  _High betweenness centrality (0.055) - this node is a cross-community bridge._
- **Are the 10 inferred relationships involving `GameRun` (e.g. with `OutpostSession.ResetRun()` and `SimulationTests.ClearedWaveWaitsForTravelAndCannotSkipAheadWhileAnyEnemyLives()`) actually correct?**
  _`GameRun` has 10 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Succeeded`, `InvalidUpgrade`, `MaximumLevel` to the rest of the system?**
  _209 weakly-connected nodes found - possible missing edges._
- **Should `PrototypeSceneBuilder.BuildScene` be split into smaller, more focused modules?**
  _Cohesion score 0.06512890094979647 - nodes in this community are weakly interconnected._
- **Why does `ProgressionResult` connect `ProgressionResult` to `Test`, `system`?**
  _High betweenness centrality (0.033) - this node is a cross-community bridge._
- **Are the 25 inferred relationships involving `ProgressionService` (e.g. with `ProgressionPersistenceTests.AnotherInterruptedFirstWriteBlocksAnAlreadyLoadedServiceWithoutAnUnhandledException()` and `ProgressionPersistenceTests.CompareAndSwapRejectsStaleWritersWithoutLosingAnotherPurchase()`) actually correct?**
  _`ProgressionService` has 25 INFERRED edges - model-reasoned connections that need verification._
- **Should `OutpostSession` be split into smaller, more focused modules?**
  _Cohesion score 0.14619883040935672 - nodes in this community are weakly interconnected._