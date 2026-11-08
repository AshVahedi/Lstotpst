using System;
using System.Collections.Generic;
using LastOutpost.Core.Combat;
using LastOutpost.Core.Configuration;
using LastOutpost.Core.Economy;
using LastOutpost.Core.Enemies;
using LastOutpost.Core.Levels;
using LastOutpost.Core.Platform;
using LastOutpost.Core.Simulation;
using NUnit.Framework;

namespace LastOutpost.Tests
{
    public sealed class SimulationTests
    {
        [Test]
        public void MovementChangesSpeedByModeAndStopsForBossAndOutcome()
        {
            var motion = new RouteMotion(new RouteConfig(2, 0.5));
            motion.Advance(1);
            Assert.That(motion.PositionX, Is.Zero);
            motion.SetMode(MovementMode.Travel);
            motion.Advance(2);
            motion.SetMode(MovementMode.Encounter);
            motion.Advance(2);
            Assert.That(motion.PositionX, Is.EqualTo(5));
            motion.SetMode(MovementMode.Boss);
            motion.Advance(100);
            motion.SetMode(MovementMode.Stopped);
            motion.Advance(100);
            Assert.That(motion.PositionX, Is.EqualTo(5));
        }

        [Test]
        public void MovementIsIndependentOfFramePartition()
        {
            var oneStep = new RouteMotion(new RouteConfig());
            var manySteps = new RouteMotion(new RouteConfig());
            oneStep.SetMode(MovementMode.Travel);
            manySteps.SetMode(MovementMode.Travel);
            oneStep.Advance(10);
            for (int i = 0; i < 500; i++) manySteps.Advance(0.02);
            Assert.That(manySteps.PositionX, Is.EqualTo(oneStep.PositionX).Within(0.000001));
        }

        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        [TestCase(-1)]
        public void InvalidDeltaDoesNotMoveOrAdvanceRun(double invalid)
        {
            var run = new GameRun(RunConfiguration.Prototype());
            run.Start();
            Assert.Throws<ArgumentOutOfRangeException>(() => run.Tick(invalid));
            Assert.That(run.Route.PositionX, Is.Zero);
            Assert.That(run.ElapsedSeconds, Is.Zero);
        }

        [Test]
        public void StartScreenIsInertAndRepeatedStartCannotResetClock()
        {
            var run = new GameRun(RunConfiguration.Prototype());
            run.Tick(10);
            Assert.That(run.Status, Is.EqualTo(RunStatus.StartScreen));
            Assert.That(run.Route.PositionX, Is.Zero);
            Assert.That(run.Enemies, Is.Empty);
            Assert.That(run.Start(), Is.True);
            run.Tick(0.5);
            Assert.That(run.Start(), Is.False);
            Assert.That(run.ElapsedSeconds, Is.EqualTo(0.5));
        }

        [Test]
        public void FirstWaveSpawnsAfterTravelAtDeterministicPositionsOnlyOnce()
        {
            var run = new GameRun(RunConfiguration.Prototype());
            var spawned = new List<int>();
            run.EnemySpawned += enemy => spawned.Add(enemy.State.Id);
            run.Start();
            run.Tick(1);
            Assert.That(run.Enemies, Is.Empty);
            run.Tick(0.5);
            Assert.That(run.Enemies.Count, Is.EqualTo(3));
            Assert.That(spawned, Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(run.Enemies[0].PositionX, Is.EqualTo(1.35 + 4.4).Within(0.00001));
            Assert.That(run.Enemies[1].PositionX, Is.EqualTo(1.35 + 5).Within(0.00001));
            Assert.That(run.Route.Mode, Is.EqualTo(MovementMode.Encounter));
            run.Tick(0.02);
            Assert.That(spawned.Count, Is.EqualTo(3));
        }

        [Test]
        public void AutomaticCombatUsesClosestTargetAndRespectsAttackInterval()
        {
            GameRun run = StartedRun();
            var shots = new List<ShotEvent>();
            run.ShotFired += shots.Add;
            run.Tick(0.1);
            Assert.That(shots.Count, Is.EqualTo(1));
            Assert.That(shots[0].TargetEnemyId, Is.EqualTo(run.Enemies[0].State.Id));
            Assert.That(run.Enemies[0].State.Health.Current, Is.EqualTo(24));
            run.Tick(0.59);
            Assert.That(shots.Count, Is.EqualTo(1));
            run.Tick(0.02);
            Assert.That(shots.Count, Is.EqualTo(2));
            Assert.That(run.Enemies[0].State.Health.Current, Is.EqualTo(16));
        }

        [Test]
        public void TargetOutsideRangeCannotBeDamagedUntilItApproaches()
        {
            var run = new GameRun(Config(spawns: new SpawnConfig(10, 1)));
            run.Start();
            run.Tick(0.01);
            Assert.That(run.Enemies[0].State.Health.Current, Is.EqualTo(32));
            for (int i = 0; i < 300; i++) run.Tick(0.02);
            Assert.That(run.Enemies[0].State.Health.Current, Is.LessThan(32));
        }

        [Test]
        public void EnemyApproachClampsAtAttackRangeAndDeadEnemyCannotMove()
        {
            var content = new PrototypeContent();
            var enemy = new EnemyAgent(new EnemyState(1, content.FastEnemy), 5);
            enemy.Approach(0, 100);
            Assert.That(enemy.PositionX, Is.EqualTo(0.8).Within(0.00001));
            enemy.State.Health.TakeDamage(100);
            enemy.Approach(10, 10);
            Assert.That(enemy.PositionX, Is.EqualTo(0.8).Within(0.00001));
        }

        [Test]
        public void NearbyLivingEnemyAttacksPlatformOnItsOwnSchedule()
        {
            var config = Config(spawns: new SpawnConfig(0.8, 0), basicDamage: 1);
            var run = new GameRun(config);
            run.Start();
            run.Tick(0.1);
            Assert.That(run.Platform.Health.Current, Is.EqualTo(88));
            run.Tick(1.49);
            Assert.That(run.Platform.Health.Current, Is.EqualTo(88));
            run.Tick(0.02);
            Assert.That(run.Platform.Health.Current, Is.EqualTo(76));
        }

        [Test]
        public void ContactEnemyAtNonzeroRoutePositionDoesNotStallOnRangeRounding()
        {
            var content = new PrototypeContent();
            var enemy = new EnemyAgent(new EnemyState(1, content.BasicEnemy), 11);
            enemy.Approach(10, 10);
            var platformHealth = new Core.Health(100);
            var target = new Target(1, platformHealth, Math.Abs(enemy.PositionX - 10));
            Assert.That(enemy.State.Attack.TryFire(new[] { target }, 0, out _), Is.True);
            Assert.That(platformHealth.Current, Is.EqualTo(96));
        }

        [Test]
        public void AmmoPurchaseEnablesMissilesAndEmptyInventoryLeavesBasicGunActive()
        {
            GameRun run = StartedRun();
            run.Platform.Resources.Collect(ResourceType.Energy, 30);
            Assert.That(run.BuyAmmunition(), Is.EqualTo(PurchaseResult.Succeeded));
            Assert.That(run.Platform.AdvancedAmmunition.Count, Is.EqualTo(20));
            var kinds = new List<ShotKind>();
            run.ShotFired += shot => kinds.Add(shot.Kind);
            run.Tick(0.01);
            Assert.That(kinds, Does.Contain(ShotKind.BasicGun));
            Assert.That(kinds, Does.Contain(ShotKind.Missile));
            Assert.That(run.Platform.AdvancedAmmunition.Count, Is.EqualTo(19));
            run.Platform.AdvancedAmmunition.TryConsume(19);
            kinds.Clear();
            run.Tick(2);
            Assert.That(kinds, Does.Contain(ShotKind.BasicGun));
            Assert.That(kinds, Does.Not.Contain(ShotKind.Missile));
        }

        [Test]
        public void RemovingMissileModuleDisablesItsFireWithoutRemovingFallback()
        {
            GameRun run = StartedRun();
            run.Platform.AdvancedAmmunition.Add(10);
            run.Platform.Modules.TryDetach(1);
            run.Platform.Modules.TryDetach(0);
            run.Tick(0.02);
            Assert.That(run.Platform.AdvancedAmmunition.Count, Is.EqualTo(10));
            Assert.That(run.Enemies[0].State.Health.Current, Is.EqualTo(24));
        }

        [Test]
        public void DefeatedEnemyAwardsEnergyAndDestructionNotificationExactlyOnce()
        {
            GameRun run = StartedRun();
            int notifications = 0;
            run.EnemyDefeated += _ => notifications++;
            run.Enemies[0].State.Health.TakeDamage(100);
            run.Tick(0.02);
            run.Tick(0.02);
            Assert.That(notifications, Is.EqualTo(1));
            Assert.That(run.Platform.Resources.GetBalance(ResourceType.Energy), Is.EqualTo(12));
            Assert.That(run.Enemies[0].State.TryCollectDrop(run.Platform.Resources), Is.False);
        }

        [Test]
        public void ClearedWaveWaitsForTravelAndCannotSkipAheadWhileAnyEnemyLives()
        {
            var run = new GameRun(Config(route: new RouteConfig(1, 0, 0, 2)));
            run.Start();
            run.Tick(0.01);
            run.Enemies[0].State.Health.TakeDamage(1000);
            run.Enemies[1].State.Health.TakeDamage(1000);
            run.Tick(0.02);
            Assert.That(run.Level.CurrentWave, Is.EqualTo(1));
            run.Enemies[2].State.Health.TakeDamage(1000);
            run.Tick(1);
            Assert.That(run.Level.CurrentWave, Is.EqualTo(1));
            Assert.That(run.Route.Mode, Is.EqualTo(MovementMode.Travel));
            run.Tick(1);
            Assert.That(run.Level.CurrentWave, Is.EqualTo(2));
            Assert.That(run.Enemies.Count, Is.EqualTo(4));
        }

        [Test]
        public void BossOnlySpawnsAfterThreeCompletedWavesAndStopsRoute()
        {
            GameRun run = StartedRun();
            AdvanceToBoss(run);
            Assert.That(run.Level.CurrentWave, Is.EqualTo(4));
            Assert.That(run.Level.BossActive, Is.True);
            Assert.That(run.Enemies.Count, Is.EqualTo(1));
            double x = run.Route.PositionX;
            run.Tick(1);
            Assert.That(run.Route.PositionX, Is.EqualTo(x));
            Assert.That(run.Route.Mode, Is.EqualTo(MovementMode.Boss));
        }

        [Test]
        public void BossDeathPublishesVictoryOnceAndFreezesMovementCombatAndPurchases()
        {
            GameRun run = StartedRun();
            AdvanceToBoss(run);
            int events = 0;
            run.Finished += status => { Assert.That(status, Is.EqualTo(RunStatus.Victory)); events++; };
            run.Enemies[0].State.Health.TakeDamage(1000);
            run.Tick(0.02);
            double x = run.Route.PositionX;
            double clock = run.ElapsedSeconds;
            run.Tick(100);
            Assert.That(run.Status, Is.EqualTo(RunStatus.Victory));
            Assert.That(events, Is.EqualTo(1));
            Assert.That(run.Route.PositionX, Is.EqualTo(x));
            Assert.That(run.ElapsedSeconds, Is.EqualTo(clock));
            Assert.That(run.BuyAmmunition(), Is.EqualTo(PurchaseResult.RunNotActive));
            Assert.That(run.ActivateSpecial(), Is.EqualTo(PurchaseResult.RunNotActive));
        }

        [Test]
        public void PlatformDeathPublishesDefeatOnceAndStopsEverything()
        {
            GameRun run = StartedRun();
            int finishes = 0;
            int shots = 0;
            run.Finished += _ => finishes++;
            run.ShotFired += _ => shots++;
            run.Platform.Health.TakeDamage(100);
            run.Tick(0.02);
            run.Tick(100);
            Assert.That(run.Status, Is.EqualTo(RunStatus.Defeat));
            Assert.That(run.Route.Mode, Is.EqualTo(MovementMode.Stopped));
            Assert.That(finishes, Is.EqualTo(1));
            Assert.That(shots, Is.Zero);
            Assert.That(run.Repair(), Is.EqualTo(PurchaseResult.RunNotActive));
        }

        [Test]
        public void SimultaneousBossAndPlatformDeathStillProducesOnlyDefeat()
        {
            GameRun run = StartedRun();
            AdvanceToBoss(run);
            run.Enemies[0].State.Health.TakeDamage(1000);
            run.Platform.Health.TakeDamage(100);
            run.Tick(0.02);
            Assert.That(run.Status, Is.EqualTo(RunStatus.Defeat));
        }

        [Test]
        public void PurchaseCommandBoundaryRejectsStartScreenAndInsufficientFundsAtomically()
        {
            var run = new GameRun(Config());
            Assert.That(run.BuyAmmunition(), Is.EqualTo(PurchaseResult.RunNotActive));
            Assert.That(run.Repair(), Is.EqualTo(PurchaseResult.RunNotActive));
            Assert.That(run.ActivateSpecial(), Is.EqualTo(PurchaseResult.RunNotActive));
            run.Start();
            run.Tick(0.01);
            run.Platform.Health.TakeDamage(40);
            Assert.That(run.BuyAmmunition(), Is.EqualTo(PurchaseResult.InsufficientEnergy));
            Assert.That(run.Repair(), Is.EqualTo(PurchaseResult.InsufficientEnergy));
            Assert.That(run.ActivateSpecial(), Is.EqualTo(PurchaseResult.InsufficientEnergy));
            Assert.That(run.Platform.Health.Current, Is.EqualTo(60));
            Assert.That(run.Platform.AdvancedAmmunition.Count, Is.Zero);
            Assert.That(run.Platform.Resources.GetBalance(ResourceType.Energy), Is.Zero);
        }

        [Test]
        public void RepairAndSpecialCommandsUseExistingRulesAndEmitAlreadyAppliedShot()
        {
            GameRun run = StartedRun();
            run.Platform.Resources.Collect(ResourceType.Energy, 130);
            run.Platform.Health.TakeDamage(50);
            Assert.That(run.Repair(), Is.EqualTo(PurchaseResult.Succeeded));
            Assert.That(run.Platform.Health.Current, Is.EqualTo(75));
            int specialShots = 0;
            run.ShotFired += shot => { if (shot.Kind == ShotKind.Special) specialShots++; };
            Assert.That(run.ActivateSpecial(), Is.EqualTo(PurchaseResult.Succeeded));
            Assert.That(run.Enemies[0].State.Health.IsDead, Is.True);
            Assert.That(run.Platform.Resources.GetBalance(ResourceType.Energy), Is.EqualTo(12));
            Assert.That(specialShots, Is.EqualTo(1));
        }

        [Test]
        public void FreshRunResetsAllTransientStateAndTimers()
        {
            GameRun first = StartedRun();
            first.Platform.Resources.Collect(ResourceType.Energy, 100);
            first.BuyAmmunition();
            first.Tick(1);
            var second = new GameRun(first.Configuration);
            Assert.That(second.ElapsedSeconds, Is.Zero);
            Assert.That(second.Route.PositionX, Is.Zero);
            Assert.That(second.Platform.Resources.GetBalance(ResourceType.Energy), Is.Zero);
            Assert.That(second.Platform.AdvancedAmmunition.Count, Is.Zero);
            Assert.That(second.Platform.Health.Current, Is.EqualTo(100));
            Assert.That(second.Status, Is.EqualTo(RunStatus.StartScreen));
        }

        [Test]
        public void CompleteDefaultRunWithTacticalAmmoPurchasesCanWinDeterministically()
        {
            Assert.That(PlayDefaultRun(), Is.EqualTo(PlayDefaultRun()));
        }

        private static string PlayDefaultRun()
        {
            var run = new GameRun(RunConfiguration.Prototype());
            run.Start();
            int waves = 0;
            run.EncounterStarted += (_, __) => waves++;
            for (int tick = 0; tick < 15000 && run.Status == RunStatus.Playing; tick++)
            {
                if (run.Platform.Resources.GetBalance(ResourceType.Energy) >= 30 && run.Platform.AdvancedAmmunition.Count <= 5)
                    run.BuyAmmunition();
                run.Tick(0.02);
            }
            Assert.That(run.Status, Is.EqualTo(RunStatus.Victory));
            Assert.That(waves, Is.EqualTo(4));
            return run.Platform.Health.Current + ":" + run.Platform.Resources.GetBalance(ResourceType.Energy) + ":" + run.ElapsedSeconds;
        }

        private static GameRun StartedRun()
        {
            var run = new GameRun(Config(route: new RouteConfig(0.9, 0.1, 1, 0)));
            run.Start();
            // End exactly at the travel boundary: roster exists, but no shot has fired yet.
            run.Tick(1);
            return run;
        }

        private static RunConfiguration Config(RouteConfig route = null, SpawnConfig spawns = null, int basicDamage = 8)
        {
            var content = new PrototypeContent();
            return new RunConfiguration(content.GetStartingStats(), content.LevelOne,
                new WeaponConfig("basic-gun", basicDamage, 0.6, 6, 0, new ClosestTargetStrategy()),
                content.MissileLauncher, content.Purchases, content.SpecialAttack,
                route ?? new RouteConfig(0.9, 0.1, 0, 0), spawns ?? new SpawnConfig());
        }

        private static void AdvanceToBoss(GameRun run)
        {
            for (int wave = 1; wave <= 3; wave++)
            {
                Assert.That(run.Level.CurrentWave, Is.EqualTo(wave));
                for (int i = 0; i < run.Enemies.Count; i++) run.Enemies[i].State.Health.TakeDamage(1000);
                run.Tick(0.0001);
            }
        }
    }
}
