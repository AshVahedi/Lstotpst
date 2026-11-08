using System;
using LastOutpost.Core;
using LastOutpost.Core.Combat;
using LastOutpost.Core.Configuration;
using NUnit.Framework;

namespace LastOutpost.Tests
{
    public sealed class WeaponTests
    {
        [Test]
        public void BasicWeaponFiresWithNoAmmunitionInventory()
        {
            var weapon = new Weapon(new PrototypeContent().BasicGun);
            var target = new Target(1, new Health(100), 2);
            Assert.That(weapon.TryFire(new[] { target }, 0, out Target hit), Is.True);
            Assert.That(hit, Is.SameAs(target));
            Assert.That(target.Health.Current, Is.EqualTo(92));
        }

        [Test]
        public void UnlimitedWeaponDoesNotConsumeEvenAnAttachedEmptyInventory()
        {
            var ammo = new Ammunition(0, 60);
            var weapon = new Weapon(new PrototypeContent().BasicGun, ammo);
            Assert.That(weapon.TryFire(new[] { new Target(1, new Health(100), 1) }, 0, out _), Is.True);
            Assert.That(ammo.Count, Is.Zero);
        }

        [Test]
        public void MissileConsumesOneRoundAndCannotFireWhenEmpty()
        {
            var ammo = new Ammunition(1, 60);
            var weapon = new Weapon(new PrototypeContent().MissileLauncher, ammo);
            var target = new Target(1, new Health(100), 2);
            Assert.That(weapon.TryFire(new[] { target }, 0, out _), Is.True);
            Assert.That(ammo.Count, Is.Zero);
            Assert.That(target.Health.Current, Is.EqualTo(70));
            Assert.That(weapon.TryFire(new[] { target }, 2, out Target hit), Is.False);
            Assert.That(hit, Is.Null);
            Assert.That(target.Health.Current, Is.EqualTo(70));
            Assert.That(weapon.NextAttackTimeSeconds, Is.EqualTo(1.8));
        }

        [Test]
        public void AttackIntervalIsRespectedAtExactBoundary()
        {
            var ammo = new Ammunition(10);
            var weapon = new Weapon(new PrototypeContent().MissileLauncher, ammo);
            var targets = new[] { new Target(1, new Health(300), 1) };
            Assert.That(weapon.TryFire(targets, 0, out _), Is.True);
            Assert.That(weapon.TryFire(targets, 1.799, out _), Is.False);
            Assert.That(ammo.Count, Is.EqualTo(9));
            Assert.That(targets[0].Health.Current, Is.EqualTo(270));
            Assert.That(weapon.TryFire(targets, 1.8, out _), Is.True);
            Assert.That(ammo.Count, Is.EqualTo(8));
        }

        [Test]
        public void OutOfRangeTargetDoesNotConsumeAmmoOrStartCooldown()
        {
            var ammo = new Ammunition(5);
            var weapon = new Weapon(new PrototypeContent().MissileLauncher, ammo);
            var far = new Target(1, new Health(100), 7.001);
            Assert.That(weapon.TryFire(new[] { far }, 3, out _), Is.False);
            Assert.That(ammo.Count, Is.EqualTo(5));
            Assert.That(weapon.NextAttackTimeSeconds, Is.Zero);
            Assert.That(far.Health.Current, Is.EqualTo(100));
            Assert.That(weapon.TryFire(new[] { new Target(1, far.Health, 7) }, 3, out _), Is.True);
        }

        [Test]
        public void TargetingSkipsDeadTargetsAndChoosesClosestThenLowestId()
        {
            var dead = new Target(1, new Health(10), 0);
            dead.Health.TakeDamage(10);
            var closeHighId = new Target(4, new Health(10), 2);
            var closeLowId = new Target(3, new Health(10), 2);
            var far = new Target(2, new Health(10), 4);
            var targets = new[] { dead, far, closeHighId, null, closeLowId };
            var strategy = new ClosestTargetStrategy();
            Assert.That(strategy.Select(targets, 6), Is.SameAs(closeLowId));
            Array.Reverse(targets);
            Assert.That(strategy.Select(targets, 6), Is.SameAs(closeLowId));
            Assert.That(strategy.Select(targets, 1), Is.Null);
        }

        [Test]
        public void EmptyOrDeadTargetsDoNotFire()
        {
            var weapon = new Weapon(new PrototypeContent().BasicGun);
            Assert.That(weapon.TryFire(Array.Empty<Target>(), 0, out _), Is.False);
            var dead = new Target(1, new Health(1), 1);
            dead.Health.TakeDamage(1);
            Assert.That(weapon.TryFire(new[] { dead }, 0, out _), Is.False);
            Assert.That(weapon.NextAttackTimeSeconds, Is.Zero);
        }

        [Test]
        public void MultiRoundWeaponRejectsInsufficientAmmoWithoutPartialConsumption()
        {
            var config = new WeaponConfig("salvo", 50, 2, 5, 3, new ClosestTargetStrategy());
            var ammo = new Ammunition(2);
            var weapon = new Weapon(config, ammo);
            var target = new Target(1, new Health(100), 1);
            Assert.That(weapon.TryFire(new[] { target }, 0, out _), Is.False);
            Assert.That(ammo.Count, Is.EqualTo(2));
            Assert.That(target.Health.Current, Is.EqualTo(100));
            Assert.That(weapon.NextAttackTimeSeconds, Is.Zero);
        }

        [Test]
        public void BasicWeaponRemainsAvailableAfterAdvancedAmmoRunsOut()
        {
            var content = new PrototypeContent();
            var ammo = new Ammunition(1);
            var missiles = new Weapon(content.MissileLauncher, ammo);
            var basic = new Weapon(content.BasicGun);
            var target = new Target(1, new Health(100), 1);
            Assert.That(missiles.TryFire(new[] { target }, 0, out _), Is.True);
            Assert.That(missiles.TryFire(new[] { target }, 2, out _), Is.False);
            Assert.That(basic.TryFire(new[] { target }, 2, out _), Is.True);
            Assert.That(target.Health.Current, Is.EqualTo(62));
        }

        [Test]
        public void TimeJumpsDoNotProduceCatchUpBursts()
        {
            var weapon = new Weapon(new PrototypeContent().BasicGun);
            var target = new Target(1, new Health(100), 1);
            Assert.That(weapon.TryFire(new[] { target }, 100, out _), Is.True);
            Assert.That(weapon.TryFire(new[] { target }, 100, out _), Is.False);
            Assert.That(target.Health.Current, Is.EqualTo(92));
        }

        [TestCase(double.NaN)]
        [TestCase(double.PositiveInfinity)]
        [TestCase(-1)]
        public void InvalidTimesAndDistancesAreRejected(double invalid)
        {
            var weapon = new Weapon(new PrototypeContent().BasicGun);
            Assert.Throws<ArgumentOutOfRangeException>(() => weapon.TryFire(Array.Empty<Target>(), invalid, out _));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Target(1, new Health(1), invalid));
        }

        [Test]
        public void AmmunitionBoundsAndInvalidOperationsAreHandled()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Ammunition(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Ammunition(5, 4));
            var ammo = new Ammunition(int.MaxValue - 1);
            Assert.That(ammo.Add(int.MaxValue), Is.EqualTo(1));
            Assert.That(ammo.Count, Is.EqualTo(int.MaxValue));
            Assert.Throws<ArgumentOutOfRangeException>(() => ammo.Add(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => ammo.TryConsume(-1));
            var zeroCapacity = new Ammunition(0, 0);
            Assert.That(zeroCapacity.Add(20), Is.Zero);
            Assert.That(zeroCapacity.TryConsume(1), Is.False);
        }

        [Test]
        public void AdvancedWeaponRequiresInventoryAndValidConfiguration()
        {
            Assert.Throws<ArgumentNullException>(() => new Weapon(new PrototypeContent().MissileLauncher));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WeaponConfig("x", 0, 1, 1, 0, new ClosestTargetStrategy()));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WeaponConfig("x", 1, 0, 1, 0, new ClosestTargetStrategy()));
            Assert.Throws<ArgumentOutOfRangeException>(() => new WeaponConfig("x", 1, 1, -1, 0, new ClosestTargetStrategy()));
        }

        [Test]
        public void WeaponRejectsOutOfRangeTargetFromCustomStrategy()
        {
            var target = new Target(1, new Health(100), 100);
            var config = new WeaponConfig("test", 10, 1, 5, 0, new FixedTargetStrategy(target));
            Assert.That(new Weapon(config).TryFire(new[] { target }, 0, out _), Is.False);
            Assert.That(target.Health.Current, Is.EqualTo(100));
        }

        private sealed class FixedTargetStrategy : ITargetingStrategy
        {
            private readonly Target target;
            public FixedTargetStrategy(Target target) { this.target = target; }
            public Target Select(System.Collections.Generic.IReadOnlyList<Target> targets, double range) => target;
        }
    }
}
