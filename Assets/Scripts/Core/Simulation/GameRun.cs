using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LastOutpost.Core.Combat;
using LastOutpost.Core.Economy;
using LastOutpost.Core.Levels;
using LastOutpost.Core.Platform;

namespace LastOutpost.Core.Simulation
{
    public enum RunStatus { StartScreen, Playing, Victory, Defeat }

    /// <summary>
    /// Single-threaded coordinator shared by Unity and headless tests. Existing rules remain the
    /// authority. Subscribers observe committed results and must not reenter this run's commands.
    /// </summary>
    public sealed class GameRun
    {
        private readonly List<EnemyAgent> enemies = new List<EnemyAgent>(8);
        private readonly RunCombat combat;
        private readonly TacticalPurchases purchases;
        private double travelRemaining;
        public RunConfiguration Configuration { get; }
        public PlatformState Platform { get; }
        public LevelProgression Level { get; }
        public RouteMotion Route { get; }
        public IReadOnlyList<EnemyAgent> Enemies { get; }
        public RunStatus Status { get; private set; }
        public double ElapsedSeconds { get; private set; }
        public event Action<EnemyAgent> EnemySpawned;
        public event Action<EnemyAgent> EnemyDefeated;
        public event Action<ShotEvent> ShotFired;
        public event Action<int, bool> EncounterStarted;
        public event Action<RunStatus> Finished;

        public GameRun(RunConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Platform = configuration.CreatePlatform();
            Level = new LevelProgression(configuration.Level, Platform.Health);
            Route = new RouteMotion(configuration.Route);
            combat = new RunCombat(Platform, configuration);
            combat.ShotFired += shot => ShotFired?.Invoke(shot);
            purchases = new TacticalPurchases(Platform, configuration.Purchases, configuration.Special);
            Enemies = new ReadOnlyCollection<EnemyAgent>(enemies);
        }

        public bool Start()
        {
            if (Status != RunStatus.StartScreen) return false;
            Status = RunStatus.Playing;
            travelRemaining = Configuration.Route.InitialTravelSeconds;
            Route.SetMode(MovementMode.Travel);
            return true;
        }

        /// <summary>Unity calls this from FixedUpdate using scaled fixedDeltaTime. Zero is a no-op.</summary>
        public void Tick(double deltaSeconds)
        {
            Guard.NonNegativeFinite(deltaSeconds, nameof(deltaSeconds));
            if (Status != RunStatus.Playing || deltaSeconds == 0) return;
            // External damage may have occurred between ticks. Resolve death before movement/fire.
            if (RefreshState()) return;
            double combatSeconds = deltaSeconds;
            if (Level.State == LevelState.Ready || Level.State == LevelState.BetweenEncounters)
            {
                double travel = Math.Min(deltaSeconds, travelRemaining);
                Route.SetMode(MovementMode.Travel);
                Route.Advance(travel);
                ElapsedSeconds += travel;
                travelRemaining -= travel;
                if (travelRemaining > 0) return;
                BeginEncounter();
                combatSeconds -= travel;
            }
            if (combatSeconds > 0)
            {
                ElapsedSeconds = Guard.NonNegativeFinite(ElapsedSeconds + combatSeconds, nameof(deltaSeconds));
                Route.Advance(combatSeconds);
                for (int i = 0; i < enemies.Count; i++) enemies[i].Approach(Route.PositionX, combatSeconds);
                combat.Tick(Enemies, Route.PositionX, ElapsedSeconds);
            }
            RefreshState();
        }

        public PurchaseResult BuyAmmunition() => Status == RunStatus.Playing ? purchases.BuyAmmunition() : PurchaseResult.RunNotActive;
        public PurchaseResult Repair() => Status == RunStatus.Playing ? purchases.Repair() : PurchaseResult.RunNotActive;

        public PurchaseResult ActivateSpecial()
        {
            if (Status != RunStatus.Playing) return PurchaseResult.RunNotActive;
            PurchaseResult result = purchases.ActivateSpecial(combat.BuildTargets(Enemies, Route.PositionX), ElapsedSeconds, out Target hit);
            if (result != PurchaseResult.Succeeded) return result;
            for (int i = 0; i < enemies.Count; i++)
                if (enemies[i].State.Id == hit.Id)
                {
                    ShotFired?.Invoke(new ShotEvent(ShotKind.Special, Route.PositionX, enemies[i].PositionX, hit.Id));
                    break;
                }
            RefreshState();
            return result;
        }

        private void BeginEncounter()
        {
            if (!Level.TryBeginNextEncounter()) throw new InvalidOperationException("Encounter began before its roster was complete.");
            enemies.Clear();
            for (int i = 0; i < Level.ActiveEnemies.Count; i++)
                enemies.Add(new EnemyAgent(Level.ActiveEnemies[i], Route.PositionX + Configuration.Spawns.DistanceAhead + i * Configuration.Spawns.Spacing));
            Route.SetMode(Level.BossActive ? MovementMode.Boss : MovementMode.Encounter);
            EncounterStarted?.Invoke(Level.CurrentWave, Level.BossActive);
            for (int i = 0; i < enemies.Count; i++) EnemySpawned?.Invoke(enemies[i]);
        }

        private void ResolveDefeats()
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyAgent enemy = enemies[i];
                if (!enemy.State.Health.IsDead || enemy.DefeatReported) continue;
                enemy.State.TryCollectDrop(Platform.Resources);
                enemy.DefeatReported = true;
                EnemyDefeated?.Invoke(enemy);
            }
        }

        private bool FinishIfNeeded()
        {
            if (Level.State != LevelState.Victory && Level.State != LevelState.Defeat) return false;
            if (Status != RunStatus.Playing) return true;
            Status = Level.State == LevelState.Victory ? RunStatus.Victory : RunStatus.Defeat;
            Route.SetMode(MovementMode.Stopped);
            Finished?.Invoke(Status);
            return true;
        }

        private bool RefreshState()
        {
            LevelState previous = Level.State;
            ResolveDefeats();
            Level.Refresh();
            if (FinishIfNeeded()) return true;
            if (previous == LevelState.EncounterActive && Level.State == LevelState.BetweenEncounters)
            {
                travelRemaining = Configuration.Route.BetweenEncounterSeconds;
                Route.SetMode(MovementMode.Travel);
            }
            return false;
        }
    }
}
