using System;
using System.IO;
using System.Runtime.Serialization;
using LastOutpost.Core.Economy;
using LastOutpost.Core.Progression;
using LastOutpost.Core.Simulation;
using NUnit.Framework;

namespace LastOutpost.Tests
{
    public sealed class ProgressionIntegrationTests
    {
        private static readonly JsonProgressionCodec Codec = new JsonProgressionCodec();
        private static readonly ProgressionConfiguration Config = ProgressionConfiguration.Prototype();
        private static MemoryProgressionStore Seed(long credits = 0, int health = 0, int damage = 0) =>
            new MemoryProgressionStore(Codec.Encode(new ProgressionData { Credits = credits,
                UpgradeLevels = new UpgradeLevelsData { Health = health, WeaponDamage = damage } }));
        private static ProgressionController Open(IProgressionStore store)
        { var flow = new ProgressionController(Config, store); Assert.That(flow.Initialize(), Is.EqualTo(ProgressionResult.Succeeded)); return flow; }

        [Test]
        public void LoadingPrecedesRunInitializationAndRepeatedInitializationDoesNotReadAgain()
        {
            var store = new ProbeStore { Inner = Seed(500, 3, 4) };
            using (var flow = new ProgressionController(Config, store))
            {
                Assert.That(flow.View.Credits, Is.Null);
                Assert.That(flow.StartRun(RunConfiguration.Prototype()), Is.Not.EqualTo(ProgressionResult.Succeeded));
                Assert.That(flow.Run, Is.Null);
                Assert.That(store.Reads, Is.Zero);
                flow.Initialize(); flow.Initialize(); flow.Initialize();
                Assert.That(store.Reads, Is.EqualTo(1));
                Assert.That(flow.View.Credits, Is.EqualTo(500));
                Assert.That(flow.View.CanStart, Is.True);
                flow.StartRun(RunConfiguration.Prototype());
                Assert.That(flow.Run.Platform.Health.Maximum, Is.EqualTo(130));
                Assert.That(flow.Run.Configuration.BasicGun.Damage, Is.EqualTo(10));
                Assert.That(flow.Run.Configuration.MissileLauncher.Damage, Is.EqualTo(36));
                Assert.That(flow.Run.Platform.AdvancedAmmunition.Capacity, Is.EqualTo(100));
            }
        }

        [Test]
        public void OverlappingInitializeAndReentrantPurchasesCannotQueueTransactions()
        {
            var store = new ProbeStore { Inner = Seed(500) };
            using (var flow = new ProgressionController(Config, store))
            {
                store.OnRead = () => {
                    Assert.That(flow.Initialize(), Is.Not.EqualTo(ProgressionResult.Succeeded));
                    Assert.That(flow.StartRun(RunConfiguration.Prototype()), Is.Not.EqualTo(ProgressionResult.Succeeded));
                };
                flow.Initialize();
                store.OnCommit = () => {
                    Assert.That(flow.View.IsBusy, Is.True);
                    Assert.That(flow.View.Upgrades[0].CanPurchase, Is.False);
                    Assert.That(flow.BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.Busy));
                };
                Assert.That(flow.BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.Succeeded));
                Assert.That(store.Commits, Is.EqualTo(1));
                Assert.That(flow.View.Credits, Is.EqualTo(300));
                Assert.That(flow.View.Upgrades[0].Level, Is.EqualTo(1));
            }
        }

        [Test]
        public void MenuPreviewUsesAuthoritativeEffectsAndCostsAndRefreshesAfterPurchase()
        {
            using (var flow = Open(Seed(500)))
            {
                Assert.That(flow.ShowUpgrades(true), Is.True);
                UpgradeMenuRow row = flow.View.Upgrades[0];
                Assert.That(row.CurrentEffect, Is.EqualTo(100)); Assert.That(row.NextEffect, Is.EqualTo(110));
                Assert.That(row.Price, Is.EqualTo(200)); Assert.That(row.CanPurchase, Is.True);
                flow.BuyUpgrade(UpgradeKind.MaximumHealth);
                row = flow.View.Upgrades[0];
                Assert.That(row.CurrentText, Is.EqualTo("110 HP"));
                Assert.That(row.NextText, Is.EqualTo("120 HP"));
                Assert.That(row.Price, Is.EqualTo(300)); Assert.That(row.CanPurchase, Is.True);
                Assert.That(flow.View.Credits, Is.EqualTo(300));
            }
        }

        [Test]
        public void UnaffordableAndMaximumUpgradesAreDisabledAndCannotAlterTheProfile()
        {
            var data = new ProgressionData { Credits = 199 }; data.UpgradeLevels.Armor = 10;
            var store = new MemoryProgressionStore(Codec.Encode(data));
            using (var flow = Open(store))
            {
                Assert.That(flow.View.Upgrades[0].CanPurchase, Is.False);
                Assert.That(flow.View.CreditsAreCurrent, Is.True);
                Assert.That(flow.View.Upgrades[1].IsMaximum, Is.True);
                Assert.That(flow.View.Upgrades[1].Price, Is.Null);
                Assert.That(flow.View.Upgrades[1].NextEffect, Is.Null);
                string before = store.Read();
                Assert.That(flow.BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.InsufficientCredits));
                Assert.That(flow.BuyUpgrade(UpgradeKind.Armor), Is.EqualTo(ProgressionResult.MaximumLevel));
                Assert.That(store.Read(), Is.EqualTo(before));
                Assert.That(flow.View.Credits, Is.EqualTo(199));
            }
        }

        [Test]
        public void RestartAndReturningToMenuDoNotStackModifiersOrCarryEnergy()
        {
            using (var flow = Open(Seed(500, 3, 4)))
            {
                RunConfiguration template = RunConfiguration.Prototype();
                flow.StartRun(template);
                string firstId = flow.Run.RunId;
                flow.Run.Platform.Resources.Collect(ResourceType.Energy, 80);
                Assert.That(flow.BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.NotBetweenRuns));
                Assert.That(flow.ReturnToMenu(), Is.EqualTo(ProgressionResult.Succeeded));
                Assert.That(flow.Run, Is.Null); Assert.That(flow.View.Credits, Is.EqualTo(500));
                flow.StartRun(template);
                Assert.That(flow.Run.RunId, Is.Not.EqualTo(firstId));
                Assert.That(flow.Run.Configuration.BasicGun.Damage, Is.EqualTo(10));
                Assert.That(flow.Run.Platform.Health.Maximum, Is.EqualTo(130));
                Assert.That(flow.Run.Platform.Resources.GetBalance(ResourceType.Energy), Is.Zero);
                Assert.That(template.BasicGun.Damage, Is.EqualTo(8));
            }
        }

        [Test]
        public void VictoryCommitsBeforeNotificationAndSceneStyleRebindingNeverRepaysIt()
        {
            var store = Seed();
            using (var flow = Open(store))
            {
                int finishes = 0;
                flow.Finished += _ => { finishes++; Assert.That(flow.View.CommittedReward, Is.EqualTo(500)); };
                flow.StartRun(RunConfiguration.Prototype());
                GameRun run = flow.Run;
                ProgressionTests.Win(run);
                Assert.That(flow.Screen, Is.EqualTo(ProgressionScreen.Victory));
                Assert.That(flow.View.Credits, Is.EqualTo(500));
                string committed = store.Read();
                for (int i = 0; i < 3; i++)
                    using (var binding = new ProgressionMenuBinding(flow, view => Assert.That(view.CommittedReward, Is.EqualTo(500))))
                        flow.Initialize();
                run.Tick(100);
                Assert.That(store.Read(), Is.EqualTo(committed)); Assert.That(finishes, Is.EqualTo(1));
                var loaded = new ProgressionService(Config, store);
                Assert.That(loaded.CompleteRun(run), Is.EqualTo(ProgressionResult.AlreadyProcessed));
                flow.ReturnToMenu(); flow.StartRun(RunConfiguration.Prototype()); ProgressionTests.Win(flow.Run);
                Assert.That(flow.View.Credits, Is.EqualTo(1000));
            }
        }

        [Test]
        public void DefeatShowsZeroCommittedCreditsAndCanReturnToBetweenRuns()
        {
            using (var flow = Open(Seed()))
            {
                flow.StartRun(RunConfiguration.Prototype()); flow.Run.Platform.Health.TakeDamage(int.MaxValue);
                flow.Advance(0.1);
                Assert.That(flow.Screen, Is.EqualTo(ProgressionScreen.Defeat));
                Assert.That(flow.View.CommittedReward, Is.Zero); Assert.That(flow.View.Credits, Is.Zero);
                Assert.That(flow.ReturnToMenu(), Is.EqualTo(ProgressionResult.Succeeded));
                Assert.That(flow.View.CanStart, Is.True);
            }
        }

        [Test]
        public void FailedRewardSaveRemainsPendingAndOnlyAnIdempotentRetryCanFinishIt()
        {
            var store = new ProbeStore { Inner = Seed() };
            using (var flow = Open(store))
            {
                flow.StartRun(RunConfiguration.Prototype()); store.FailWrite = true;
                ProgressionTests.Win(flow.Run);
                Assert.That(flow.Screen, Is.EqualTo(ProgressionScreen.Recovery));
                Assert.That(flow.View.Credits, Is.Zero); Assert.That(flow.View.CommittedReward, Is.Null);
                Assert.That(flow.View.CanStart, Is.False); Assert.That(flow.ReturnToMenu(), Is.EqualTo(ProgressionResult.ReloadRequired));
                GameRun pending = flow.Run;
                store.FailWrite = false;
                flow.RetryLoad();
                Assert.That(flow.Run, Is.SameAs(pending)); Assert.That(flow.View.CanRetrySave, Is.True);
                Assert.That(flow.RetrySave(), Is.EqualTo(ProgressionResult.Succeeded));
                Assert.That(flow.View.Credits, Is.EqualTo(500)); Assert.That(flow.View.CommittedReward, Is.EqualTo(500));
            }
        }

        [Test]
        public void AnUncertainCommittedRewardIsRecognizedOnReloadWithoutAnotherCreditGrant()
        {
            var store = new ProbeStore { Inner = Seed() };
            using (var flow = Open(store))
            {
                flow.StartRun(RunConfiguration.Prototype()); store.LoseAcknowledgment = true;
                ProgressionTests.Win(flow.Run);
                Assert.That(flow.View.CommittedReward, Is.Null);
                store.FailRead = false; store.LoseAcknowledgment = false;
                string committed = store.Inner.Read();
                flow.RetryLoad();
                Assert.That(flow.Screen, Is.EqualTo(ProgressionScreen.Victory));
                Assert.That(flow.View.CommittedReward, Is.EqualTo(500));
                Assert.That(store.Inner.Read(), Is.EqualTo(committed));
            }
        }

        [Test]
        public void StaleWriterConflictDisablesPurchasesAndReloadDoesNotRetryThePurchase()
        {
            var store = Seed(500);
            using (var flow = Open(store))
            {
                new ProgressionService(Config, store).BuyUpgrade(UpgradeKind.Armor);
                Assert.That(flow.BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.Conflict));
                Assert.That(flow.View.Upgrades[0].CanPurchase, Is.False);
                Assert.That(flow.View.CreditsAreCurrent, Is.False);
                Assert.That(flow.BuyUpgrade(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.NotBetweenRuns));
                flow.RetryLoad();
                Assert.That(flow.View.Credits, Is.EqualTo(250));
                Assert.That(flow.View.CreditsAreCurrent, Is.True);
                Assert.That(flow.View.Upgrades[0].Level, Is.Zero); Assert.That(flow.View.Upgrades[1].Level, Is.EqualTo(1));
            }
        }

        [TestCase("{damaged")]
        [TestCase("{\"schemaVersion\":99}")]
        public void InvalidSaveStaysPreservedAndResetRequiresBothRequestAndConfirmation(string payload)
        {
            var store = new MemoryProgressionStore(payload);
            using (var flow = new ProgressionController(Config, store))
            {
                flow.Initialize(); Assert.That(flow.Screen, Is.EqualTo(ProgressionScreen.Recovery));
                Assert.That(flow.View.Credits, Is.Null); Assert.That(flow.View.Upgrades, Is.Empty);
                Assert.That(flow.ConfirmNewProfile(true), Is.EqualTo(ProgressionResult.ConfirmationRequired));
                flow.RequestNewProfile();
                Assert.That(flow.ConfirmNewProfile(false), Is.EqualTo(ProgressionResult.ConfirmationRequired));
                flow.CancelNewProfile(); Assert.That(store.Read(), Is.EqualTo(payload));
                flow.RequestNewProfile(); flow.ConfirmNewProfile(true);
                Assert.That(flow.Screen, Is.EqualTo(ProgressionScreen.Start));
                Assert.That(flow.View.Credits, Is.Zero); Assert.That(store.Backups[0], Is.EqualTo(payload));
            }
        }

        [Test]
        public void ResetConfirmationDoesNotOverwriteDataChangedSinceTheRequest()
        {
            var store = new MemoryProgressionStore("{bad");
            using (var flow = new ProgressionController(Config, store))
            {
                flow.Initialize(); flow.RequestNewProfile();
                string newer = Codec.Encode(new ProgressionData { Credits = 500 });
                store.Commit("{bad", newer);
                Assert.That(flow.ConfirmNewProfile(true), Is.EqualTo(ProgressionResult.Conflict));
                Assert.That(store.Read(), Is.EqualTo(newer)); Assert.That(store.Backups, Is.Empty);
                flow.RetryLoad(); Assert.That(flow.View.Credits, Is.EqualTo(500));
            }
        }

        [Test]
        public void DisposableUiBindingsUnsubscribeAndDoNotOwnTheProfile()
        {
            using (var flow = Open(Seed(500)))
            {
                int rendered = 0;
                var binding = new ProgressionMenuBinding(flow, _ => rendered++);
                Assert.That(rendered, Is.EqualTo(1)); binding.Dispose(); binding.Dispose();
                flow.BuyUpgrade(UpgradeKind.MaximumHealth);
                Assert.That(rendered, Is.EqualTo(1)); Assert.That(flow.View.Credits, Is.EqualTo(300));
                using (var next = new ProgressionMenuBinding(flow, view => { rendered++; Assert.That(view.Credits, Is.EqualTo(300)); })) { }
            }
        }

        [Test]
        public void FailedRegistrationNeverExposesARunAndReloadAllowsFreshRegistration()
        {
            var store = new ProbeStore { Inner = Seed(), FailWrite = true };
            using (var flow = Open(store))
            {
                Assert.That(flow.StartRun(RunConfiguration.Prototype()), Is.EqualTo(ProgressionResult.PersistenceFailed));
                Assert.That(flow.Run, Is.Null); Assert.That(flow.View.CanStart, Is.False);
                store.FailWrite = false; flow.RetryLoad();
                Assert.That(flow.StartRun(RunConfiguration.Prototype()), Is.EqualTo(ProgressionResult.Succeeded));
            }
        }

        [Test]
        public void FailedAbandonPausesTheRunUntilTheSameCloseTransactionCanBeRetried()
        {
            var store = new ProbeStore { Inner = Seed() };
            using (var flow = Open(store))
            {
                flow.StartRun(RunConfiguration.Prototype()); store.FailWrite = true;
                flow.ReturnToMenu(); double elapsed = flow.Run.ElapsedSeconds;
                flow.Advance(100); Assert.That(flow.Run.ElapsedSeconds, Is.EqualTo(elapsed));
                store.FailWrite = false; flow.RetryLoad(); flow.RetrySave();
                Assert.That(flow.Run, Is.Null); Assert.That(flow.View.Credits, Is.Zero);
            }
        }

        [Test]
        public void ReceiptDistinguishesAnAbandonedRunFromAPaidVictory()
        {
            var store = Seed(); var service = new ProgressionService(Config, store);
            service.CreateRun(RunConfiguration.Prototype(), out ProgressionRunSession session);
            using (session)
            {
                session.Abandon(); ProgressionTests.Win(session.Run);
                Assert.That(session.LastCompletionResult, Is.EqualTo(ProgressionResult.AlreadyProcessed));
                RunReceipt receipt = service.GetRunReceipt(session.Run.RunId);
                Assert.That(receipt.Outcome, Is.EqualTo(SavedRunOutcome.Abandoned));
                Assert.That(receipt.AwardedCredits, Is.Zero);
            }
        }

        [TestCase(typeof(InvalidDataContractException))]
        [TestCase(typeof(NotSupportedException))]
        public void UnsupportedSaveReaderProducesRecoveryWithoutOverwritingData(Type type)
        {
            var store = Seed(500); string before = store.Read();
            using (var flow = new ProgressionController(Config, store, new UnsupportedReader((Exception)Activator.CreateInstance(type))))
            {
                Assert.That(flow.Initialize(), Is.EqualTo(ProgressionResult.PersistenceFailed));
                Assert.That(flow.Screen, Is.EqualTo(ProgressionScreen.Recovery)); Assert.That(flow.View.CanStart, Is.False);
                Assert.That(store.Read(), Is.EqualTo(before));
            }
        }

        [Test]
        public void ClosedControllerRejectsCommandsAndExposesAClosedDisabledView()
        {
            var flow = Open(Seed(500)); flow.Dispose(); flow.Dispose();
            Assert.That(flow.View.Screen, Is.EqualTo(ProgressionScreen.Closed));
            Assert.That(flow.View.CanStart, Is.False);
            Assert.That(flow.Initialize(), Is.EqualTo(ProgressionResult.NotInitialized));
            Assert.That(flow.BuyUpgrade(UpgradeKind.MaximumHealth), Is.Not.EqualTo(ProgressionResult.Succeeded));
        }

        private sealed class UnsupportedReader : IProgressionCodec
        {
            private readonly Exception error;
            public UnsupportedReader(Exception error) => this.error = error;
            public ProgressionData Decode(string json, ProgressionConfiguration config) => throw error;
            public string Encode(ProgressionData data) => Codec.Encode(data);
        }

        private sealed class ProbeStore : IProgressionStore
        {
            public MemoryProgressionStore Inner;
            public int Reads, Commits;
            public bool FailWrite, FailRead, LoseAcknowledgment;
            public Action OnRead, OnCommit;
            public string Read() { Reads++; OnRead?.Invoke(); if (FailRead) throw new IOException("read failure"); return Inner.Read(); }
            public StoreCommitResult Commit(string expected, string replacement)
            {
                Commits++; OnCommit?.Invoke();
                if (FailWrite) throw new IOException("write failure");
                StoreCommitResult result = Inner.Commit(expected, replacement);
                if (LoseAcknowledgment) { FailRead = true; throw new IOException("lost acknowledgment"); }
                return result;
            }
        }
    }
}
