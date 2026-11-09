using System;
using System.IO;
using LastOutpost.Core;
using LastOutpost.Core.Combat;
using LastOutpost.Core.Economy;
using LastOutpost.Core.Platform;
using LastOutpost.Core.Progression;
using LastOutpost.Core.Simulation;
using NUnit.Framework;

namespace LastOutpost.Tests
{
    public sealed class ProgressionTests
    {
        private static readonly JsonProgressionCodec Codec = new JsonProgressionCodec();

        [TestCase(UpgradeKind.MaximumHealth, 0, 200L)]
        [TestCase(UpgradeKind.MaximumHealth, 1, 300L)]
        [TestCase(UpgradeKind.MaximumHealth, 2, 450L)]
        [TestCase(UpgradeKind.MaximumHealth, 3, 675L)]
        [TestCase(UpgradeKind.MaximumHealth, 9, 7689L)]
        [TestCase(UpgradeKind.Armor, 0, 250L)]
        [TestCase(UpgradeKind.Armor, 1, 375L)]
        [TestCase(UpgradeKind.Armor, 2, 563L)]
        [TestCase(UpgradeKind.Armor, 3, 844L)]
        [TestCase(UpgradeKind.Armor, 9, 9611L)]
        [TestCase(UpgradeKind.WeaponDamage, 0, 300L)]
        [TestCase(UpgradeKind.WeaponDamage, 1, 480L)]
        [TestCase(UpgradeKind.WeaponDamage, 2, 768L)]
        [TestCase(UpgradeKind.WeaponDamage, 3, 1229L)]
        [TestCase(UpgradeKind.WeaponDamage, 9, 20616L)]
        [TestCase(UpgradeKind.AmmunitionCapacity, 0, 150L)]
        [TestCase(UpgradeKind.AmmunitionCapacity, 1, 210L)]
        [TestCase(UpgradeKind.AmmunitionCapacity, 2, 294L)]
        [TestCase(UpgradeKind.AmmunitionCapacity, 3, 412L)]
        [TestCase(UpgradeKind.AmmunitionCapacity, 9, 3100L)]
        public void PricesUseTheConfiguredFormulaAndFinalCeiling(UpgradeKind kind, int currentLevel, long expected)
        {
            Assert.That(ProgressionConfiguration.Prototype().Get(kind).CostAt(currentLevel), Is.EqualTo(expected));
        }

        [Test]
        public void IntermediatePriceRoundingIsNotCompounded()
        {
            var definition = new UpgradeDefinition(UpgradeKind.Armor, 1, 1.5m, 0m);
            Assert.That(definition.CostAt(1), Is.EqualTo(2));
            Assert.That(definition.CostAt(2), Is.EqualTo(3)); // ceil(2.25), not ceil(2*1.5) compounded later.
            Assert.That(definition.CostAt(3), Is.EqualTo(4)); // ceil(3.375), not 5.
            var precise = new UpgradeDefinition(UpgradeKind.Armor, 1, 1.0000000000000000000000000001m, 0m);
            Assert.That(precise.CostAt(9), Is.EqualTo(2));
        }

        [TestCase(-1, 1.5, 1, 10)]
        [TestCase(100, 0, 1, 10)]
        [TestCase(100, 0.5, 1, 10)]
        [TestCase(100, 1.5, -1, 10)]
        [TestCase(100, 1.5, 1, 0)]
        [TestCase(100, 1.5, 1, 1001)]
        public void InvalidUpgradeConfigurationIsRejected(long cost, double growth, int increase, int maximum)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new UpgradeDefinition(UpgradeKind.MaximumHealth, cost, (decimal)growth, increase, maximum));
        }

        [Test]
        public void OverflowingPricesAreRejectedBeforeTransactions()
        {
            Assert.Throws<OverflowException>(() => new UpgradeDefinition(UpgradeKind.MaximumHealth, long.MaxValue, 2m, 1m));
            Assert.Throws<ArgumentException>(() => new UpgradeDefinition(UpgradeKind.MaximumHealth, 10, 1m, 0.5m));
            Assert.Throws<ArgumentOutOfRangeException>(() => new UpgradeDefinition((UpgradeKind)99, 10, 1m, 1m));
            Assert.Throws<ArgumentOutOfRangeException>(() => ProgressionConfiguration.Prototype().Get(UpgradeKind.Armor).CostAt(10));
        }

        [Test]
        public void ConstantAndZeroPricesAreValidConfigurations()
        {
            Assert.That(new UpgradeDefinition(UpgradeKind.Armor, 123, 1m, 0m).CostAt(9), Is.EqualTo(123));
            Assert.That(new UpgradeDefinition(UpgradeKind.Armor, 0, decimal.MaxValue, 0m).CostAt(9), Is.Zero);
        }

        [Test]
        public void MissingSaveStartsAtZeroWithoutWriting()
        {
            var store = new MemoryProgressionStore();
            var service = Open(store);
            Assert.That(service.Snapshot.Credits, Is.Zero);
            Assert.That(service.Snapshot.Revision, Is.Zero);
            Assert.That(service.Snapshot.HighestUnlockedLevel, Is.EqualTo(1));
            Assert.That(store.Read(), Is.Null);
            foreach (UpgradeKind kind in Enum.GetValues(typeof(UpgradeKind))) Assert.That(service.Snapshot.Level(kind), Is.Zero);
        }

        [TestCase(UpgradeKind.MaximumHealth, 200L)]
        [TestCase(UpgradeKind.Armor, 250L)]
        [TestCase(UpgradeKind.WeaponDamage, 300L)]
        [TestCase(UpgradeKind.AmmunitionCapacity, 150L)]
        public void PurchaseDeductsExactPriceIncrementsOneLevelAndPersistsBoth(UpgradeKind kind, long price)
        {
            var store = Seed(500);
            var service = Open(store);
            Assert.That(service.BuyUpgrade(kind), Is.EqualTo(ProgressionResult.Succeeded));
            Assert.That(service.Snapshot.Credits, Is.EqualTo(500 - price));
            Assert.That(service.Snapshot.Level(kind), Is.EqualTo(1));
            var reloaded = Open(store).Snapshot;
            Assert.That(reloaded.Credits, Is.EqualTo(500 - price));
            Assert.That(reloaded.Level(kind), Is.EqualTo(1));
            Assert.That(reloaded.Revision, Is.EqualTo(1));
        }

        [Test]
        public void InsufficientCreditsAndInvalidUpgradeLeaveEntireSaveUnchanged()
        {
            var store = Seed(199);
            string before = store.Read();
            var service = Open(store);
            Assert.That(service.BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.InsufficientCredits));
            Assert.That(service.BuyUpgrade((UpgradeKind)99), Is.EqualTo(ProgressionResult.InvalidUpgrade));
            Assert.That(service.TryGetPrice((UpgradeKind)99, out _), Is.False);
            Assert.That(store.Read(), Is.EqualTo(before));
            Assert.That(service.Snapshot.Credits, Is.EqualTo(199));
            Assert.That(service.Snapshot.Level(UpgradeKind.MaximumHealth), Is.Zero);
        }

        [TestCase(UpgradeKind.MaximumHealth)]
        [TestCase(UpgradeKind.Armor)]
        [TestCase(UpgradeKind.WeaponDamage)]
        [TestCase(UpgradeKind.AmmunitionCapacity)]
        public void MaximumLevelCannotChargeOrIncrease(UpgradeKind kind)
        {
            var data = new ProgressionData { Credits = long.MaxValue };
            SetLevel(data, kind, 10);
            var store = new MemoryProgressionStore(Codec.Encode(data));
            string before = store.Read();
            var service = Open(store);
            Assert.That(service.BuyUpgrade(kind), Is.EqualTo(ProgressionResult.MaximumLevel));
            Assert.That(service.TryGetPrice(kind, out _), Is.False);
            Assert.That(store.Read(), Is.EqualTo(before));
        }

        [Test]
        public void AllTenPurchasesFollowCurrentLevelPricesAndReachCap()
        {
            var service = Open(Seed(100000));
            long spent = 0;
            for (int level = 0; level < 10; level++)
            {
                Assert.That(service.TryGetPrice(UpgradeKind.MaximumHealth, out long price), Is.True);
                Assert.That(price, Is.EqualTo(service.Configuration.Get(UpgradeKind.MaximumHealth).CostAt(level)));
                spent += price;
                Assert.That(service.BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.Succeeded));
            }
            Assert.That(service.Snapshot.Credits, Is.EqualTo(100000 - spent));
            Assert.That(service.Snapshot.Level(UpgradeKind.MaximumHealth), Is.EqualTo(10));
        }

        [Test]
        public void StatsUseAnImmutableProfileSnapshotAndDoNotMutateBaseConfiguration()
        {
            var data = new ProgressionData();
            data.UpgradeLevels.Health = 3;
            data.UpgradeLevels.Armor = 4;
            data.UpgradeLevels.WeaponDamage = 4;
            data.UpgradeLevels.AmmunitionCapacity = 2;
            var service = Open(new MemoryProgressionStore(Codec.Encode(data)));
            RunConfiguration original = RunConfiguration.Prototype();
            var provider = new PermanentStatsProvider(service.Configuration, service.Snapshot);
            RunConfiguration applied = provider.ApplyTo(original);
            Assert.That(provider.GetStartingStats().MaximumHealth, Is.EqualTo(130));
            Assert.That(provider.GetStartingStats().AmmunitionCapacity, Is.EqualTo(140));
            Assert.That(provider.GetStartingStats().DamageReduction, Is.EqualTo(0.12m));
            Assert.That(provider.WeaponMultiplier, Is.EqualTo(1.20m));
            Assert.That(applied.BasicGun.Damage, Is.EqualTo(10)); // 8*1.2 = 9.6, nearest with ties away from zero.
            Assert.That(applied.MissileLauncher.Damage, Is.EqualTo(36));
            Assert.That(original.Platform.MaximumHealth, Is.EqualTo(100));
            Assert.That(original.Platform.AmmunitionCapacity, Is.EqualTo(60));
            Assert.That(original.BasicGun.Damage, Is.EqualTo(8));
            Assert.That(original.MissileLauncher.Damage, Is.EqualTo(30));
            Assert.That(applied.Special, Is.SameAs(original.Special));
        }

        [TestCase(0, 100, 100, 8, 30)]
        [TestCase(10, 200, 300, 12, 45)]
        public void DefaultAndMaximumStatsAreExplicit(int level, int hp, int ammo, int basic, int missile)
        {
            var data = new ProgressionData();
            foreach (UpgradeKind kind in Enum.GetValues(typeof(UpgradeKind))) SetLevel(data, kind, level);
            var service = Open(new MemoryProgressionStore(Codec.Encode(data)));
            Assert.That(service.CreateRun(RunConfiguration.Prototype(), out ProgressionRunSession session), Is.EqualTo(ProgressionResult.Succeeded));
            using (session)
            {
                Assert.That(session.Run.Platform.Health.Maximum, Is.EqualTo(hp));
                Assert.That(session.Run.Platform.AdvancedAmmunition.Capacity, Is.EqualTo(ammo));
                Assert.That(session.Run.Configuration.BasicGun.Damage, Is.EqualTo(basic));
                Assert.That(session.Run.Configuration.MissileLauncher.Damage, Is.EqualTo(missile));
            }
        }

        [Test]
        public void AStatsSnapshotFromAnotherCatalogCannotBypassTheUpgradeCap()
        {
            var data = new ProgressionData();
            data.UpgradeLevels.Health = 3;
            var original = Open(new MemoryProgressionStore(Codec.Encode(data)));
            var basis = ProgressionConfiguration.Prototype();
            var limited = new ProgressionConfiguration(basis.BasePlatform, new[] {
                new UpgradeDefinition(UpgradeKind.MaximumHealth, 200, 1.5m, 10m, 2),
                basis.Get(UpgradeKind.Armor), basis.Get(UpgradeKind.WeaponDamage), basis.Get(UpgradeKind.AmmunitionCapacity)
            }, new[] { basis.GetReward("level-1") });
            Assert.Throws<ArgumentOutOfRangeException>(() => new PermanentStatsProvider(limited, original.Snapshot));
            Assert.That(original.Snapshot.Level(UpgradeKind.MaximumHealth), Is.EqualTo(3));
        }

        [TestCase(10, 7)]
        [TestCase(4, 3)]
        [TestCase(1, 1)]
        [TestCase(0, 0)]
        [TestCase(1000, 100)]
        public void ArmorRoundsRemainingDamageUpAndClampsAtZeroHealth(int damage, int applied)
        {
            var health = new Health(100, 0.30m);
            Assert.That(health.TakeDamage(damage), Is.EqualTo(applied));
            Assert.That(health.Current, Is.EqualTo(100 - applied));
        }

        [Test]
        public void ArmorConfigurationIsCappedAndNeverMakesDamageNegative()
        {
            var config = Config(500, armorStep: 0.1m);
            var data = new ProgressionData();
            data.UpgradeLevels.Armor = 10;
            var service = new ProgressionService(config, new MemoryProgressionStore(Codec.Encode(data)));
            Assert.That(new PermanentStatsProvider(config, service.Snapshot).GetStartingStats().DamageReduction, Is.EqualTo(0.30m));
            var immune = new Health(100, 1m);
            Assert.That(immune.TakeDamage(int.MaxValue), Is.Zero);
            Assert.Throws<ArgumentOutOfRangeException>(() => immune.TakeDamage(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new Health(100, -0.1m));
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlatformStats(100, 100, 2, 1.1m));
        }

        [Test]
        public void ExistingInitialAmmunitionIsClampedToConfiguredProgressionCapacity()
        {
            RunConfiguration template = With(RunConfiguration.Prototype(), initialAmmo: 60);
            var config = Config(500, ammoBase: 10);
            var service = new ProgressionService(config, new MemoryProgressionStore());
            service.CreateRun(template, out ProgressionRunSession session);
            using (session)
            {
                Assert.That(session.Run.Platform.AdvancedAmmunition.Count, Is.EqualTo(10));
                Assert.That(session.Run.Platform.AdvancedAmmunition.Capacity, Is.EqualTo(10));
            }
        }

        [Test]
        public void RunStartsUseCurrentProfileWhileExistingRunStatsStayFixed()
        {
            var service = Open(Seed(500));
            service.CreateRun(RunConfiguration.Prototype(), out ProgressionRunSession first);
            using (first)
            {
                Assert.That(service.BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.Succeeded));
                Assert.That(first.Run.Platform.Health.Maximum, Is.EqualTo(100));
                service.CreateRun(RunConfiguration.Prototype(), out ProgressionRunSession second);
                using (second) Assert.That(second.Run.Platform.Health.Maximum, Is.EqualTo(110));
            }
        }

        [Test]
        public void ArmorAndDamageUpgradesFlowThroughActualRunCombat()
        {
            var data = new ProgressionData();
            data.UpgradeLevels.Armor = 10;
            data.UpgradeLevels.WeaponDamage = 4;
            var service = Open(new MemoryProgressionStore(Codec.Encode(data)));
            RunConfiguration template = With(RunConfiguration.Prototype(), spawns: new SpawnConfig(0.7, 0));
            service.CreateRun(template, out ProgressionRunSession session);
            using (session)
            {
                session.Run.Start();
                session.Run.Tick(template.Route.InitialTravelSeconds);
                session.Run.Tick(0.01);
                Assert.That(session.Run.Enemies[0].State.Health.Current, Is.EqualTo(22));
                Assert.That(session.Run.Platform.Health.Current, Is.EqualTo(91)); // three 4-damage hits -> three 3-damage hits.
            }
        }

        [Test]
        public void EnergyAndCreditsNeverConvertOrCarryAcrossRuns()
        {
            var service = Open(Seed(500));
            service.CreateRun(RunConfiguration.Prototype(), out ProgressionRunSession first);
            using (first)
            {
                first.Run.Start();
                first.Run.Platform.Resources.Collect(ResourceType.Energy, 100);
                Assert.That(first.Run.BuyAmmunition(), Is.EqualTo(PurchaseResult.Succeeded));
                Assert.That(service.Snapshot.Credits, Is.EqualTo(500));
                Assert.That(service.BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.Succeeded));
                Assert.That(first.Run.Platform.Resources.GetBalance(ResourceType.Energy), Is.EqualTo(70));
                first.Abandon();
            }
            service.CreateRun(RunConfiguration.Prototype(), out ProgressionRunSession second);
            using (second)
            {
                Assert.That(second.Run.Platform.Resources.GetBalance(ResourceType.Energy), Is.Zero);
                Assert.That(service.Snapshot.Credits, Is.EqualTo(300));
            }
        }

        [Test]
        public void ConfirmedVictoryAwards500OnceAndRecordsUnlockAndCompletion()
        {
            var store = new MemoryProgressionStore();
            var service = Open(store);
            service.CreateRun(RunConfiguration.Prototype(), out ProgressionRunSession session);
            using (session)
            {
                Assert.That(service.CompleteRun(session.Run), Is.EqualTo(ProgressionResult.RunNotFinished));
                Win(session.Run);
                Assert.That(session.LastCompletionResult, Is.EqualTo(ProgressionResult.Succeeded));
                string before = store.Read();
                Assert.That(service.CompleteRun(session.Run), Is.EqualTo(ProgressionResult.AlreadyProcessed));
                Assert.That(session.RetryCompletion(), Is.EqualTo(ProgressionResult.AlreadyProcessed));
                Assert.That(store.Read(), Is.EqualTo(before));
                Assert.That(service.Snapshot.Credits, Is.EqualTo(500));
                Assert.That(service.Snapshot.HighestUnlockedLevel, Is.EqualTo(2));
                Assert.That(service.Snapshot.CompletedLevelIds, Is.EquivalentTo(new[] { "level-1" }));
                Assert.That(service.Snapshot.RewardedRuns, Is.EqualTo(1));
            }
        }

        [Test]
        public void RewardDeduplicationSurvivesSaveLoadAndNewRunsEarnAgain()
        {
            var store = new MemoryProgressionStore();
            var service = Open(store);
            service.CreateRun(RunConfiguration.Prototype(), out ProgressionRunSession first);
            using (first)
            {
                Win(first.Run);
                var reloaded = Open(store);
                Assert.That(reloaded.CompleteRun(first.Run), Is.EqualTo(ProgressionResult.AlreadyProcessed));
                Assert.That(reloaded.Snapshot.Credits, Is.EqualTo(500));
                reloaded.CreateRun(RunConfiguration.Prototype(), out ProgressionRunSession second);
                using (second)
                {
                    Assert.That(second.Run.RunId, Is.Not.EqualTo(first.Run.RunId));
                    Win(second.Run);
                    Assert.That(reloaded.Snapshot.Credits, Is.EqualTo(1000));
                    Assert.That(reloaded.Snapshot.RewardedRuns, Is.EqualTo(2));
                    Assert.That(Open(store).Snapshot.Credits, Is.EqualTo(1000));
                }
            }
        }

        [Test]
        public void DefeatAbandonAndRestartGrantNoCredits()
        {
            var service = Open(new MemoryProgressionStore());
            service.CreateRun(RunConfiguration.Prototype(), out ProgressionRunSession defeated);
            using (defeated)
            {
                defeated.Run.Start();
                defeated.Run.Platform.Health.TakeDamage(int.MaxValue);
                defeated.Run.Tick(0.01);
                Assert.That(defeated.LastCompletionResult, Is.EqualTo(ProgressionResult.Succeeded));
                Assert.That(service.CompleteRun(defeated.Run), Is.EqualTo(ProgressionResult.AlreadyProcessed));
            }
            service.CreateRun(RunConfiguration.Prototype(), out ProgressionRunSession abandoned);
            using (abandoned)
            {
                Assert.That(abandoned.Abandon(), Is.EqualTo(ProgressionResult.Succeeded));
                Win(abandoned.Run); // A closed/abandoned run cannot later claim victory.
                Assert.That(abandoned.LastCompletionResult, Is.EqualTo(ProgressionResult.AlreadyProcessed));
            }
            service.CreateRun(RunConfiguration.Prototype(), out ProgressionRunSession restarted);
            using (restarted) Assert.That(restarted.Run.Status, Is.EqualTo(RunStatus.StartScreen));
            Assert.That(service.Snapshot.Credits, Is.Zero);
            Assert.That(service.Snapshot.RewardedRuns, Is.Zero);
        }

        [Test]
        public void UnregisteredOrMismatchedRunCannotClaimRewards()
        {
            var service = Open(new MemoryProgressionStore());
            var foreign = new GameRun(RunConfiguration.Prototype());
            Win(foreign);
            Assert.That(service.CompleteRun(foreign), Is.EqualTo(ProgressionResult.InvalidRun));
            Assert.That(service.CompleteRun(null), Is.EqualTo(ProgressionResult.InvalidRun));
            Assert.That(service.Snapshot.Credits, Is.Zero);
        }

        [Test]
        public void RewardAmountIsConfigurableAndSnapshottedAtRegistration()
        {
            var store = new MemoryProgressionStore();
            var service = new ProgressionService(Config(725), store);
            service.CreateRun(RunConfiguration.Prototype(), out ProgressionRunSession session);
            session.Dispose(); // Simulate reconnecting the completion boundary with new balance data.
            Win(session.Run);
            var changed = new ProgressionService(Config(10), store);
            Assert.That(changed.CompleteRun(session.Run), Is.EqualTo(ProgressionResult.Succeeded));
            Assert.That(changed.Snapshot.Credits, Is.EqualTo(725));
        }

        [Test]
        public void DuplicateAndMalformedRunIdsAreRejectedDurably()
        {
            string id = Guid.NewGuid().ToString("N");
            var store = new MemoryProgressionStore();
            var service = new ProgressionService(ProgressionConfiguration.Prototype(), store, ids: new FixedId(id));
            service.CreateRun(RunConfiguration.Prototype(), out ProgressionRunSession session);
            session.Dispose();
            string before = store.Read();
            var again = new ProgressionService(ProgressionConfiguration.Prototype(), store, ids: new FixedId(id));
            Assert.That(again.CreateRun(RunConfiguration.Prototype(), out _), Is.EqualTo(ProgressionResult.InvalidRun));
            Assert.That(store.Read(), Is.EqualTo(before));
            var bad = new ProgressionService(ProgressionConfiguration.Prototype(), store, ids: new FixedId("bad"));
            Assert.That(bad.CreateRun(RunConfiguration.Prototype(), out _), Is.EqualTo(ProgressionResult.InvalidRun));
        }

        [TestCase("")]
        [TestCase("not-a-guid")]
        [TestCase("00000000000000000000000000000000")]
        [TestCase("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")]
        public void EveryGameRunRejectsNoncanonicalIds(string invalid)
        {
            Assert.Throws<ArgumentException>(() => new GameRun(RunConfiguration.Prototype(), invalid));
        }

        [Test]
        public void OverflowingRewardOrRevisionCannotMutateOrMarkARewardPaid()
        {
            var store = Seed(long.MaxValue - 499);
            var service = Open(store);
            service.CreateRun(RunConfiguration.Prototype(), out ProgressionRunSession session);
            using (session)
            {
                string before = store.Read();
                Win(session.Run);
                Assert.That(session.LastCompletionResult, Is.EqualTo(ProgressionResult.Overflow));
                Assert.That(store.Read(), Is.EqualTo(before));
                Assert.That(service.Snapshot.RewardedRuns, Is.Zero);
            }
            var data = new ProgressionData { Credits = 500, Revision = long.MaxValue };
            store = new MemoryProgressionStore(Codec.Encode(data));
            string prior = store.Read();
            Assert.That(Open(store).BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.Overflow));
            Assert.That(store.Read(), Is.EqualTo(prior));
        }

        [Test]
        public void UnsupportedOrMalformedCatalogAndEffectOverflowAreRejected()
        {
            var config = ProgressionConfiguration.Prototype();
            Assert.Throws<ArgumentException>(() => new ProgressionConfiguration(config.BasePlatform,
                new[] { config.Get(UpgradeKind.Armor) }, new[] { new LevelRewardDefinition("level-1", 1, 500) }));
            Assert.Throws<OverflowException>(() => Config(500, healthBase: int.MaxValue));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LevelRewardDefinition("level-1", 1, -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LevelRewardDefinition("level-1", int.MaxValue, 500));
        }

        [Test]
        public void InvalidRunConfigurationAndUnknownLevelsNeverRegisterOrSave()
        {
            var store = new MemoryProgressionStore();
            var service = Open(store);
            Assert.That(service.CreateRun(null, out _), Is.EqualTo(ProgressionResult.InvalidRunConfiguration));
            var foreign = Config(500, levelId: "future-level", levelNumber: 1);
            service = new ProgressionService(foreign, store);
            Assert.That(service.CreateRun(RunConfiguration.Prototype(), out _), Is.EqualTo(ProgressionResult.UnknownLevel));
            var locked = Config(500, levelNumber: 2);
            service = new ProgressionService(locked, store);
            Assert.That(service.CreateRun(RunConfiguration.Prototype(), out _), Is.EqualTo(ProgressionResult.LevelLocked));
            Assert.That(store.Read(), Is.Null);
        }

        [Test]
        public void SnapshotCannotBeChangedByLaterServiceTransactions()
        {
            var service = Open(Seed(500));
            ProgressionSnapshot snapshot = service.Snapshot;
            service.BuyUpgrade(UpgradeKind.MaximumHealth);
            Assert.That(snapshot.Credits, Is.EqualTo(500));
            Assert.That(snapshot.Level(UpgradeKind.MaximumHealth), Is.Zero);
        }

        [TestCase(1, 8, 32)]
        [TestCase(4, 10, 36)]
        [TestCase(10, 12, 45)]
        public void WeaponRoundingUsesNearestIntegerWithHalfTiesAwayFromZero(int level, int basic, int missile)
        {
            var data = new ProgressionData();
            data.UpgradeLevels.WeaponDamage = level;
            var service = Open(new MemoryProgressionStore(Codec.Encode(data)));
            var config = new PermanentStatsProvider(service.Configuration, service.Snapshot).ApplyTo(RunConfiguration.Prototype());
            Assert.That(config.BasicGun.Damage, Is.EqualTo(basic));
            Assert.That(config.MissileLauncher.Damage, Is.EqualTo(missile));
        }

        [Test]
        public void ScaledWeaponOverflowDoesNotRegisterARun()
        {
            var data = new ProgressionData();
            data.UpgradeLevels.WeaponDamage = 1;
            var store = new MemoryProgressionStore(Codec.Encode(data));
            string before = store.Read();
            var service = Open(store);
            RunConfiguration seed = RunConfiguration.Prototype();
            var huge = new WeaponConfig("huge", int.MaxValue, 1, 10, 0, new ClosestTargetStrategy());
            var template = new RunConfiguration(seed.Platform, seed.Level, huge, seed.MissileLauncher,
                seed.Purchases, seed.Special, seed.Route, seed.Spawns);
            Assert.That(service.CreateRun(template, out ProgressionRunSession session), Is.EqualTo(ProgressionResult.Overflow));
            Assert.That(session, Is.Null);
            Assert.That(service.Snapshot.RegisteredRuns, Is.Zero);
            Assert.That(store.Read(), Is.EqualTo(before));
        }

        [Test]
        public void ConcurrentServiceRewardClaimsCannotPayTheSameRunTwice()
        {
            var store = new MemoryProgressionStore();
            var first = Open(store);
            first.CreateRun(RunConfiguration.Prototype(), out ProgressionRunSession session);
            session.Dispose();
            var stale = Open(store);
            Win(session.Run);
            Assert.That(first.CompleteRun(session.Run), Is.EqualTo(ProgressionResult.Succeeded));
            Assert.That(stale.CompleteRun(session.Run), Is.EqualTo(ProgressionResult.Conflict));
            stale.Reload();
            Assert.That(stale.CompleteRun(session.Run), Is.EqualTo(ProgressionResult.AlreadyProcessed));
            Assert.That(stale.Snapshot.Credits, Is.EqualTo(500));
        }

        [Test]
        public void SimultaneousBossAndPlatformDeathCannotAwardVictoryCredits()
        {
            var service = Open(new MemoryProgressionStore());
            service.CreateRun(RunConfiguration.Prototype(), out ProgressionRunSession session);
            using (session)
            {
                GameRun run = session.Run;
                run.Start();
                run.Tick(run.Configuration.Route.InitialTravelSeconds + 0.001);
                for (int wave = 0; wave < 3; wave++)
                {
                    foreach (EnemyAgent enemy in run.Enemies) enemy.State.Health.TakeDamage(int.MaxValue);
                    run.Tick(run.Configuration.Route.BetweenEncounterSeconds + 0.01);
                }
                Assert.That(run.Level.BossActive, Is.True);
                run.Enemies[0].State.Health.TakeDamage(int.MaxValue);
                run.Platform.Health.TakeDamage(int.MaxValue);
                run.Tick(0.01);
                Assert.That(run.Status, Is.EqualTo(RunStatus.Defeat));
                Assert.That(service.Snapshot.Credits, Is.Zero);
                Assert.That(service.Snapshot.RewardedRuns, Is.Zero);
            }
        }

        private static ProgressionService Open(IProgressionStore store) => new ProgressionService(ProgressionConfiguration.Prototype(), store);
        private static MemoryProgressionStore Seed(long credits) => new MemoryProgressionStore(Codec.Encode(new ProgressionData { Credits = credits }));
        private static void SetLevel(ProgressionData data, UpgradeKind kind, int level)
        {
            switch (kind)
            {
                case UpgradeKind.MaximumHealth: data.UpgradeLevels.Health = level; break;
                case UpgradeKind.Armor: data.UpgradeLevels.Armor = level; break;
                case UpgradeKind.WeaponDamage: data.UpgradeLevels.WeaponDamage = level; break;
                case UpgradeKind.AmmunitionCapacity: data.UpgradeLevels.AmmunitionCapacity = level; break;
            }
        }
        private sealed class FixedId : IRunIdGenerator
        {
            private readonly string id;
            public FixedId(string id) => this.id = id;
            public string Create() => id;
        }
        private static ProgressionConfiguration Config(long reward, decimal armorStep = 0.03m, int ammoBase = 100,
            int healthBase = 100, string levelId = "level-1", int levelNumber = 1) => new ProgressionConfiguration(
            new PlatformStats(healthBase, ammoBase, 2), new[] {
                new UpgradeDefinition(UpgradeKind.MaximumHealth, 200, 1.5m, 10m),
                new UpgradeDefinition(UpgradeKind.Armor, 250, 1.5m, armorStep),
                new UpgradeDefinition(UpgradeKind.WeaponDamage, 300, 1.6m, 0.05m),
                new UpgradeDefinition(UpgradeKind.AmmunitionCapacity, 150, 1.4m, 20m)
            }, new[] { new LevelRewardDefinition(levelId, levelNumber, reward) });

        internal static void Win(GameRun run)
        {
            run.Start();
            run.Tick(run.Configuration.Route.InitialTravelSeconds + 0.001);
            for (int step = 0; step < 8 && run.Status == RunStatus.Playing; step++)
            {
                foreach (EnemyAgent enemy in run.Enemies) enemy.State.Health.TakeDamage(int.MaxValue);
                run.Tick(run.Configuration.Route.BetweenEncounterSeconds + 0.01);
            }
            Assert.That(run.Status, Is.EqualTo(RunStatus.Victory));
        }

        private static RunConfiguration With(RunConfiguration template, int initialAmmo = 0, SpawnConfig spawns = null) =>
            new RunConfiguration(template.Platform, template.Level, template.BasicGun, template.MissileLauncher,
                template.Purchases, template.Special, template.Route, spawns ?? template.Spawns, initialAmmo);
    }
}
