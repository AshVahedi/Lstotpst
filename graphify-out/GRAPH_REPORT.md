# Graph Report - Lstotpst  (2026-11-08)

## Corpus Check
- 55 files · ~32,465 words
- Verdict: corpus is large enough that graph structure adds value.
- Unclassified: 25 file(s) not represented in the graph (top: (none) 16, .asmdef 8, .props 1)
- WARNING: 25 of 97 files seen (26%) have no supported extractor and are missing from this graph entirely. It may look complete while representing only part of the codebase.

## Summary
- 868 nodes · 2321 edges · 29 communities (26 shown, 3 thin omitted)
- Extraction: 75% EXTRACTED · 25% INFERRED · 0% AMBIGUOUS · INFERRED: 585 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Graph Freshness
- Built from commit: `64e7475a`
- Run `git rev-parse HEAD` and compare to check if the graph is stale.
- Run `graphify update .` after code changes (no API cost).

## Community Hubs (Navigation)
- PrototypeSceneBuilder.BuildScene
- Test
- system
- with
- GameRun
- PrototypeSettings.Build
- OutpostSession
- PlatformState
- steps
- LevelProgression
- WeaponConfig
- LastOutpost.Core.Tests.csproj
- manifest.json
- update.py
- with
- PrototypeSettings.Build
- RunConfiguration
- SpecialAttackConfig
- TacticalPurchaseConfig
- UnityCiTests
- EnemySpawner
- AssemblyAuditTests
- steps
- unity-verification.yml
- Guard.NonNegative
- build_android
- android
- env
- editor

## God Nodes (most connected - your core abstractions)
1. `GameRun` - 48 edges
2. `Health` - 39 edges
3. `PrototypeContent` - 39 edges
4. `Target` - 35 edges
5. `OutpostSession` - 32 edges
6. `with` - 31 edges
7. `GameplayIntegrationTests` - 31 edges
8. `AssemblyAuditTests` - 27 edges
9. `SimulationTests` - 27 edges
10. `Weapon` - 25 edges

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

## Communities (29 total, 3 thin omitted)

### Community 0 - "PrototypeSceneBuilder.BuildScene"
Cohesion: 0.07
Nodes (5): PrototypeSceneBuilder, CameraFollow, CombatFeedback, ShotTrace, SceneBuilderTests

### Community 1 - "Test"
Cohesion: 0.07
Nodes (43): Ammunition, Capacity, Count, ClosestTargetStrategy, ITargetingStrategy, Target, Distance, Health (+35 more)

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
Cohesion: 0.20
Nodes (12): LevelDefinition, DisplayName, Encounters, Id, SpawnGroup, Count, Enemy, WaveDefinition (+4 more)

### Community 6 - "OutpostSession"
Cohesion: 0.06
Nodes (22): PurchaseResult, CoolingDown, InsufficientEnergy, NoEffect, PlatformDestroyed, RunNotActive, Succeeded, RunStatus (+14 more)

### Community 7 - "PlatformState"
Cohesion: 0.13
Nodes (19): ModuleDefinition, Id, Kind, ModuleKind, Armor, BasicGun, MissileLauncher, Reactor (+11 more)

### Community 8 - "steps"
Cohesion: 0.09
Nodes (21): runs-on, steps, timeout-minutes, jobs, core-tests, name, on, pull_request (+13 more)

### Community 9 - "LevelProgression"
Cohesion: 0.11
Nodes (15): LevelProgression, ActiveEnemies, BossActive, CurrentWave, Definition, Progress, RemainingEnemies, State (+7 more)

### Community 10 - "WeaponConfig"
Cohesion: 0.12
Nodes (16): WeaponConfig, AmmunitionPerShot, AttackIntervalSeconds, Damage, Id, Range, Targeting, EnemyDefinition (+8 more)

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

### Community 20 - "EnemySpawner"
Cohesion: 0.07
Nodes (21): EnemyAgent, DefeatReported, PositionX, State, RunCombat, ShotEvent, FromX, Kind (+13 more)

### Community 22 - "steps"
Cohesion: 0.24
Nodes (14): steps, steps, UNITY_EMAIL, UNITY_LICENSE, UNITY_LICENSE_SERVER, UNITY_PASSWORD, UNITY_SERIAL, steps (+6 more)

### Community 23 - "unity-verification.yml"
Cohesion: 0.17
Nodes (11): concurrency, cancel-in-progress, group, name, runs-on, timeout-minutes, jobs, infrastructure (+3 more)

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

## Knowledge Gaps
- **149 isolated node(s):** `Capacity`, `Count`, `Distance`, `Health`, `Id` (+144 more)
  These have ≤1 connection - possible missing edges. (Counts symbols only; 292 node(s) total have ≤1 connection when file, concept and rationale nodes are included.)
- **3 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `GameRun` connect `GameRun` to `PrototypeSceneBuilder.BuildScene`, `Test`, `system`, `OutpostSession`, `PlatformState`, `LevelProgression`, `RunConfiguration`, `EnemySpawner`?**
  _High betweenness centrality (0.081) - this node is a cross-community bridge._
- **Are the 10 inferred relationships involving `GameRun` (e.g. with `OutpostSession.ResetRun()` and `SimulationTests.ClearedWaveWaitsForTravelAndCannotSkipAheadWhileAnyEnemyLives()`) actually correct?**
  _`GameRun` has 10 INFERRED edges - model-reasoned connections that need verification._
- **What connects `Capacity`, `Count`, `Distance` to the rest of the system?**
  _149 weakly-connected nodes found - possible missing edges._
- **Should `PrototypeSceneBuilder.BuildScene` be split into smaller, more focused modules?**
  _Cohesion score 0.06994047619047619 - nodes in this community are weakly interconnected._
- **Why does `OutpostSession` connect `OutpostSession` to `PrototypeSceneBuilder.BuildScene`, `system`, `EnemySpawner`, `GameRun`?**
  _High betweenness centrality (0.030) - this node is a cross-community bridge._
- **Are the 26 inferred relationships involving `Health` (e.g. with `EnemyState.EnemyState()` and `PlatformState.PlatformState()`) actually correct?**
  _`Health` has 26 INFERRED edges - model-reasoned connections that need verification._
- **Should `Test` be split into smaller, more focused modules?**
  _Cohesion score 0.06675508399646331 - nodes in this community are weakly interconnected._