# Graph Report - Lstotpst  (2026-11-12)

## Corpus Check
- 76 files · ~66,672 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 26 file(s) not represented in the graph (top: (none) 16, .asmdef 8, .xml 1)
- WARNING: 26 of 125 files seen (21%) have no supported extractor and are missing from this graph entirely. It may look complete while representing only part of the codebase.

## Summary
- 1652 nodes · 4890 edges · 115 communities (58 shown, 57 thin omitted)
- Extraction: 73% EXTRACTED · 27% INFERRED · 0% AMBIGUOUS · INFERRED: 1344 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `2ab0b96a`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- PrototypeSceneBuilder.BuildScene
- GeneratorTestWorkspace
- system
- with
- GameRun
- PrototypeContent.PrototypeContent
- PrototypeHud
- ModuleDefinition
- steps
- LevelProgression
- ProgressionMenuView
- LastOutpost.Core.Tests.csproj
- manifest.json
- update.py
- with
- Health.TakeDamage
- MemoryProgressionStore
- FileProgressionStore
- UpgradeKind
- UnityCiTests
- PlatformStats
- AssemblyAuditTests
- steps
- unity-verification.yml
- Health
- build_android
- system_collections_generic
- env
- editor
- com.unity.ext.nunit
- ProgressionData
- ProgressionResult
- ProgressionSnapshot
- ProgressionAdapterTests
- OutpostSession
- ProgressionController
- SceneBuilderTests.cs
- EnemyState
- EquipmentDefinition
- ProgressionTests.Config
- PrototypeContent
- LoadoutResult
- ProgressionService
- LevelState
- OutpostSession.StepSimulation
- LastOutpost.Core.Simulation
- UpgradeDefinition
- ProgressionConfiguration.Prototype
- Health
- EquipmentPersistenceTests
- EnemySpawner
- LoadoutRunBuilder.Build
- ProgressionRunSession
- Test
- LoadoutMenuRow
- UnitySyntaxCheck.csproj
- RunConfiguration
- ProgressionLoadError
- PlatformState
- CombatFeedback
- UpgradeMenuRow
- IRunIdGenerator
- ProgressionMenuBinding
- PrototypeSceneBuilder.Generate
- RunReceipt
- LoadoutOperation
- ProgressionScreen
- LastOutpost.Core.Modules
- LoadoutMenuOption
- LoadoutMenuView
- infrastructure
- Program.cs

## God Nodes (most connected - your core abstractions)
1. `ProgressionService` - 84 edges
2. `ProgressionController` - 70 edges
3. `GameRun` - 62 edges
4. `ProgressionData` - 61 edges
5. `MemoryProgressionStore` - 59 edges
6. `ProgressionTests` - 43 edges
7. `ProgressionResult` - 43 edges
8. `OutpostSession` - 42 edges
9. `Health` - 42 edges
10. `PrototypeContent` - 39 edges

## Surprising Connections (you probably didn't know these)
- `ProgressionMenuView` --references--> `ProgressionScreen`  [EXTRACTED]
  Assets/Scripts/Core/Progression/ProgressionMenuView.cs → Assets/Scripts/Core/Progression/ProgressionController.cs
- `ProgressionController` --references--> `ProgressionMenuView`  [EXTRACTED]
  Assets/Scripts/Core/Progression/ProgressionController.cs → Assets/Scripts/Core/Progression/ProgressionMenuView.cs
- `ProgressionMenuView` --references--> `LoadoutMenuView`  [EXTRACTED]
  Assets/Scripts/Core/Progression/ProgressionMenuView.cs → Assets/Scripts/Core/Progression/LoadoutMenuView.cs
- `PrototypeHud` --references--> `ProgressionMenuView`  [EXTRACTED]
  Assets/Scripts/UI/PrototypeHud.cs → Assets/Scripts/Core/Progression/ProgressionMenuView.cs
- `PlatformState` --references--> `ResourceWallet`  [EXTRACTED]
  Assets/Scripts/Core/Platform/PlatformState.cs → Assets/Scripts/Core/Economy/ResourceWallet.cs

## Import Cycles
- None detected.

## Communities (115 total, 57 thin omitted)

### Community 2 - "system"
Cohesion: 0.20
Nodes (6): LastOutpost.Tests, LastOutpost.Core.Combat, LastOutpost.Core.Configuration, LastOutpost.Core, LastOutpost.Core.Economy, LastOutpost.Core.Enemies

### Community 3 - "with"
Cohesion: 0.06
Nodes (33): env, runs-on, steps, timeout-minutes, GRAPHIFY_BACKEND, GRAPHIFY_MAX_WORKERS, GRAPHIFY_OUT, GRAPHIFY_QUERY_LOG_DISABLE (+25 more)

### Community 4 - "GameRun"
Cohesion: 0.08
Nodes (28): GameRun, Configuration, ElapsedSeconds, Enemies, Level, Platform, Route, RunId (+20 more)

### Community 5 - "PrototypeContent.PrototypeContent"
Cohesion: 0.10
Nodes (24): SpecialAttackConfig, CooldownSeconds, Damage, EnergyCost, Range, Targeting, TacticalPurchaseConfig, AmmunitionAmount (+16 more)

### Community 6 - "PrototypeHud"
Cohesion: 0.12
Nodes (10): PurchaseResult, CoolingDown, InsufficientEnergy, NoEffect, PlatformDestroyed, RunNotActive, Succeeded, PrototypeHud (+2 more)

### Community 7 - "ModuleDefinition"
Cohesion: 0.15
Nodes (15): ModuleDefinition, Id, Kind, ModuleKind, Armor, BasicGun, MissileLauncher, Reactor (+7 more)

### Community 8 - "steps"
Cohesion: 0.09
Nodes (21): runs-on, steps, timeout-minutes, jobs, core-tests, name, on, pull_request (+13 more)

### Community 9 - "LevelProgression"
Cohesion: 0.16
Nodes (9): LevelProgression, ActiveEnemies, BossActive, CurrentWave, Definition, Progress, RemainingEnemies, State (+1 more)

### Community 10 - "ProgressionMenuView"
Cohesion: 0.14
Nodes (14): ProgressionMenuView, CanReload, CanRetrySave, CanStart, CommittedReward, ConfirmingReset, Credits, CreditsAreCurrent (+6 more)

### Community 11 - "LastOutpost.Core.Tests.csproj"
Cohesion: 0.22
Nodes (7): netstandard2.1, Microsoft.NET.Test.Sdk (17.11.1), NUnit (3.14.0), NUnit3TestAdapter (4.6.0), Microsoft.NET.Sdk, net8.0, Microsoft.NET.Sdk

### Community 12 - "manifest.json"
Cohesion: 0.36
Nodes (7): dependencies, com.unity.modules.imageconversion@1.0.0, com.unity.modules.imgui@1.0.0, com.unity.test-framework@1.8.0, com.unity.modules.imageconversion, com.unity.modules.imgui, com.unity.test-framework

### Community 13 - "update.py"
Cohesion: 0.06
Nodes (19): main(), GraphifyWorkflowTests, command(), detect_code(), ensure_version(), main(), tool_environment(), update() (+11 more)

### Community 14 - "with"
Cohesion: 0.07
Nodes (29): with, allowDirtyBuild, androidExportType, androidTargetSdkVersion, androidVersionCode, artifactsPath, buildMethod, buildName (+21 more)

### Community 15 - "Health.TakeDamage"
Cohesion: 0.17
Nodes (8): ResourceType, Energy, ResourceWallet, TacticalPurchases, Config, NextSpecialTimeSeconds, SpecialAttack, EconomyTests

### Community 16 - "MemoryProgressionStore"
Cohesion: 0.17
Nodes (3): MemoryProgressionStore, Backups, ProgressionTests

### Community 17 - "FileProgressionStore"
Cohesion: 0.07
Nodes (11): FileProgressionStore, HasPendingWrite, PendingPath, SavePath, IProgressionRecoveryStore, StoreCommitResult, Committed, Conflict (+3 more)

### Community 18 - "UpgradeKind"
Cohesion: 0.18
Nodes (10): ProgressionConfiguration, BasePlatform, BaseWeaponMultiplier, Equipment, MaximumDamageReduction, UpgradeKind, AmmunitionCapacity, Armor (+2 more)

### Community 20 - "PlatformStats"
Cohesion: 0.22
Nodes (8): IPlatformStatsProvider, PlatformStats, AmmunitionCapacity, DamageReduction, MaximumHealth, ModuleSlots, PermanentStatsProvider, WeaponMultiplier

### Community 22 - "steps"
Cohesion: 0.24
Nodes (14): steps, steps, UNITY_EMAIL, UNITY_LICENSE, UNITY_LICENSE_SERVER, UNITY_PASSWORD, UNITY_SERIAL, steps (+6 more)

### Community 23 - "unity-verification.yml"
Cohesion: 0.14
Nodes (13): if, name, needs, runs-on, timeout-minutes, concurrency, cancel-in-progress, group (+5 more)

### Community 24 - "Health"
Cohesion: 0.17
Nodes (14): Ammunition, Capacity, Count, ClosestTargetStrategy, ITargetingStrategy, Target, Distance, Health (+6 more)

### Community 25 - "build_android"
Cohesion: 0.22
Nodes (9): default, description, type, build_android, on, pull_request, workflow_dispatch, paths (+1 more)

### Community 27 - "env"
Cohesion: 0.33
Nodes (6): env, ANDROID_IMAGE, EDITOR_IMAGE, GAME_CI_CLI_VERSION, OUTPOST_SOURCE_SHA, UNITY_VERSION

### Community 28 - "editor"
Cohesion: 0.40
Nodes (5): name, needs, runs-on, timeout-minutes, editor

### Community 29 - "com.unity.ext.nunit"
Cohesion: 0.08
Nodes (29): dependencies, depth, source, version, dependencies, depth, source, version (+21 more)

### Community 30 - "ProgressionData"
Cohesion: 0.12
Nodes (9): IProgressionCodec, ProgressionData, ProgressionLoadException, Reason, RunRecordData, UpgradeLevelsData, LegacyProgressionData, UnsupportedReader (+1 more)

### Community 32 - "ProgressionResult"
Cohesion: 0.09
Nodes (22): ProgressionResult, AlreadyProcessed, Busy, ConfirmationRequired, Conflict, InsufficientCredits, InvalidLoadout, InvalidRun (+14 more)

### Community 34 - "ProgressionSnapshot"
Cohesion: 0.20
Nodes (9): ProgressionSnapshot, CompletedLevelIds, Credits, HighestUnlockedLevel, Loadout, RegisteredRuns, Revision, RewardedRuns (+1 more)

### Community 36 - "OutpostSession"
Cohesion: 0.11
Nodes (10): IProgressionStore, OutpostSession, CanCommand, IsPaused, Progression, Run, ProgressionHost, BootError (+2 more)

### Community 37 - "ProgressionController"
Cohesion: 0.13
Nodes (11): ProgressionController, CanUseMenu, IsBusy, IsInitialized, LastBackup, LastResult, Message, RequiresReload (+3 more)

### Community 38 - "SceneBuilderTests.cs"
Cohesion: 0.18
Nodes (3): LastOutpost.Tests.Editor, LastOutpost.Editor, LastOutpost.Tests.PlayMode

### Community 39 - "EnemyState"
Cohesion: 0.20
Nodes (10): EnemyState, Attack, Definition, DropCollected, Health, Id, EnemyAgent, DefeatReported (+2 more)

### Community 40 - "EquipmentDefinition"
Cohesion: 0.05
Nodes (55): EquipmentCatalog, Definitions, EquipmentCategory, AdvancedWeapon, BasicWeapon, DefensiveModule, SupportModule, EquipmentConfiguration (+47 more)

### Community 41 - "ProgressionTests.Config"
Cohesion: 0.43
Nodes (4): LevelRewardDefinition, LevelId, LevelNumber, VictoryCredits

### Community 42 - "PrototypeContent"
Cohesion: 0.09
Nodes (25): WeaponConfig, AmmunitionPerShot, AttackIntervalSeconds, Damage, Id, Range, Targeting, PrototypeContent (+17 more)

### Community 43 - "LoadoutResult"
Cohesion: 0.09
Nodes (23): LoadoutResult, AlreadyInstalled, Busy, CapacityExceeded, Conflict, DisabledSlot, DuplicateInstallation, Incompatible (+15 more)

### Community 44 - "ProgressionService"
Cohesion: 0.17
Nodes (6): ProgressionService, Configuration, RequiresReload, FailingFileStore, FaultStore, ProgressionPersistenceTests

### Community 45 - "LevelState"
Cohesion: 0.33
Nodes (6): LevelState, BetweenEncounters, Defeat, EncounterActive, Ready, Victory

### Community 47 - "LastOutpost.Core.Simulation"
Cohesion: 0.24
Nodes (4): LastOutpost.Presentation, LastOutpost.UI, LastOutpost.Gameplay, LastOutpost.Core.Simulation

### Community 48 - "UpgradeDefinition"
Cohesion: 0.27
Nodes (6): UpgradeDefinition, BaseCost, GrowthFactor, IncreasePerLevel, Kind, MaximumLevel

### Community 49 - "ProgressionConfiguration.Prototype"
Cohesion: 0.15
Nodes (6): JsonProgressionCodec, SchemaHeader, Config, FailingRecoveryStore, LostAcknowledgmentStore, ProgressionRecoveryTests

### Community 50 - "Health"
Cohesion: 0.23
Nodes (7): Health, Current, DamageReduction, IsDead, Maximum, Missing, HealthTests

### Community 51 - "EquipmentPersistenceTests"
Cohesion: 0.16
Nodes (3): EquipmentPersistenceTests, Config, FaultStore

### Community 53 - "EnemySpawner"
Cohesion: 0.14
Nodes (8): EnemyActor, Agent, EnemySpawner, ActiveCount, GeneratedLevelRoot, PlatformMotor, Mode, HealthBarView

### Community 54 - "LoadoutRunBuilder.Build"
Cohesion: 0.19
Nodes (10): ResolvedInstallation, Definition, SlotId, SlotIndex, RunLoadout, EnergyPerDefeat, HasAdvancedWeapon, Installations (+2 more)

### Community 56 - "ProgressionRunSession"
Cohesion: 0.15
Nodes (8): ProgressionRunSession, LastCompletionResult, Run, RunStatus, Defeat, Playing, StartScreen, Victory

### Community 58 - "LoadoutMenuRow"
Cohesion: 0.20
Nodes (10): LoadoutMenuRow, CanUnequip, Category, Enabled, EquipmentId, EquipmentName, Options, Required (+2 more)

### Community 61 - "RunConfiguration"
Cohesion: 0.17
Nodes (11): RunConfiguration, BasicGun, Equipment, InitialAmmunition, Level, MissileLauncher, Platform, Purchases (+3 more)

### Community 63 - "ProgressionLoadError"
Cohesion: 0.29
Nodes (6): ProgressionLoadError, Corrupt, InterruptedWrite, InvalidValues, StorageUnavailable, UnsupportedSchema

### Community 64 - "PlatformState"
Cohesion: 0.13
Nodes (16): PlatformState, AdvancedAmmunition, Health, Modules, Resources, RunCombat, ShotEvent, FromX (+8 more)

### Community 66 - "UpgradeMenuRow"
Cohesion: 0.18
Nodes (12): UpgradeMenuRow, CanPurchase, CurrentEffect, CurrentText, IsMaximum, Kind, Level, MaximumLevel (+4 more)

### Community 67 - "IRunIdGenerator"
Cohesion: 0.38
Nodes (3): GuidRunIdGenerator, IRunIdGenerator, FixedId

### Community 82 - "RunReceipt"
Cohesion: 0.18
Nodes (10): RunReceipt, AwardedCredits, IsTerminal, Outcome, RunId, SavedRunOutcome, Abandoned, Active (+2 more)

### Community 88 - "LoadoutOperation"
Cohesion: 0.40
Nodes (5): LoadoutOperation, Equip, Replace, RestoreDefaults, Unequip

### Community 89 - "ProgressionScreen"
Cohesion: 0.20
Nodes (10): ProgressionScreen, Closed, Defeat, Loading, Loadout, Playing, Recovery, Start (+2 more)

### Community 94 - "LoadoutMenuOption"
Cohesion: 0.25
Nodes (8): LoadoutMenuOption, Availability, CanInstall, Description, EquipmentId, Name, Operation, Owned

### Community 95 - "LoadoutMenuView"
Cohesion: 0.29
Nodes (6): LoadoutMenuView, CanEdit, CanRestoreDefaults, Slots, UnknownOwnedIds, Validation

### Community 97 - "infrastructure"
Cohesion: 0.40
Nodes (5): if, name, runs-on, timeout-minutes, infrastructure

## Knowledge Gaps
- **368 isolated node(s):** `Closed`, `Defeat`, `Loading`, `Loadout`, `Playing` (+363 more)
  These have ≤1 connection - possible missing edges. (Counts symbols only; 600 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **57 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `GameRun` connect `GameRun` to `PlatformState`, `CombatFeedback`, `OutpostSession`, `ProgressionController`, `PrototypeHud`, `EnemyState`, `EquipmentDefinition`, `LevelProgression`, `ProgressionService`, `Health.TakeDamage`, `MemoryProgressionStore`, `EnemySpawner`, `ProgressionRunSession`, `system_collections_generic`, `RunConfiguration`, `ProgressionData`?**
  _High betweenness centrality (0.088) - this node is a cross-community bridge._
- **Are the 51 inferred relationships involving `ProgressionService` (e.g. with `ProgressionController.Load()` and `EquipmentPersistenceTests.ActiveRunEquipmentAndStatsCannotBeChangedAndTheNextRunUsesTheNewSelection()`) actually correct?**
  _`ProgressionService` has 51 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Closed`, `Defeat`, `Loading` to the rest of the system?**
  _368 weakly-connected nodes found - possible missing edges._
- **Should `with` be split into smaller, more focused modules?**
  _Cohesion score 0.058823529411764705 - nodes in this community are weakly interconnected._
- **Why does `ProgressionController` connect `ProgressionController` to `GameRun`, `PrototypeHud`, `ProgressionMenuView`, `MemoryProgressionStore`, `UpgradeKind`, `system_collections_generic`, `ProgressionData`, `ProgressionResult`, `OutpostSession`, `ProgressionService`, `ProgressionConfiguration.Prototype`, `EquipmentPersistenceTests`, `ProgressionRunSession`, `Test`, `MemoryProgressionStore.Read`, `ProgressionLoadError`, `IRunIdGenerator`, `ProgressionMenuBinding`, `ProgressionScreen`?**
  _High betweenness centrality (0.064) - this node is a cross-community bridge._
- **Are the 16 inferred relationships involving `ProgressionController` (e.g. with `ProgressionHost.EnsureBoot()` and `EquipmentPersistenceTests.FailedLoadoutSaveBlocksTheUiUntilReloadAndDoesNotAutomaticallyRepeatTheChange()`) actually correct?**
  _`ProgressionController` has 16 INFERRED edges - model-reasoned connections that need verification._
- **Should `GameRun` be split into smaller, more focused modules?**
  _Cohesion score 0.07577639751552795 - nodes in this community are weakly interconnected._