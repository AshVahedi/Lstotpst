using System;
using System.IO;
using System.Runtime.Serialization;
using LastOutpost.Core.Progression;
using LastOutpost.Core.Simulation;
using NUnit.Framework;

namespace LastOutpost.Tests
{
    public sealed class ProgressionPersistenceTests
    {
        private string directory;
        private string path;
        private static readonly JsonProgressionCodec Codec = new JsonProgressionCodec();
        private static ProgressionConfiguration Config => ProgressionConfiguration.Prototype();

        [SetUp]
        public void SetUp()
        {
            directory = Path.Combine(Path.GetTempPath(), "last-outpost-progression-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            path = Path.Combine(directory, "progression-v1.json");
        }

        [TearDown]
        public void TearDown() { if (Directory.Exists(directory)) Directory.Delete(directory, true); }

        [Test]
        public void FileRoundTripPreservesCreditsLevelsAndDurableRewardLedger()
        {
            var store = new FileProgressionStore(path);
            var service = new ProgressionService(Config, store);
            Assert.That(File.Exists(path), Is.False);
            service.CreateRun(RunConfiguration.Prototype(), out ProgressionRunSession session);
            using (session)
            {
                ProgressionTests.Win(session.Run);
                service.BuyUpgrade(UpgradeKind.MaximumHealth);
                var loaded = new ProgressionService(Config, new FileProgressionStore(path));
                Assert.That(loaded.Snapshot.Credits, Is.EqualTo(300));
                Assert.That(loaded.Snapshot.Level(UpgradeKind.MaximumHealth), Is.EqualTo(1));
                Assert.That(loaded.Snapshot.RewardedRuns, Is.EqualTo(1));
                Assert.That(loaded.CompleteRun(session.Run), Is.EqualTo(ProgressionResult.AlreadyProcessed));
                Assert.That(loaded.Snapshot.HighestUnlockedLevel, Is.EqualTo(2));
                Assert.That(File.Exists(store.PendingPath), Is.False);
                StringAssert.Contains("\"schemaVersion\":" + ProgressionData.CurrentSchema, File.ReadAllText(path));
                StringAssert.DoesNotContain("Energy", File.ReadAllText(path));
            }
        }

        [TestCase("not json")]
        [TestCase("{")]
        [TestCase("null")]
        [TestCase("[]")]
        [TestCase("{}")]
        [TestCase("{\"schemaVersion\":1}")]
        [TestCase("{\"schemaVersion\":1,\"schemaVersion\":1}")]
        public void CorruptSavesAreReportedAndNeverOverwritten(string payload)
        {
            File.WriteAllText(path, payload);
            ProgressionLoadException error = Assert.Throws<ProgressionLoadException>(() => new ProgressionService(Config, new FileProgressionStore(path)));
            Assert.That(error.Reason, Is.EqualTo(ProgressionLoadError.Corrupt));
            Assert.That(File.ReadAllText(path), Is.EqualTo(payload));
        }

        [TestCase(3)]
        [TestCase(999)]
        [TestCase(0)]
        public void UnknownSchemasAreRejectedBeforeRequiredV1FieldsAreRead(int version)
        {
            string payload = "{\"schemaVersion\":" + version + ",\"futureField\":42}";
            var store = new MemoryProgressionStore(payload);
            ProgressionLoadException error = Assert.Throws<ProgressionLoadException>(() => new ProgressionService(Config, store));
            Assert.That(error.Reason, Is.EqualTo(ProgressionLoadError.UnsupportedSchema));
            Assert.That(store.Read(), Is.EqualTo(payload));
        }

        [TestCase("credits")]
        [TestCase("revision")]
        [TestCase("health")]
        [TestCase("armor")]
        [TestCase("weaponDamage")]
        [TestCase("ammunitionCapacity")]
        public void NegativeStoredNumbersAreRejected(string field)
        {
            var data = new ProgressionData();
            string json = Codec.Encode(data).Replace("\"" + field + "\":0", "\"" + field + "\":-1");
            var store = new MemoryProgressionStore(json);
            Assert.That(Assert.Throws<ProgressionLoadException>(() => new ProgressionService(Config, store)).Reason,
                Is.EqualTo(ProgressionLoadError.InvalidValues));
            Assert.That(store.Read(), Is.EqualTo(json));
        }

        [Test]
        public void OutOfRangeLevelsNullCollectionsAndFalseCompletionMetadataAreRejected()
        {
            var variants = new[] {
                new ProgressionData { UpgradeLevels = new UpgradeLevelsData { Health = 11 } },
                new ProgressionData { UpgradeLevels = null },
                new ProgressionData { Runs = null },
                new ProgressionData { CompletedLevelIds = null },
                new ProgressionData { HighestUnlockedLevel = 0 },
                new ProgressionData { HighestUnlockedLevel = 99 },
                new ProgressionData { CompletedLevelIds = new System.Collections.Generic.List<string> { "level-1" } }
            };
            foreach (ProgressionData data in variants)
                Assert.That(Assert.Throws<ProgressionLoadException>(() => Codec.Decode(Codec.Encode(data), Config)).Reason,
                    Is.EqualTo(ProgressionLoadError.InvalidValues));
        }

        [Test]
        public void DuplicateInvalidOrMismatchedRunRecordsAreRejected()
        {
            var record = new RunRecordData { RunId = Guid.NewGuid().ToString("N"), LevelId = "level-1", LevelNumber = 1, VictoryCredits = 500 };
            var data = new ProgressionData();
            data.Runs.Add(record);
            data.Runs.Add(record);
            Assert.Throws<ProgressionLoadException>(() => Codec.Decode(Codec.Encode(data), Config));
            data.Runs.RemoveAt(1);
            record.RunId = "invalid";
            Assert.Throws<ProgressionLoadException>(() => Codec.Decode(Codec.Encode(data), Config));
            record.RunId = Guid.NewGuid().ToString("N");
            record.VictoryCredits = -1;
            Assert.Throws<ProgressionLoadException>(() => Codec.Decode(Codec.Encode(data), Config));
            record.VictoryCredits = 500;
            record.LevelNumber = 2;
            Assert.Throws<ProgressionLoadException>(() => Codec.Decode(Codec.Encode(data), Config));
            record.LevelNumber = 1;
            record.Outcome = (SavedRunOutcome)99;
            Assert.Throws<ProgressionLoadException>(() => Codec.Decode(Codec.Encode(data), Config));
        }

        [Test]
        public void MissingFileWithOnlyPendingWriteDoesNotSilentlyStartOver()
        {
            var store = new FileProgressionStore(path);
            string pending = Codec.Encode(new ProgressionData { Credits = 500 });
            File.WriteAllText(store.PendingPath, pending);
            Assert.That(Assert.Throws<ProgressionLoadException>(() => new ProgressionService(Config, store)).Reason,
                Is.EqualTo(ProgressionLoadError.InterruptedWrite));
            Assert.That(File.ReadAllText(store.PendingPath), Is.EqualTo(pending));
            Assert.That(File.Exists(path), Is.False);
        }

        [Test]
        public void InterruptedPendingWriteKeepsLastValidPrimaryAndExplicitRetryCanRecover()
        {
            string valid = Codec.Encode(new ProgressionData { Credits = 500 });
            File.WriteAllText(path, valid);
            var store = new FileProgressionStore(path);
            File.WriteAllText(store.PendingPath, "{incomplete candidate");
            var service = new ProgressionService(Config, store);
            Assert.That(store.HasPendingWrite, Is.True);
            Assert.That(service.Snapshot.Credits, Is.EqualTo(500));
            Assert.That(File.ReadAllText(path), Is.EqualTo(valid));
            Assert.That(service.BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.Succeeded));
            Assert.That(new ProgressionService(Config, store).Snapshot.Credits, Is.EqualTo(300));
            Assert.That(store.HasPendingWrite, Is.False);
        }

        [Test]
        public void FailureAfterWritingPendingDoesNotChangePrimaryBalanceOrLevel()
        {
            string valid = Codec.Encode(new ProgressionData { Credits = 500 });
            File.WriteAllText(path, valid);
            var store = new FailingFileStore(path) { Fail = true };
            var service = new ProgressionService(Config, store);
            Assert.That(service.BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.PersistenceFailed));
            Assert.That(service.Snapshot.Credits, Is.EqualTo(500));
            Assert.That(service.Snapshot.Level(UpgradeKind.MaximumHealth), Is.Zero);
            Assert.That(File.ReadAllText(path), Is.EqualTo(valid));
            Assert.That(store.HasPendingWrite, Is.True);
            Assert.That(service.BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.ReloadRequired));
            store.Fail = false;
            service.Reload();
            Assert.That(service.BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.Succeeded));
            Assert.That(service.Snapshot.Credits, Is.EqualTo(300));
            Assert.That(new ProgressionService(Config, store).Snapshot.Level(UpgradeKind.MaximumHealth), Is.EqualTo(1));
        }

        [Test]
        public void FailedRunRegistrationDoesNotReturnAnUntrackedSession()
        {
            var store = new FaultStore { FailBeforeWrite = true };
            var service = new ProgressionService(Config, store);
            Assert.That(service.CreateRun(RunConfiguration.Prototype(), out ProgressionRunSession session), Is.EqualTo(ProgressionResult.PersistenceFailed));
            Assert.That(session, Is.Null);
            Assert.That(service.Snapshot.RegisteredRuns, Is.Zero);
            Assert.That(store.Inner.Read(), Is.Null);
        }

        [Test]
        public void FailedVictorySaveCanBeReloadedAndRetriedExactlyOnce()
        {
            var store = new FaultStore();
            var service = new ProgressionService(Config, store);
            service.CreateRun(RunConfiguration.Prototype(), out ProgressionRunSession session);
            using (session)
            {
                store.FailBeforeWrite = true;
                ProgressionTests.Win(session.Run);
                Assert.That(session.LastCompletionResult, Is.EqualTo(ProgressionResult.PersistenceFailed));
                Assert.That(service.Snapshot.Credits, Is.Zero);
                Assert.That(new ProgressionService(Config, store).Snapshot.RewardedRuns, Is.Zero);
                store.FailBeforeWrite = false;
                service.Reload();
                Assert.That(session.RetryCompletion(), Is.EqualTo(ProgressionResult.Succeeded));
                Assert.That(session.RetryCompletion(), Is.EqualTo(ProgressionResult.AlreadyProcessed));
                Assert.That(new ProgressionService(Config, store).Snapshot.Credits, Is.EqualTo(500));
            }
        }

        [Test]
        public void FailureAcknowledgingAnAlreadyCommittedPurchaseIsReconciledAsSuccess()
        {
            var store = new FaultStore { Inner = new MemoryProgressionStore(Codec.Encode(new ProgressionData { Credits = 500 })), FailAfterWrite = true };
            var service = new ProgressionService(Config, store);
            Assert.That(service.BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.Succeeded));
            Assert.That(service.Snapshot.Credits, Is.EqualTo(300));
            Assert.That(service.Snapshot.Level(UpgradeKind.MaximumHealth), Is.EqualTo(1));
            Assert.That(service.RequiresReload, Is.False);
        }

        [Test]
        public void UnreadableOutcomeAfterCommitIsExplicitlyUncertainAndBlocksRetryUntilReload()
        {
            var store = new FaultStore { Inner = new MemoryProgressionStore(Codec.Encode(new ProgressionData { Credits = 500 })) };
            var service = new ProgressionService(Config, store);
            store.FailAfterWrite = true;
            store.FailReadAfterWrite = true;
            Assert.That(service.BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.PersistenceUncertain));
            Assert.That(service.Snapshot.Credits, Is.EqualTo(500));
            Assert.That(service.BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.ReloadRequired));
            store.ReadFailure = false;
            service.Reload();
            Assert.That(service.Snapshot.Credits, Is.EqualTo(300));
            Assert.That(service.Snapshot.Level(UpgradeKind.MaximumHealth), Is.EqualTo(1));
        }

        [Test]
        public void CompareAndSwapRejectsStaleWritersWithoutLosingAnotherPurchase()
        {
            var store = new MemoryProgressionStore(Codec.Encode(new ProgressionData { Credits = 500 }));
            var first = new ProgressionService(Config, store);
            var stale = new ProgressionService(Config, store);
            Assert.That(first.BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.Succeeded));
            Assert.That(stale.BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.Conflict));
            Assert.That(stale.Snapshot.Credits, Is.EqualTo(500));
            Assert.That(stale.RequiresReload, Is.True);
            stale.Reload();
            Assert.That(stale.Snapshot.Credits, Is.EqualTo(300));
            Assert.That(stale.BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.Succeeded));
            Assert.That(new ProgressionService(Config, store).Snapshot.Credits, Is.Zero);
            Assert.That(new ProgressionService(Config, store).Snapshot.Level(UpgradeKind.MaximumHealth), Is.EqualTo(2));
        }

        [Test]
        public void TwoFileStoreInstancesAlsoRejectStaleWriters()
        {
            File.WriteAllText(path, Codec.Encode(new ProgressionData { Credits = 500 }));
            var first = new ProgressionService(Config, new FileProgressionStore(path));
            var stale = new ProgressionService(Config, new FileProgressionStore(path));
            first.BuyUpgrade(UpgradeKind.MaximumHealth);
            Assert.That(stale.BuyUpgrade(UpgradeKind.Armor), Is.EqualTo(ProgressionResult.Conflict));
            Assert.That(new ProgressionService(Config, new FileProgressionStore(path)).Snapshot.Level(UpgradeKind.Armor), Is.Zero);
        }

        [Test]
        public void ExclusiveFileLockProducesStorageFailureWithoutAWrite()
        {
            var store = new FileProgressionStore(path);
            var service = new ProgressionService(Config, store);
            using (var held = new FileStream(path + ".lock", FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                Assert.That(service.CreateRun(RunConfiguration.Prototype(), out _), Is.EqualTo(ProgressionResult.PersistenceUncertain));
                Assert.That(File.Exists(path), Is.False);
            }
            service.Reload();
            Assert.That(service.Snapshot.RegisteredRuns, Is.Zero);
        }

        [Test]
        public void FailedReloadKeepsLastKnownSnapshotAndBlocksFurtherTransactions()
        {
            var store = new FaultStore { Inner = new MemoryProgressionStore(Codec.Encode(new ProgressionData { Credits = 500 })) };
            var service = new ProgressionService(Config, store);
            store.ReadFailure = true;
            Assert.That(Assert.Throws<ProgressionLoadException>(() => service.Reload()).Reason, Is.EqualTo(ProgressionLoadError.StorageUnavailable));
            Assert.That(service.Snapshot.Credits, Is.EqualTo(500));
            Assert.That(service.BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.ReloadRequired));
        }

        [Test]
        public void AnotherInterruptedFirstWriteBlocksAnAlreadyLoadedServiceWithoutAnUnhandledException()
        {
            var store = new FileProgressionStore(path);
            var service = new ProgressionService(Config, store);
            File.WriteAllText(store.PendingPath, "{interrupted registration");
            Assert.That(service.CreateRun(RunConfiguration.Prototype(), out ProgressionRunSession session), Is.EqualTo(ProgressionResult.PersistenceUncertain));
            Assert.That(session, Is.Null);
            Assert.That(service.RequiresReload, Is.True);
            Assert.That(service.Snapshot.RegisteredRuns, Is.Zero);
            Assert.That(File.ReadAllText(store.PendingPath), Is.EqualTo("{interrupted registration"));
        }

        [TestCase(" garbage")]
        [TestCase("{}")]
        public void TrailingContentCannotBeMistakenForAValidSave(string suffix)
        {
            string json = Codec.Encode(new ProgressionData()) + suffix;
            Assert.Throws<ProgressionLoadException>(() => Codec.Decode(json, Config));
        }

        [Test]
        public void ExcessivelyNestedJsonAndInvalidFileEncodingAreRejectedWithoutOverwrite()
        {
            string nested = "{\"schemaVersion\":1,\"ignored\":" + new string('[', 100) + "0" + new string(']', 100) + "}";
            Assert.Throws<ProgressionLoadException>(() => Codec.Decode(nested, Config));
            byte[] malformed = { 0xff, 0xff };
            File.WriteAllBytes(path, malformed);
            Assert.That(Assert.Throws<ProgressionLoadException>(() => new ProgressionService(Config, new FileProgressionStore(path))).Reason,
                Is.EqualTo(ProgressionLoadError.Corrupt));
            Assert.That(File.ReadAllBytes(path), Is.EqualTo(malformed));
        }

        [Test]
        public void ValidEscapedStringsAndTrailingJsonWhitespaceSurviveBoundaryValidation()
        {
            string levelId = "level-\\\"}{[]";
            var basis = Config;
            var config = new ProgressionConfiguration(basis.BasePlatform, new[] { basis.Get(UpgradeKind.MaximumHealth),
                basis.Get(UpgradeKind.Armor), basis.Get(UpgradeKind.WeaponDamage), basis.Get(UpgradeKind.AmmunitionCapacity) },
                new[] { new LevelRewardDefinition(levelId, 1, 500) });
            var data = new ProgressionData();
            data.Runs.Add(new RunRecordData { RunId = Guid.NewGuid().ToString("N"), LevelId = levelId, LevelNumber = 1, VictoryCredits = 500 });
            Assert.That(Codec.Decode(Codec.Encode(data) + "\r\n \t", config).Runs[0].LevelId, Is.EqualTo(levelId));
        }

        [TestCase(typeof(SerializationException))]
        [TestCase(typeof(NotSupportedException))]
        [TestCase(typeof(InvalidDataContractException))]
        public void SerializationFailureDoesNotCallStoreOrCharge(Type errorType)
        {
            var store = new MemoryProgressionStore(Codec.Encode(new ProgressionData { Credits = 500 }));
            var codec = new FailingCodec { Error = (Exception)Activator.CreateInstance(errorType) };
            var service = new ProgressionService(Config, store, codec);
            string before = store.Read();
            Assert.That(service.BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.PersistenceFailed));
            Assert.That(store.Read(), Is.EqualTo(before));
            Assert.That(service.Snapshot.Credits, Is.EqualTo(500));
            Assert.That(service.Snapshot.Level(UpgradeKind.MaximumHealth), Is.Zero);
            Assert.That(service.RequiresReload, Is.False);
        }

        [TestCase("1.5")]
        [TestCase("true")]
        [TestCase("\"-1\"")]
        [TestCase("9223372036854775808")]
        public void InvalidStoredCreditValuesAreRejectedWithoutReplacingTheFile(string value)
        {
            string payload = Codec.Encode(new ProgressionData()).Replace("\"credits\":0", "\"credits\":" + value);
            File.WriteAllText(path, payload);
            Assert.Throws<ProgressionLoadException>(() => new ProgressionService(Config, new FileProgressionStore(path)));
            Assert.That(File.ReadAllText(path), Is.EqualTo(payload));
        }

        [TestCase("credits", "0", "500")]
        [TestCase("schemaVersion", "2", "3")]
        public void DuplicateRequiredMembersInACompleteSaveAreRejected(string member, string first, string second)
        {
            string field = "\"" + member + "\":";
            string payload = Codec.Encode(new ProgressionData()).Replace(field + first, field + first + "," + field + second);
            File.WriteAllText(path, payload);
            Assert.Throws<ProgressionLoadException>(() => new ProgressionService(Config, new FileProgressionStore(path)));
            Assert.That(File.ReadAllText(path), Is.EqualTo(payload));
        }

        private sealed class FailingFileStore : FileProgressionStore
        {
            public bool Fail;
            public FailingFileStore(string file) : base(file) { }
            protected override void PromotePending()
            {
                if (Fail) throw new IOException("Injected interruption after pending flush, before atomic replacement.");
                base.PromotePending();
            }
        }

        private sealed class FaultStore : IProgressionStore
        {
            public MemoryProgressionStore Inner = new MemoryProgressionStore();
            public bool FailBeforeWrite;
            public bool FailAfterWrite;
            public bool FailReadAfterWrite;
            public bool ReadFailure;
            public string Read() => ReadFailure ? throw new IOException("Injected read failure") : Inner.Read();
            public StoreCommitResult Commit(string expected, string replacement)
            {
                if (FailBeforeWrite) throw new IOException("Injected write failure");
                StoreCommitResult result = Inner.Commit(expected, replacement);
                if (FailReadAfterWrite) ReadFailure = true;
                if (FailAfterWrite) throw new IOException("Injected lost acknowledgment");
                return result;
            }
        }

        private sealed class FailingCodec : IProgressionCodec
        {
            public Exception Error = new SerializationException("Injected serialization failure");
            public ProgressionData Decode(string json, ProgressionConfiguration configuration) => Codec.Decode(json, configuration);
            public string Encode(ProgressionData data) => throw Error;
        }
    }
}
