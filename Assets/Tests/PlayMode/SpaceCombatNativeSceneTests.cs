using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LastOutpost.Core.Economy;
using LastOutpost.Core.Modules;
using LastOutpost.Core.Progression;
using LastOutpost.Core.Simulation;
using LastOutpost.Gameplay;
using LastOutpost.Presentation;
using LastOutpost.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace LastOutpost.Tests.PlayMode
{
    /// <summary>Loads committed scenes with a pre-existing injected owner: never opens the real player save.</summary>
    public sealed class SpaceCombatNativeSceneTests
    {
        private readonly List<Scene> loaded = new List<Scene>();
        private GameObject ownerObject;
        private ProgressionHost host;
        private string directory, file, token;
        private static T Field<T>(object owner, string name) => (T)owner.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(owner);

        [SetUp]
        public void Setup()
        {
            Assert.That(Object.FindObjectsByType<ProgressionHost>(FindObjectsInactive.Include), Is.Empty,
                "Do not replace an existing owner or use its profile as a test fixture.");
            token = Guid.NewGuid().ToString("N");
            directory = Path.Combine(Application.temporaryCachePath, "LastOutpostSpaceCombatTests", token);
            Assert.That(Directory.Exists(directory), Is.False); Directory.CreateDirectory(directory);
            file = Path.Combine(directory, ProgressionHost.SaveFileName);
            File.WriteAllText(file, new JsonProgressionCodec().Encode(new ProgressionData { Credits = 700 }));
            CreateHost();
        }
        private void CreateHost()
        {
            ownerObject = new GameObject("Owned isolated combat progression"); ownerObject.SetActive(false);
            host = ownerObject.AddComponent<ProgressionHost>(); host.ConfigureStore(ProgressionHost.CreateFileStore(directory)); ownerObject.SetActive(true);
        }
        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            foreach (var scene in loaded) if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            loaded.Clear(); if (ownerObject != null) Object.Destroy(ownerObject); yield return null;
            if (directory != null && Directory.Exists(directory))
            {
                string parent = Path.GetFullPath(Path.Combine(Application.temporaryCachePath, "LastOutpostSpaceCombatTests")) + Path.DirectorySeparatorChar;
                Assert.That(Path.GetFullPath(directory).StartsWith(parent, StringComparison.OrdinalIgnoreCase) && Path.GetFileName(directory) == token, Is.True);
                Directory.Delete(directory, true); // Only the checked unique directory created by this fixture.
            }
        }
        private static void Capture(Camera camera, string name)
        {
            string output = Environment.GetEnvironmentVariable("OUTPOST_SPACE_TASK2_CAPTURE");
            if (string.IsNullOrEmpty(output) || SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
            Directory.CreateDirectory(output);
            var target = RenderTexture.GetTemporary(540, 960, 24); var previous = camera.targetTexture; var active = RenderTexture.active;
            Texture2D texture = null;
            try
            {
                camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                texture = new Texture2D(540, 960, TextureFormat.RGB24, false); texture.ReadPixels(new Rect(0, 0, 540, 960), 0, 0); texture.Apply();
                File.WriteAllBytes(Path.Combine(output, name + ".png"), texture.EncodeToPNG());
            }
            finally { camera.targetTexture = previous; RenderTexture.active = active; if (texture != null) Object.Destroy(texture); RenderTexture.ReleaseTemporary(target); }
        }

        [UnityTest]
        public IEnumerator GeneratedEncounterWinsLosesAndReloadsInLegacyWithIsolatedFilePersistence()
        {
            const string spacePath = "Assets/SpacePrototypeGenerated/Scenes/SpaceFoundation.unity";
            yield return SceneManager.LoadSceneAsync(spacePath, LoadSceneMode.Additive);
            Scene scene = SceneManager.GetSceneByPath(spacePath); loaded.Add(scene);
            GameObject root = scene.GetRootGameObjects().Single(g => g.GetComponent<GeneratedLevelRoot>() != null);
            var session = root.GetComponent<OutpostSession>(); var hud = root.GetComponent<PrototypeHud>();
            session.Configure(Field<PrototypeSettings>(session, "settings"), host, false);
            Assert.That(session.UsesSpaceCombat, Is.True); Assert.That(session.Progression, Is.SameAs(host.Controller));
            var camera = root.GetComponentInChildren<Camera>(); camera.aspect = 9f / 16; camera.GetComponent<ShipCameraFraming>().Apply();
            var ship = root.GetComponentInChildren<ShipAssemblyView>(); var motor = root.GetComponent<PlatformMotor>();
            var view = root.GetComponentInChildren<SpaceCombatView>();
            Assert.That(hud.StartRun(), Is.True); GameRun run = session.Run;
            var directions = new HashSet<LastOutpost.Core.Combat.ArrivalSide>(); var shots = new HashSet<string>(); int hostileShots = 0;
            run.Space.EnemyArrived += enemy => directions.Add(enemy.Side);
            run.Space.ProjectileLaunched += projectile => {
                if (projectile.Hostile) { hostileShots++; return; } shots.Add(projectile.WeaponId);
                var weapon = run.Space.Weapons.Single(w => w.Definition.Id == projectile.WeaponId);
                if (weapon.SlotId != "core") Assert.That(projectile.Origin.X, Is.EqualTo(ship.GetMount(weapon.SlotId).localPosition.x).Within(1e-5));
            };
            bool captured = false, sideCaptured = false, bossCaptured = false, bossFireCaptured = false;
            for (int i = 0; i < 2400 && run.Status == RunStatus.Playing; i++)
            {
                session.StepSimulation(0.1f); motor.Synchronize(); view.Synchronize();
                if (i % 10 == 0) yield return null;
                if (!captured && run.ElapsedSeconds > 5) { Capture(camera, "space-task2-combat-world"); captured = true; }
                if (!sideCaptured && !run.Level.BossActive && run.Space.Enemies.Any(e => e.Active && e.Targetable && !e.Agent.State.Health.IsDead &&
                    (e.Side == LastOutpost.Core.Combat.ArrivalSide.Left || e.Side == LastOutpost.Core.Combat.ArrivalSide.Right)))
                { yield return null; Capture(camera, "space-task2-side-arrival-world"); sideCaptured = true; }
                if (!bossCaptured && run.Level.BossActive && run.Space.Enemies.Any(e => e.Active && e.Targetable && e.Position.Y - run.Space.ShipPosition.Y < 6.8))
                { yield return null; Capture(camera, "space-task2-boss-world"); bossCaptured = true; }
                if (!bossFireCaptured && run.Space.Projectiles.Any(p => p.Active && p.Hostile))
                { yield return null; Capture(camera, "space-task2-boss-fire-world"); bossFireCaptured = true; }
            }
            Assert.That(run.Status, Is.EqualTo(RunStatus.Victory)); Assert.That(run.Level.CurrentWave, Is.EqualTo(4));
            Assert.That(directions.Count, Is.EqualTo(4)); Assert.That(shots.Count, Is.EqualTo(3));
            Assert.That(run.Space.FriendlyHits, Is.GreaterThan(0)); Assert.That(run.Space.SpawnCount, Is.EqualTo(14));
            Assert.That(hostileShots, Is.GreaterThan(0)); Assert.That(sideCaptured && bossCaptured && bossFireCaptured, Is.True);
            Assert.That(host.Controller.View.Credits, Is.EqualTo(1200)); Assert.That(run.Platform.Resources.GetBalance(ResourceType.Energy), Is.Zero);
            Assert.That(run.Platform.AdvancedAmmunition.Count, Is.Zero); Assert.That(run.Space.Projectiles.All(p => !p.Active), Is.True);
            session.StepSimulation(1); Assert.That(host.Controller.View.Credits, Is.EqualTo(1200));
            Assert.That(hud.ReturnToMenu(), Is.True);
            Assert.That(hud.BuyPermanent(UpgradeKind.MaximumHealth), Is.EqualTo(ProgressionResult.Succeeded));
            Assert.That(hud.ChangeEquipment(LoadoutOperation.Unequip, "secondary"), Is.EqualTo(LoadoutResult.Succeeded));
            Assert.That(ship.InstalledModuleCount, Is.EqualTo(3));
            Assert.That(hud.StartRun(), Is.True); GameRun defeat = session.Run;
            Assert.That(defeat.Space.Weapons.Count, Is.EqualTo(2)); Assert.That(defeat.Platform.Health.Maximum, Is.EqualTo(110));
            foreach (var weapon in defeat.Space.Weapons) weapon.Enabled = false;
            for (int i = 0; i < 4000 && defeat.Status == RunStatus.Playing; i++) { session.StepSimulation(0.1f); if (i % 50 == 0) yield return null; }
            Assert.That(defeat.Status, Is.EqualTo(RunStatus.Defeat)); Assert.That(host.Controller.View.Credits, Is.EqualTo(1000));
            Assert.That(defeat.Space.ContactCount + defeat.Space.HostileHits, Is.GreaterThan(0));
            Assert.That(hud.ReturnToMenu(), Is.True); byte[] saved = File.ReadAllBytes(file);
            yield return SceneManager.UnloadSceneAsync(scene); Object.Destroy(ownerObject); yield return null;
            CreateHost();
            const string legacyPath = "Assets/LastOutpostGenerated/Scenes/IndustrialWasteland.unity";
            yield return SceneManager.LoadSceneAsync(legacyPath, LoadSceneMode.Additive);
            Scene legacy = SceneManager.GetSceneByPath(legacyPath); loaded.Add(legacy);
            root = legacy.GetRootGameObjects().Single(g => g.GetComponent<GeneratedLevelRoot>() != null);
            session = root.GetComponent<OutpostSession>(); session.Configure(Field<PrototypeSettings>(session, "settings"), host, false);
            Assert.That(File.ReadAllBytes(file), Is.EqualTo(saved), "Scene/profile loading must not rewrite the file.");
            Assert.That(host.Controller.View.Credits, Is.EqualTo(1000)); Assert.That(session.BeginRun(), Is.True);
            Assert.That(session.Run.Space, Is.Null); Assert.That(session.Run.Platform.Health.Maximum, Is.EqualTo(110));
            Assert.That(session.Run.Configuration.Equipment.HasAdvancedWeapon, Is.False);
            session.StepSimulation(1); root.GetComponent<PlatformMotor>().Synchronize();
            Transform vehicle = Field<Transform>(root.GetComponent<PlatformMotor>(), "vehicle");
            Assert.That(vehicle.position.x, Is.GreaterThan(0)); Assert.That(vehicle.position.y, Is.Zero);
            host.Controller.ReturnToMenu();
            var data = new JsonProgressionCodec().Decode(File.ReadAllText(file), ProgressionConfiguration.Prototype());
            Assert.That(data.SchemaVersion, Is.EqualTo(2)); Assert.That(data.Equipment.OwnedEquipmentIds.Count, Is.EqualTo(4));
            Debug.Log("SPACE_TASK2_NATIVE: waves=4 arrivals=4 weapon-kinds=3 boss-shots=" + hostileShots +
                " victory=500 defeat=0 credits=1000 health=110 legacy=+X schema=2");
        }
    }
}
