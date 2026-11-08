using System;
using LastOutpost.Core.Combat;
using LastOutpost.Core.Configuration;
using LastOutpost.Core.Economy;
using LastOutpost.Core.Enemies;
using LastOutpost.Core.Platform;
using NUnit.Framework;

namespace LastOutpost.Tests
{
    public sealed class EconomyTests
    {
        private PlatformState platform;
        private PrototypeContent content;
        private TacticalPurchases purchases;

        [SetUp]
        public void SetUp()
        {
            content = new PrototypeContent();
            platform = content.CreatePlatform();
            purchases = new TacticalPurchases(platform, content.Purchases, content.SpecialAttack);
        }

        [Test]
        public void CollectionAndSpendingUseExactNonnegativeBalances()
        {
            var wallet = platform.Resources;
            Assert.That(wallet.GetBalance(ResourceType.Energy), Is.Zero);
            wallet.Collect(ResourceType.Energy, 40);
            wallet.Collect(ResourceType.Energy, 10);
            Assert.That(wallet.TrySpend(ResourceType.Energy, 30), Is.True);
            Assert.That(wallet.GetBalance(ResourceType.Energy), Is.EqualTo(20));
            Assert.That(wallet.TrySpend(ResourceType.Energy, 21), Is.False);
            Assert.That(wallet.GetBalance(ResourceType.Energy), Is.EqualTo(20));
            Assert.That(wallet.TrySpend(ResourceType.Energy, 20), Is.True);
            Assert.That(wallet.GetBalance(ResourceType.Energy), Is.Zero);
        }

        [Test]
        public void InvalidResourceOperationsLeaveBalanceUnchanged()
        {
            var wallet = platform.Resources;
            wallet.Collect(ResourceType.Energy, int.MaxValue);
            Assert.Throws<OverflowException>(() => wallet.Collect(ResourceType.Energy, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => wallet.Collect(ResourceType.Energy, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => wallet.TrySpend(ResourceType.Energy, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => wallet.GetBalance((ResourceType)99));
            Assert.That(wallet.GetBalance(ResourceType.Energy), Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void AmmunitionPurchaseCostsThirtyAndAddsTwenty()
        {
            platform.Resources.Collect(ResourceType.Energy, 30);
            Assert.That(purchases.BuyAmmunition(), Is.EqualTo(PurchaseResult.Succeeded));
            Assert.That(platform.AdvancedAmmunition.Count, Is.EqualTo(20));
            Assert.That(platform.Resources.GetBalance(ResourceType.Energy), Is.Zero);
        }

        [Test]
        public void FailedAmmunitionPurchaseChangesNothing()
        {
            platform.Resources.Collect(ResourceType.Energy, 29);
            Assert.That(purchases.BuyAmmunition(), Is.EqualTo(PurchaseResult.InsufficientEnergy));
            Assert.That(platform.Resources.GetBalance(ResourceType.Energy), Is.EqualTo(29));
            Assert.That(platform.AdvancedAmmunition.Count, Is.Zero);
        }

        [Test]
        public void AmmunitionCapacityClampsPartialPurchaseAndRejectsFullInventory()
        {
            platform.Resources.Collect(ResourceType.Energy, 100);
            platform.AdvancedAmmunition.Add(55);
            Assert.That(purchases.BuyAmmunition(), Is.EqualTo(PurchaseResult.Succeeded));
            Assert.That(platform.AdvancedAmmunition.Count, Is.EqualTo(60));
            Assert.That(platform.Resources.GetBalance(ResourceType.Energy), Is.EqualTo(70));
            Assert.That(purchases.BuyAmmunition(), Is.EqualTo(PurchaseResult.NoEffect));
            Assert.That(platform.Resources.GetBalance(ResourceType.Energy), Is.EqualTo(70));
        }

        [Test]
        public void RepairCostsFiftyAndRestoresTwentyFive()
        {
            platform.Health.TakeDamage(40);
            platform.Resources.Collect(ResourceType.Energy, 80);
            Assert.That(purchases.Repair(), Is.EqualTo(PurchaseResult.Succeeded));
            Assert.That(platform.Health.Current, Is.EqualTo(85));
            Assert.That(platform.Resources.GetBalance(ResourceType.Energy), Is.EqualTo(30));
        }

        [Test]
        public void UnaffordableRepairDoesNotHealOrSpend()
        {
            platform.Health.TakeDamage(40);
            platform.Resources.Collect(ResourceType.Energy, 49);
            Assert.That(purchases.Repair(), Is.EqualTo(PurchaseResult.InsufficientEnergy));
            Assert.That(platform.Health.Current, Is.EqualTo(60));
            Assert.That(platform.Resources.GetBalance(ResourceType.Energy), Is.EqualTo(49));
        }

        [Test]
        public void RepairClampsAndFullHealthRejectsWithoutCharging()
        {
            platform.Resources.Collect(ResourceType.Energy, 100);
            platform.Health.TakeDamage(3);
            Assert.That(purchases.Repair(), Is.EqualTo(PurchaseResult.Succeeded));
            Assert.That(platform.Health.Current, Is.EqualTo(100));
            Assert.That(purchases.Repair(), Is.EqualTo(PurchaseResult.NoEffect));
            Assert.That(platform.Resources.GetBalance(ResourceType.Energy), Is.EqualTo(50));
        }

        [Test]
        public void DestroyedPlatformCannotPurchaseOrBeRevived()
        {
            platform.Resources.Collect(ResourceType.Energy, 500);
            platform.Health.TakeDamage(100);
            var target = new Target(1, new Core.Health(200), 1);
            Assert.That(purchases.Repair(), Is.EqualTo(PurchaseResult.PlatformDestroyed));
            Assert.That(purchases.BuyAmmunition(), Is.EqualTo(PurchaseResult.PlatformDestroyed));
            Assert.That(purchases.ActivateSpecial(new[] { target }, 0, out _), Is.EqualTo(PurchaseResult.PlatformDestroyed));
            Assert.That(platform.Resources.GetBalance(ResourceType.Energy), Is.EqualTo(500));
            Assert.That(platform.AdvancedAmmunition.Count, Is.Zero);
            Assert.That(platform.Health.IsDead, Is.True);
            Assert.That(target.Health.Current, Is.EqualTo(200));
        }

        [Test]
        public void SpecialAttackCostsEightyAndDealsDeterministicDamage()
        {
            platform.Resources.Collect(ResourceType.Energy, 100);
            var target = new Target(1, new Core.Health(200), 2);
            Assert.That(purchases.ActivateSpecial(new[] { target }, 0, out Target hit), Is.EqualTo(PurchaseResult.Succeeded));
            Assert.That(hit, Is.SameAs(target));
            Assert.That(target.Health.Current, Is.EqualTo(80));
            Assert.That(platform.Resources.GetBalance(ResourceType.Energy), Is.EqualTo(20));
        }

        [Test]
        public void UnaffordableSpecialHasNoDamageOrCooldownSideEffects()
        {
            platform.Resources.Collect(ResourceType.Energy, 79);
            var target = new Target(1, new Core.Health(200), 2);
            Assert.That(purchases.ActivateSpecial(new[] { target }, 4, out Target hit), Is.EqualTo(PurchaseResult.InsufficientEnergy));
            Assert.That(hit, Is.Null);
            Assert.That(target.Health.Current, Is.EqualTo(200));
            Assert.That(platform.Resources.GetBalance(ResourceType.Energy), Is.EqualTo(79));
            Assert.That(purchases.NextSpecialTimeSeconds, Is.Zero);
        }

        [Test]
        public void SpecialWithoutValidTargetDoesNotCharge()
        {
            platform.Resources.Collect(ResourceType.Energy, 100);
            var dead = new Target(1, new Core.Health(10), 1);
            dead.Health.TakeDamage(10);
            var far = new Target(2, new Core.Health(10), 11);
            Assert.That(purchases.ActivateSpecial(new[] { dead, far }, 0, out _), Is.EqualTo(PurchaseResult.NoEffect));
            Assert.That(platform.Resources.GetBalance(ResourceType.Energy), Is.EqualTo(100));
            Assert.That(far.Health.Current, Is.EqualTo(10));
        }

        [Test]
        public void OptionalSpecialCooldownIsEnforcedWithoutSpendingOnFailure()
        {
            var config = new SpecialAttackConfig(80, 10, 10, 5, new ClosestTargetStrategy());
            var service = new TacticalPurchases(platform, content.Purchases, config);
            platform.Resources.Collect(ResourceType.Energy, 300);
            var targets = new[] { new Target(1, new Core.Health(100), 1) };
            Assert.That(service.ActivateSpecial(targets, 1, out _), Is.EqualTo(PurchaseResult.Succeeded));
            Assert.That(service.ActivateSpecial(targets, 5.999, out _), Is.EqualTo(PurchaseResult.CoolingDown));
            Assert.That(platform.Resources.GetBalance(ResourceType.Energy), Is.EqualTo(220));
            Assert.That(targets[0].Health.Current, Is.EqualTo(90));
            Assert.That(service.ActivateSpecial(targets, 6, out _), Is.EqualTo(PurchaseResult.Succeeded));
        }

        [Test]
        public void CostsAndEffectsAreConfigurableIncludingFreePurchases()
        {
            var service = new TacticalPurchases(platform, new TacticalPurchaseConfig(0, 3, 2, 7), content.SpecialAttack);
            Assert.That(service.BuyAmmunition(), Is.EqualTo(PurchaseResult.Succeeded));
            Assert.That(platform.AdvancedAmmunition.Count, Is.EqualTo(3));
            platform.Health.TakeDamage(20);
            platform.Resources.Collect(ResourceType.Energy, 2);
            Assert.That(service.Repair(), Is.EqualTo(PurchaseResult.Succeeded));
            Assert.That(platform.Health.Current, Is.EqualTo(87));
        }

        [Test]
        public void EnemyDropCannotBeCollectedAliveOrAwardedTwiceAcrossWallets()
        {
            var enemy = new EnemyState(1, content.BasicEnemy);
            Assert.That(enemy.TryCollectDrop(platform.Resources), Is.False);
            enemy.Health.TakeDamage(100);
            Assert.That(enemy.TryCollectDrop(platform.Resources), Is.True);
            Assert.That(enemy.TryCollectDrop(platform.Resources), Is.False);
            var otherWallet = new ResourceWallet();
            Assert.That(enemy.TryCollectDrop(otherWallet), Is.False);
            Assert.That(platform.Resources.GetBalance(ResourceType.Energy), Is.EqualTo(12));
            Assert.That(otherWallet.GetBalance(ResourceType.Energy), Is.Zero);
        }

        [Test]
        public void OverflowingDropIsNotMarkedCollectedAndCanBeRetried()
        {
            var enemy = new EnemyState(1, content.BasicEnemy);
            enemy.Health.TakeDamage(100);
            platform.Resources.Collect(ResourceType.Energy, int.MaxValue);
            Assert.Throws<OverflowException>(() => enemy.TryCollectDrop(platform.Resources));
            Assert.That(enemy.DropCollected, Is.False);
            Assert.That(platform.Resources.GetBalance(ResourceType.Energy), Is.EqualTo(int.MaxValue));
            platform.Resources.TrySpend(ResourceType.Energy, 12);
            Assert.That(enemy.TryCollectDrop(platform.Resources), Is.True);
            Assert.That(platform.Resources.GetBalance(ResourceType.Energy), Is.EqualTo(int.MaxValue));
        }

        [Test]
        public void SeparateEnemyInstancesEachAwardTheirOwnDrop()
        {
            var first = new EnemyState(1, content.FastEnemy);
            var second = new EnemyState(2, content.FastEnemy);
            first.Health.TakeDamage(100);
            second.Health.TakeDamage(100);
            Assert.That(first.TryCollectDrop(platform.Resources), Is.True);
            Assert.That(second.TryCollectDrop(platform.Resources), Is.True);
            Assert.That(platform.Resources.GetBalance(ResourceType.Energy), Is.EqualTo(20));
        }
    }
}
