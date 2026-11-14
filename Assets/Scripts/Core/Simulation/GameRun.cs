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
        private double spaceRemainder;
        public const double SpaceStepSeconds = 1d / 120;
        public RunConfiguration Configuration { get; }
        public string RunId { get; }
        public PlatformState Platform { get; }
        public LevelProgression Level { get; }
        public RouteMotion Route { get; }
        public SpaceCombatRun Space { get; }
        public IReadOnlyList<EnemyAgent> Enemies { get; }
        public RunStatus Status { get; private set; }
        public double ElapsedSeconds { get; private set; }
        public event Action<EnemyAgent> EnemySpawned;
        public event Action<EnemyAgent> EnemyDefeated;
        public event Action<ShotEvent> ShotFired;
        public event Action<int, bool> EncounterStarted;
        public event Action<RunStatus> Finished;

        public GameRun(RunConfiguration configuration, string runId = null)
        {
            RunId = runId ?? Guid.NewGuid().ToString("N");
            if (!IsValidRunId(RunId)) throw new ArgumentException("Run IDs must be nonempty lowercase GUIDs in N format.", nameof(runId));
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            Platform = configuration.CreatePlatform();
            Level = new LevelProgression(configuration.Level, Platform.Health);
            Route = new RouteMotion(configuration.Route);
            if (configuration.SpaceCombat != null) Space = new SpaceCombatRun(configuration.SpaceCombat, Platform, configuration.Equipment);
            combat = new RunCombat(Platform, configuration);
            combat.ShotFired += shot => ShotFired?.Invoke(shot);
            purchases = new TacticalPurchases(Platform, configuration.Purchases, configuration.Special);
            Enemies = new ReadOnlyCollection<EnemyAgent>(enemies);
        }

        public static bool IsValidRunId(string value) => value != null && value.Length == 32 &&
            Guid.TryParseExact(value, "N", out Guid id) && id != Guid.Empty && id.ToString("N") == value;

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
            if (Space == null) { Advance(deltaSeconds); return; }
            // Fixed simulation quantum makes collision and independent weapon schedules invariant
            // to render-frame partitioning. Carry the remainder; never drop time on a long frame.
            spaceRemainder = Guard.NonNegativeFinite(spaceRemainder + deltaSeconds, nameof(deltaSeconds));
            while (spaceRemainder + 1e-12 >= SpaceStepSeconds && Status == RunStatus.Playing)
            {
                spaceRemainder = Math.Max(0, spaceRemainder - SpaceStepSeconds);
                Advance(SpaceStepSeconds);
            }
        }

        private void Advance(double deltaSeconds)
        {
            // External damage may have occurred between ticks. Resolve death before movement/fire.
            if (RefreshState()) return;
            double combatSeconds = deltaSeconds;
            if (Level.State == LevelState.Ready || Level.State == LevelState.BetweenEncounters)
            {
                double travel = Math.Min(deltaSeconds, travelRemaining);
                Route.SetMode(MovementMode.Travel);
                Route.Advance(travel);
                Space?.AdvanceTravel(Route.PositionX);
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
                if (Space != null) Space.Tick(combatSeconds, ElapsedSeconds, Route.PositionX);
                else
                {
                    for (int i = 0; i < enemies.Count; i++) enemies[i].Approach(Route.PositionX, combatSeconds);
                    combat.Tick(Enemies, Route.PositionX, ElapsedSeconds);
                }
            }
            RefreshState();
        }

        public PurchaseResult BuyAmmunition() => Status != RunStatus.Playing ? PurchaseResult.RunNotActive : Space != null ? PurchaseResult.NoEffect :
            Configuration.Equipment != null && !Configuration.Equipment.HasAdvancedWeapon ? PurchaseResult.NoEffect : purchases.BuyAmmunition();
        public PurchaseResult Repair() => Status != RunStatus.Playing ? PurchaseResult.RunNotActive : Space != null ? PurchaseResult.NoEffect : purchases.Repair();

        public PurchaseResult ActivateSpecial()
        {
            if (Status != RunStatus.Playing) return PurchaseResult.RunNotActive;
            if (Space != null) return PurchaseResult.NoEffect;
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
            Space?.BeginEncounter(Enemies, ElapsedSeconds, Route.PositionX);
            EncounterStarted?.Invoke(Level.CurrentWave, Level.BossActive);
            for (int i = 0; i < enemies.Count; i++) EnemySpawned?.Invoke(enemies[i]);
        }

        private void ResolveDefeats()
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                EnemyAgent enemy = enemies[i];
                if (!enemy.State.Health.IsDead || enemy.DefeatReported) continue;
                if (Space == null) enemy.State.TryCollectDrop(Platform.Resources, Configuration.Equipment?.EnergyPerDefeat ?? 0);
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
            Space?.ClearProjectiles();
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
                Space?.ClearProjectiles();
            }
            return false;
        }
    }
}
