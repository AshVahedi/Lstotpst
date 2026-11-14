# Graph Report - Lstotpst  (2026-11-13)

## Corpus Check
- 85 files · ~73,870 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 26 file(s) not represented in the graph (top: (none) 16, .asmdef 8, .xml 1)

## Summary
- 1778 nodes · 5362 edges · 97 communities (52 shown, 45 thin omitted)
- Extraction: 74% EXTRACTED · 26% INFERRED · 0% AMBIGUOUS · INFERRED: 1400 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `4e2f53bb`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- ShipAssemblyView
- ShipAssemblyView
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
- ProgressionTests
- OutpostSession
- ProgressionConfiguration.Prototype
- UnityCiTests
- PlatformStats
- AssemblyAuditTests
- steps
- unity-verification.yml
- Health
- build_android
- HorizontalShipLayout
- env
- editor
- com.unity.ext.nunit
- ProgressionData
- ProgressionResult
- ProgressionSnapshot
- ProgressionAdapterTests
- ProgressionController
- EquipmentTests
- EquipmentSlot
- LoadoutRules.Change
- PrototypeContent
- LoadoutResult
- GameplayIntegrationTests.SetUp
- EquipmentDefinition
- OutpostSession.StepSimulation
- LevelState
- InstalledEquipment
- ProgressionRecoveryTests
- Health
- ProgressionService
- UpgradeLevelsData
- EnemySpawner
- ProgressionRunSession
- Test
- LoadoutMenuRow
- UnitySyntaxCheck.csproj
- RunConfiguration
- ProgressionLoadError
- PlatformState
- IRunIdGenerator
- PrototypeSceneBuilder.Generate
- RunReceipt
- ProgressionService.ChangeLoadout
- infrastructure

## God Nodes (most connected - your core abstractions)
1. `ProgressionService` - 84 edges
2. `ProgressionController` - 70 edges
3. `ProgressionData` - 64 edges
4. `MemoryProgressionStore` - 59 edges
5. `GameRun` - 55 edges
6. `ProgressionTests` - 43 edges
7. `ProgressionResult` - 42 edges
8. `Health` - 42 edges
9. `PrototypeContent` - 39 edges
10. `LoadoutResult` - 36 edges

## Surprising Connections (you probably didn't know these)
- `LoadoutMenuView` --references--> `LoadoutResult`  [EXTRACTED]
  Assets/Scripts/Core/Progression/LoadoutMenuView.cs → Assets/Scripts/Core/Modules/EquipmentProfile.cs
- `ProgressionController` --references--> `ProgressionMenuView`  [EXTRACTED]
  Assets/Scripts/Core/Progression/ProgressionController.cs → Assets/Scripts/Core/Progression/ProgressionMenuView.cs
- `UpgradeMenuRow` --references--> `UpgradeKind`  [EXTRACTED]
  Assets/Scripts/Core/Progression/ProgressionMenuView.cs → Assets/Scripts/Core/Progression/ProgressionConfiguration.cs
- `PlatformState` --references--> `ResourceWallet`  [EXTRACTED]
  Assets/Scripts/Core/Platform/PlatformState.cs → Assets/Scripts/Core/Economy/ResourceWallet.cs
- `TacticalPurchases` --references--> `PlatformState`  [EXTRACTED]
  Assets/Scripts/Core/Economy/TacticalPurchases.cs → Assets/Scripts/Core/Platform/PlatformState.cs

## Import Cycles
- None detected.

## Communities (97 total, 45 thin omitted)

### Community 1 - "ShipAssemblyView"
Cohesion: 0.19
Nodes (5): ShipAssemblyView, Bounds, InstalledModuleCount, ValidationError, SpaceFoundationPlayModeTests

### Community 2 - "system"
Cohesion: 0.06
Nodes (21): LoadoutRunBuilder, RouteProjection, GeneratorTestWorkspace, LastOutpost.Tests.Editor, LastOutpost.Core.Progression, LastOutpost.Tests, LastOutpost.Presentation, LastOutpost.UI (+13 more)

### Community 3 - "with"
Cohesion: 0.06
Nodes (33): env, runs-on, steps, timeout-minutes, GRAPHIFY_BACKEND, GRAPHIFY_MAX_WORKERS, GRAPHIFY_OUT, GRAPHIFY_QUERY_LOG_DISABLE (+25 more)

### Community 4 - "GameRun"
Cohesion: 0.06
Nodes (35): PurchaseResult, CoolingDown, InsufficientEnergy, NoEffect, PlatformDestroyed, RunNotActive, Succeeded, GameRun (+27 more)

### Community 5 - "PrototypeContent.PrototypeContent"
Cohesion: 0.11
Nodes (24): SpecialAttackConfig, CooldownSeconds, Damage, EnergyCost, Range, Targeting, TacticalPurchaseConfig, AmmunitionAmount (+16 more)

### Community 6 - "PrototypeHud"
Cohesion: 0.09
Nodes (4): ProgressionMenuBinding, PrototypeHud, LastFeedback, ProgressionView

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
Cohesion: 0.05
Nodes (42): LoadoutMenuView, CanEdit, CanRestoreDefaults, Slots, UnknownOwnedIds, Validation, ProgressionScreen, Closed (+34 more)

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

### Community 17 - "OutpostSession"
Cohesion: 0.05
Nodes (19): FileProgressionStore, HasPendingWrite, PendingPath, SavePath, IProgressionRecoveryStore, IProgressionStore, StoreCommitResult, Committed (+11 more)

### Community 18 - "ProgressionConfiguration.Prototype"
Cohesion: 0.09
Nodes (22): LevelRewardDefinition, LevelId, LevelNumber, VictoryCredits, ProgressionConfiguration, BasePlatform, BaseWeaponMultiplier, Equipment (+14 more)

### Community 20 - "PlatformStats"
Cohesion: 0.23
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

### Community 26 - "HorizontalShipLayout"
Cohesion: 0.06
Nodes (27): HorizontalShipLayout, BayWidth, CoreWidth, Gap, MaximumHalfExtent, Points, WidthLimit, ShipAttachmentPoint (+19 more)

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
Cohesion: 0.08
Nodes (12): IProgressionCodec, JsonProgressionCodec, ProgressionData, ProgressionLoadException, Reason, RunRecordData, SchemaHeader, LegacyProgressionData (+4 more)

### Community 32 - "ProgressionResult"
Cohesion: 0.09
Nodes (22): ProgressionResult, AlreadyProcessed, Busy, ConfirmationRequired, Conflict, InsufficientCredits, InvalidLoadout, InvalidRun (+14 more)

### Community 34 - "ProgressionSnapshot"
Cohesion: 0.18
Nodes (9): ProgressionSnapshot, CompletedLevelIds, Credits, HighestUnlockedLevel, Loadout, RegisteredRuns, Revision, RewardedRuns (+1 more)

### Community 37 - "ProgressionController"
Cohesion: 0.15
Nodes (11): ProgressionController, CanUseMenu, IsBusy, IsInitialized, LastBackup, LastResult, Message, RequiresReload (+3 more)

### Community 38 - "EquipmentTests"
Cohesion: 0.21
Nodes (10): EquipmentCatalog, Definitions, EquipmentConfiguration, Catalog, Slots, TotalCapacity, LoadoutSnapshot, Installations (+2 more)

### Community 39 - "EquipmentSlot"
Cohesion: 0.11
Nodes (18): EquipmentCategory, AdvancedWeapon, BasicWeapon, DefensiveModule, SupportModule, EquipmentSlot, AllowedCategories, Capacity (+10 more)

### Community 40 - "LoadoutRules.Change"
Cohesion: 0.28
Nodes (3): EquipmentInstallationData, EquipmentProfileData, LoadoutRules

### Community 42 - "PrototypeContent"
Cohesion: 0.08
Nodes (25): WeaponConfig, AmmunitionPerShot, AttackIntervalSeconds, Damage, Id, Range, Targeting, PrototypeContent (+17 more)

### Community 43 - "LoadoutResult"
Cohesion: 0.09
Nodes (23): LoadoutResult, AlreadyInstalled, Busy, CapacityExceeded, Conflict, DisabledSlot, DuplicateInstallation, Incompatible (+15 more)

### Community 45 - "EquipmentDefinition"
Cohesion: 0.10
Nodes (21): EquipmentDefinition, AvailableByDefault, Category, CompatibleSlots, Description, DisplayName, Id, InstallationSize (+13 more)

### Community 47 - "LevelState"
Cohesion: 0.33
Nodes (6): LevelState, BetweenEncounters, Defeat, EncounterActive, Ready, Victory

### Community 48 - "InstalledEquipment"
Cohesion: 0.50
Nodes (3): InstalledEquipment, EquipmentId, SlotId

### Community 49 - "ProgressionRecoveryTests"
Cohesion: 0.08
Nodes (6): FailingFileStore, AcknowledgmentStore, FailingRecoveryStore, LostAcknowledgmentStore, ProgressionRecoveryTests, UncertainResetStore

### Community 50 - "Health"
Cohesion: 0.12
Nodes (17): EnemyState, Attack, Definition, DropCollected, Health, Id, Health, Current (+9 more)

### Community 51 - "ProgressionService"
Cohesion: 0.15
Nodes (7): ProgressionService, Configuration, RequiresReload, MemoryProgressionStore, Backups, EquipmentPersistenceTests, FaultStore

### Community 53 - "EnemySpawner"
Cohesion: 0.09
Nodes (13): EnemyActor, Agent, EnemySpawner, ActiveCount, GeneratedLevelRoot, RouteOrientation, LegacyHorizontal, SpaceForwardY (+5 more)

### Community 56 - "ProgressionRunSession"
Cohesion: 0.13
Nodes (8): ProgressionRunSession, LastCompletionResult, Run, RunStatus, Defeat, Playing, StartScreen, Victory

### Community 58 - "LoadoutMenuRow"
Cohesion: 0.11
Nodes (18): LoadoutMenuOption, Availability, CanInstall, Description, EquipmentId, Name, Operation, Owned (+10 more)

### Community 61 - "RunConfiguration"
Cohesion: 0.09
Nodes (20): ResolvedInstallation, Definition, SlotId, SlotIndex, RunLoadout, EnergyPerDefeat, HasAdvancedWeapon, Installations (+12 more)

### Community 63 - "ProgressionLoadError"
Cohesion: 0.29
Nodes (6): ProgressionLoadError, Corrupt, InterruptedWrite, InvalidValues, StorageUnavailable, UnsupportedSchema

### Community 64 - "PlatformState"
Cohesion: 0.13
Nodes (16): PlatformState, AdvancedAmmunition, Health, Modules, Resources, RunCombat, ShotEvent, FromX (+8 more)

### Community 67 - "IRunIdGenerator"
Cohesion: 0.38
Nodes (3): GuidRunIdGenerator, IRunIdGenerator, FixedId

### Community 76 - "PrototypeSceneBuilder.Generate"
Cohesion: 0.09
Nodes (3): SpaceSceneBuilder, SceneBuilderTests, SpaceSceneBuilderTests

### Community 82 - "RunReceipt"
Cohesion: 0.18
Nodes (10): RunReceipt, AwardedCredits, IsTerminal, Outcome, RunId, SavedRunOutcome, Abandoned, Active (+2 more)

### Community 94 - "ProgressionService.ChangeLoadout"
Cohesion: 0.25
Nodes (5): LoadoutOperation, Equip, Replace, RestoreDefaults, Unequip

### Community 97 - "infrastructure"
Cohesion: 0.40
Nodes (5): if, name, runs-on, timeout-minutes, infrastructure

## Knowledge Gaps
- **390 isolated node(s):** `Bounds`, `ValidationError`, `InstalledModuleCount`, `CanEdit`, `CanRestoreDefaults` (+385 more)
  These have ≤1 connection - possible missing edges. (Counts symbols only; 613 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **45 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `GameRun` connect `GameRun` to `PlatformState`, `system`, `ProgressionController`, `LevelProgression`, `Health.TakeDamage`, `ProgressionTests`, `OutpostSession`, `Health`, `ProgressionService`, `ProgressionRunSession`, `RunConfiguration`, `ProgressionData`?**
  _High betweenness centrality (0.048) - this node is a cross-community bridge._
- **Are the 51 inferred relationships involving `ProgressionService` (e.g. with `ProgressionController.Load()` and `EquipmentPersistenceTests.ActiveRunEquipmentAndStatsCannotBeChangedAndTheNextRunUsesTheNewSelection()`) actually correct?**
  _`ProgressionService` has 51 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Bounds`, `ValidationError`, `InstalledModuleCount` to the rest of the system?**
  _390 weakly-connected nodes found - possible missing edges._
- **Should `system` be split into smaller, more focused modules?**
  _Cohesion score 0.05784313725490196 - nodes in this community are weakly interconnected._
- **Why does `ProgressionController` connect `ProgressionController` to `ProgressionResult`, `system`, `IRunIdGenerator`, `ProgressionController.Load`, `GameRun`, `PrototypeHud`, `ProgressionController.Initialize`, `ProgressionMenuView`, `OutpostSession`, `ProgressionConfiguration.Prototype`, `ProgressionService`, `ProgressionRecoveryTests`, `ProgressionRunSession`, `Test`, `ProgressionData`, `ProgressionLoadError`?**
  _High betweenness centrality (0.044) - this node is a cross-community bridge._
- **Are the 16 inferred relationships involving `ProgressionController` (e.g. with `ProgressionHost.EnsureBoot()` and `EquipmentPersistenceTests.FailedLoadoutSaveBlocksTheUiUntilReloadAndDoesNotAutomaticallyRepeatTheChange()`) actually correct?**
  _`ProgressionController` has 16 INFERRED edges - model-reasoned connections that need verification._
- **Should `with` be split into smaller, more focused modules?**
  _Cohesion score 0.058823529411764705 - nodes in this community are weakly interconnected._