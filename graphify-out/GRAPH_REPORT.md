# Graph Report - Lstotpst  (2026-11-09)

## Corpus Check
- 67 files · ~53,126 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 26 file(s) not represented in the graph (top: (none) 16, .asmdef 8, .xml 1)
- WARNING: 26 of 114 files seen (23%) have no supported extractor and are missing from this graph entirely. It may look complete while representing only part of the codebase.

## Summary
- 1352 nodes · 3921 edges · 60 communities (46 shown, 14 thin omitted)
- Extraction: 74% EXTRACTED · 26% INFERRED · 0% AMBIGUOUS · INFERRED: 1024 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `b3b835ba`
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
- ProgressionTests
- ProgressionService
- IProgressionStore
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
- PrototypeContent.PrototypeContent
- IRunIdGenerator
- Health
- ProgressionResult
- EnemyState
- ProgressionSnapshot
- ProgressionData
- OutpostSession
- ShotEvent
- GameplayIntegrationTests.SetUp
- Test
- RunReceipt
- ProgressionHost
- PrototypeContent
- ProgressionController
- ProgressionRecoveryTests
- PrototypeHud
- UpgradeMenuRow
- ProgressionTests.ArmorConfigurationIsCappedAndNeverMakesDamageNegative
- ProgressionRunSession
- ProgressionScreen
- ProgressionLoadError
- PurchaseResult

## God Nodes (most connected - your core abstractions)
1. `ProgressionService` - 54 edges
2. `ProgressionController` - 54 edges
3. `OutpostSession` - 50 edges
4. `GameRun` - 48 edges
5. `ProgressionData` - 45 edges
6. `ProgressionTests` - 43 edges
7. `MemoryProgressionStore` - 42 edges
8. `Health` - 42 edges
9. `ProgressionResult` - 42 edges
10. `PrototypeContent` - 39 edges

## Surprising Connections (you probably didn't know these)
- `EnemyActor` --references--> `EnemyAgent`  [EXTRACTED]
  Assets/Scripts/Enemies/EnemyActor.cs → Assets/Scripts/Core/Simulation/EnemyAgent.cs
- `EnemySpawner` --references--> `GameRun`  [EXTRACTED]
  Assets/Scripts/Enemies/EnemySpawner.cs → Assets/Scripts/Core/Simulation/GameRun.cs
- `EnemySpawner` --references--> `OutpostSession`  [EXTRACTED]
  Assets/Scripts/Enemies/EnemySpawner.cs → Assets/Scripts/Levels/OutpostSession.cs
- `PlatformMotor` --references--> `MovementMode`  [EXTRACTED]
  Assets/Scripts/Platform/PlatformMotor.cs → Assets/Scripts/Core/Simulation/RouteMotion.cs
- `PlatformMotor` --references--> `OutpostSession`  [EXTRACTED]
  Assets/Scripts/Platform/PlatformMotor.cs → Assets/Scripts/Levels/OutpostSession.cs

## Import Cycles
- None detected.

## Communities (60 total, 14 thin omitted)

### Community 1 - "EnemySpawner"
Cohesion: 0.10
Nodes (11): EnemyActor, Agent, EnemySpawner, ActiveCount, GeneratedLevelRoot, PlatformMotor, Mode, CameraFollow (+3 more)

### Community 2 - "system"
Cohesion: 0.06
Nodes (18): LastOutpost.Tests.Editor, LastOutpost.Core.Progression, LastOutpost.Tests, LastOutpost.Presentation, LastOutpost.UI, LastOutpost.Core.Combat, LastOutpost.Core.Configuration, LastOutpost.Editor (+10 more)

### Community 3 - "with"
Cohesion: 0.06
Nodes (33): env, runs-on, steps, timeout-minutes, GRAPHIFY_BACKEND, GRAPHIFY_MAX_WORKERS, GRAPHIFY_OUT, GRAPHIFY_QUERY_LOG_DISABLE (+25 more)

### Community 4 - "GameRun"
Cohesion: 0.06
Nodes (33): GameRun, Configuration, ElapsedSeconds, Enemies, Level, Platform, Route, RunId (+25 more)

### Community 5 - "PrototypeSettings.Build"
Cohesion: 0.06
Nodes (37): EnemyDefinition, Attack, EnergyDrop, Id, IsBoss, MaximumHealth, MovementSpeed, LevelDefinition (+29 more)

### Community 7 - "PlatformState"
Cohesion: 0.11
Nodes (19): ModuleDefinition, Id, Kind, ModuleKind, Armor, BasicGun, MissileLauncher, Reactor (+11 more)

### Community 8 - "steps"
Cohesion: 0.09
Nodes (21): runs-on, steps, timeout-minutes, jobs, core-tests, name, on, pull_request (+13 more)

### Community 9 - "Test"
Cohesion: 0.12
Nodes (15): LevelProgression, ActiveEnemies, BossActive, CurrentWave, Definition, Progress, RemainingEnemies, State (+7 more)

### Community 10 - "ProgressionMenuView"
Cohesion: 0.15
Nodes (13): ProgressionMenuView, CanReload, CanRetrySave, CanStart, CommittedReward, ConfirmingReset, Credits, CreditsAreCurrent (+5 more)

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
Cohesion: 0.16
Nodes (8): ResourceType, Energy, ResourceWallet, TacticalPurchases, Config, NextSpecialTimeSeconds, SpecialAttack, EconomyTests

### Community 16 - "ProgressionTests"
Cohesion: 0.19
Nodes (3): MemoryProgressionStore, Backups, ProgressionTests

### Community 17 - "ProgressionService"
Cohesion: 0.20
Nodes (10): ProgressionService, Configuration, RequiresReload, FileProgressionStore, HasPendingWrite, PendingPath, SavePath, FailingFileStore (+2 more)

### Community 18 - "IProgressionStore"
Cohesion: 0.19
Nodes (6): IProgressionRecoveryStore, IProgressionStore, StoreCommitResult, Committed, Conflict, CountingStore

### Community 20 - "ProgressionConfiguration"
Cohesion: 0.10
Nodes (20): LevelRewardDefinition, LevelId, LevelNumber, VictoryCredits, ProgressionConfiguration, BasePlatform, BaseWeaponMultiplier, MaximumDamageReduction (+12 more)

### Community 22 - "steps"
Cohesion: 0.24
Nodes (14): steps, steps, UNITY_EMAIL, UNITY_LICENSE, UNITY_LICENSE_SERVER, UNITY_PASSWORD, UNITY_SERIAL, steps (+6 more)

### Community 23 - "unity-verification.yml"
Cohesion: 0.17
Nodes (11): concurrency, cancel-in-progress, group, name, runs-on, timeout-minutes, jobs, infrastructure (+3 more)

### Community 24 - "Target"
Cohesion: 0.16
Nodes (13): Ammunition, Capacity, Count, Target, Distance, Health, Id, Weapon (+5 more)

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

### Community 29 - "PrototypeContent.PrototypeContent"
Cohesion: 0.11
Nodes (16): ClosestTargetStrategy, ITargetingStrategy, WeaponConfig, AmmunitionPerShot, AttackIntervalSeconds, Damage, Id, Range (+8 more)

### Community 30 - "IRunIdGenerator"
Cohesion: 0.38
Nodes (3): GuidRunIdGenerator, IRunIdGenerator, FixedId

### Community 31 - "Health"
Cohesion: 0.22
Nodes (7): Health, Current, DamageReduction, IsDead, Maximum, Missing, HealthTests

### Community 32 - "ProgressionResult"
Cohesion: 0.08
Nodes (21): ProgressionResult, AlreadyProcessed, Busy, ConfirmationRequired, Conflict, InsufficientCredits, InvalidRun, InvalidRunConfiguration (+13 more)

### Community 33 - "EnemyState"
Cohesion: 0.21
Nodes (10): EnemyState, Attack, Definition, DropCollected, Health, Id, EnemyAgent, DefeatReported (+2 more)

### Community 34 - "ProgressionSnapshot"
Cohesion: 0.25
Nodes (8): ProgressionSnapshot, CompletedLevelIds, Credits, HighestUnlockedLevel, RegisteredRuns, Revision, RewardedRuns, Snapshot

### Community 35 - "ProgressionData"
Cohesion: 0.10
Nodes (9): IProgressionCodec, JsonProgressionCodec, ProgressionData, ProgressionLoadException, Reason, RunRecordData, SchemaHeader, UpgradeLevelsData (+1 more)

### Community 36 - "OutpostSession"
Cohesion: 0.19
Nodes (5): OutpostSession, CanCommand, IsPaused, Progression, Run

### Community 37 - "ShotEvent"
Cohesion: 0.20
Nodes (10): ShotEvent, FromX, Kind, TargetEnemyId, ToX, ShotKind, BasicGun, Enemy (+2 more)

### Community 39 - "Test"
Cohesion: 0.14
Nodes (3): ProbeStore, ProgressionIntegrationTests, UnsupportedReader

### Community 40 - "RunReceipt"
Cohesion: 0.18
Nodes (10): RunReceipt, AwardedCredits, IsTerminal, Outcome, RunId, SavedRunOutcome, Abandoned, Active (+2 more)

### Community 41 - "ProgressionHost"
Cohesion: 0.20
Nodes (4): ProgressionHost, BootError, Controller, ProgressionAdapterTests

### Community 42 - "PrototypeContent"
Cohesion: 0.12
Nodes (15): PrototypeContent, BasicEnemy, BasicGun, Boss, FastEnemy, LevelOne, MissileLauncher, Purchases (+7 more)

### Community 44 - "ProgressionController"
Cohesion: 0.14
Nodes (11): ProgressionController, CanUseMenu, IsBusy, IsInitialized, LastBackup, LastResult, Message, RequiresReload (+3 more)

### Community 45 - "ProgressionRecoveryTests"
Cohesion: 0.09
Nodes (5): AcknowledgmentStore, FailingRecoveryStore, LostAcknowledgmentStore, ProgressionRecoveryTests, UncertainResetStore

### Community 46 - "PrototypeHud"
Cohesion: 0.14
Nodes (4): ProgressionMenuBinding, PrototypeHud, LastFeedback, ProgressionView

### Community 48 - "UpgradeMenuRow"
Cohesion: 0.18
Nodes (12): UpgradeMenuRow, CanPurchase, CurrentEffect, CurrentText, IsMaximum, Kind, Level, MaximumLevel (+4 more)

### Community 51 - "ProgressionRunSession"
Cohesion: 0.22
Nodes (3): ProgressionRunSession, LastCompletionResult, Run

### Community 52 - "ProgressionScreen"
Cohesion: 0.22
Nodes (9): ProgressionScreen, Closed, Defeat, Loading, Playing, Recovery, Start, Upgrades (+1 more)

### Community 53 - "ProgressionLoadError"
Cohesion: 0.29
Nodes (6): ProgressionLoadError, Corrupt, InterruptedWrite, InvalidValues, StorageUnavailable, UnsupportedSchema

### Community 56 - "PurchaseResult"
Cohesion: 0.29
Nodes (7): PurchaseResult, CoolingDown, InsufficientEnergy, NoEffect, PlatformDestroyed, RunNotActive, Succeeded

## Knowledge Gaps
- **261 isolated node(s):** `Agent`, `ActiveCount`, `Mode`, `Closed`, `Defeat` (+256 more)
  These have ≤1 connection - possible missing edges. (Counts symbols only; 440 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **14 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `ProgressionController` connect `ProgressionController` to `ProgressionResult`, `system`, `ProgressionData`, `OutpostSession`, `GameplayIntegrationTests`, `GameplayIntegrationTests.SetUp`, `ProgressionHost`, `ProgressionMenuView`, `ProgressionController.Load`, `PrototypeHud`, `ProgressionService`, `IProgressionStore`, `ProgressionRunSession`, `ProgressionConfiguration`, `ProgressionLoadError`, `ProgressionScreen`, `IRunIdGenerator`?**
  _High betweenness centrality (0.059) - this node is a cross-community bridge._
- **Are the 30 inferred relationships involving `ProgressionService` (e.g. with `ProgressionController.Load()` and `ProgressionIntegrationTests.ReceiptDistinguishesAnAbandonedRunFromAPaidVictory()`) actually correct?**
  _`ProgressionService` has 30 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Agent`, `ActiveCount`, `Mode` to the rest of the system?**
  _261 weakly-connected nodes found - possible missing edges._
- **Should `EnemySpawner` be split into smaller, more focused modules?**
  _Cohesion score 0.0975609756097561 - nodes in this community are weakly interconnected._
- **Why does `GameRun` connect `GameRun` to `EnemySpawner`, `system`, `PlatformState`, `Test`, `ProgressionTests`, `Target`?**
  _High betweenness centrality (0.045) - this node is a cross-community bridge._
- **Are the 2 inferred relationships involving `ProgressionController` (e.g. with `ProgressionHost.EnsureBoot()` and `GameplayIntegrationTests.SetUp()`) actually correct?**
  _`ProgressionController` has 2 INFERRED edges - model-reasoned connections that need verification._
- **Should `system` be split into smaller, more focused modules?**
  _Cohesion score 0.06357388316151202 - nodes in this community are weakly interconnected._