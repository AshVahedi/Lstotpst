# Graph Report - Lstotpst  (2026-11-09)

## Corpus Check
- 67 files · ~53,309 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 26 file(s) not represented in the graph (top: (none) 16, .asmdef 8, .xml 1)
- WARNING: 26 of 114 files seen (23%) have no supported extractor and are missing from this graph entirely. It may look complete while representing only part of the codebase.

## Summary
- 1351 nodes · 3979 edges · 70 communities (46 shown, 24 thin omitted)
- Extraction: 73% EXTRACTED · 27% INFERRED · 0% AMBIGUOUS · INFERRED: 1077 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `972a46b1`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- PrototypeSceneBuilder.BuildScene
- EnemySpawner
- system
- with
- GameRun
- PrototypeSettings.Build
- GameplayIntegrationTests
- PlatformState
- steps
- Test
- ProgressionMenuView
- LastOutpost.Core.Tests.csproj
- manifest.json
- update.py
- with
- EconomyTests
- ProgressionService
- ProgressionData
- ProgressionTests
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
- TacticalPurchases
- IRunIdGenerator
- Health
- ProgressionController
- EnemyState
- ProgressionSnapshot
- ProgressionData
- OutpostSession
- ShotEvent
- GameplayIntegrationTests.SetUp
- Test
- RunReceipt
- ProgressionAdapterTests
- PrototypeContent
- Target
- GameRun.Tick
- FileProgressionStore
- PrototypeHud
- RouteConfig
- MovementMode
- RunConfiguration
- SimulationTests.Config
- LevelState
- RunStatus
- PurchaseResult

## God Nodes (most connected - your core abstractions)
1. `ProgressionController` - 66 edges
2. `ProgressionService` - 54 edges
3. `ProgressionData` - 52 edges
4. `OutpostSession` - 50 edges
5. `GameRun` - 48 edges
6. `MemoryProgressionStore` - 47 edges
7. `ProgressionTests` - 43 edges
8. `ProgressionResult` - 42 edges
9. `Health` - 42 edges
10. `PrototypeContent` - 39 edges

## Surprising Connections (you probably didn't know these)
- `UpgradeMenuRow` --references--> `UpgradeKind`  [EXTRACTED]
  Assets/Scripts/Core/Progression/ProgressionMenuView.cs → Assets/Scripts/Core/Progression/ProgressionConfiguration.cs
- `ProgressionController` --references--> `ProgressionConfiguration`  [EXTRACTED]
  Assets/Scripts/Core/Progression/ProgressionController.cs → Assets/Scripts/Core/Progression/ProgressionConfiguration.cs
- `ProgressionService` --references--> `ProgressionConfiguration`  [EXTRACTED]
  Assets/Scripts/Core/Progression/ProgressionService.cs → Assets/Scripts/Core/Progression/ProgressionConfiguration.cs
- `ProgressionIntegrationTests` --references--> `ProgressionConfiguration`  [EXTRACTED]
  Assets/Tests/EditMode/ProgressionIntegrationTests.cs → Assets/Scripts/Core/Progression/ProgressionConfiguration.cs
- `ProgressionPersistenceTests` --references--> `ProgressionConfiguration`  [EXTRACTED]
  Assets/Tests/EditMode/ProgressionPersistenceTests.cs → Assets/Scripts/Core/Progression/ProgressionConfiguration.cs

## Import Cycles
- None detected.

## Communities (70 total, 24 thin omitted)

### Community 1 - "EnemySpawner"
Cohesion: 0.10
Nodes (11): EnemyActor, Agent, EnemySpawner, ActiveCount, GeneratedLevelRoot, PlatformMotor, Mode, CameraFollow (+3 more)

### Community 2 - "system"
Cohesion: 0.07
Nodes (18): LastOutpost.Tests.Editor, LastOutpost.Core.Progression, LastOutpost.Tests, LastOutpost.Presentation, LastOutpost.UI, LastOutpost.Core.Combat, LastOutpost.Core.Configuration, LastOutpost.Editor (+10 more)

### Community 3 - "with"
Cohesion: 0.06
Nodes (33): env, runs-on, steps, timeout-minutes, GRAPHIFY_BACKEND, GRAPHIFY_MAX_WORKERS, GRAPHIFY_OUT, GRAPHIFY_QUERY_LOG_DISABLE (+25 more)

### Community 4 - "GameRun"
Cohesion: 0.13
Nodes (9): GameRun, Configuration, ElapsedSeconds, Enemies, Level, Platform, Route, RunId (+1 more)

### Community 5 - "PrototypeSettings.Build"
Cohesion: 0.13
Nodes (17): TacticalPurchaseConfig, AmmunitionAmount, AmmunitionCost, RepairCost, RepairHealth, LevelDefinition, DisplayName, Encounters (+9 more)

### Community 7 - "PlatformState"
Cohesion: 0.11
Nodes (19): ModuleDefinition, Id, Kind, ModuleKind, Armor, BasicGun, MissileLauncher, Reactor (+11 more)

### Community 8 - "steps"
Cohesion: 0.09
Nodes (21): runs-on, steps, timeout-minutes, jobs, core-tests, name, on, pull_request (+13 more)

### Community 9 - "Test"
Cohesion: 0.15
Nodes (9): LevelProgression, ActiveEnemies, BossActive, CurrentWave, Definition, Progress, RemainingEnemies, State (+1 more)

### Community 10 - "ProgressionMenuView"
Cohesion: 0.06
Nodes (34): ProgressionScreen, Closed, Defeat, Loading, Playing, Recovery, Start, Upgrades (+26 more)

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
Cohesion: 0.23
Nodes (4): ResourceType, Energy, ResourceWallet, EconomyTests

### Community 16 - "ProgressionService"
Cohesion: 0.20
Nodes (3): ProgressionService, Configuration, RequiresReload

### Community 17 - "ProgressionData"
Cohesion: 0.22
Nodes (5): ProgressionData, MemoryProgressionStore, Backups, FaultStore, ProgressionPersistenceTests

### Community 20 - "ProgressionConfiguration"
Cohesion: 0.06
Nodes (28): IPlatformStatsProvider, PlatformStats, AmmunitionCapacity, DamageReduction, MaximumHealth, ModuleSlots, LevelRewardDefinition, LevelId (+20 more)

### Community 22 - "steps"
Cohesion: 0.24
Nodes (14): steps, steps, UNITY_EMAIL, UNITY_LICENSE, UNITY_LICENSE_SERVER, UNITY_PASSWORD, UNITY_SERIAL, steps (+6 more)

### Community 23 - "unity-verification.yml"
Cohesion: 0.17
Nodes (11): concurrency, cancel-in-progress, group, name, runs-on, timeout-minutes, jobs, infrastructure (+3 more)

### Community 24 - "Target"
Cohesion: 0.22
Nodes (7): Ammunition, Capacity, Count, Weapon, Config, NextAttackTimeSeconds, WeaponTests

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

### Community 29 - "TacticalPurchases"
Cohesion: 0.19
Nodes (10): SpecialAttackConfig, CooldownSeconds, Damage, EnergyCost, Range, Targeting, TacticalPurchases, Config (+2 more)

### Community 30 - "IRunIdGenerator"
Cohesion: 0.38
Nodes (3): GuidRunIdGenerator, IRunIdGenerator, FixedId

### Community 31 - "Health"
Cohesion: 0.21
Nodes (7): Health, Current, DamageReduction, IsDead, Maximum, Missing, HealthTests

### Community 32 - "ProgressionController"
Cohesion: 0.05
Nodes (41): ProgressionController, CanUseMenu, IsBusy, IsInitialized, LastBackup, LastResult, Message, RequiresReload (+33 more)

### Community 33 - "EnemyState"
Cohesion: 0.20
Nodes (10): EnemyState, Attack, Definition, DropCollected, Health, Id, EnemyAgent, DefeatReported (+2 more)

### Community 34 - "ProgressionSnapshot"
Cohesion: 0.20
Nodes (9): ProgressionSnapshot, CompletedLevelIds, Credits, HighestUnlockedLevel, RegisteredRuns, Revision, RewardedRuns, UpgradeLevelsData (+1 more)

### Community 35 - "ProgressionData"
Cohesion: 0.10
Nodes (8): IProgressionCodec, JsonProgressionCodec, ProgressionLoadException, Reason, RunRecordData, SchemaHeader, UnsupportedReader, FailingCodec

### Community 36 - "OutpostSession"
Cohesion: 0.11
Nodes (8): OutpostSession, CanCommand, IsPaused, Progression, Run, ProgressionHost, BootError, Controller

### Community 37 - "ShotEvent"
Cohesion: 0.20
Nodes (10): ShotEvent, FromX, Kind, TargetEnemyId, ToX, ShotKind, BasicGun, Enemy (+2 more)

### Community 39 - "Test"
Cohesion: 0.23
Nodes (4): ProgressionMenuBinding, IProgressionStore, ProbeStore, ProgressionIntegrationTests

### Community 40 - "RunReceipt"
Cohesion: 0.18
Nodes (10): RunReceipt, AwardedCredits, IsTerminal, Outcome, RunId, SavedRunOutcome, Abandoned, Active (+2 more)

### Community 42 - "PrototypeContent"
Cohesion: 0.08
Nodes (26): WeaponConfig, AmmunitionPerShot, AttackIntervalSeconds, Damage, Id, Range, Targeting, PrototypeContent (+18 more)

### Community 43 - "Target"
Cohesion: 0.21
Nodes (8): ClosestTargetStrategy, ITargetingStrategy, Target, Distance, Health, Id, RunCombat, FixedTargetStrategy

### Community 45 - "FileProgressionStore"
Cohesion: 0.06
Nodes (14): FileProgressionStore, HasPendingWrite, PendingPath, SavePath, IProgressionRecoveryStore, StoreCommitResult, Committed, Conflict (+6 more)

### Community 46 - "PrototypeHud"
Cohesion: 0.15
Nodes (3): PrototypeHud, LastFeedback, ProgressionView

### Community 48 - "RouteConfig"
Cohesion: 0.22
Nodes (9): RouteConfig, BetweenEncounterSeconds, EncounterSpeed, InitialTravelSeconds, TravelSpeed, RouteMotion, Mode, PositionX (+1 more)

### Community 49 - "MovementMode"
Cohesion: 0.20
Nodes (6): MovementMode, Boss, Encounter, Idle, Stopped, Travel

### Community 50 - "RunConfiguration"
Cohesion: 0.18
Nodes (10): RunConfiguration, BasicGun, InitialAmmunition, Level, MissileLauncher, Platform, Purchases, Route (+2 more)

### Community 51 - "SimulationTests.Config"
Cohesion: 0.33
Nodes (4): Guard, SpawnConfig, DistanceAhead, Spacing

### Community 52 - "LevelState"
Cohesion: 0.33
Nodes (6): LevelState, BetweenEncounters, Defeat, EncounterActive, Ready, Victory

### Community 53 - "RunStatus"
Cohesion: 0.40
Nodes (5): RunStatus, Defeat, Playing, StartScreen, Victory

### Community 56 - "PurchaseResult"
Cohesion: 0.29
Nodes (7): PurchaseResult, CoolingDown, InsufficientEnergy, NoEffect, PlatformDestroyed, RunNotActive, Succeeded

## Knowledge Gaps
- **261 isolated node(s):** `MaximumHealth`, `Armor`, `WeaponDamage`, `AmmunitionCapacity`, `Kind` (+256 more)
  These have ≤1 connection - possible missing edges. (Counts symbols only; 442 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **24 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `ProgressionController` connect `ProgressionController` to `system`, `ProgressionData`, `OutpostSession`, `GameplayIntegrationTests`, `Test`, `GameplayIntegrationTests.SetUp`, `ProgressionMenuView`, `FileProgressionStore`, `PrototypeHud`, `ProgressionService`, `ProgressionData`, `ProgressionConfiguration`, `IRunIdGenerator`?**
  _High betweenness centrality (0.088) - this node is a cross-community bridge._
- **Are the 13 inferred relationships involving `ProgressionController` (e.g. with `ProgressionHost.EnsureBoot()` and `ProgressionIntegrationTests.InvalidSaveStaysPreservedAndResetRequiresBothRequestAndConfirmation()`) actually correct?**
  _`ProgressionController` has 13 INFERRED edges - model-reasoned connections that need verification._
- **What connects `MaximumHealth`, `Armor`, `WeaponDamage` to the rest of the system?**
  _261 weakly-connected nodes found - possible missing edges._
- **Should `EnemySpawner` be split into smaller, more focused modules?**
  _Cohesion score 0.0975609756097561 - nodes in this community are weakly interconnected._
- **Why does `OutpostSession` connect `OutpostSession` to `PrototypeSceneBuilder.BuildScene`, `EnemySpawner`, `system`, `ProgressionController`, `GameplayIntegrationTests`, `GameplayIntegrationTests.SetUp`, `ProgressionAdapterTests`, `PrototypeHud`?**
  _High betweenness centrality (0.037) - this node is a cross-community bridge._
- **Are the 30 inferred relationships involving `ProgressionService` (e.g. with `ProgressionController.Load()` and `ProgressionIntegrationTests.ReceiptDistinguishesAnAbandonedRunFromAPaidVictory()`) actually correct?**
  _`ProgressionService` has 30 INFERRED edges - model-reasoned connections that need verification._
- **Should `system` be split into smaller, more focused modules?**
  _Cohesion score 0.0675549322113137 - nodes in this community are weakly interconnected._