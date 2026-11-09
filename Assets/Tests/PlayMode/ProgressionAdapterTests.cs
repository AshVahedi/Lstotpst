using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LastOutpost.Core.Progression;
using LastOutpost.Core.Simulation;
using LastOutpost.Gameplay;
using LastOutpost.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace LastOutpost.Tests.PlayMode
{
    [NonParallelizable]
    public sealed class ProgressionAdapterTests
    {
        private readonly List<GameObject> owned = new List<GameObject>();
        private PrototypeSettings settings;
        private Scene extraScene;

        [SetUp]
        public void SetUp()
        {
            Assert.That(UnityEngine.Object.FindObjectsByType<ProgressionHost>(FindObjectsInactive.Include,
                FindObjectsSortMode.None), Is.Empty, "Run in an isolated Test Runner scene; do not touch an existing player's profile.");
            settings = ScriptableObject.CreateInstance<PrototypeSettings>();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (GameObject item in owned) if (item != null) UnityEngine.Object.Destroy(item);
            owned.Clear();
            if (settings != null) UnityEngine.Object.Destroy(settings);
            yield return null;
            if (extraScene.IsValid() && extraScene.isLoaded) yield return SceneManager.UnloadSceneAsync(extraScene);
        }

        private ProgressionHost Host(IProgressionStore store)
        {
            var root = new GameObject("Owned progression host"); owned.Add(root); root.SetActive(false);
            var host = root.AddComponent<ProgressionHost>(); host.ConfigureStore(store); root.SetActive(true);
            return host;
        }

        private OutpostSession Session(ProgressionHost host)
        {
            var root = new GameObject("Owned session"); owned.Add(root);
            var session = root.AddComponent<OutpostSession>(); session.Configure(settings, host, false); return session;
        }

        private static MemoryProgressionStore Seed() => new MemoryProgressionStore(new JsonProgressionCodec().Encode(new ProgressionData { Credits = 500 }));

        [Test]
        public void HostLoadsBeforeGameplayAndHudBindsTheAuthoritativeMenu()
        {
            var host = Host(Seed()); var session = Session(host);
            var hud = session.gameObject.AddComponent<PrototypeHud>(); hud.Configure(session);
            Assert.That(session.Run, Is.Null); Assert.That(hud.ProgressionView.Credits, Is.EqualTo(500));
            Assert.That(hud.BuyPermanent(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.Succeeded));
            Assert.That(hud.ProgressionView.Credits, Is.EqualTo(300));
            Assert.That(hud.StartRun(), Is.True);
            Assert.That(session.Run, Is.SameAs(host.Controller.Run));
            Assert.That(session.Run.Platform.Health.Maximum, Is.EqualTo(110));
            Assert.That(session.Run.Platform.AdvancedAmmunition.Capacity, Is.EqualTo(100));
        }

        [UnityTest]
        public IEnumerator SceneReplacementAdoptsOnePersistentProfileAndRunAndOneSimulationDriver()
        {
            var host = Host(Seed()); var original = Session(host); original.BeginRun();
            GameRun run = original.Run;
            int oldFinishes = 0, newFinishes = 0;
            original.Finished += _ => oldFinishes++;
            extraScene = SceneManager.CreateScene("Progression fixture " + Guid.NewGuid().ToString("N"));
            var otherStore = new CountingStore();
            var duplicateRoot = new GameObject("Owned duplicate host"); owned.Add(duplicateRoot); duplicateRoot.SetActive(false);
            SceneManager.MoveGameObjectToScene(duplicateRoot, extraScene);
            var duplicate = duplicateRoot.AddComponent<ProgressionHost>(); duplicate.ConfigureStore(otherStore); duplicateRoot.SetActive(true);
            var next = Session(duplicate); SceneManager.MoveGameObjectToScene(next.gameObject, extraScene);
            next.Finished += _ => newFinishes++;
            Assert.That(next.Progression, Is.SameAs(host.Controller)); Assert.That(next.Run, Is.SameAs(run));
            Assert.That(otherStore.Reads, Is.Zero);
            original.StepSimulation(0.1f); Assert.That(run.ElapsedSeconds, Is.Zero);
            next.StepSimulation(0.1f); Assert.That(run.ElapsedSeconds, Is.EqualTo(0.1).Within(0.0001));
            yield return null;
            Assert.That(duplicate == null, Is.True);
            Assert.That(UnityEngine.Object.FindObjectsByType<ProgressionHost>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            run.Platform.Health.TakeDamage(100); next.StepSimulation(0.1f);
            Assert.That(oldFinishes, Is.Zero); Assert.That(newFinishes, Is.EqualTo(1));
        }

        [Test]
        public void HudDisableAndRepeatedEnableOnlyRebindViewsWithoutReloadOrExtraPurchases()
        {
            var host = Host(Seed()); var session = Session(host);
            var hud = session.gameObject.AddComponent<PrototypeHud>(); hud.Configure(session);
            hud.enabled = false; Assert.That(hud.ProgressionView, Is.Null);
            host.Controller.BuyUpgrade(UpgradeKind.MaximumHealth);
            Assert.That(hud.ProgressionView, Is.Null);
            hud.enabled = true; hud.enabled = false; hud.enabled = true;
            Assert.That(hud.ProgressionView.Credits, Is.EqualTo(300));
            Assert.That(hud.ProgressionView.Upgrades[0].Level, Is.EqualTo(1));
        }

        [Test]
        public void VictoryEventObservesSavedCreditsAndResultsRebindingDoesNotPayAgain()
        {
            var store = Seed(); var host = Host(store); var session = Session(host);
            var hud = session.gameObject.AddComponent<PrototypeHud>(); hud.Configure(session); hud.StartRun();
            int finishes = 0;
            session.Finished += _ => { finishes++; Assert.That(hud.ProgressionView.CommittedReward, Is.EqualTo(500)); };
            Win(session);
            Assert.That(hud.ProgressionView.Credits, Is.EqualTo(1000));
            string saved = store.Read(); hud.enabled = false; hud.enabled = true;
            Assert.That(hud.ProgressionView.CommittedReward, Is.EqualTo(500)); Assert.That(store.Read(), Is.EqualTo(saved));
            Assert.That(finishes, Is.EqualTo(1)); Assert.That(hud.ReturnToMenu(), Is.True);
            Assert.That(session.Run, Is.Null); Assert.That(hud.ProgressionView.CanStart, Is.True);
        }

        [Test]
        public void CorruptProfileBlocksStartAndRecoveryRequiresConfirmation()
        {
            var store = new MemoryProgressionStore("{bad"); var host = Host(store); var session = Session(host);
            var hud = session.gameObject.AddComponent<PrototypeHud>(); hud.Configure(session);
            Assert.That(hud.StartRun(), Is.False); Assert.That(session.Run, Is.Null);
            Assert.That(hud.ProgressionView.Screen, Is.EqualTo(ProgressionScreen.Recovery));
            host.Controller.RequestNewProfile(); host.Controller.ConfirmNewProfile(false);
            Assert.That(store.Read(), Is.EqualTo("{bad"));
            host.Controller.ConfirmNewProfile(true);
            Assert.That(hud.ProgressionView.Credits, Is.Zero); Assert.That(store.Backups[0], Is.EqualTo("{bad"));
        }

        [Test]
        public void PersistentPathUsesTheExistingSchemaFileNameAndRejectsEmptyDirectories()
        {
            var store = ProgressionHost.CreateFileStore(Path.Combine(Path.GetTempPath(), "outpost-path-fixture"));
            Assert.That(store.SavePath, Is.EqualTo(Path.GetFullPath(Path.Combine(Path.GetTempPath(), "outpost-path-fixture", "progression-v1.json"))));
            Assert.Throws<ArgumentException>(() => ProgressionHost.CreateFileStore(""));
        }

        private static void Win(OutpostSession session)
        {
            session.StepSimulation((float)(session.Run.Configuration.Route.InitialTravelSeconds + 0.01));
            for (int encounter = 0; encounter < 4; encounter++)
            {
                foreach (EnemyAgent enemy in session.Run.Enemies) enemy.State.Health.TakeDamage(10000);
                session.StepSimulation((float)(session.Run.Configuration.Route.BetweenEncounterSeconds + 0.01));
            }
        }

        private sealed class CountingStore : IProgressionStore
        {
            public int Reads;
            public string Read() { Reads++; return null; }
            public StoreCommitResult Commit(string expected, string replacement) => throw new InvalidOperationException("Duplicate owner must not write.");
        }
    }
}
