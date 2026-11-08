# Graph Report - Lstotpst  (2026-11-08)

## Corpus Check
- 50 files · ~22,605 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 25 file(s) not represented in the graph (top: (none) 16, .asmdef 8, .props 1)
- WARNING: 25 of 89 files seen (28%) have no supported extractor and are missing from this graph entirely. It may look complete while representing only part of the codebase.

## Summary
- 694 nodes · 2033 edges · 27 communities (23 shown, 4 thin omitted)
- Extraction: 71% EXTRACTED · 29% INFERRED · 0% AMBIGUOUS · INFERRED: 585 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `c7c08455`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- PrototypeSceneBuilder
- Test
- system
- with
- GameRun
- PrototypeSettings.Build
- GameplayIntegrationTests
- PlatformState
- steps
- Test
- EnemyDefinition
- LastOutpost.Core.Tests.csproj
- manifest.json
- update.py
- OutpostSession
- PrototypeSettings.Build
- RunConfiguration
- SpecialAttackConfig
- TacticalPurchaseConfig
- RunCombat
- EnemySpawner
- CombatFeedback
- RunStatus
- Transform
- PrototypeSettings
- SceneBuilderTests.RegenerationPreservesSerializedReferencesSettingsGuidsAndUnrelatedRoots
- PlatformMotor

## God Nodes (most connected - your core abstractions)
1. `GameRun` - 48 edges
2. `Health` - 39 edges
3. `PrototypeContent` - 39 edges
4. `Target` - 35 edges
5. `OutpostSession` - 35 edges
6. `GameplayIntegrationTests` - 31 edges
7. `SimulationTests` - 27 edges
8. `Weapon` - 25 edges
9. `EnemySpawner` - 24 edges
10. `RunConfiguration` - 23 edges

## Surprising Connections (you probably didn't know these)
- `PlatformState` --references--> `Ammunition`  [EXTRACTED]
  Assets/Scripts/Core/Platform/PlatformState.cs → Assets/Scripts/Core/Combat/Ammunition.cs
- `RunCombat` --references--> `Target`  [EXTRACTED]
  Assets/Scripts/Core/Simulation/RunCombat.cs → Assets/Scripts/Core/Combat/Targeting.cs
- `RunCombat` --references--> `Weapon`  [EXTRACTED]
  Assets/Scripts/Core/Simulation/RunCombat.cs → Assets/Scripts/Core/Combat/Weapon.cs
- `PlatformState` --references--> `ResourceWallet`  [EXTRACTED]
  Assets/Scripts/Core/Platform/PlatformState.cs → Assets/Scripts/Core/Economy/ResourceWallet.cs
- `TacticalPurchases` --references--> `PlatformState`  [EXTRACTED]
  Assets/Scripts/Core/Economy/TacticalPurchases.cs → Assets/Scripts/Core/Platform/PlatformState.cs

## Import Cycles
- None detected.

## Communities (27 total, 4 thin omitted)

### Community 1 - "Test"
Cohesion: 0.07
Nodes (42): Ammunition, Capacity, Count, ClosestTargetStrategy, ITargetingStrategy, Target, Distance, Health (+34 more)

### Community 2 - "system"
Cohesion: 0.09
Nodes (17): LastOutpost.Tests.Editor, LastOutpost.Tests, LastOutpost.Presentation, LastOutpost.UI, LastOutpost.Core.Combat, LastOutpost.Core.Configuration, LastOutpost.Editor, LastOutpost.Tests.PlayMode (+9 more)

### Community 3 - "with"
Cohesion: 0.06
Nodes (33): env, runs-on, steps, timeout-minutes, GRAPHIFY_BACKEND, GRAPHIFY_MAX_WORKERS, GRAPHIFY_OUT, GRAPHIFY_QUERY_LOG_DISABLE (+25 more)

### Community 4 - "GameRun"
Cohesion: 0.08
Nodes (27): GameRun, Configuration, ElapsedSeconds, Enemies, Level, Platform, Route, Status (+19 more)

### Community 5 - "PrototypeSettings.Build"
Cohesion: 0.18
Nodes (12): LevelDefinition, DisplayName, Encounters, Id, SpawnGroup, Count, Enemy, WaveDefinition (+4 more)

### Community 6 - "GameplayIntegrationTests"
Cohesion: 0.10
Nodes (10): PurchaseResult, CoolingDown, InsufficientEnergy, NoEffect, PlatformDestroyed, RunNotActive, Succeeded, PrototypeHud (+2 more)

### Community 7 - "PlatformState"
Cohesion: 0.13
Nodes (19): ModuleDefinition, Id, Kind, ModuleKind, Armor, BasicGun, MissileLauncher, Reactor (+11 more)

### Community 8 - "steps"
Cohesion: 0.09
Nodes (21): runs-on, steps, timeout-minutes, jobs, core-tests, name, on, pull_request (+13 more)

### Community 9 - "Test"
Cohesion: 0.10
Nodes (10): ResourceType, Energy, ResourceWallet, TacticalPurchases, Config, NextSpecialTimeSeconds, SpecialAttack, EconomyTests (+2 more)

### Community 10 - "EnemyDefinition"
Cohesion: 0.29
Nodes (7): EnemyDefinition, Attack, EnergyDrop, Id, IsBoss, MaximumHealth, MovementSpeed

### Community 11 - "LastOutpost.Core.Tests.csproj"
Cohesion: 0.28
Nodes (6): netstandard2.1, Microsoft.NET.Test.Sdk (17.11.1), NUnit (3.14.0), NUnit3TestAdapter (4.6.0), Microsoft.NET.Sdk, net8.0

### Community 12 - "manifest.json"
Cohesion: 0.36
Nodes (7): dependencies, com.unity.modules.imageconversion@1.0.0, com.unity.modules.imgui@1.0.0, com.unity.test-framework@1.4.5, com.unity.modules.imageconversion, com.unity.modules.imgui, com.unity.test-framework

### Community 13 - "update.py"
Cohesion: 0.09
Nodes (10): main(), GraphifyWorkflowTests, command(), detect_code(), ensure_version(), main(), tool_environment(), update() (+2 more)

### Community 14 - "OutpostSession"
Cohesion: 0.16
Nodes (4): OutpostSession, CanCommand, IsPaused, Run

### Community 15 - "PrototypeSettings.Build"
Cohesion: 0.24
Nodes (5): IPlatformStatsProvider, PlatformStats, AmmunitionCapacity, MaximumHealth, ModuleSlots

### Community 16 - "RunConfiguration"
Cohesion: 0.18
Nodes (10): RunConfiguration, BasicGun, InitialAmmunition, Level, MissileLauncher, Platform, Purchases, Route (+2 more)

### Community 17 - "SpecialAttackConfig"
Cohesion: 0.33
Nodes (6): SpecialAttackConfig, CooldownSeconds, Damage, EnergyCost, Range, Targeting

### Community 18 - "TacticalPurchaseConfig"
Cohesion: 0.40
Nodes (5): TacticalPurchaseConfig, AmmunitionAmount, AmmunitionCost, RepairCost, RepairHealth

### Community 19 - "RunCombat"
Cohesion: 0.08
Nodes (25): LevelProgression, ActiveEnemies, BossActive, CurrentWave, Definition, Progress, RemainingEnemies, State (+17 more)

### Community 20 - "EnemySpawner"
Cohesion: 0.18
Nodes (8): EnemyAgent, DefeatReported, PositionX, State, EnemyActor, Agent, EnemySpawner, ActiveCount

### Community 21 - "CombatFeedback"
Cohesion: 0.27
Nodes (3): GeneratedLevelRoot, CombatFeedback, ShotTrace

### Community 22 - "RunStatus"
Cohesion: 0.33
Nodes (5): RunStatus, Defeat, Playing, StartScreen, Victory

### Community 24 - "PrototypeSettings"
Cohesion: 0.48
Nodes (3): EnemyTuning, PrototypeSettings, WeaponTuning

## Knowledge Gaps
- **149 isolated node(s):** `Capacity`, `Count`, `Distance`, `Health`, `Id` (+144 more)
  These have ≤1 connection - possible missing edges. (Counts symbols only; 222 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **4 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `GameRun` connect `GameRun` to `system`, `GameplayIntegrationTests`, `PlatformState`, `Test`, `OutpostSession`, `RunConfiguration`, `RunCombat`, `EnemySpawner`, `CombatFeedback`, `RunStatus`?**
  _High betweenness centrality (0.118) - this node is a cross-community bridge._
- **Are the 10 inferred relationships involving `GameRun` (e.g. with `OutpostSession.ResetRun()` and `SimulationTests.ClearedWaveWaitsForTravelAndCannotSkipAheadWhileAnyEnemyLives()`) actually correct?**
  _`GameRun` has 10 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Capacity`, `Count`, `Distance` to the rest of the system?**
  _149 weakly-connected nodes found - possible missing edges._
- **Should `Test` be split into smaller, more focused modules?**
  _Cohesion score 0.07431168136861802 - nodes in this community are weakly interconnected._
- **Why does `OutpostSession` connect `OutpostSession` to `PrototypeSceneBuilder`, `system`, `GameRun`, `GameplayIntegrationTests`, `EnemySpawner`, `CombatFeedback`, `RunStatus`, `Transform`, `PrototypeSettings`, `SceneBuilderTests.RegenerationPreservesSerializedReferencesSettingsGuidsAndUnrelatedRoots`, `PlatformMotor`?**
  _High betweenness centrality (0.048) - this node is a cross-community bridge._
- **Are the 26 inferred relationships involving `Health` (e.g. with `EnemyState.EnemyState()` and `PlatformState.PlatformState()`) actually correct?**
  _`Health` has 26 INFERRED edges - model-reasoned connections that need verification._
- **Should `system` be split into smaller, more focused modules?**
  _Cohesion score 0.08738277919863598 - nodes in this community are weakly interconnected._