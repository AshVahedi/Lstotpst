# Graph Report - Lstotpst  (2026-11-08)

## Corpus Check
- 52 files · ~30,227 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 25 file(s) not represented in the graph (top: (none) 16, .asmdef 8, .props 1)
- WARNING: 25 of 94 files seen (27%) have no supported extractor and are missing from this graph entirely. It may look complete while representing only part of the codebase.

## Summary
- 750 nodes · 2144 edges · 27 communities (24 shown, 3 thin omitted)
- Extraction: 73% EXTRACTED · 27% INFERRED · 0% AMBIGUOUS · INFERRED: 585 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `51443e57`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- PrototypeSceneBuilder.BuildScene
- Test
- system
- with
- GameRun
- PrototypeSettings.Build
- GameplayIntegrationTests
- PlatformState
- steps
- LevelProgression
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
- AssemblyAuditTests
- GameplayIntegrationTests
- PrototypeContent
- PrototypeSettings
- WeaponConfig
- PlatformMotor

## God Nodes (most connected - your core abstractions)
1. `GameRun` - 48 edges
2. `Health` - 39 edges
3. `PrototypeContent` - 39 edges
4. `Target` - 35 edges
5. `OutpostSession` - 32 edges
6. `GameplayIntegrationTests` - 31 edges
7. `AssemblyAuditTests` - 27 edges
8. `SimulationTests` - 27 edges
9. `Weapon` - 25 edges
10. `RunConfiguration` - 23 edges

## Surprising Connections (you probably didn't know these)
- `CombatFeedback` --references--> `GameRun`  [EXTRACTED]
  Assets/Scripts/Presentation/CombatFeedback.cs → Assets/Scripts/Core/Simulation/GameRun.cs
- `CombatFeedback` --references--> `OutpostSession`  [EXTRACTED]
  Assets/Scripts/Presentation/CombatFeedback.cs → Assets/Scripts/Levels/OutpostSession.cs
- `PlatformState` --references--> `Ammunition`  [EXTRACTED]
  Assets/Scripts/Core/Platform/PlatformState.cs → Assets/Scripts/Core/Combat/Ammunition.cs
- `WeaponConfig` --references--> `ITargetingStrategy`  [EXTRACTED]
  Assets/Scripts/Core/Combat/Weapon.cs → Assets/Scripts/Core/Combat/Targeting.cs
- `SpecialAttackConfig` --references--> `ITargetingStrategy`  [EXTRACTED]
  Assets/Scripts/Core/Economy/TacticalPurchases.cs → Assets/Scripts/Core/Combat/Targeting.cs

## Import Cycles
- None detected.

## Communities (27 total, 3 thin omitted)

### Community 0 - "PrototypeSceneBuilder.BuildScene"
Cohesion: 0.07
Nodes (5): PrototypeSceneBuilder, CameraFollow, CombatFeedback, ShotTrace, SceneBuilderTests

### Community 1 - "Test"
Cohesion: 0.07
Nodes (31): Ammunition, Capacity, Count, ClosestTargetStrategy, ITargetingStrategy, Target, Distance, Health (+23 more)

### Community 2 - "system"
Cohesion: 0.08
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
Cohesion: 0.13
Nodes (9): PurchaseResult, CoolingDown, InsufficientEnergy, NoEffect, PlatformDestroyed, RunNotActive, Succeeded, PrototypeHud (+1 more)

### Community 7 - "PlatformState"
Cohesion: 0.13
Nodes (19): ModuleDefinition, Id, Kind, ModuleKind, Armor, BasicGun, MissileLauncher, Reactor (+11 more)

### Community 8 - "steps"
Cohesion: 0.09
Nodes (21): runs-on, steps, timeout-minutes, jobs, core-tests, name, on, pull_request (+13 more)

### Community 9 - "LevelProgression"
Cohesion: 0.09
Nodes (19): TacticalPurchases, Config, NextSpecialTimeSeconds, SpecialAttack, LevelProgression, ActiveEnemies, BossActive, CurrentWave (+11 more)

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
Cohesion: 0.07
Nodes (13): main(), GraphifyWorkflowTests, command(), detect_code(), ensure_version(), main(), tool_environment(), update() (+5 more)

### Community 14 - "OutpostSession"
Cohesion: 0.14
Nodes (9): RunStatus, Defeat, Playing, StartScreen, Victory, OutpostSession, CanCommand, IsPaused (+1 more)

### Community 15 - "PrototypeSettings.Build"
Cohesion: 0.33
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
Cohesion: 0.18
Nodes (11): RunCombat, ShotEvent, FromX, Kind, TargetEnemyId, ToX, ShotKind, BasicGun (+3 more)

### Community 20 - "EnemySpawner"
Cohesion: 0.14
Nodes (10): EnemyAgent, DefeatReported, PositionX, State, EnemyActor, Agent, EnemySpawner, ActiveCount (+2 more)

### Community 23 - "PrototypeContent"
Cohesion: 0.22
Nodes (9): PrototypeContent, BasicEnemy, BasicGun, Boss, FastEnemy, LevelOne, MissileLauncher, Purchases (+1 more)

### Community 24 - "PrototypeSettings"
Cohesion: 0.36
Nodes (3): EnemyTuning, PrototypeSettings, WeaponTuning

### Community 25 - "WeaponConfig"
Cohesion: 0.29
Nodes (7): WeaponConfig, AmmunitionPerShot, AttackIntervalSeconds, Damage, Id, Range, Targeting

## Knowledge Gaps
- **149 isolated node(s):** `Capacity`, `Count`, `Distance`, `Health`, `Id` (+144 more)
  These have ≤1 connection - possible missing edges. (Counts symbols only; 228 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **3 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `GameRun` connect `GameRun` to `PrototypeSceneBuilder.BuildScene`, `system`, `GameplayIntegrationTests`, `PlatformState`, `LevelProgression`, `OutpostSession`, `RunConfiguration`, `RunCombat`, `EnemySpawner`?**
  _High betweenness centrality (0.109) - this node is a cross-community bridge._
- **Are the 10 inferred relationships involving `GameRun` (e.g. with `OutpostSession.ResetRun()` and `SimulationTests.ClearedWaveWaitsForTravelAndCannotSkipAheadWhileAnyEnemyLives()`) actually correct?**
  _`GameRun` has 10 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Capacity`, `Count`, `Distance` to the rest of the system?**
  _149 weakly-connected nodes found - possible missing edges._
- **Should `PrototypeSceneBuilder.BuildScene` be split into smaller, more focused modules?**
  _Cohesion score 0.06826923076923076 - nodes in this community are weakly interconnected._
- **Why does `OutpostSession` connect `OutpostSession` to `PrototypeSceneBuilder.BuildScene`, `system`, `GameRun`, `GameplayIntegrationTests`, `EnemySpawner`, `GameplayIntegrationTests`, `PrototypeSettings`, `PlatformMotor`?**
  _High betweenness centrality (0.041) - this node is a cross-community bridge._
- **Are the 26 inferred relationships involving `Health` (e.g. with `EnemyState.EnemyState()` and `PlatformState.PlatformState()`) actually correct?**
  _`Health` has 26 INFERRED edges - model-reasoned connections that need verification._
- **Should `Test` be split into smaller, more focused modules?**
  _Cohesion score 0.07239382239382239 - nodes in this community are weakly interconnected._