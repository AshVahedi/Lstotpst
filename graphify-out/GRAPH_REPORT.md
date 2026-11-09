# Graph Report - Lstotpst  (2026-11-09)

## Corpus Check
- 75 files · ~61,991 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 26 file(s) not represented in the graph (top: (none) 16, .asmdef 8, .xml 1)
- WARNING: 26 of 123 files seen (21%) have no supported extractor and are missing from this graph entirely. It may look complete while representing only part of the codebase.

## Summary
- 1600 nodes · 4619 edges · 72 communities (57 shown, 15 thin omitted)
- Extraction: 75% EXTRACTED · 25% INFERRED · 0% AMBIGUOUS · INFERRED: 1168 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `8e7cbd11`
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
- ProgressionController
- EnemyState
- LoadoutSnapshot
- ProgressionData
- OutpostSession
- ShotEvent
- UpgradeDefinition
- Test
- EquipmentTests
- ProgressionAdapterTests
- PrototypeContent
- LoadoutResult
- EquipmentDefinition
- ProgressionConfiguration.Prototype
- PrototypeHud
- EquipmentConfiguration.Prototype
- UpgradeMenuRow
- LoadoutMenuRow
- RunConfiguration
- Guard.NonNegative
- ProgressionScreen
- RunLoadout
- UpgradeKind
- PurchaseResult
- LoadoutMenuView
- LoadoutMenuOption
- RunReceipt
- WeaponConfig
- ProgressionLoadError
- LevelState
- EquipmentStat
- IRunIdGenerator
- GameplayIntegrationTests.SetUp
- LoadoutRunBuilder.Build

## God Nodes (most connected - your core abstractions)
1. `ProgressionService` - 84 edges
2. `ProgressionController` - 71 edges
3. `ProgressionData` - 60 edges
4. `GameRun` - 56 edges
5. `OutpostSession` - 44 edges
6. `ProgressionTests` - 43 edges
7. `ProgressionResult` - 42 edges
8. `PrototypeHud` - 41 edges
9. `PrototypeContent` - 39 edges
10. `Health` - 38 edges

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

## Communities (72 total, 15 thin omitted)

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
Cohesion: 0.07
Nodes (28): EnemyDefinition, Attack, EnergyDrop, Id, IsBoss, MaximumHealth, MovementSpeed, LevelDefinition (+20 more)

### Community 7 - "PlatformState"
Cohesion: 0.14
Nodes (15): ModuleDefinition, Id, Kind, ModuleKind, Armor, BasicGun, MissileLauncher, Reactor (+7 more)

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
Cohesion: 0.18
Nodes (8): ResourceType, Energy, ResourceWallet, TacticalPurchases, Config, NextSpecialTimeSeconds, SpecialAttack, EconomyTests

### Community 16 - "Test"
Cohesion: 0.13
Nodes (7): ProgressionService, Configuration, RequiresReload, MemoryProgressionStore, Backups, EquipmentPersistenceTests, FailingPromotion

### Community 17 - "FileProgressionStore"
Cohesion: 0.07
Nodes (11): FileProgressionStore, HasPendingWrite, PendingPath, SavePath, IProgressionRecoveryStore, IProgressionStore, StoreCommitResult, Committed (+3 more)

### Community 20 - "ProgressionConfiguration"
Cohesion: 0.19
Nodes (11): LevelRewardDefinition, LevelId, LevelNumber, VictoryCredits, ProgressionConfiguration, BasePlatform, BaseWeaponMultiplier, Equipment (+3 more)

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

### Community 29 - "EquipmentSlot"
Cohesion: 0.11
Nodes (18): EquipmentCategory, AdvancedWeapon, BasicWeapon, DefensiveModule, SupportModule, EquipmentSlot, AllowedCategories, Capacity (+10 more)

### Community 30 - "LoadoutRules.Change"
Cohesion: 0.19
Nodes (7): EquipmentConfiguration, Catalog, Slots, TotalCapacity, EquipmentInstallationData, EquipmentProfileData, LoadoutRules

### Community 31 - "Health"
Cohesion: 0.13
Nodes (12): Health, Current, DamageReduction, IsDead, Maximum, Missing, PlatformState, AdvancedAmmunition (+4 more)

### Community 32 - "ProgressionController"
Cohesion: 0.06
Nodes (36): ProgressionController, CanUseMenu, IsBusy, IsInitialized, LastBackup, LastResult, Message, RequiresReload (+28 more)

### Community 33 - "EnemyState"
Cohesion: 0.18
Nodes (10): EnemyState, Attack, Definition, DropCollected, Health, Id, EnemyAgent, DefeatReported (+2 more)

### Community 34 - "LoadoutSnapshot"
Cohesion: 0.12
Nodes (15): InstalledEquipment, EquipmentId, SlotId, LoadoutSnapshot, Installations, OwnedEquipmentIds, ProgressionSnapshot, CompletedLevelIds (+7 more)

### Community 35 - "ProgressionData"
Cohesion: 0.10
Nodes (13): IProgressionCodec, JsonProgressionCodec, ProgressionData, ProgressionLoadException, Reason, RunRecordData, SchemaHeader, UpgradeLevelsData (+5 more)

### Community 36 - "OutpostSession"
Cohesion: 0.11
Nodes (8): OutpostSession, CanCommand, IsPaused, Progression, Run, ProgressionHost, BootError, Controller

### Community 37 - "ShotEvent"
Cohesion: 0.15
Nodes (11): RunCombat, ShotEvent, FromX, Kind, TargetEnemyId, ToX, ShotKind, BasicGun (+3 more)

### Community 38 - "UpgradeDefinition"
Cohesion: 0.19
Nodes (6): UpgradeDefinition, BaseCost, GrowthFactor, IncreasePerLevel, Kind, MaximumLevel

### Community 39 - "Test"
Cohesion: 0.18
Nodes (4): ProgressionMenuBinding, FaultStore, ProbeStore, ProgressionIntegrationTests

### Community 40 - "EquipmentTests"
Cohesion: 0.28
Nodes (4): EquipmentModifier, Stat, Value, EquipmentTests

### Community 42 - "PrototypeContent"
Cohesion: 0.14
Nodes (14): PrototypeContent, BasicEnemy, BasicGun, Boss, FastEnemy, LevelOne, MissileLauncher, Purchases (+6 more)

### Community 43 - "LoadoutResult"
Cohesion: 0.09
Nodes (23): LoadoutResult, AlreadyInstalled, Busy, CapacityExceeded, Conflict, DisabledSlot, DuplicateInstallation, Incompatible (+15 more)

### Community 44 - "EquipmentDefinition"
Cohesion: 0.15
Nodes (12): EquipmentDefinition, AvailableByDefault, Category, CompatibleSlots, Description, DisplayName, Id, InstallationSize (+4 more)

### Community 45 - "ProgressionConfiguration.Prototype"
Cohesion: 0.15
Nodes (4): FailingFileStore, FailingRecoveryStore, LostAcknowledgmentStore, ProgressionRecoveryTests

### Community 46 - "PrototypeHud"
Cohesion: 0.10
Nodes (3): PrototypeHud, LastFeedback, ProgressionView

### Community 48 - "UpgradeMenuRow"
Cohesion: 0.18
Nodes (12): UpgradeMenuRow, CanPurchase, CurrentEffect, CurrentText, IsMaximum, Kind, Level, MaximumLevel (+4 more)

### Community 49 - "LoadoutMenuRow"
Cohesion: 0.20
Nodes (10): LoadoutMenuRow, CanUnequip, Category, Enabled, EquipmentId, EquipmentName, Options, Required (+2 more)

### Community 50 - "RunConfiguration"
Cohesion: 0.12
Nodes (11): RunConfiguration, BasicGun, Equipment, InitialAmmunition, Level, MissileLauncher, Platform, Purchases (+3 more)

### Community 52 - "ProgressionScreen"
Cohesion: 0.20
Nodes (10): ProgressionScreen, Closed, Defeat, Loading, Loadout, Playing, Recovery, Start (+2 more)

### Community 53 - "RunLoadout"
Cohesion: 0.18
Nodes (9): ResolvedInstallation, Definition, SlotId, SlotIndex, RunLoadout, EnergyPerDefeat, HasAdvancedWeapon, Installations (+1 more)

### Community 54 - "UpgradeKind"
Cohesion: 0.28
Nodes (5): UpgradeKind, AmmunitionCapacity, Armor, MaximumHealth, WeaponDamage

### Community 56 - "PurchaseResult"
Cohesion: 0.29
Nodes (7): PurchaseResult, CoolingDown, InsufficientEnergy, NoEffect, PlatformDestroyed, RunNotActive, Succeeded

### Community 57 - "LoadoutMenuView"
Cohesion: 0.29
Nodes (6): LoadoutMenuView, CanEdit, CanRestoreDefaults, Slots, UnknownOwnedIds, Validation

### Community 58 - "LoadoutMenuOption"
Cohesion: 0.15
Nodes (13): LoadoutOperation, Equip, Replace, RestoreDefaults, Unequip, LoadoutMenuOption, Availability, CanInstall (+5 more)

### Community 60 - "RunReceipt"
Cohesion: 0.18
Nodes (10): RunReceipt, AwardedCredits, IsTerminal, Outcome, RunId, SavedRunOutcome, Abandoned, Active (+2 more)

### Community 62 - "WeaponConfig"
Cohesion: 0.13
Nodes (15): ClosestTargetStrategy, ITargetingStrategy, WeaponConfig, AmmunitionPerShot, AttackIntervalSeconds, Damage, Id, Range (+7 more)

### Community 63 - "ProgressionLoadError"
Cohesion: 0.33
Nodes (6): ProgressionLoadError, Corrupt, InterruptedWrite, InvalidValues, StorageUnavailable, UnsupportedSchema

### Community 64 - "LevelState"
Cohesion: 0.33
Nodes (6): LevelState, BetweenEncounters, Defeat, EncounterActive, Ready, Victory

### Community 65 - "EquipmentStat"
Cohesion: 0.33
Nodes (6): EquipmentStat, AmmunitionCapacity, DamageReduction, EnergyPerDefeat, MaximumHealth, WeaponDamageMultiplier

### Community 69 - "IRunIdGenerator"
Cohesion: 0.36
Nodes (3): GuidRunIdGenerator, IRunIdGenerator, FixedId

### Community 77 - "LoadoutRunBuilder.Build"
Cohesion: 0.27
Nodes (3): LoadoutRunBuilder, PermanentStatsProvider, WeaponMultiplier

## Knowledge Gaps
- **363 isolated node(s):** `MaximumHealth`, `Armor`, `WeaponDamage`, `AmmunitionCapacity`, `Kind` (+358 more)
  These have ≤1 connection - possible missing edges. (Counts symbols only; 563 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **15 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `ProgressionController` connect `ProgressionController` to `system`, `ProgressionData`, `GameRun`, `IRunIdGenerator`, `OutpostSession`, `Test`, `GameplayIntegrationTests`, `ProgressionMenuView`, `GameplayIntegrationTests.SetUp`, `ProgressionConfiguration.Prototype`, `Test`, `ProgressionConfiguration`, `ProgressionScreen`?**
  _High betweenness centrality (0.101) - this node is a cross-community bridge._
- **Are the 51 inferred relationships involving `ProgressionService` (e.g. with `ProgressionController.Load()` and `EquipmentPersistenceTests.ActiveRunEquipmentAndStatsCannotBeChangedAndTheNextRunUsesTheNewSelection()`) actually correct?**
  _`ProgressionService` has 51 INFERRED edges - model-reasoned connections that need verification._
- **What connects `MaximumHealth`, `Armor`, `WeaponDamage` to the rest of the system?**
  _363 weakly-connected nodes found - possible missing edges._
- **Should `EnemySpawner` be split into smaller, more focused modules?**
  _Cohesion score 0.10128205128205128 - nodes in this community are weakly interconnected._
- **Why does `GameRun` connect `GameRun` to `ProgressionController`, `EnemySpawner`, `system`, `ShotEvent`, `PlatformState`, `Test`, `ProgressionTests`, `RunConfiguration`?**
  _High betweenness centrality (0.049) - this node is a cross-community bridge._
- **Are the 16 inferred relationships involving `ProgressionController` (e.g. with `ProgressionHost.EnsureBoot()` and `EquipmentPersistenceTests.FailedLoadoutSaveBlocksTheUiUntilReloadAndDoesNotAutomaticallyRepeatTheChange()`) actually correct?**
  _`ProgressionController` has 16 INFERRED edges - model-reasoned connections that need verification._
- **Should `system` be split into smaller, more focused modules?**
  _Cohesion score 0.06208791208791209 - nodes in this community are weakly interconnected._