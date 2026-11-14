# Graph Report - Lstotpst  (2026-11-14)

## Corpus Check
- 94 files · ~85,073 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 27 file(s) not represented in the graph (top: (none) 16, .asmdef 8, .xml 1)

## Summary
- 2011 nodes · 6287 edges · 96 communities (64 shown, 32 thin omitted)
- Extraction: 74% EXTRACTED · 26% INFERRED · 0% AMBIGUOUS · INFERRED: 1632 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `0f0f435c`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- ShipAssemblyView
- ShipAssemblyView
- system
- with
- Health
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
- SpaceWeaponDefinition
- OutpostSession
- ProgressionConfiguration.Prototype
- UnityCiTests
- SpaceCombatTests
- AssemblyAuditTests
- steps
- unity-verification.yml
- Health
- build_android
- ShipFoundationTests
- env
- editor
- com.unity.ext.nunit
- ProgressionData
- LoadoutRules.Change
- ProgressionSnapshot
- ProgressionAdapterTests
- SceneBuilderTests
- HorizontalShipLayout
- EquipmentTests
- WeaponConfig
- LoadoutRules.Change
- SpaceProjectile
- PrototypeContent
- LoadoutResult
- GameRun
- ArgumentOutOfRangeException
- OutpostSession.StepSimulation
- LevelState
- HorizontalShipLayout.Bounds
- ProgressionController
- SpaceCombatView
- ProgressionService
- SpaceShipSettings.Build
- EnemySpawner
- SpaceCombatConfiguration
- ShipPoint
- OutpostSession
- Test
- LoadoutMenuRow
- UnitySyntaxCheck.csproj
- EquipmentTests.PermanentThenEquipmentOrderingRoundsWeaponDamageOnlyAtTheEnd
- EquipmentDefinition
- ProgressionLoadError
- PlatformState
- SpaceSceneBuilder
- EnemyState
- ProgressionTests.DuplicateAndMalformedRunIdsAreRejectedDurably
- PurchaseResult
- SlotCategory
- TestCase
- RunReceipt
- RunConfiguration
- SpaceEnemy
- SpaceCombatPlayModeTests
- LoadoutMenuOption
- WeaponFacing
- infrastructure
- GeneratorTestWorkspace
- SpaceCombatRun

## God Nodes (most connected - your core abstractions)
1. `ProgressionService` - 85 edges
2. `ProgressionController` - 73 edges
3. `ProgressionData` - 67 edges
4. `MemoryProgressionStore` - 66 edges
5. `GameRun` - 60 edges
6. `SpaceCombatRun` - 48 edges
7. `ProgressionTests` - 43 edges
8. `OutpostSession` - 43 edges
9. `SpaceCombatTests` - 42 edges
10. `Health` - 42 edges

## Surprising Connections (you probably didn't know these)
- `Snapshot` --calls--> `ProgressionSnapshot`  [INFERRED]
  Assets/Scripts/Core/Progression/ProgressionService.cs → Assets/Scripts/Core/Progression/ProgressionData.cs
- `SpaceWeaponInstance` --references--> `SpaceWeaponDefinition`  [EXTRACTED]
  Assets/Scripts/Core/Combat/SpaceWeaponInstance.cs → Assets/Scripts/Core/Combat/SpaceCombatConfiguration.cs
- `RunConfiguration` --references--> `SpaceCombatConfiguration`  [EXTRACTED]
  Assets/Scripts/Core/Simulation/RunConfiguration.cs → Assets/Scripts/Core/Combat/SpaceCombatConfiguration.cs
- `SpaceCombatRun` --references--> `SpaceCombatConfiguration`  [EXTRACTED]
  Assets/Scripts/Core/Simulation/SpaceCombatRun.cs → Assets/Scripts/Core/Combat/SpaceCombatConfiguration.cs
- `SpaceCombatRun` --references--> `SpaceWeaponInstance`  [EXTRACTED]
  Assets/Scripts/Core/Simulation/SpaceCombatRun.cs → Assets/Scripts/Core/Combat/SpaceWeaponInstance.cs

## Import Cycles
- None detected.

## Communities (96 total, 32 thin omitted)

### Community 1 - "ShipAssemblyView"
Cohesion: 0.16
Nodes (4): ShipAssemblyView, Bounds, InstalledModuleCount, ValidationError

### Community 2 - "system"
Cohesion: 0.05
Nodes (27): ArrivalSide, Front, Left, Rear, Right, ShipSide, Left, Right (+19 more)

### Community 3 - "with"
Cohesion: 0.06
Nodes (33): env, runs-on, steps, timeout-minutes, GRAPHIFY_BACKEND, GRAPHIFY_MAX_WORKERS, GRAPHIFY_OUT, GRAPHIFY_QUERY_LOG_DISABLE (+25 more)

### Community 4 - "Health"
Cohesion: 0.16
Nodes (7): Health, Current, DamageReduction, IsDead, Maximum, Missing, HealthTests

### Community 5 - "PrototypeContent.PrototypeContent"
Cohesion: 0.10
Nodes (24): SpecialAttackConfig, CooldownSeconds, Damage, EnergyCost, Range, Targeting, TacticalPurchaseConfig, AmmunitionAmount (+16 more)

### Community 6 - "PrototypeHud"
Cohesion: 0.13
Nodes (3): PrototypeHud, LastFeedback, ProgressionView

### Community 7 - "ModuleDefinition"
Cohesion: 0.15
Nodes (15): ModuleDefinition, Id, Kind, ModuleKind, Armor, BasicGun, MissileLauncher, Reactor (+7 more)

### Community 8 - "steps"
Cohesion: 0.09
Nodes (21): runs-on, steps, timeout-minutes, jobs, core-tests, name, on, pull_request (+13 more)

### Community 9 - "LevelProgression"
Cohesion: 0.15
Nodes (9): LevelProgression, ActiveEnemies, BossActive, CurrentWave, Definition, Progress, RemainingEnemies, State (+1 more)

### Community 10 - "ProgressionMenuView"
Cohesion: 0.06
Nodes (36): ProgressionScreen, Closed, Defeat, Loading, Loadout, Playing, Recovery, Start (+28 more)

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
Cohesion: 0.16
Nodes (8): ResourceType, Energy, ResourceWallet, TacticalPurchases, Config, NextSpecialTimeSeconds, SpecialAttack, EconomyTests

### Community 16 - "SpaceWeaponDefinition"
Cohesion: 0.14
Nodes (11): SpaceWeaponDefinition, ArcDegrees, Cooldown, Damage, Facing, Id, Instances, ProjectileSpeed (+3 more)

### Community 17 - "OutpostSession"
Cohesion: 0.07
Nodes (11): FileProgressionStore, HasPendingWrite, PendingPath, SavePath, IProgressionRecoveryStore, StoreCommitResult, Committed, Conflict (+3 more)

### Community 18 - "ProgressionConfiguration.Prototype"
Cohesion: 0.10
Nodes (22): LevelRewardDefinition, LevelId, LevelNumber, VictoryCredits, ProgressionConfiguration, BasePlatform, BaseWeaponMultiplier, Equipment (+14 more)

### Community 20 - "SpaceCombatTests"
Cohesion: 0.18
Nodes (9): SpaceWeaponInstance, Definition, Enabled, InstanceId, LocalFacing, LocalPosition, NextFireTime, ShotsFired (+1 more)

### Community 22 - "steps"
Cohesion: 0.24
Nodes (14): steps, steps, UNITY_EMAIL, UNITY_LICENSE, UNITY_LICENSE_SERVER, UNITY_PASSWORD, UNITY_SERIAL, steps (+6 more)

### Community 23 - "unity-verification.yml"
Cohesion: 0.14
Nodes (13): if, name, needs, runs-on, timeout-minutes, concurrency, cancel-in-progress, group (+5 more)

### Community 24 - "Health"
Cohesion: 0.16
Nodes (14): Ammunition, Capacity, Count, ClosestTargetStrategy, ITargetingStrategy, Target, Distance, Health (+6 more)

### Community 25 - "build_android"
Cohesion: 0.22
Nodes (9): default, description, type, build_android, on, pull_request, workflow_dispatch, paths (+1 more)

### Community 26 - "ShipFoundationTests"
Cohesion: 0.27
Nodes (5): ShipAttachmentPoint, Side, SlotId, Step, ShipFoundationTests

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
Cohesion: 0.10
Nodes (14): IProgressionCodec, ProgressionData, ProgressionLoadException, Reason, RunRecordData, UpgradeLevelsData, LegacyProgressionData, ProgressionService (+6 more)

### Community 32 - "LoadoutRules.Change"
Cohesion: 0.28
Nodes (3): EquipmentInstallationData, EquipmentProfileData, LoadoutRules

### Community 34 - "ProgressionSnapshot"
Cohesion: 0.22
Nodes (8): ProgressionSnapshot, CompletedLevelIds, Credits, HighestUnlockedLevel, Loadout, RegisteredRuns, Revision, RewardedRuns

### Community 37 - "HorizontalShipLayout"
Cohesion: 0.18
Nodes (10): HorizontalShipLayout, BayWidth, CoreWidth, Gap, MaximumHalfExtent, Points, WidthLimit, ShipPoint (+2 more)

### Community 38 - "EquipmentTests"
Cohesion: 0.18
Nodes (14): EquipmentCatalog, Definitions, EquipmentConfiguration, Catalog, Slots, TotalCapacity, EquipmentSlot, AllowedCategories (+6 more)

### Community 39 - "WeaponConfig"
Cohesion: 0.12
Nodes (20): WeaponConfig, AmmunitionPerShot, AttackIntervalSeconds, Damage, Id, Range, Targeting, EnemyDefinition (+12 more)

### Community 40 - "LoadoutRules.Change"
Cohesion: 0.24
Nodes (7): InstalledEquipment, EquipmentId, SlotId, LoadoutSnapshot, Installations, OwnedEquipmentIds, EquipmentTests

### Community 41 - "SpaceProjectile"
Cohesion: 0.22
Nodes (9): SpaceProjectile, Active, Hostile, Origin, Position, Serial, TargetId, Velocity (+1 more)

### Community 42 - "PrototypeContent"
Cohesion: 0.15
Nodes (12): PrototypeContent, BasicEnemy, BasicGun, Boss, FastEnemy, LevelOne, MissileLauncher, Purchases (+4 more)

### Community 43 - "LoadoutResult"
Cohesion: 0.09
Nodes (23): LoadoutResult, AlreadyInstalled, Busy, CapacityExceeded, Conflict, DisabledSlot, DuplicateInstallation, Incompatible (+15 more)

### Community 44 - "GameRun"
Cohesion: 0.06
Nodes (34): GameRun, Configuration, ElapsedSeconds, Enemies, Level, Platform, Route, RunId (+26 more)

### Community 45 - "ArgumentOutOfRangeException"
Cohesion: 0.23
Nodes (9): EquipmentModifier, Stat, Value, EquipmentStat, AmmunitionCapacity, DamageReduction, EnergyPerDefeat, MaximumHealth (+1 more)

### Community 47 - "LevelState"
Cohesion: 0.33
Nodes (6): LevelState, BetweenEncounters, Defeat, EncounterActive, Ready, Victory

### Community 48 - "HorizontalShipLayout.Bounds"
Cohesion: 0.33
Nodes (5): ShipBounds, HalfExtent, Left, Right, Width

### Community 49 - "ProgressionController"
Cohesion: 0.04
Nodes (42): ProgressionController, CanUseMenu, IsBusy, IsInitialized, LastBackup, LastResult, Message, RequiresReload (+34 more)

### Community 51 - "ProgressionService"
Cohesion: 0.12
Nodes (5): MemoryProgressionStore, Backups, EquipmentPersistenceTests, FaultStore, ProgressionTests

### Community 53 - "EnemySpawner"
Cohesion: 0.08
Nodes (15): EnemyActor, Agent, EnemySpawner, ActiveCount, GeneratedLevelRoot, RouteOrientation, LegacyHorizontal, SpaceForwardY (+7 more)

### Community 54 - "SpaceCombatConfiguration"
Cohesion: 0.11
Nodes (15): SpaceCombatConfiguration, EnemyLifetime, EnemyProjectileSpeed, EquipmentWeapons, HullLaser, Layout, MaximumEnemies, MaximumProjectiles (+7 more)

### Community 55 - "ShipPoint"
Cohesion: 0.18
Nodes (5): ShipHitBox, Center, HalfHeight, HalfWidth, SpaceGeometry

### Community 56 - "OutpostSession"
Cohesion: 0.10
Nodes (9): OutpostSession, CanCommand, IsPaused, Progression, Run, UsesSpaceCombat, ProgressionHost, BootError (+1 more)

### Community 57 - "Test"
Cohesion: 0.17
Nodes (5): ProgressionMenuBinding, IProgressionStore, ProbeStore, ProgressionIntegrationTests, UnsupportedReader

### Community 58 - "LoadoutMenuRow"
Cohesion: 0.12
Nodes (16): LoadoutMenuRow, CanUnequip, Category, Enabled, EquipmentId, EquipmentName, Options, Required (+8 more)

### Community 62 - "EquipmentDefinition"
Cohesion: 0.11
Nodes (17): EquipmentCategory, AdvancedWeapon, BasicWeapon, DefensiveModule, SupportModule, EquipmentDefinition, AvailableByDefault, Category (+9 more)

### Community 63 - "ProgressionLoadError"
Cohesion: 0.33
Nodes (6): ProgressionLoadError, Corrupt, InterruptedWrite, InvalidValues, StorageUnavailable, UnsupportedSchema

### Community 64 - "PlatformState"
Cohesion: 0.13
Nodes (16): PlatformState, AdvancedAmmunition, Health, Modules, Resources, RunCombat, ShotEvent, FromX (+8 more)

### Community 66 - "EnemyState"
Cohesion: 0.21
Nodes (10): EnemyState, Attack, Definition, DropCollected, Health, Id, EnemyAgent, DefeatReported (+2 more)

### Community 67 - "ProgressionTests.DuplicateAndMalformedRunIdsAreRejectedDurably"
Cohesion: 0.38
Nodes (3): GuidRunIdGenerator, IRunIdGenerator, FixedId

### Community 69 - "PurchaseResult"
Cohesion: 0.29
Nodes (7): PurchaseResult, CoolingDown, InsufficientEnergy, NoEffect, PlatformDestroyed, RunNotActive, Succeeded

### Community 71 - "SlotCategory"
Cohesion: 0.40
Nodes (5): SlotCategory, Defense, PrimaryWeapon, SecondaryWeapon, Utility

### Community 82 - "RunReceipt"
Cohesion: 0.18
Nodes (10): RunReceipt, AwardedCredits, IsTerminal, Outcome, RunId, SavedRunOutcome, Abandoned, Active (+2 more)

### Community 84 - "RunConfiguration"
Cohesion: 0.06
Nodes (22): ResolvedInstallation, Definition, SlotId, SlotIndex, RunLoadout, EnergyPerDefeat, HasAdvancedWeapon, Installations (+14 more)

### Community 88 - "SpaceEnemy"
Cohesion: 0.15
Nodes (9): SpaceEnemy, Active, Agent, Escaped, Position, PreviousPosition, Radius, Side (+1 more)

### Community 91 - "SpaceCombatPlayModeTests"
Cohesion: 0.12
Nodes (4): SpaceCombatSettings, SpaceCombatNativeSceneTests, SpaceCombatPlayModeTests, SpaceFoundationPlayModeTests

### Community 94 - "LoadoutMenuOption"
Cohesion: 0.15
Nodes (13): LoadoutOperation, Equip, Replace, RestoreDefaults, Unequip, LoadoutMenuOption, Availability, CanInstall (+5 more)

### Community 95 - "WeaponFacing"
Cohesion: 0.33
Nodes (6): WeaponFacing, Forward, Left, Outboard, Rear, Right

### Community 97 - "infrastructure"
Cohesion: 0.40
Nodes (5): if, name, runs-on, timeout-minutes, infrastructure

### Community 104 - "SpaceCombatRun"
Cohesion: 0.09
Nodes (14): SpaceCombatRun, ContactCount, Enemies, ExpiredEnemies, Forward, FriendlyHits, HalfHeight, HalfWidth (+6 more)

## Knowledge Gaps
- **464 isolated node(s):** `Id`, `Damage`, `Cooldown`, `Range`, `ArcDegrees` (+459 more)
  These have ≤1 connection - possible missing edges. (Counts symbols only; 690 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **32 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `SpaceCombatRun` connect `SpaceCombatRun` to `ShipAssemblyView`, `system`, `WeaponConfig`, `SpaceProjectile`, `LevelProgression`, `GameRun`, `SpaceCombatView`, `SpaceCombatTests`, `SpaceCombatConfiguration`, `ShipPoint`, `SpaceEnemy`, `LoadoutMenuRow`?**
  _High betweenness centrality (0.048) - this node is a cross-community bridge._
- **Are the 52 inferred relationships involving `ProgressionService` (e.g. with `ProgressionController.Load()` and `EquipmentPersistenceTests.ActiveRunEquipmentAndStatsCannotBeChangedAndTheNextRunUsesTheNewSelection()`) actually correct?**
  _`ProgressionService` has 52 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Id`, `Damage`, `Cooldown` to the rest of the system?**
  _464 weakly-connected nodes found - possible missing edges._
- **Should `system` be split into smaller, more focused modules?**
  _Cohesion score 0.05238299699441821 - nodes in this community are weakly interconnected._
- **Why does `ProgressionController` connect `ProgressionController` to `system`, `ProgressionTests.DuplicateAndMalformedRunIdsAreRejectedDurably`, `ProgressionMenuView`, `GameRun`, `ProgressionConfiguration.Prototype`, `ProgressionService`, `OutpostSession`, `Test`, `ProgressionData`?**
  _High betweenness centrality (0.048) - this node is a cross-community bridge._
- **Are the 21 inferred relationships involving `ProgressionController` (e.g. with `ProgressionHost.EnsureBoot()` and `EquipmentPersistenceTests.FailedLoadoutSaveBlocksTheUiUntilReloadAndDoesNotAutomaticallyRepeatTheChange()`) actually correct?**
  _`ProgressionController` has 21 INFERRED edges - model-reasoned connections that need verification._
- **Should `with` be split into smaller, more focused modules?**
  _Cohesion score 0.058823529411764705 - nodes in this community are weakly interconnected._