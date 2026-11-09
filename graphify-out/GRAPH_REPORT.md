# Graph Report - Lstotpst  (2026-11-09)

## Corpus Check
- 75 files · ~61,991 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 26 file(s) not represented in the graph (top: (none) 16, .asmdef 8, .xml 1)
- WARNING: 26 of 123 files seen (21%) have no supported extractor and are missing from this graph entirely. It may look complete while representing only part of the codebase.

## Summary
- 1600 nodes · 4650 edges · 89 communities (55 shown, 34 thin omitted)
- Extraction: 74% EXTRACTED · 26% INFERRED · 0% AMBIGUOUS · INFERRED: 1199 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `f5f25b37`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- PrototypeSceneBuilder.BuildScene
- EnemySpawner
- system
- with
- GameRun
- PrototypeContent.PrototypeContent
- JsonProgressionCodec
- PlatformState
- steps
- Test
- ProgressionMenuView
- LastOutpost.Core.Tests.csproj
- manifest.json
- update.py
- with
- EconomyTests
- Test
- FileProgressionStore
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
- EquipmentSlot
- LoadoutRules.Change
- Health
- ProgressionResult
- EnemyState
- LoadoutSnapshot
- ProgressionData
- OutpostSession
- ProgressionController
- Test
- EquipmentTests
- MemoryProgressionStore
- PrototypeContent
- LoadoutResult
- EquipmentDefinition
- ProgressionRecoveryTests
- PrototypeHud
- EquipmentConfiguration
- UpgradeMenuRow
- LoadoutMenuRow
- RunConfiguration
- ProgressionHost
- ProgressionScreen
- Transform
- UncertainResetStore
- ProgressionRunSession
- LoadoutMenuView
- LoadoutMenuOption
- RunReceipt
- CombatFeedback
- ProgressionLoadError
- LevelState
- IRunIdGenerator
- GameplayIntegrationTests.SetUp

## God Nodes (most connected - your core abstractions)
1. `ProgressionService` - 84 edges
2. `ProgressionController` - 71 edges
3. `ProgressionData` - 60 edges
4. `GameRun` - 60 edges
5. `OutpostSession` - 44 edges
6. `ProgressionResult` - 43 edges
7. `ProgressionTests` - 43 edges
8. `PrototypeHud` - 41 edges
9. `PrototypeContent` - 39 edges
10. `Health` - 38 edges

## Surprising Connections (you probably didn't know these)
- `SpawnGroup` --references--> `EnemyDefinition`  [EXTRACTED]
  Assets/Scripts/Core/Levels/LevelDefinition.cs → Assets/Scripts/Core/Enemies/EnemyState.cs
- `LevelProgression` --references--> `EnemyState`  [EXTRACTED]
  Assets/Scripts/Core/Levels/LevelProgression.cs → Assets/Scripts/Core/Enemies/EnemyState.cs
- `EnemyAgent` --references--> `EnemyState`  [EXTRACTED]
  Assets/Scripts/Core/Simulation/EnemyAgent.cs → Assets/Scripts/Core/Enemies/EnemyState.cs
- `EquipmentDefinition` --references--> `ModuleKind`  [EXTRACTED]
  Assets/Scripts/Core/Modules/EquipmentConfiguration.cs → Assets/Scripts/Core/Modules/ModuleRack.cs
- `ResolvedInstallation` --references--> `EquipmentDefinition`  [EXTRACTED]
  Assets/Scripts/Core/Modules/RunLoadout.cs → Assets/Scripts/Core/Modules/EquipmentConfiguration.cs

## Import Cycles
- None detected.

## Communities (89 total, 34 thin omitted)

### Community 1 - "EnemySpawner"
Cohesion: 0.17
Nodes (9): EnemyAgent, DefeatReported, PositionX, State, EnemyActor, Agent, EnemySpawner, ActiveCount (+1 more)

### Community 2 - "system"
Cohesion: 0.06
Nodes (18): LastOutpost.Tests.Editor, LastOutpost.Core.Progression, LastOutpost.Tests, LastOutpost.Presentation, LastOutpost.UI, LastOutpost.Core.Combat, LastOutpost.Core.Configuration, LastOutpost.Editor (+10 more)

### Community 3 - "with"
Cohesion: 0.06
Nodes (33): env, runs-on, steps, timeout-minutes, GRAPHIFY_BACKEND, GRAPHIFY_MAX_WORKERS, GRAPHIFY_OUT, GRAPHIFY_QUERY_LOG_DISABLE (+25 more)

### Community 4 - "GameRun"
Cohesion: 0.05
Nodes (39): GameRun, Configuration, ElapsedSeconds, Enemies, Level, Platform, Route, RunId (+31 more)

### Community 5 - "PrototypeContent.PrototypeContent"
Cohesion: 0.10
Nodes (25): ClosestTargetStrategy, SpecialAttackConfig, CooldownSeconds, Damage, EnergyCost, Range, Targeting, TacticalPurchaseConfig (+17 more)

### Community 6 - "JsonProgressionCodec"
Cohesion: 0.12
Nodes (5): IProgressionCodec, JsonProgressionCodec, SchemaHeader, UnsupportedReader, FailingCodec

### Community 7 - "PlatformState"
Cohesion: 0.10
Nodes (21): ModuleDefinition, Id, Kind, ModuleKind, Armor, BasicGun, MissileLauncher, Reactor (+13 more)

### Community 8 - "steps"
Cohesion: 0.09
Nodes (21): runs-on, steps, timeout-minutes, jobs, core-tests, name, on, pull_request (+13 more)

### Community 9 - "Test"
Cohesion: 0.16
Nodes (9): LevelProgression, ActiveEnemies, BossActive, CurrentWave, Definition, Progress, RemainingEnemies, State (+1 more)

### Community 10 - "ProgressionMenuView"
Cohesion: 0.14
Nodes (14): ProgressionMenuView, CanReload, CanRetrySave, CanStart, CommittedReward, ConfirmingReset, Credits, CreditsAreCurrent (+6 more)

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
Cohesion: 0.13
Nodes (15): ResourceType, Energy, ResourceWallet, PurchaseResult, CoolingDown, InsufficientEnergy, NoEffect, PlatformDestroyed (+7 more)

### Community 16 - "Test"
Cohesion: 0.14
Nodes (5): ProgressionService, Configuration, RequiresReload, EquipmentPersistenceTests, FaultStore

### Community 17 - "FileProgressionStore"
Cohesion: 0.23
Nodes (4): FileProgressionStore, HasPendingWrite, PendingPath, SavePath

### Community 20 - "ProgressionConfiguration"
Cohesion: 0.08
Nodes (22): LevelRewardDefinition, LevelId, LevelNumber, VictoryCredits, ProgressionConfiguration, BasePlatform, BaseWeaponMultiplier, Equipment (+14 more)

### Community 22 - "steps"
Cohesion: 0.24
Nodes (14): steps, steps, UNITY_EMAIL, UNITY_LICENSE, UNITY_LICENSE_SERVER, UNITY_PASSWORD, UNITY_SERIAL, steps (+6 more)

### Community 23 - "unity-verification.yml"
Cohesion: 0.17
Nodes (11): concurrency, cancel-in-progress, group, name, runs-on, timeout-minutes, jobs, infrastructure (+3 more)

### Community 24 - "Target"
Cohesion: 0.17
Nodes (13): Ammunition, Capacity, Count, ITargetingStrategy, Target, Distance, Health, Id (+5 more)

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

### Community 29 - "EquipmentSlot"
Cohesion: 0.12
Nodes (18): EquipmentCategory, AdvancedWeapon, BasicWeapon, DefensiveModule, SupportModule, EquipmentSlot, AllowedCategories, Capacity (+10 more)

### Community 30 - "LoadoutRules.Change"
Cohesion: 0.24
Nodes (3): EquipmentInstallationData, EquipmentProfileData, LoadoutRules

### Community 31 - "Health"
Cohesion: 0.14
Nodes (12): Health, Current, DamageReduction, IsDead, Maximum, Missing, PlatformState, AdvancedAmmunition (+4 more)

### Community 32 - "ProgressionResult"
Cohesion: 0.09
Nodes (22): ProgressionResult, AlreadyProcessed, Busy, ConfirmationRequired, Conflict, InsufficientCredits, InvalidLoadout, InvalidRun (+14 more)

### Community 33 - "EnemyState"
Cohesion: 0.16
Nodes (6): EnemyState, Attack, Definition, DropCollected, Health, Id

### Community 34 - "LoadoutSnapshot"
Cohesion: 0.13
Nodes (15): InstalledEquipment, EquipmentId, SlotId, LoadoutSnapshot, Installations, OwnedEquipmentIds, ProgressionSnapshot, CompletedLevelIds (+7 more)

### Community 35 - "ProgressionData"
Cohesion: 0.13
Nodes (9): ProgressionData, ProgressionLoadException, Reason, RunRecordData, UpgradeLevelsData, LegacyProgressionData, FailingFileStore, FaultStore (+1 more)

### Community 36 - "OutpostSession"
Cohesion: 0.15
Nodes (5): OutpostSession, CanCommand, IsPaused, Progression, Run

### Community 37 - "ProgressionController"
Cohesion: 0.14
Nodes (11): ProgressionController, CanUseMenu, IsBusy, IsInitialized, LastBackup, LastResult, Message, RequiresReload (+3 more)

### Community 40 - "EquipmentTests"
Cohesion: 0.18
Nodes (10): EquipmentModifier, Stat, Value, EquipmentStat, AmmunitionCapacity, DamageReduction, EnergyPerDefeat, MaximumHealth (+2 more)

### Community 41 - "MemoryProgressionStore"
Cohesion: 0.20
Nodes (7): IProgressionRecoveryStore, IProgressionStore, MemoryProgressionStore, Backups, StoreCommitResult, Committed, Conflict

### Community 42 - "PrototypeContent"
Cohesion: 0.08
Nodes (26): WeaponConfig, AmmunitionPerShot, AttackIntervalSeconds, Damage, Id, Range, Targeting, PrototypeContent (+18 more)

### Community 43 - "LoadoutResult"
Cohesion: 0.09
Nodes (23): LoadoutResult, AlreadyInstalled, Busy, CapacityExceeded, Conflict, DisabledSlot, DuplicateInstallation, Incompatible (+15 more)

### Community 44 - "EquipmentDefinition"
Cohesion: 0.15
Nodes (12): EquipmentDefinition, AvailableByDefault, Category, CompatibleSlots, Description, DisplayName, Id, InstallationSize (+4 more)

### Community 45 - "ProgressionRecoveryTests"
Cohesion: 0.22
Nodes (4): FailingPromotion, FailingRecoveryStore, LostAcknowledgmentStore, ProgressionRecoveryTests

### Community 46 - "PrototypeHud"
Cohesion: 0.07
Nodes (7): ProgressionMenuBinding, PrototypeHud, LastFeedback, ProgressionView, GameplayIntegrationTests, CountingStore, ProgressionAdapterTests

### Community 47 - "EquipmentConfiguration"
Cohesion: 0.29
Nodes (6): EquipmentCatalog, Definitions, EquipmentConfiguration, Catalog, Slots, TotalCapacity

### Community 48 - "UpgradeMenuRow"
Cohesion: 0.18
Nodes (12): UpgradeMenuRow, CanPurchase, CurrentEffect, CurrentText, IsMaximum, Kind, Level, MaximumLevel (+4 more)

### Community 49 - "LoadoutMenuRow"
Cohesion: 0.20
Nodes (10): LoadoutMenuRow, CanUnequip, Category, Enabled, EquipmentId, EquipmentName, Options, Required (+2 more)

### Community 50 - "RunConfiguration"
Cohesion: 0.07
Nodes (23): ResolvedInstallation, Definition, SlotId, SlotIndex, RunLoadout, EnergyPerDefeat, HasAdvancedWeapon, Installations (+15 more)

### Community 51 - "ProgressionHost"
Cohesion: 0.21
Nodes (3): ProgressionHost, BootError, Controller

### Community 52 - "ProgressionScreen"
Cohesion: 0.20
Nodes (10): ProgressionScreen, Closed, Defeat, Loading, Loadout, Playing, Recovery, Start (+2 more)

### Community 53 - "Transform"
Cohesion: 0.24
Nodes (4): PlatformMotor, Mode, CameraFollow, HealthBarView

### Community 56 - "ProgressionRunSession"
Cohesion: 0.18
Nodes (8): ProgressionRunSession, LastCompletionResult, Run, RunStatus, Defeat, Playing, StartScreen, Victory

### Community 57 - "LoadoutMenuView"
Cohesion: 0.29
Nodes (6): LoadoutMenuView, CanEdit, CanRestoreDefaults, Slots, UnknownOwnedIds, Validation

### Community 58 - "LoadoutMenuOption"
Cohesion: 0.15
Nodes (13): LoadoutOperation, Equip, Replace, RestoreDefaults, Unequip, LoadoutMenuOption, Availability, CanInstall (+5 more)

### Community 60 - "RunReceipt"
Cohesion: 0.18
Nodes (10): RunReceipt, AwardedCredits, IsTerminal, Outcome, RunId, SavedRunOutcome, Abandoned, Active (+2 more)

### Community 63 - "ProgressionLoadError"
Cohesion: 0.29
Nodes (6): ProgressionLoadError, Corrupt, InterruptedWrite, InvalidValues, StorageUnavailable, UnsupportedSchema

### Community 64 - "LevelState"
Cohesion: 0.33
Nodes (6): LevelState, BetweenEncounters, Defeat, EncounterActive, Ready, Victory

### Community 69 - "IRunIdGenerator"
Cohesion: 0.38
Nodes (3): GuidRunIdGenerator, IRunIdGenerator, FixedId

## Knowledge Gaps
- **363 isolated node(s):** `Id`, `MaximumHealth`, `MovementSpeed`, `Attack`, `EnergyDrop` (+358 more)
  These have ≤1 connection - possible missing edges. (Counts symbols only; 570 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **34 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `ProgressionController` connect `ProgressionController` to `ProgressionResult`, `system`, `GameRun`, `IRunIdGenerator`, `ProgressionController.Load`, `Test`, `JsonProgressionCodec`, `OutpostSession`, `ProgressionMenuView`, `GameplayIntegrationTests.SetUp`, `ProgressionRecoveryTests`, `PrototypeHud`, `Test`, `ProgressionHost`, `ProgressionConfiguration`, `ProgressionScreen`, `ProgressionRunSession`, `ProgressionLoadError`?**
  _High betweenness centrality (0.092) - this node is a cross-community bridge._
- **Are the 51 inferred relationships involving `ProgressionService` (e.g. with `ProgressionController.Load()` and `EquipmentPersistenceTests.ActiveRunEquipmentAndStatsCannotBeChangedAndTheNextRunUsesTheNewSelection()`) actually correct?**
  _`ProgressionService` has 51 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Id`, `MaximumHealth`, `MovementSpeed` to the rest of the system?**
  _363 weakly-connected nodes found - possible missing edges._
- **Should `system` be split into smaller, more focused modules?**
  _Cohesion score 0.06073674752920036 - nodes in this community are weakly interconnected._
- **Why does `GameRun` connect `GameRun` to `EnemySpawner`, `system`, `LoadoutSnapshot`, `ProgressionData`, `ProgressionController`, `PlatformState`, `PrototypeHud`, `Test`, `ProgressionTests`, `RunConfiguration`, `ProgressionRunSession`, `CombatFeedback`?**
  _High betweenness centrality (0.044) - this node is a cross-community bridge._
- **Are the 16 inferred relationships involving `ProgressionController` (e.g. with `ProgressionHost.EnsureBoot()` and `EquipmentPersistenceTests.FailedLoadoutSaveBlocksTheUiUntilReloadAndDoesNotAutomaticallyRepeatTheChange()`) actually correct?**
  _`ProgressionController` has 16 INFERRED edges - model-reasoned connections that need verification._
- **Should `with` be split into smaller, more focused modules?**
  _Cohesion score 0.058823529411764705 - nodes in this community are weakly interconnected._