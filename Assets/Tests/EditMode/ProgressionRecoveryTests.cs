using System;
using System.IO;
using LastOutpost.Core.Progression;
using NUnit.Framework;

namespace LastOutpost.Tests
{
    public sealed class ProgressionRecoveryTests
    {
        private string directory, path;
        [SetUp] public void SetUp()
        { directory = Path.Combine(Path.GetTempPath(), "outpost-recovery-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory); path = Path.Combine(directory, "progression-v1.json"); }
        [TearDown] public void TearDown() { Directory.Delete(directory, true); }

        [TestCase(false)]
        [TestCase(true)]
        public void ConfirmedResetBacksUpRawCorruptAndPendingBytesBeforeCreatingSchemaOne(bool primaryExists)
        {
            byte[] bad = { 0xff, 0xff, 0, 5 };
            if (primaryExists) File.WriteAllBytes(path, bad);
            File.WriteAllText(path + ".pending", "{interrupted");
            var store = new FileProgressionStore(path);
            using (var flow = new ProgressionController(ProgressionConfiguration.Prototype(), store))
            {
                flow.Initialize(); Assert.That(flow.Screen, Is.EqualTo(ProgressionScreen.Recovery));
                flow.RequestNewProfile(); Assert.That(File.ReadAllText(path + ".pending"), Is.EqualTo("{interrupted"));
                Assert.That(flow.ConfirmNewProfile(true), Is.EqualTo(ProgressionResult.Succeeded));
                Assert.That(flow.View.Credits, Is.Zero); Assert.That(flow.View.CanStart, Is.True);
                if (primaryExists) Assert.That(File.ReadAllBytes(Path.Combine(flow.LastBackup, Path.GetFileName(path))), Is.EqualTo(bad));
                Assert.That(File.ReadAllText(Path.Combine(flow.LastBackup, Path.GetFileName(path) + ".pending")), Is.EqualTo("{interrupted"));
                Assert.That(File.Exists(path + ".pending"), Is.False);
                Assert.That(new ProgressionService(ProgressionConfiguration.Prototype(), store).Snapshot.Credits, Is.Zero);
            }
        }

        [Test]
        public void ANewerFileInvalidatesAResetTokenAndProducesNoBackupOrReplacement()
        {
            File.WriteAllText(path, "{bad"); var store = new FileProgressionStore(path);
            string token = store.CaptureRecoveryToken(); File.WriteAllText(path, "newer data");
            Assert.That(store.ResetWithBackup(token, "replacement", out string backup), Is.EqualTo(StoreCommitResult.Conflict));
            Assert.That(backup, Is.Null); Assert.That(File.ReadAllText(path), Is.EqualTo("newer data"));
            Assert.That(Directory.GetDirectories(directory), Is.Empty);
        }

        [Test]
        public void FailedAtomicPromotionPreservesOriginalAndBackupAndDoesNotReportSuccessfulReset()
        {
            File.WriteAllText(path, "{original");
            var store = new FailingRecoveryStore(path);
            using (var flow = new ProgressionController(ProgressionConfiguration.Prototype(), store))
            {
                flow.Initialize(); flow.RequestNewProfile();
                Assert.That(flow.ConfirmNewProfile(true), Is.EqualTo(ProgressionResult.PersistenceUncertain));
                Assert.That(flow.Screen, Is.EqualTo(ProgressionScreen.Recovery));
                Assert.That(flow.View.Credits, Is.Null); Assert.That(flow.View.CanStart, Is.False);
                Assert.That(File.ReadAllText(path), Is.EqualTo("{original"));
                string backup = Directory.GetDirectories(directory)[0];
                Assert.That(File.ReadAllText(Path.Combine(backup, Path.GetFileName(path))), Is.EqualTo("{original"));
            }
        }

        [Test]
        public void RetryLoadAfterExternalRepairLoadsTheExistingProfileRatherThanStartingOver()
        {
            File.WriteAllText(path, "{bad");
            using (var flow = new ProgressionController(ProgressionConfiguration.Prototype(), new FileProgressionStore(path)))
            {
                flow.Initialize();
                string repaired = new JsonProgressionCodec().Encode(new ProgressionData { Credits = 725 });
                File.WriteAllText(path, repaired);
                Assert.That(flow.RetryLoad(), Is.EqualTo(ProgressionResult.Succeeded));
                Assert.That(flow.View.Credits, Is.EqualTo(725)); Assert.That(File.ReadAllText(path), Is.EqualTo(repaired));
            }
        }

        [Test]
        public void MissingSaveInitializesWithoutWritingAndRegistrationIsTheFirstCommit()
        {
            using (var flow = new ProgressionController(ProgressionConfiguration.Prototype(), new FileProgressionStore(path)))
            {
                Assert.That(flow.Initialize(), Is.EqualTo(ProgressionResult.Succeeded));
                Assert.That(flow.View.Credits, Is.Zero); Assert.That(flow.View.HighestUnlockedLevel, Is.EqualTo(1));
                Assert.That(File.Exists(path), Is.False);
                flow.StartRun(LastOutpost.Core.Simulation.RunConfiguration.Prototype());
                Assert.That(File.Exists(path), Is.True);
                Assert.That(flow.Run.Platform.Resources.GetBalance(LastOutpost.Core.Economy.ResourceType.Energy), Is.Zero);
            }
        }

        [Test]
        public void LostAcknowledgmentAfterResetRecognizesTheNewPrimaryAndPreservesTheBackup()
        {
            File.WriteAllText(path, "{old");
            using (var flow = new ProgressionController(ProgressionConfiguration.Prototype(), new LostAcknowledgmentStore(path)))
            {
                flow.Initialize(); flow.RequestNewProfile();
                Assert.That(flow.ConfirmNewProfile(true), Is.EqualTo(ProgressionResult.Succeeded));
                Assert.That(flow.View.Credits, Is.Zero); Assert.That(flow.View.CanStart, Is.True);
                Assert.That(File.ReadAllText(Path.Combine(flow.LastBackup, Path.GetFileName(path))), Is.EqualTo("{old"));
            }
        }

        private sealed class FailingRecoveryStore : FileProgressionStore
        {
            public FailingRecoveryStore(string path) : base(path) { }
            protected override void PromotePending() => throw new IOException("promotion failure");
        }
        [Test]
        public void ReloadAfterUncertainConfirmedResetDetachesTheOldPendingRunWithoutPayingIt()
        {
            var store = new UncertainResetStore(path);
            File.WriteAllText(path, new JsonProgressionCodec().Encode(new ProgressionData { Credits = 500 }));
            using (var flow = new ProgressionController(ProgressionConfiguration.Prototype(), store))
            {
                flow.Initialize(); flow.StartRun(LastOutpost.Core.Simulation.RunConfiguration.Prototype());
                store.RejectCommit = true; ProgressionTests.Win(flow.Run);
                Assert.That(flow.Screen, Is.EqualTo(ProgressionScreen.Recovery));
                flow.RequestNewProfile(); store.RejectCommit = false; store.LoseResetAcknowledgment = true;
                Assert.That(flow.ConfirmNewProfile(true), Is.EqualTo(ProgressionResult.PersistenceUncertain));
                Assert.That(flow.Run, Is.Not.Null);
                store.FailRead = false;
                Assert.That(flow.RetryLoad(), Is.EqualTo(ProgressionResult.Succeeded));
                Assert.That(flow.Run, Is.Null); Assert.That(flow.View.CanStart, Is.True);
                Assert.That(flow.View.Credits, Is.Zero); Assert.That(flow.View.CommittedReward, Is.Null);
                Assert.That(File.Exists(Path.Combine(flow.LastBackup, Path.GetFileName(path))), Is.True);
            }
        }

        private sealed class UncertainResetStore : IProgressionRecoveryStore
        {
            public bool RejectCommit, LoseResetAcknowledgment, FailRead;
            private readonly AcknowledgmentStore inner;
            public UncertainResetStore(string path) { inner = new AcknowledgmentStore(path, AfterPromotion); }
            public string Read() => FailRead ? throw new IOException("unreadable") : inner.Read();
            public StoreCommitResult Commit(string expected, string replacement)
            { if (RejectCommit) throw new IOException("write failed"); return inner.Commit(expected, replacement); }
            public string CaptureRecoveryToken() => inner.CaptureRecoveryToken();
            public StoreCommitResult ResetWithBackup(string token, string replacement, out string backup)
                => inner.ResetWithBackup(token, replacement, out backup);
            private void AfterPromotion()
            {
                if (!LoseResetAcknowledgment) return;
                LoseResetAcknowledgment = false; FailRead = true; throw new IOException("reset acknowledgment lost");
            }
            private sealed class AcknowledgmentStore : FileProgressionStore
            {
                private readonly Action after;
                public AcknowledgmentStore(string path, Action after) : base(path) { this.after = after; }
                protected override void PromotePending() { base.PromotePending(); after(); }
            }
        }
        private sealed class LostAcknowledgmentStore : FileProgressionStore
        {
            public LostAcknowledgmentStore(string path) : base(path) { }
            protected override void PromotePending() { base.PromotePending(); throw new IOException("lost acknowledgment"); }
        }
    }
}
