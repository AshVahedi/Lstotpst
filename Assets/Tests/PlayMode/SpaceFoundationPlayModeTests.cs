using System.Collections;
using System.Linq;
using LastOutpost.Core.Modules;
using LastOutpost.Core.Progression;
using LastOutpost.Gameplay;
using LastOutpost.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LastOutpost.Tests.PlayMode
{
    public sealed class SpaceFoundationPlayModeTests
    {
        private GameObject root, ship;
        private OutpostSession session;
        private PrototypeSettings tuning;
        private SpaceShipSettings geometry;
        private ProgressionController controller;
        private MemoryProgressionStore store;
        private ShipAssemblyView view;
        private PlatformMotor motor;
        private Texture2D texture;
        private Sprite sprite;
        private Material material;

        [SetUp]
        public void Setup()
        {
            root = new GameObject("Owned space fixture"); ship = new GameObject("Ship Core"); ship.transform.SetParent(root.transform, false);
            tuning = ScriptableObject.CreateInstance<PrototypeSettings>(); tuning.initialTravelSeconds = 10;
            geometry = ScriptableObject.CreateInstance<SpaceShipSettings>();
            store = new MemoryProgressionStore(new JsonProgressionCodec().Encode(new ProgressionData { Credits = 700 }));
            controller = new ProgressionController(ProgressionConfiguration.Prototype(), store); controller.Initialize();
            session = root.AddComponent<OutpostSession>(); session.Configure(tuning, controller, false);
            motor = root.AddComponent<PlatformMotor>(); motor.Configure(session, ship.transform, Vector3.zero, RouteOrientation.SpaceForwardY);
            texture = new Texture2D(8, 8); texture.Apply(); sprite = Sprite.Create(texture, new Rect(0, 0, 8, 8), Vector2.one * 0.5f, 8);
            material = new Material(Shader.Find("Sprites/Default"));
            view = ship.AddComponent<ShipAssemblyView>(); view.Configure(session, geometry, sprite, material);
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            Object.Destroy(root); Object.Destroy(tuning); Object.Destroy(geometry); Object.Destroy(sprite); Object.Destroy(texture); Object.Destroy(material);
            controller.Dispose(); yield return null;
        }

        [UnityTest]
        public IEnumerator AutomaticForwardProgressionKeepsLateralPositionAndCameraCentered()
        {
            var cameraObject = new GameObject("Space camera"); cameraObject.transform.SetParent(root.transform, false);
            cameraObject.transform.position = new Vector3(0, 0, -10);
            Camera camera = cameraObject.AddComponent<Camera>(); camera.aspect = 9f / 16;
            cameraObject.AddComponent<CameraFollow>().Configure(ship.transform, 0, RouteOrientation.SpaceForwardY);
            cameraObject.AddComponent<ShipCameraFraming>().Configure(geometry);
            Assert.That(session.BeginRun(), Is.True); session.StepSimulation(1); motor.Synchronize();
            yield return null; yield return null;
            Assert.That(ship.transform.position.x, Is.Zero); Assert.That(ship.transform.position.y, Is.EqualTo(0.9f).Within(0.0001));
            Assert.That(camera.transform.position.x, Is.Zero); Assert.That(camera.transform.position.y, Is.EqualTo(ship.transform.position.y));
            Vector3 center = camera.WorldToViewportPoint(ship.transform.position);
            Assert.That(center.x, Is.EqualTo(0.5f).Within(0.0001)); Assert.That(center.y, Is.EqualTo(0.5f).Within(0.0001));
            foreach (var mount in geometry.Build().Points)
            {
                float x = (float)geometry.Build().Position(mount.SlotId).X;
                float edge = x + Mathf.Sign(x) * geometry.bayWidth / 2;
                float viewport = camera.WorldToViewportPoint(ship.transform.position + Vector3.right * edge).x;
                Assert.That(viewport, Is.InRange(0.1f, 0.9f));
            }
        }

        [Test]
        public void ExistingSlotInstallationsDriveSymmetricAndAsymmetricModulesWithoutNewInventory()
        {
            Assert.That(view.InstalledModuleCount, Is.EqualTo(4)); Assert.That(view.Bounds.Width, Is.EqualTo(5.2).Within(0.0001));
            Assert.That(view.GetMount("primary").localPosition.x, Is.EqualTo(-1.1).Within(0.0001));
            Assert.That(view.GetMount("utility").localPosition.x, Is.EqualTo(2.15).Within(0.0001));
            Assert.That(controller.ChangeLoadout(LoadoutOperation.Unequip, "utility"), Is.EqualTo(LoadoutResult.Succeeded));
            Assert.That(view.IsInstalled("utility"), Is.False); Assert.That(view.InstalledModuleCount, Is.EqualTo(3));
            Assert.That(view.ValidationError, Is.Null, "An empty optional bay is not a profile error.");
            Assert.That(view.Bounds.Left, Is.EqualTo(-2.6).Within(0.0001)); Assert.That(view.Bounds.Right, Is.EqualTo(1.55).Within(0.0001));
            Assert.That(controller.View.Credits, Is.EqualTo(700));
            Assert.That(session.BeginRun(), Is.True);
            Assert.That(controller.ChangeLoadout(LoadoutOperation.Unequip, "secondary"), Is.EqualTo(LoadoutResult.RunActive));
            Assert.That(view.InstalledModuleCount, Is.EqualTo(3));
        }

        [Test]
        public void EmptyInnerBayRetainsConnectedOuterModuleAndReloadPreservesSelection()
        {
            Assert.That(controller.ChangeLoadout(LoadoutOperation.Unequip, "secondary"), Is.EqualTo(LoadoutResult.Succeeded));
            Assert.That(view.IsInstalled("utility"), Is.True); Assert.That(view.IsInstalled("secondary"), Is.False);
            Assert.That(view.ValidationError, Is.Null);
            Assert.That(view.Bounds.Right, Is.EqualTo(2.6).Within(0.0001));
            string payload = store.Read(); controller.Dispose();
            controller = new ProgressionController(ProgressionConfiguration.Prototype(), new MemoryProgressionStore(payload)); controller.Initialize();
            session.Configure(tuning, controller, false);
            Assert.That(view.IsInstalled("secondary"), Is.False); Assert.That(view.IsInstalled("utility"), Is.True);
            Assert.That(controller.View.Credits, Is.EqualTo(700));
        }

        [UnityTest]
        public IEnumerator ReenableDoesNotDuplicateMountsOrWriteAProfile()
        {
            string before = store.Read();
            view.enabled = false; view.enabled = true; view.enabled = false; view.enabled = true;
            yield return null;
            Assert.That(ship.transform.Cast<Transform>().Count(t => t.name == "Horizontal attachment mounts" && t.gameObject.activeSelf), Is.EqualTo(1));
            Assert.That(view.InstalledModuleCount, Is.EqualTo(4)); Assert.That(store.Read(), Is.EqualTo(before));
        }

        [UnityTest]
        public IEnumerator EnemyAndResolvedShotViewsUseTheSamePositiveYAxis()
        {
            tuning.initialTravelSeconds = 0;
            EnemyActor Template(string name)
            { var g = new GameObject(name); g.transform.SetParent(root.transform, false); g.SetActive(false); return g.AddComponent<EnemyActor>(); }
            var spawner = root.AddComponent<EnemySpawner>();
            var actors = new GameObject("Actors"); actors.transform.SetParent(root.transform, false);
            spawner.Configure(session, Template("Basic"), Template("Fast"), Template("Boss"), actors.transform, Vector3.zero, RouteOrientation.SpaceForwardY);
            var effects = new GameObject("Effects"); effects.transform.SetParent(root.transform, false);
            root.AddComponent<CombatFeedback>().Configure(session, material, effects.transform, Vector3.zero, RouteOrientation.SpaceForwardY);
            Assert.That(session.BeginRun(), Is.True); session.StepSimulation(0.1f);
            yield return null;
            Assert.That(spawner.ActiveCount, Is.EqualTo(3));
            foreach (var enemy in session.Run.Enemies)
            { Assert.That(spawner.FindActor(enemy.State.Id).transform.position.x, Is.Zero); Assert.That(spawner.FindActor(enemy.State.Id).transform.position.y, Is.EqualTo((float)enemy.PositionX).Within(0.0001)); }
            LineRenderer line = effects.GetComponentInChildren<LineRenderer>(); Assert.That(line, Is.Not.Null);
            Assert.That(line.GetPosition(0).x, Is.Zero); Assert.That(line.GetPosition(1).x, Is.Zero);
            Assert.That(line.GetPosition(1).y, Is.GreaterThan(line.GetPosition(0).y));
        }

        [Test]
        public void LegacyDefaultProjectionAndCorruptSaveContractRemainIntact()
        {
            Assert.That(RouteProjection.Position(Vector3.one, 3, RouteOrientation.LegacyHorizontal), Is.EqualTo(new Vector3(4, 1, 1)));
            controller.Dispose(); store = new MemoryProgressionStore("{bad");
            controller = new ProgressionController(ProgressionConfiguration.Prototype(), store); controller.Initialize();
            session.Configure(tuning, controller, false);
            Assert.That(view.InstalledModuleCount, Is.Zero); Assert.That(view.ValidationError, Is.Not.Null);
            Assert.That(session.BeginRun(), Is.False); Assert.That(store.Read(), Is.EqualTo("{bad"));
        }

        [Test]
        public void UnknownInstalledGearIsRetainedBlockedAndNotRenderedAsDefaultEquipment()
        {
            var data = new ProgressionData { Credits = 700 };
            data.Equipment.Installations.Single(i => i.SlotId == "secondary").EquipmentId = "retired-space-item";
            string payload = new JsonProgressionCodec().Encode(data);
            controller.Dispose(); store = new MemoryProgressionStore(payload);
            controller = new ProgressionController(ProgressionConfiguration.Prototype(), store); controller.Initialize();
            session.Configure(tuning, controller, false);
            Assert.That(controller.View.Loadout.Validation, Is.EqualTo(LoadoutResult.UnknownEquipment));
            Assert.That(view.ValidationError, Is.Not.Null); Assert.That(view.InstalledModuleCount, Is.Zero);
            Assert.That(session.BeginRun(), Is.False); Assert.That(store.Read(), Is.EqualTo(payload));
        }

        [Test]
        public void UnboundRuntimeViewDoesNotSubstituteEditorPreviewEquipment()
        {
            var unbound = new GameObject("Uninitialized owner"); unbound.transform.SetParent(root.transform, false);
            view.Configure(unbound.AddComponent<OutpostSession>(), geometry, sprite, material);
            Assert.That(view.InstalledModuleCount, Is.Zero); Assert.That(view.ValidationError, Is.Not.Null);
            Assert.That(controller.View.Credits, Is.EqualTo(700));
        }
    }
}
