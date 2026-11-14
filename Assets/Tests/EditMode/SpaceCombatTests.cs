using System;
using System.Collections.Generic;
using System.Linq;
using LastOutpost.Core;
using LastOutpost.Core.Combat;
using LastOutpost.Core.Economy;
using LastOutpost.Core.Enemies;
using LastOutpost.Core.Modules;
using LastOutpost.Core.Platform;
using LastOutpost.Core.Progression;
using LastOutpost.Core.Simulation;
using NUnit.Framework;

namespace LastOutpost.Tests
{
    /// <summary>Shared .NET/Unity regressions. No wall clock, Unity physics or real player storage.</summary>
    public sealed class SpaceCombatTests
    {
        private static readonly ShipPoint Zero = new ShipPoint(0, 0), Forward = new ShipPoint(0, 1);
        private static SpaceWeaponDefinition Definition(double cooldown = 1, WeaponFacing facing = WeaponFacing.Forward,
            double arc = 90, double range = 6) => new SpaceWeaponDefinition("test", 4, cooldown, range, arc, 20, facing);
        private static SpaceEnemy Target(int id, double x, double y, bool active = true) => new SpaceEnemy(new EnemyAgent(
            new EnemyState(id, new EnemyDefinition("test", 100, 0, new WeaponConfig("attack", 3, 1, 1, 0, new ClosestTargetStrategy()), 0)), 0),
            new ShipPoint(x, y), active: active);
        private static GameRun Run(SpaceCombatConfiguration config = null)
        { var run = new GameRun(RunConfiguration.Prototype().WithSpaceCombat(config ?? SpaceCombatConfiguration.Prototype())); run.Start(); return run; }
        private static bool Emit(SpaceWeaponInstance weapon, SpaceEnemy enemy) => true;

        [TestCase(WeaponFacing.Forward, 0, 4)]
        [TestCase(WeaponFacing.Left, -4, 0)]
        [TestCase(WeaponFacing.Right, 4, 0)]
        [TestCase(WeaponFacing.Rear, 0, -4)]
        public void CardinalWeaponsOnlyAcquireTheirOwnArc(WeaponFacing facing, double x, double y)
        {
            var weapon = new SpaceWeaponInstance("gun", "core", Definition(facing: facing), Zero);
            SpaceEnemy valid = Target(1, x, y), opposite = Target(2, -x, -y);
            Assert.That(weapon.SelectTarget(new[] { opposite, valid }, Zero, Forward), Is.SameAs(valid));
            Assert.That(weapon.SelectTarget(new[] { opposite }, Zero, Forward), Is.Null);
        }

        [TestCase(45, true)] [TestCase(-45, true)] [TestCase(45.001, false)] [TestCase(-45.001, false)]
        public void ArcEdgesAreInclusiveAndExteriorIsRejected(double angle, bool eligible)
        {
            double radians = angle * Math.PI / 180;
            Assert.That(SpaceGeometry.Eligible(Zero, Forward, new ShipPoint(Math.Sin(radians) * 4, Math.Cos(radians) * 4), 4, 90), Is.EqualTo(eligible));
        }
        [TestCase(6, true)] [TestCase(6.00001, false)] [TestCase(5.99999, true)]
        public void RangeBoundaryUsesTheActualMount(double y, bool eligible) =>
            Assert.That(SpaceGeometry.Eligible(new ShipPoint(-2, 1), Forward, new ShipPoint(-2, 1 + y), 6, 90), Is.EqualTo(eligible));

        [Test]
        public void CoincidenceAndRotatedShipHaveExplicitGeometry()
        {
            Assert.That(SpaceGeometry.Eligible(Zero, Forward, Zero, 1, 1), Is.True);
            var weapon = new SpaceWeaponInstance("gun", "core", Definition(), new ShipPoint(1, 0));
            ShipPoint rotatedForward = new ShipPoint(1, 0);
            Assert.That(weapon.Origin(Zero, rotatedForward).Y, Is.EqualTo(-1));
            Assert.That(weapon.SelectTarget(new[] { Target(1, 4, -1) }, Zero, rotatedForward), Is.Not.Null);
        }

        [Test]
        public void OutboardCoverageChangesWithPlacement()
        {
            var left = new SpaceWeaponInstance("left", "primary", Definition(facing: WeaponFacing.Outboard), new ShipPoint(-1, 0));
            var right = new SpaceWeaponInstance("right", "secondary", left.Definition, new ShipPoint(1, 0));
            var targets = new[] { Target(1, -4, 0), Target(2, 4, 0) };
            Assert.That(left.SelectTarget(targets, Zero, Forward).Agent.State.Id, Is.EqualTo(1));
            Assert.That(right.SelectTarget(targets, Zero, Forward).Agent.State.Id, Is.EqualTo(2));
        }

        [Test]
        public void ClosestSelectionIsStableAndIgnoresDeadPendingOrMissingTargets()
        {
            SpaceEnemy a = Target(8, -1, 3), b = Target(2, 1, 3), dead = Target(1, 0, 1), pending = Target(3, 0, 0.5, false);
            dead.Agent.State.Health.TakeDamage(100);
            var gun = new SpaceWeaponInstance("gun", "core", Definition(), Zero);
            Assert.That(gun.SelectTarget(new[] { a, null, dead, pending, b }, Zero, Forward), Is.SameAs(b));
            Assert.That(gun.SelectTarget(new[] { b, a }, Zero, Forward), Is.SameAs(b));
            Assert.That(gun.SelectTarget(new[] { Target(9, 0, 2), b }, Zero, Forward).Agent.State.Id, Is.EqualTo(9));
        }

        [Test]
        public void IdenticalDefinitionsHaveIndependentTimersAndStartReady()
        {
            var def = Definition(); var first = new SpaceWeaponInstance("one", "core", def, Zero);
            var second = new SpaceWeaponInstance("two", "core", def, Zero); var targets = new[] { Target(1, 0, 2) };
            Assert.That(first.TryFire(0, Zero, Forward, targets, Emit), Is.True);
            Assert.That(first.TryFire(0.5, Zero, Forward, targets, Emit), Is.False);
            Assert.That(second.TryFire(0.5, Zero, Forward, targets, Emit), Is.True);
            Assert.That(first.TryFire(1, Zero, Forward, targets, Emit), Is.True);
            Assert.That(second.TryFire(1, Zero, Forward, targets, Emit), Is.False);
            Assert.That(second.NextFireTime, Is.EqualTo(1.5));
        }

        [Test]
        public void NoTargetOrFailedEmissionNeverResetsCooldown()
        {
            var gun = new SpaceWeaponInstance("gun", "core", Definition(), Zero); var targets = new[] { Target(1, 0, 2) };
            Assert.That(gun.TryFire(0, Zero, Forward, targets, Emit), Is.True);
            Assert.That(gun.TryFire(2, Zero, Forward, new SpaceEnemy[0], Emit), Is.False);
            Assert.That(gun.Remaining(2), Is.Zero);
            Assert.That(gun.TryFire(3, Zero, Forward, targets, (w, t) => false), Is.False);
            Assert.That(gun.NextFireTime, Is.EqualTo(1));
            Assert.That(gun.TryFire(3, Zero, Forward, targets, Emit), Is.True);
            Assert.That(gun.NextFireTime, Is.EqualTo(4));
        }

        [Test]
        public void DisableDoesNotFireOrResetAndRemovalDoesNotDelayOtherWeapons()
        {
            var gun = new SpaceWeaponInstance("gun", "core", Definition(), Zero); var targets = new[] { Target(1, 0, 2) };
            gun.TryFire(0, Zero, Forward, targets, Emit); gun.Enabled = false;
            Assert.That(gun.TryFire(2, Zero, Forward, targets, Emit), Is.False);
            gun.Enabled = true; Assert.That(gun.TryFire(2, Zero, Forward, targets, Emit), Is.True);
            var run = Run(); var other = run.Space.Weapons[1]; double next = other.NextFireTime;
            Assert.That(run.Space.RemoveWeapon("core/laser"), Is.True);
            Assert.That(run.Space.RemoveWeapon("core/laser"), Is.False);
            Assert.That(run.Space.Weapons.Count, Is.EqualTo(2)); Assert.That(other.NextFireTime, Is.EqualTo(next));
        }

        [Test]
        public void DifferentCooldownsResetOnlyAfterEachSuccessfulShot()
        {
            var fast = new SpaceWeaponInstance("fast", "core", Definition(0.25), Zero);
            var slow = new SpaceWeaponInstance("slow", "core", Definition(1.5), Zero); var targets = new[] { Target(1, 0, 2) };
            for (int i = 0; i <= 8; i++) { fast.TryFire(i * 0.25, Zero, Forward, targets, Emit); slow.TryFire(i * 0.25, Zero, Forward, targets, Emit); }
            Assert.That(fast.ShotsFired, Is.EqualTo(9)); Assert.That(slow.ShotsFired, Is.EqualTo(2));
            Assert.That(fast.NextFireTime, Is.EqualTo(2.25)); Assert.That(slow.NextFireTime, Is.EqualTo(3));
        }

        [TestCase(30)] [TestCase(60)] [TestCase(144)]
        public void RunSchedulesMovementAndDamageAreFramePartitionIndependent(int frames)
        {
            GameRun whole = Run(), divided = Run(); whole.Tick(20);
            for (int i = 0; i < frames * 20; i++) divided.Tick(1d / frames);
            Assert.That(divided.ElapsedSeconds, Is.EqualTo(whole.ElapsedSeconds).Within(1e-9));
            Assert.That(divided.Route.PositionX, Is.EqualTo(whole.Route.PositionX).Within(1e-9));
            Assert.That(divided.Platform.Health.Current, Is.EqualTo(whole.Platform.Health.Current));
            Assert.That(divided.Space.FriendlyHits, Is.EqualTo(whole.Space.FriendlyHits));
            Assert.That(divided.Space.Weapons.Select(w => w.ShotsFired), Is.EqualTo(whole.Space.Weapons.Select(w => w.ShotsFired)));
            Assert.That(divided.Space.Enemies.Select(e => e.Position.X), Is.EqualTo(whole.Space.Enemies.Select(e => e.Position.X)));
        }

        [Test]
        public void ShotIntervalsNeverRunFasterThanTheirDefinition()
        {
            var run = Run(); var last = new Dictionary<string, double>();
            run.Space.ProjectileLaunched += shot => {
                if (shot.Hostile) return;
                var weapon = run.Space.Weapons.First(w => w.Definition.Id == shot.WeaponId);
                if (last.TryGetValue(shot.WeaponId, out double time)) Assert.That(run.ElapsedSeconds - time + 1e-10, Is.GreaterThanOrEqualTo(weapon.Definition.Cooldown));
                last[shot.WeaponId] = run.ElapsedSeconds;
                ShipPoint origin = weapon.Origin(run.Space.ShipPosition, Forward);
                Assert.That(shot.Origin.X, Is.EqualTo(origin.X)); Assert.That(shot.Origin.Y, Is.EqualTo(origin.Y));
            };
            run.Tick(30); Assert.That(last.Count, Is.EqualTo(3));
        }

        [Test]
        public void SpaceUsesNeitherEnergyNorLimitedAmmoAndDisallowsLegacyCommands()
        {
            var run = Run();
            Assert.That(run.BuyAmmunition(), Is.EqualTo(PurchaseResult.NoEffect));
            Assert.That(run.Repair(), Is.EqualTo(PurchaseResult.NoEffect));
            Assert.That(run.ActivateSpecial(), Is.EqualTo(PurchaseResult.NoEffect));
            run.Tick(35);
            Assert.That(run.Space.Weapons.All(w => w.ShotsFired > 0), Is.True);
            Assert.That(run.Platform.Resources.GetBalance(ResourceType.Energy), Is.Zero);
            Assert.That(run.Platform.AdvancedAmmunition.Count, Is.Zero);
        }

        [Test]
        public void SpawnedEnemiesCoverFourDirectionsAndRemainOutsideCameraAndShip()
        {
            var run = Run(); var sides = new HashSet<ArrivalSide>(); int arrivals = 0;
            run.Space.SetViewport(5, 9);
            run.Space.EnemyArrived += enemy => {
                sides.Add(enemy.Side); arrivals++;
                ShipPoint relative = SpaceGeometry.Subtract(enemy.Position, run.Space.ShipPosition);
                bool horizontal = enemy.Side == ArrivalSide.Left || enemy.Side == ArrivalSide.Right;
                Assert.That(horizontal ? Math.Abs(relative.X) : Math.Abs(relative.Y), Is.GreaterThan(horizontal ? 5 : 9));
                foreach (var box in run.Space.Hull) Assert.That(box.Hit(relative, relative, enemy.Radius), Is.EqualTo(double.PositiveInfinity));
            };
            run.Tick(60); Assert.That(sides.Count, Is.EqualTo(4)); Assert.That(arrivals, Is.GreaterThanOrEqualTo(14));
        }

        [Test]
        public void ConfiguredDistributionAndSeedAreDeterministic()
        {
            var basis = SpaceCombatConfiguration.Prototype();
            var left = new SpaceCombatConfiguration(basis.Layout, basis.HullLaser, new Dictionary<string, SpaceWeaponDefinition>(basis.EquipmentWeapons),
                new[] { ArrivalSide.Left }, seed: 3);
            var a = Run(left); var b = Run(left); a.Tick(5); b.Tick(5);
            Assert.That(a.Space.Enemies.All(e => e.Side == ArrivalSide.Left), Is.True);
            Assert.That(a.Space.Enemies.Select(e => e.Position.Y), Is.EqualTo(b.Space.Enemies.Select(e => e.Position.Y)));
        }

        [Test]
        public void BoundedSpawnQueueAndProjectilePoolDoNotAccumulate()
        {
            var b = SpaceCombatConfiguration.Prototype();
            var cfg = new SpaceCombatConfiguration(b.Layout, b.HullLaser, new Dictionary<string, SpaceWeaponDefinition>(b.EquipmentWeapons),
                b.SpawnDistribution, maximumEnemies: 2, maximumProjectiles: 3);
            var run = Run(cfg); int maximum = 0;
            for (int i = 0; i < 3000 && run.Status == RunStatus.Playing; i++)
            {
                run.Tick(0.02); int active = run.Space.Enemies.Count(e => e.Active && !e.Agent.State.Health.IsDead);
                maximum = Math.Max(maximum, active); Assert.That(active, Is.LessThanOrEqualTo(2));
                Assert.That(run.Space.Projectiles.Count, Is.EqualTo(3));
            }
            Assert.That(maximum, Is.EqualTo(2)); Assert.That(run.Space.FriendlyHits, Is.GreaterThan(0));
        }

        [Test]
        public void SweptGeometryCoversWideModulesAndAppliesOneAggregateContact()
        {
            var layout = SpaceCombatConfiguration.Prototype().Layout;
            var boxes = ShipHitBox.Build(layout, new[] { "primary", "secondary", "defense", "utility" });
            Assert.That(boxes.Any(b => b.Hit(new ShipPoint(-4, 0), new ShipPoint(4, 0), 0.1) <= 1), Is.True);
            Assert.That(boxes.Any(b => b.Hit(new ShipPoint(2.15, 3), new ShipPoint(2.15, -3), 0.1) <= 1), Is.True);
            var platform = new PlatformState(new PlatformStats(100, 0, 4)); var combat = new SpaceCombatRun(SpaceCombatConfiguration.Prototype(), platform, null);
            foreach (var weapon in combat.Weapons) weapon.Enabled = false;
            var agent = new EnemyAgent(new EnemyState(1, new EnemyDefinition("rammer", 100, 100,
                new WeaponConfig("ram", 3, 1, 1, 0, new ClosestTargetStrategy()), 0)), 0);
            // Cross the full module/rail footprint in one step: a contact is still only one event.
            combat.BeginEncounter(new[] { agent }, 0, 0);
            combat.Tick(1, 1, 0);
            Assert.That(combat.ContactCount, Is.EqualTo(1)); Assert.That(platform.Health.Current, Is.EqualTo(97));
            combat.Tick(1, 2, 0); Assert.That(platform.Health.Current, Is.EqualTo(97));
        }

        [Test]
        public void SweptProjectilesIncludeTangencyAndDoNotTunnelThroughMovingTargets()
        {
            Assert.That(SpaceGeometry.CircleHit(new ShipPoint(-100, 0), new ShipPoint(100, 0), Zero, 0.4), Is.LessThan(1));
            Assert.That(SpaceGeometry.CircleHit(new ShipPoint(-2, 1), new ShipPoint(2, 1), Zero, 1), Is.EqualTo(0.5));
            Assert.That(SpaceGeometry.CircleHit(new ShipPoint(-2, 2), new ShipPoint(2, 2), Zero, 1), Is.EqualTo(double.PositiveInfinity));
            var run = Run(); run.Tick(25); Assert.That(run.Space.FriendlyHits, Is.GreaterThan(0));
        }

        [Test]
        public void LifetimeExpiryCleansEnemiesWithoutAwardingEnergy()
        {
            var b = SpaceCombatConfiguration.Prototype();
            var cfg = new SpaceCombatConfiguration(b.Layout, b.HullLaser, new Dictionary<string, SpaceWeaponDefinition>(b.EquipmentWeapons), b.SpawnDistribution,
                enemyLifetime: 0.01);
            var run = Run(cfg); foreach (var weapon in run.Space.Weapons) weapon.Enabled = false;
            run.Tick(5); Assert.That(run.Space.ExpiredEnemies, Is.GreaterThan(0));
            Assert.That(run.Platform.Resources.GetBalance(ResourceType.Energy), Is.Zero);
        }

        [Test]
        public void DefaultEncounterCompletesAllWavesBossAndCommitsVictoryOnce()
        {
            var store = new MemoryProgressionStore(); var controller = new ProgressionController(ProgressionConfiguration.Prototype(), store);
            controller.Initialize(); Assert.That(controller.StartRun(RunConfiguration.Prototype().WithSpaceCombat(SpaceCombatConfiguration.Prototype())), Is.EqualTo(ProgressionResult.Succeeded));
            GameRun run = controller.Run;
            for (int i = 0; i < 12000 && run.Status == RunStatus.Playing; i++) controller.Advance(0.02f);
            Assert.That(run.Status, Is.EqualTo(RunStatus.Victory)); Assert.That(run.Level.CurrentWave, Is.EqualTo(4));
            Assert.That(run.Space.SpawnCount, Is.EqualTo(14)); Assert.That(controller.View.Credits, Is.EqualTo(500));
            controller.Advance(1); Assert.That(controller.View.Credits, Is.EqualTo(500));
            Assert.That(run.Space.Projectiles.All(p => !p.Active), Is.True); controller.Dispose();
        }

        [Test]
        public void DefeatFromCombatCommitsNoRewardAndClearsProjectiles()
        {
            var store = new MemoryProgressionStore(); var controller = new ProgressionController(ProgressionConfiguration.Prototype(), store); controller.Initialize();
            controller.StartRun(RunConfiguration.Prototype().WithSpaceCombat(SpaceCombatConfiguration.Prototype())); GameRun run = controller.Run;
            foreach (var weapon in run.Space.Weapons) weapon.Enabled = false;
            for (int i = 0; i < 18000 && run.Status == RunStatus.Playing; i++) controller.Advance(0.02f);
            Assert.That(run.Status, Is.EqualTo(RunStatus.Defeat)); Assert.That(controller.View.Credits, Is.Zero);
            Assert.That(run.Space.HostileHits + run.Space.ContactCount, Is.GreaterThan(0));
            Assert.That(run.Space.Projectiles.All(p => !p.Active), Is.True); controller.Dispose();
        }

        [Test]
        public void LegacyConfigurationStillUsesTheOriginalCombatPath()
        {
            var legacy = new GameRun(RunConfiguration.Prototype()); legacy.Start();
            for (int i = 0; i < 500; i++) legacy.Tick(0.02);
            Assert.That(legacy.Space, Is.Null); Assert.That(legacy.Platform.Resources.GetBalance(ResourceType.Energy), Is.GreaterThan(0));
            Assert.That(legacy.Route.PositionX, Is.GreaterThan(0));
        }

        [Test]
        public void InvalidDefinitionsAndGeometryAreRejectedBeforeUse()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Definition(cooldown: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => Definition(arc: 361));
            Assert.Throws<ArgumentOutOfRangeException>(() => Definition(range: double.NaN));
            Assert.Throws<ArgumentException>(() => new SpaceWeaponInstance("bad", "core", Definition(facing: WeaponFacing.Outboard), Zero));
            var run = Run(); Assert.Throws<ArgumentOutOfRangeException>(() => run.Tick(double.NaN));
            Assert.Throws<ArgumentException>(() => run.Space.SetViewport(1, 8));
        }

        [Test]
        public void HullEmitterFollowsTravelBeforeTheFirstWave()
        {
            var run = Run(); run.Tick(1);
            Assert.That(run.Space.ShipPosition.Y, Is.EqualTo(run.Route.PositionX).Within(1e-9));
            Assert.That(run.Space.Weapons[0].Origin(run.Space.ShipPosition, Forward).Y, Is.EqualTo(run.Route.PositionX + 0.8).Within(1e-9));
            Assert.That(run.Space.Weapons.All(w => w.ShotsFired == 0), Is.True);
        }

        [Test]
        public void FriendlyProjectilesCrossTheHullWithoutSelfDamage()
        {
            var b = SpaceCombatConfiguration.Prototype();
            var cfg = new SpaceCombatConfiguration(b.Layout, new SpaceWeaponDefinition("laser", 5, 0.25, 10, 180, 24, WeaponFacing.Right),
                new Dictionary<string, SpaceWeaponDefinition>(b.EquipmentWeapons), new[] { ArrivalSide.Right });
            var platform = new PlatformState(new PlatformStats(100, 0, 4)); var combat = new SpaceCombatRun(cfg, platform, null);
            foreach (var weapon in combat.Weapons.Skip(1)) weapon.Enabled = false;
            combat.BeginEncounter(new[] { Target(1, 0, 0).Agent }, 0, 0);
            combat.SetViewport(10, 12); // Enlarge after placement so the stationary target is in view.
            for (int i = 1; i <= 360; i++) combat.Tick(GameRun.SpaceStepSeconds, i * GameRun.SpaceStepSeconds, 0);
            Assert.That(combat.FriendlyHits, Is.GreaterThan(0)); Assert.That(platform.Health.Current, Is.EqualTo(100));
            Assert.That(combat.HostileHits, Is.Zero);
        }

        [Test]
        public void HostileProjectilesApplyOneHitAcrossOverlappingHullBoxes()
        {
            var platform = new PlatformState(new PlatformStats(1000, 0, 4));
            var combat = new SpaceCombatRun(SpaceCombatConfiguration.Prototype(), platform, null);
            foreach (var weapon in combat.Weapons) weapon.Enabled = false;
            var boss = new EnemyAgent(new EnemyState(1, new EnemyDefinition("boss", 1000, 0,
                new WeaponConfig("attack", 12, 0.2, 100, 0, new ClosestTargetStrategy()), 0, true)), 0);
            combat.BeginEncounter(new[] { boss }, 0, 0);
            combat.SetViewport(10, 12);
            for (int i = 1; i <= 600; i++) combat.Tick(GameRun.SpaceStepSeconds, i * GameRun.SpaceStepSeconds, 0);
            Assert.That(combat.HostileHits, Is.GreaterThan(0)); Assert.That(platform.Health.Current, Is.EqualTo(1000 - 12 * combat.HostileHits));
            Assert.That(combat.ContactCount, Is.Zero);
        }

        [Test]
        public void DisabledEmitterRetainsItsInFlightProjectileAndPoolSlotsAreReused()
        {
            var run = Run(); int launched = 0; var serials = new HashSet<long>();
            run.Space.ProjectileLaunched += p => { launched++; Assert.That(serials.Add(p.Serial), Is.True); };
            for (int i = 0; i < 2000 && !run.Space.Projectiles.Any(p => p.Active && !p.Hostile); i++) run.Tick(0.01);
            Assert.That(launched, Is.GreaterThan(0)); Assert.That(run.Space.Projectiles.Any(p => p.Active && !p.Hostile), Is.True);
            foreach (var weapon in run.Space.Weapons) weapon.Enabled = false;
            int hits = run.Space.FriendlyHits; run.Tick(2);
            Assert.That(run.Space.FriendlyHits, Is.GreaterThan(hits));
            Assert.That(run.Space.Projectiles.Any(p => p.Active && !p.Hostile), Is.False);
        }

        [Test]
        public void DefeatNotificationsAreOnceOnlyAcrossCleanupAndLaterTicks()
        {
            var run = Run(); var ids = new HashSet<int>();
            run.EnemyDefeated += enemy => Assert.That(ids.Add(enemy.State.Id), Is.True);
            run.Tick(120); int count = ids.Count; run.Tick(10);
            Assert.That(count, Is.EqualTo(14)); Assert.That(ids.Count, Is.EqualTo(count));
        }

        [Test]
        public void WidthChangesMoveSpawnBoundariesAndOptionalEquipmentChangesCoverage()
        {
            var equipment = EquipmentConfiguration.Prototype();
            var wide = new HorizontalShipLayout(equipment, HorizontalShipLayout.DefaultPoints(), 1, 1.5, 0.1, 2, 8);
            var cfg = SpaceCombatConfiguration.Prototype(wide);
            // Wider framing moves arrivals away from all installed module footprints.
            Assert.That(cfg.Layout.MaximumHalfExtent, Is.GreaterThan(2.6));
            var combat = new SpaceCombatRun(cfg, new PlatformState(new PlatformStats(100, 0, 4)), null);
            combat.SetViewport(6, 10); combat.BeginEncounter(new[] { Target(1, 0, 0).Agent }, 0, 0);
            Assert.That(Math.Abs(combat.Enemies[0].Position.X), Is.GreaterThan(6));
        }

        [Test]
        public void UpgradeAndSelectionReloadWithoutSchemaOrOwnershipChanges()
        {
            var codec = new JsonProgressionCodec(); var data = new ProgressionData { Credits = 1000 };
            var store = new MemoryProgressionStore(codec.Encode(data));
            var controller = new ProgressionController(ProgressionConfiguration.Prototype(), store); controller.Initialize();
            Assert.That(controller.BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.Succeeded));
            Assert.That(controller.BuyUpgrade(UpgradeKind.WeaponDamage), Is.EqualTo(ProgressionResult.Succeeded));
            Assert.That(controller.ChangeLoadout(LoadoutOperation.Unequip, "secondary"), Is.EqualTo(LoadoutResult.Succeeded));
            var template = RunConfiguration.Prototype().WithSpaceCombat(SpaceCombatConfiguration.Prototype());
            Assert.That(controller.StartRun(template), Is.EqualTo(ProgressionResult.Succeeded));
            Assert.That(controller.Run.Platform.Health.Maximum, Is.EqualTo(110));
            Assert.That(controller.Run.Space.Weapons.Count, Is.EqualTo(2));
            Assert.That(controller.Run.Space.Weapons.Single(w => w.Definition.Id == "cannon").Definition.Damage, Is.EqualTo(25));
            Assert.That(template.SpaceCombat.EquipmentWeapons["basic-cannon"].Damage, Is.EqualTo(24));
            Assert.That(controller.ChangeLoadout(LoadoutOperation.Equip, "secondary", "missile-launcher"), Is.EqualTo(LoadoutResult.RunActive));
            controller.ReturnToMenu(); controller.Dispose();
            var reloaded = new ProgressionController(ProgressionConfiguration.Prototype(), store); reloaded.Initialize();
            reloaded.StartRun(RunConfiguration.Prototype());
            Assert.That(reloaded.Run.Space, Is.Null); Assert.That(reloaded.Run.Configuration.Equipment.HasAdvancedWeapon, Is.False);
            Assert.That(reloaded.Run.Platform.Health.Maximum, Is.EqualTo(110)); reloaded.ReturnToMenu(); reloaded.Dispose();
            Assert.That(codec.Decode(store.Read(), ProgressionConfiguration.Prototype()).SchemaVersion, Is.EqualTo(2));
        }

        [Test]
        public void SpaceFailureDoesNotRegisterARunOrRewriteTheSave()
        {
            var store = new MemoryProgressionStore(); var controller = new ProgressionController(ProgressionConfiguration.Prototype(), store); controller.Initialize();
            var b = SpaceCombatConfiguration.Prototype();
            var invalid = new SpaceCombatConfiguration(b.Layout, b.HullLaser, new Dictionary<string, SpaceWeaponDefinition>(), b.SpawnDistribution);
            Assert.That(controller.StartRun(RunConfiguration.Prototype().WithSpaceCombat(invalid)), Is.EqualTo(ProgressionResult.InvalidRunConfiguration));
            Assert.That(store.Read(), Is.Null); controller.Dispose();
        }

        [TestCase(2)] [TestCase(3)]
        public void InstalledBatteryCanSupplyIdenticalIndependentEmitters(int count)
        {
            var b = SpaceCombatConfiguration.Prototype();
            var definitions = new Dictionary<string, SpaceWeaponDefinition>(b.EquipmentWeapons);
            definitions["basic-cannon"] = new SpaceWeaponDefinition("cannon", 24, 0.9, 6, 220, 13, WeaponFacing.Outboard, count);
            var run = Run(new SpaceCombatConfiguration(b.Layout, b.HullLaser, definitions, b.SpawnDistribution));
            var cannons = run.Space.Weapons.Where(w => w.Definition.Id == "cannon").ToArray();
            Assert.That(cannons.Length, Is.EqualTo(count)); Assert.That(cannons.Select(w => w.InstanceId).Distinct().Count(), Is.EqualTo(count));
            Assert.That(cannons.All(w => ReferenceEquals(w.Definition, cannons[0].Definition)), Is.True);
            cannons[0].Enabled = false; run.Tick(15);
            Assert.That(cannons[0].ShotsFired, Is.Zero); Assert.That(cannons.Skip(1).All(w => w.ShotsFired > 0), Is.True);
            Assert.That(cannons.All(w => Math.Abs(w.LocalPosition.Y) <= 0.25), Is.True);
            Assert.That(run.Space.RemoveWeapon(cannons[0].InstanceId), Is.True);
            Assert.That(run.Space.Weapons.Count, Is.EqualTo(count + 1));
        }

        [Test]
        public void MovingShipReevaluatesAnAliveTargetsLocalFiringRegion()
        {
            var platform = new PlatformState(new PlatformStats(100, 0, 4));
            var combat = new SpaceCombatRun(SpaceCombatConfiguration.Prototype(), platform, null);
            foreach (var weapon in combat.Weapons) weapon.Enabled = false;
            var boss = new EnemyAgent(new EnemyState(1, new EnemyDefinition("boss", 1000, 0,
                new WeaponConfig("attack", 1, 1, 1, 0, new ClosestTargetStrategy()), 0, true)), 0);
            combat.BeginEncounter(new[] { boss }, 0, 0);
            combat.SetViewport(10, 12); combat.Tick(0.001, 0.001, 0);
            var gun = new SpaceWeaponInstance("gun", "core", Definition(range: 20, arc: 45), Zero);
            Assert.That(gun.SelectTarget(combat.Enemies, Zero, Forward), Is.Not.Null);
            combat.Tick(1, 1, 20);
            Assert.That(boss.State.Health.IsDead, Is.False);
            Assert.That(gun.SelectTarget(combat.Enemies, combat.ShipPosition, Forward), Is.Null);
            Assert.That(gun.Remaining(1), Is.Zero);
        }

        [Test]
        public void PermanentStatsHelperPreservesAndScalesTheOptionalSpacePath()
        {
            var data = new ProgressionData(); data.UpgradeLevels.WeaponDamage = 1;
            var config = ProgressionConfiguration.Prototype();
            var service = new ProgressionService(config, new MemoryProgressionStore(new JsonProgressionCodec().Encode(data)));
            var original = RunConfiguration.Prototype().WithSpaceCombat(SpaceCombatConfiguration.Prototype());
            var result = new PermanentStatsProvider(config, service.Snapshot).ApplyTo(original);
            Assert.That(result.SpaceCombat, Is.Not.Null);
            Assert.That(result.SpaceCombat.EquipmentWeapons["basic-cannon"].Damage, Is.EqualTo(25));
            Assert.That(original.SpaceCombat.EquipmentWeapons["basic-cannon"].Damage, Is.EqualTo(24));
        }

        [Test]
        public void SimultaneousSpaceBossAndShipDeathRetainsDefeatPriorityAndNoReward()
        {
            var controller = new ProgressionController(ProgressionConfiguration.Prototype(), new MemoryProgressionStore()); controller.Initialize();
            controller.StartRun(RunConfiguration.Prototype().WithSpaceCombat(SpaceCombatConfiguration.Prototype()));
            for (int i = 0; i < 2000 && !controller.Run.Level.BossActive; i++) controller.Advance(0.02f);
            var run = controller.Run; Assert.That(run.Level.BossActive, Is.True);
            run.Platform.Health.TakeDamage(int.MaxValue); // Armor still applies; raw current HP is not guaranteed lethal.
            var boss = run.Enemies.Single(e => e.State.Definition.IsBoss); boss.State.Health.TakeDamage(int.MaxValue);
            Assert.That(run.Platform.Health.IsDead && boss.State.Health.IsDead, Is.True);
            controller.Advance(0.02f);
            Assert.That(run.Status, Is.EqualTo(RunStatus.Defeat)); Assert.That(controller.View.Credits, Is.Zero); controller.Dispose();
        }

        [Test]
        public void OffscreenArrivalsLeaveWeaponsReadyUntilTheyEnterTheViewport()
        {
            var b = SpaceCombatConfiguration.Prototype();
            var config = new SpaceCombatConfiguration(b.Layout, b.HullLaser, new Dictionary<string, SpaceWeaponDefinition>(b.EquipmentWeapons), new[] { ArrivalSide.Left });
            var combat = new SpaceCombatRun(config, new PlatformState(new PlatformStats(100, 0, 4)), null);
            var agent = new EnemyAgent(new EnemyState(1, new EnemyDefinition("approach", 1000, 1,
                new WeaponConfig("ram", 1, 1, 1, 0, new ClosestTargetStrategy()), 0)), 0);
            combat.BeginEncounter(new[] { agent }, 0, 0); combat.Tick(1, 1, 0);
            var cannon = combat.Weapons.Single(w => w.Definition.Id == "cannon");
            Assert.That(combat.Enemies[0].Targetable, Is.False); Assert.That(cannon.ShotsFired, Is.Zero); Assert.That(cannon.Remaining(1), Is.Zero);
            combat.Tick(1, 2, 0);
            Assert.That(combat.Enemies[0].Targetable, Is.True); Assert.That(cannon.ShotsFired, Is.EqualTo(1));
        }
    }
}
