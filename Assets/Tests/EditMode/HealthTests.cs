using System;
using LastOutpost.Core;
using NUnit.Framework;

namespace LastOutpost.Tests
{
    public sealed class HealthTests
    {
        [Test]
        public void DamageReducesHealthAndReportsActualDamage()
        {
            var health = new Health(100);
            Assert.That(health.TakeDamage(30), Is.EqualTo(30));
            Assert.That(health.Current, Is.EqualTo(70));
            Assert.That(health.IsDead, Is.False);
        }

        [Test]
        public void LethalDamageClampsAtZeroAndDeathIsStable()
        {
            var health = new Health(100);
            Assert.That(health.TakeDamage(int.MaxValue), Is.EqualTo(100));
            Assert.That(health.Current, Is.Zero);
            Assert.That(health.IsDead, Is.True);
            Assert.That(health.TakeDamage(10), Is.Zero);
            Assert.That(health.Heal(100), Is.Zero);
        }

        [Test]
        public void HealingClampsAtMaximumWithoutOverflow()
        {
            var health = new Health(int.MaxValue);
            health.TakeDamage(25);
            Assert.That(health.Heal(int.MaxValue), Is.EqualTo(25));
            Assert.That(health.Current, Is.EqualTo(int.MaxValue));
            Assert.That(health.Heal(1), Is.Zero);
        }

        [Test]
        public void InvalidInputsThrowBeforeChangingHealth()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Health(0));
            var health = new Health(100);
            Assert.Throws<ArgumentOutOfRangeException>(() => health.TakeDamage(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => health.Heal(-1));
            Assert.That(health.Current, Is.EqualTo(100));
        }

        [Test]
        public void ZeroDamageAndHealingAreNoOps()
        {
            var health = new Health(100);
            Assert.That(health.TakeDamage(0), Is.Zero);
            Assert.That(health.Heal(0), Is.Zero);
            Assert.That(health.Current, Is.EqualTo(100));
        }
    }
}
