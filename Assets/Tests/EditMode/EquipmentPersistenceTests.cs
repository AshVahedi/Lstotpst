using System;
using System.IO;
using LastOutpost.Core.Modules;
using LastOutpost.Core.Progression;
using LastOutpost.Core.Simulation;
using NUnit.Framework;

namespace LastOutpost.Tests
{
    public sealed class EquipmentPersistenceTests
    {
        private static readonly JsonProgressionCodec Codec = new JsonProgressionCodec();
        private static ProgressionConfiguration Config => ProgressionConfiguration.Prototype();
        private const string PaidRun = "0123456789abcdef0123456789abcdef";
        internal const string Legacy = "{\"schemaVersion\":1,\"revision\":7,\"credits\":450," +
            "\"upgradeLevels\":{\"health\":1,\"armor\":2,\"weaponDamage\":3,\"ammunitionCapacity\":4}," +
            "\"highestUnlockedLevel\":2,\"completedLevelIds\":[\"level-1\"],\"runs\":[{" +
            "\"runId\":\"" + PaidRun + "\",\"levelId\":\"level-1\",\"levelNumber\":1,\"victoryCredits\":500,\"outcome\":1}]}";

        [Test]
        public void V1MigrationPreservesEveryProgressionFieldAndDoesNotWriteDuringLoad()
        {
            var store = new MemoryProgressionStore(Legacy); var service = new ProgressionService(Config, store);
            Assert.That(store.Read(), Is.EqualTo(Legacy)); Assert.That(service.Snapshot.Credits, Is.EqualTo(450));
            Assert.That(service.Snapshot.Revision, Is.EqualTo(7)); Assert.That(service.Snapshot.HighestUnlockedLevel, Is.EqualTo(2));
            Assert.That(service.Snapshot.CompletedLevelIds, Is.EqualTo(new[] { "level-1" }));
            Assert.That(service.Snapshot.Level(UpgradeKind.MaximumHealth), Is.EqualTo(1));
            Assert.That(service.Snapshot.Level(UpgradeKind.Armor), Is.EqualTo(2));
            Assert.That(service.Snapshot.Level(UpgradeKind.WeaponDamage), Is.EqualTo(3));
            Assert.That(service.Snapshot.Level(UpgradeKind.AmmunitionCapacity), Is.EqualTo(4));
            Assert.That(service.Snapshot.RegisteredRuns, Is.EqualTo(1)); Assert.That(service.Snapshot.RewardedRuns, Is.EqualTo(1));
            Assert.That(service.GetRunReceipt(PaidRun).AwardedCredits, Is.EqualTo(500));
            Assert.That(service.Snapshot.Loadout.OwnedEquipmentIds.Count, Is.EqualTo(4));
            Assert.That(service.ValidateLoadout(), Is.EqualTo(LoadoutResult.Succeeded));
            Assert.That(Codec.Decode(Legacy, Config).SchemaVersion, Is.EqualTo(2));
        }

        [Test]
        public void FirstSuccessfulTransactionWritesV2AndMigrationCannotDuplicateOldRewards()
        {
            var store = new MemoryProgressionStore(Legacy); var service = new ProgressionService(Config, store);
            var oldRun = new GameRun(RunConfiguration.Prototype(), PaidRun); ProgressionTests.Win(oldRun);
            Assert.That(service.CompleteRun(oldRun), Is.EqualTo(ProgressionResult.AlreadyProcessed));
            Assert.That(store.Read(), Is.EqualTo(Legacy));
            Assert.That(service.Unequip("utility"), Is.EqualTo(LoadoutResult.Succeeded));
            StringAssert.Contains("\"schemaVersion\":2", store.Read());
            var reloaded = new ProgressionService(Config, store);
            Assert.That(reloaded.Snapshot.Revision, Is.EqualTo(8)); Assert.That(reloaded.Snapshot.Credits, Is.EqualTo(450));
            Assert.That(reloaded.Snapshot.Loadout.At("utility"), Is.Null);
            Assert.That(reloaded.Snapshot.Loadout.OwnedEquipmentIds.Count, Is.EqualTo(4));
            Assert.That(reloaded.CompleteRun(oldRun), Is.EqualTo(ProgressionResult.AlreadyProcessed));
            Assert.That(reloaded.Snapshot.RewardedRuns, Is.EqualTo(1));
        }

        [Test]
        public void FailedMigrationWriteKeepsTheExactV1PrimaryAndAllPublishedState()
        {
            var store = new FaultStore(Legacy) { Fail = true }; var service = new ProgressionService(Config, store);
            Assert.That(service.Unequip("utility"), Is.EqualTo(LoadoutResult.PersistenceFailed));
            Assert.That(store.Inner.Read(), Is.EqualTo(Legacy)); Assert.That(service.Snapshot.Loadout.At("utility"), Is.EqualTo("reactor-booster"));
            Assert.That(service.Snapshot.Revision, Is.EqualTo(7));
            Assert.That(service.Unequip("defense"), Is.EqualTo(LoadoutResult.RecoveryRequired));
            store.Fail = false; service.Reload();
            Assert.That(service.Unequip("utility"), Is.EqualTo(LoadoutResult.Succeeded));
        }

        [Test]
        public void MigrationStillUsesTheOriginalV1PayloadAsTheStaleWriterComparison()
        {
            var store = new MemoryProgressionStore(Legacy);
            var first = new ProgressionService(Config, store); var stale = new ProgressionService(Config, store);
            first.Unequip("utility"); string current = store.Read();
            Assert.That(stale.Unequip("defense"), Is.EqualTo(LoadoutResult.Conflict)); Assert.That(store.Read(), Is.EqualTo(current));
            Assert.That(stale.Snapshot.Loadout.Installations.Count, Is.EqualTo(4));
            stale.Reload(); Assert.That(stale.Snapshot.Loadout.Installations.Count, Is.EqualTo(3));
            Assert.That(stale.Snapshot.Loadout.At("defense"), Is.EqualTo("armor-plating"));
        }

        [Test]
        public void EquipUnequipAndRestoreDefaultsPersistTogetherWithoutChangingCreditsOrUpgrades()
        {
            var store = new MemoryProgressionStore(Legacy); var service = new ProgressionService(Config, store);
            service.Unequip("secondary");
            Assert.That(service.Equip("secondary", "missile-launcher"), Is.EqualTo(LoadoutResult.Succeeded));
            service.Unequip("utility"); Assert.That(service.RestoreDefaultLoadout(), Is.EqualTo(LoadoutResult.Succeeded));
            var loaded = new ProgressionService(Config, store);
            Assert.That(loaded.Snapshot.Loadout.Installations.Count, Is.EqualTo(4)); Assert.That(loaded.Snapshot.Credits, Is.EqualTo(450));
            Assert.That(loaded.Snapshot.Level(UpgradeKind.MaximumHealth), Is.EqualTo(1)); Assert.That(loaded.Snapshot.RewardedRuns, Is.EqualTo(1));
        }

        [Test]
        public void ValidReplacementIsAtomicAndARejectedChangeDoesNotSaveARevision()
        {
            var alternate = EquipmentTests.Utility(); var config = EquipmentTests.Config(EquipmentTests.Extended(alternate));
            var store = new MemoryProgressionStore(); var service = new ProgressionService(config, store);
            Assert.That(service.Replace("utility", alternate.Id), Is.EqualTo(LoadoutResult.Succeeded)); string before = store.Read();
            Assert.That(service.Snapshot.Loadout.At("utility"), Is.EqualTo(alternate.Id));
            Assert.That(service.Replace("utility", "basic-cannon"), Is.EqualTo(LoadoutResult.Incompatible));
            Assert.That(store.Read(), Is.EqualTo(before)); Assert.That(service.Snapshot.Loadout.At("utility"), Is.EqualTo(alternate.Id));
        }

        [Test]
        public void UnknownEquipmentRemainsInStorageAndRequiresAnExplicitLoadoutRepair()
        {
            var data = new ProgressionData { Credits = 500 }; data.Equipment.OwnedEquipmentIds.Add("retired-module");
            data.Equipment.Installations[2].EquipmentId = "retired-module";
            var store = new MemoryProgressionStore(Codec.Encode(data)); string original = store.Read();
            var service = new ProgressionService(Config, store);
            Assert.That(service.ValidateLoadout(), Is.EqualTo(LoadoutResult.UnknownEquipment));
            Assert.That(service.CreateRun(RunConfiguration.Prototype(), out var session), Is.EqualTo(ProgressionResult.InvalidLoadout));
            Assert.That(session, Is.Null); Assert.That(store.Read(), Is.EqualTo(original));
            service.BuyUpgrade(UpgradeKind.MaximumHealth);
            Assert.That(Codec.Decode(store.Read(), Config).Equipment.Installations[2].EquipmentId, Is.EqualTo("retired-module"));
            Assert.That(service.Replace("defense", "armor-plating"), Is.EqualTo(LoadoutResult.Succeeded));
            Assert.That(new ProgressionService(Config, store).Snapshot.Loadout.OwnedEquipmentIds, Does.Contain("retired-module"));
        }

        [Test]
        public void UnknownSlotIsPreservedAndOnlyExplicitRestoreReplacesItsAssignment()
        {
            var data = new ProgressionData(); data.Equipment.OwnedEquipmentIds.Add("future-module");
            data.Equipment.Installations.Add(new EquipmentInstallationData { SlotId = "future-bay", EquipmentId = "future-module" });
            var store = new MemoryProgressionStore(Codec.Encode(data));
            using (var flow = new ProgressionController(Config, store))
            {
                flow.Initialize(); Assert.That(flow.View.CanStart, Is.False);
                Assert.That(flow.View.Loadout.Validation, Is.EqualTo(LoadoutResult.UnknownSlot));
                Assert.That(flow.View.Loadout.Slots.Count, Is.EqualTo(5)); Assert.That(flow.View.Loadout.UnknownOwnedIds, Does.Contain("future-module"));
                Assert.That(flow.ChangeLoadout(LoadoutOperation.RestoreDefaults), Is.EqualTo(LoadoutResult.Succeeded));
                Assert.That(flow.View.CanStart, Is.True);
                Assert.That(Codec.Decode(store.Read(), Config).Equipment.OwnedEquipmentIds, Does.Contain("future-module"));
            }
        }

        [Test]
        public void RestoreDefaultDoesNotGrantMissingOwnership()
        {
            var data = new ProgressionData(); data.Equipment.OwnedEquipmentIds.Remove("missile-launcher");
            data.Equipment.Installations.RemoveAt(1); var store = new MemoryProgressionStore(Codec.Encode(data)); string before = store.Read();
            var service = new ProgressionService(Config, store);
            Assert.That(service.RestoreDefaultLoadout(), Is.EqualTo(LoadoutResult.NotOwned)); Assert.That(store.Read(), Is.EqualTo(before));
            Assert.That(service.Equip("secondary", "missile-launcher"), Is.EqualTo(LoadoutResult.NotOwned));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void MalformedStoredEquipmentIsRejectedWithoutReplacingItsPayload(int defect)
        {
            var data = new ProgressionData();
            switch (defect)
            {
                case 0: data.Equipment.OwnedEquipmentIds.Add("basic-cannon"); break;
                case 1: data.Equipment.Installations.Add(new EquipmentInstallationData { SlotId = "primary", EquipmentId = "basic-cannon" }); break;
                case 2: data.Equipment.Installations[0].EquipmentId = " "; break;
                case 3: data.Equipment = null; break;
            }
            string payload = Codec.Encode(data); var store = new MemoryProgressionStore(payload);
            Assert.Throws<ProgressionLoadException>(() => new ProgressionService(Config, store)); Assert.That(store.Read(), Is.EqualTo(payload));
        }

        [Test]
        public void MissingV2EquipmentCannotBeMistakenForAMigration()
        {
            string payload = Codec.Encode(new ProgressionData()).Replace("\"equipment\":", "\"ignoredEquipment\":");
            var store = new MemoryProgressionStore(payload);
            var error = Assert.Throws<ProgressionLoadException>(() => new ProgressionService(Config, store));
            Assert.That(error.Reason, Is.EqualTo(ProgressionLoadError.Corrupt)); Assert.That(store.Read(), Is.EqualTo(payload));
        }

        [Test]
        public void LoadoutViewRefreshesFromTheProfileAndCannotEnableAnInvalidOrMidRunCommand()
        {
            var store = new MemoryProgressionStore();
            using (var flow = new ProgressionController(Config, store))
            {
                Assert.That(flow.View.Loadout.CanEdit, Is.False); flow.Initialize(); flow.ShowLoadout(true);
                Assert.That(flow.View.Loadout.Slots[0].CanUnequip, Is.False);
                Assert.That(flow.ChangeLoadout(LoadoutOperation.Unequip, "utility"), Is.EqualTo(LoadoutResult.Succeeded));
                Assert.That(flow.View.Loadout.Slots[3].EquipmentId, Is.Null);
                Assert.That(flow.View.Loadout.Slots[3].Options[0].CanInstall, Is.True);
                Assert.That(flow.ChangeLoadout(LoadoutOperation.Equip, "utility", "basic-cannon"), Is.EqualTo(LoadoutResult.Incompatible));
                Assert.That(flow.View.Loadout.Slots[3].EquipmentId, Is.Null);
                flow.StartRun(RunConfiguration.Prototype()); var run = flow.Run;
                Assert.That(flow.View.Loadout.CanEdit, Is.False);
                Assert.That(flow.ChangeLoadout(LoadoutOperation.RestoreDefaults), Is.EqualTo(LoadoutResult.RunActive));
                Assert.That(run.Configuration.Equipment.EnergyPerDefeat, Is.Zero);
            }
        }

        [Test]
        public void ActiveRunEquipmentAndStatsCannotBeChangedAndTheNextRunUsesTheNewSelection()
        {
            var service = new ProgressionService(Config, new MemoryProgressionStore());
            service.CreateRun(RunConfiguration.Prototype(), out var first);
            using (first)
            {
                first.Run.Start(); Assert.That(service.Unequip("utility"), Is.EqualTo(LoadoutResult.RunActive));
                Assert.That(first.Run.Platform.Modules.IsReadOnly, Is.True);
                Assert.That(first.Run.Platform.Modules.TryDetach(0), Is.False);
                Assert.That(first.Run.Platform.Modules.TryAttach(0, new ModuleDefinition("x", ModuleKind.BasicGun)), Is.False);
                Assert.That(first.Abandon(), Is.EqualTo(ProgressionResult.Succeeded));
                Assert.That(service.Unequip("utility"), Is.EqualTo(LoadoutResult.Succeeded));
                Assert.That(first.Run.Configuration.Equipment.EnergyPerDefeat, Is.EqualTo(1));
                service.CreateRun(RunConfiguration.Prototype(), out var second);
                using (second)
                {
                    Assert.That(second.Run.Configuration.Equipment.EnergyPerDefeat, Is.Zero);
                    Assert.That(second.Run.Configuration.Platform.MaximumHealth, Is.EqualTo(100));
                    Assert.That(second.Run.RunId, Is.Not.EqualTo(first.Run.RunId));
                }
            }
        }

        [Test]
        public void FailedLoadoutSaveBlocksTheUiUntilReloadAndDoesNotAutomaticallyRepeatTheChange()
        {
            var store = new FaultStore(Codec.Encode(new ProgressionData())) { Fail = true };
            using (var flow = new ProgressionController(Config, store))
            {
                flow.Initialize();
                Assert.That(flow.ChangeLoadout(LoadoutOperation.Unequip, "defense"), Is.EqualTo(LoadoutResult.PersistenceFailed));
                Assert.That(flow.Screen, Is.EqualTo(ProgressionScreen.Recovery)); Assert.That(flow.View.Loadout.CanEdit, Is.False);
                Assert.That(flow.Initialize(), Is.Not.EqualTo(ProgressionResult.Succeeded));
                Assert.That(flow.View.Loadout.Slots[2].EquipmentId, Is.EqualTo("armor-plating"));
                store.Fail = false; flow.RetryLoad();
                Assert.That(flow.View.Loadout.Slots[2].EquipmentId, Is.EqualTo("armor-plating"));
                Assert.That(flow.View.Loadout.CanEdit, Is.True);
            }
        }

        [Test]
        public void FileMigrationPromotionFailureRetainsTheOldPrimaryAndARecoverablePendingV2()
        {
            string directory = Path.Combine(Path.GetTempPath(), "outpost-equipment-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory); string path = Path.Combine(directory, "progression-v1.json");
            try
            {
                File.WriteAllText(path, Legacy); var store = new FailingPromotion(path);
                var service = new ProgressionService(Config, store);
                Assert.That(service.Unequip("utility"), Is.EqualTo(LoadoutResult.PersistenceFailed));
                Assert.That(File.ReadAllText(path), Is.EqualTo(Legacy)); StringAssert.Contains("\"schemaVersion\":2", File.ReadAllText(store.PendingPath));
                var loaded = new ProgressionService(Config, new FileProgressionStore(path));
                Assert.That(loaded.Snapshot.Loadout.At("utility"), Is.EqualTo("reactor-booster"));
                Assert.That(loaded.Unequip("utility"), Is.EqualTo(LoadoutResult.Succeeded));
                Assert.That(File.Exists(store.PendingPath), Is.False);
                loaded = new ProgressionService(Config, new FileProgressionStore(path));
                Assert.That(loaded.Snapshot.Loadout.At("utility"), Is.Null); Assert.That(loaded.Snapshot.Credits, Is.EqualTo(450));
            }
            finally { Directory.Delete(directory, true); }
        }

        private sealed class FaultStore : IProgressionStore
        {
            public readonly MemoryProgressionStore Inner; public bool Fail;
            public FaultStore(string payload) { Inner = new MemoryProgressionStore(payload); }
            public string Read() => Inner.Read();
            public StoreCommitResult Commit(string expected, string replacement)
            { if (Fail) throw new IOException("Injected equipment save failure"); return Inner.Commit(expected, replacement); }
        }
        private sealed class FailingPromotion : FileProgressionStore
        {
            public FailingPromotion(string path) : base(path) { }
            protected override void PromotePending() => throw new IOException("Interrupted migration");
        }
    }
}
