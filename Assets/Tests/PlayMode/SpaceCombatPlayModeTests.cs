using System.Collections;
using System.Linq;
using LastOutpost.Core.Combat;
using LastOutpost.Core.Modules;
using LastOutpost.Core.Progression;
using LastOutpost.Core.Simulation;
using LastOutpost.Gameplay;
using LastOutpost.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LastOutpost.Tests.PlayMode
{
    public sealed class SpaceCombatPlayModeTests
    {
        private GameObject root, ship;
        private PrototypeSettings settings;
        private SpaceShipSettings geometry;
        private SpaceCombatSettings combat;
        private ProgressionController controller;
        private OutpostSession session;
        private PlatformMotor motor;
        private ShipAssemblyView modules;
        private SpaceCombatView projectiles;
        private Texture2D texture;
        private Sprite sprite;
        private Material material;

        [SetUp]
        public void Setup()
        {
            root = new GameObject("Owned space combat fixture"); ship = new GameObject("Hull"); ship.transform.SetParent(root.transform, false);
            settings = ScriptableObject.CreateInstance<PrototypeSettings>(); geometry = ScriptableObject.CreateInstance<SpaceShipSettings>();
            combat = ScriptableObject.CreateInstance<SpaceCombatSettings>();
            controller = new ProgressionController(ProgressionConfiguration.Prototype(), new MemoryProgressionStore()); controller.Initialize();
            session = root.AddComponent<OutpostSession>(); session.Configure(settings, controller, false); session.ConfigureSpaceCombat(combat, geometry);
            motor = root.AddComponent<PlatformMotor>(); motor.Configure(session, ship.transform, Vector3.zero, RouteOrientation.SpaceForwardY);
            texture = new Texture2D(8, 8); texture.Apply(); sprite = Sprite.Create(texture, new Rect(0, 0, 8, 8), Vector2.one * 0.5f, 8);
            material = new Material(Shader.Find("Sprites/Default"));
            modules = ship.AddComponent<ShipAssemblyView>(); modules.Configure(session, geometry, sprite, material);
            var cameraObject = new GameObject("Owned combat camera"); cameraObject.transform.SetParent(root.transform, false);
            var camera = cameraObject.AddComponent<Camera>(); camera.orthographic = true; camera.aspect = 9f / 16;
            camera.transform.position = new Vector3(0, 0, -10);
            cameraObject.AddComponent<ShipCameraFraming>().Configure(geometry); cameraObject.AddComponent<CameraFollow>().Configure(ship.transform, 0, RouteOrientation.SpaceForwardY);
            var effects = new GameObject("Owned projectile views"); effects.transform.SetParent(root.transform, false);
            projectiles = effects.AddComponent<SpaceCombatView>(); projectiles.Configure(session, camera, sprite, material);
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            Object.Destroy(root); Object.Destroy(settings); Object.Destroy(geometry); Object.Destroy(combat);
            Object.Destroy(material); Object.Destroy(sprite); Object.Destroy(texture); controller.Dispose(); yield return null;
        }

        [UnityTest]
        public IEnumerator EquippedWeaponsUseActualMountsAndAdvanceAutomatically()
        {
            Assert.That(session.BeginRun(), Is.True); var run = session.Run;
            Assert.That(run.Space.Weapons.Count, Is.EqualTo(3));
            foreach (var weapon in run.Space.Weapons.Where(w => w.SlotId != "core"))
                Assert.That((float)weapon.LocalPosition.X, Is.EqualTo(modules.GetMount(weapon.SlotId).localPosition.x).Within(1e-5));
            session.StepSimulation(20); motor.Synchronize(); yield return null;
            Assert.That(ship.transform.position.x, Is.Zero); Assert.That(ship.transform.position.y, Is.GreaterThan(0));
            Assert.That(run.Space.Weapons.All(w => w.ShotsFired > 0), Is.True);
            Assert.That(run.Space.Weapons.Select(w => w.Definition.Cooldown).Distinct().Count(), Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator PoolViewsMatchModelAndContainNoDamageColliders()
        {
            session.BeginRun(); session.StepSimulation(5); motor.Synchronize(); projectiles.Synchronize(); yield return null;
            Assert.That(projectiles.VisibleProjectileCount, Is.EqualTo(session.Run.Space.Projectiles.Count(p => p.Active)));
            Assert.That(projectiles.GetComponentsInChildren<SpriteRenderer>().Length, Is.EqualTo(combat.maximumProjectiles + 3));
            Assert.That(root.GetComponentsInChildren<Component>(true).Where(c => c != null && c.GetType().Name.Contains("Collider")), Is.Empty);
            int capacity = projectiles.transform.childCount;
            session.RestartRun(); session.StepSimulation(5); projectiles.Synchronize();
            Assert.That(projectiles.transform.childCount, Is.EqualTo(capacity));
        }

        [UnityTest]
        public IEnumerator UnequippingMissilesRemovesOneIndependentEmitterAndModule()
        {
            Assert.That(controller.ChangeLoadout(LoadoutOperation.Unequip, "secondary"), Is.EqualTo(LoadoutResult.Succeeded));
            Assert.That(session.BeginRun(), Is.True); yield return null;
            Assert.That(session.Run.Space.Weapons.Count, Is.EqualTo(2)); Assert.That(modules.IsInstalled("secondary"), Is.False);
            Assert.That(session.Run.Space.Weapons.Any(w => w.Definition.Id == "missile"), Is.False);
            Assert.That(controller.ChangeLoadout(LoadoutOperation.Equip, "secondary", "missile-launcher"), Is.EqualTo(LoadoutResult.RunActive));
        }

        [UnityTest]
        public IEnumerator SideMountRelocationChangesWorldFacingWithoutChangingOwnership()
        {
            geometry.mounts[0].side = ShipSide.Right; geometry.mounts[1].side = ShipSide.Left;
            modules.Configure(session, geometry, sprite, material);
            session.ConfigureSpaceCombat(combat, geometry); session.BeginRun(); yield return null;
            var cannon = session.Run.Space.Weapons.Single(w => w.Definition.Id == "cannon");
            var missile = session.Run.Space.Weapons.Single(w => w.Definition.Id == "missile");
            Assert.That(cannon.LocalPosition.X, Is.GreaterThan(0)); Assert.That(cannon.LocalFacing.X, Is.EqualTo(1));
            Assert.That((float)cannon.LocalPosition.X, Is.EqualTo(modules.GetMount("primary").localPosition.x).Within(1e-5));
            Assert.That(missile.LocalPosition.X, Is.LessThan(0)); Assert.That(missile.LocalFacing.X, Is.EqualTo(-1));
            Assert.That(controller.View.Loadout.Slots.Single(s => s.SlotId == "primary").EquipmentId, Is.EqualTo("basic-cannon"));
        }

        [UnityTest]
        public IEnumerator EnemyViewsUseModelCoordinatesAndHideQueuedArrivals()
        {
            combat.maximumEnemies = 1; session.ConfigureSpaceCombat(combat, geometry); session.BeginRun(); session.StepSimulation(2);
            var enemy = session.Run.Space.Enemies.First(e => e.Active); var pending = session.Run.Space.Enemies.First(e => !e.Active);
            var actorObject = new GameObject("Owned enemy view"); actorObject.transform.SetParent(root.transform, false);
            var actor = actorObject.AddComponent<EnemyActor>(); actor.Bind(enemy.Agent, Vector3.zero, RouteOrientation.SpaceForwardY, enemy);
            Assert.That(actor.transform.position.x, Is.EqualTo((float)enemy.Position.X));
            Assert.That(actor.transform.position.y, Is.EqualTo((float)enemy.Position.Y));
            actor.Bind(pending.Agent, Vector3.zero, RouteOrientation.SpaceForwardY, pending);
            Assert.That(actorObject.activeSelf, Is.False); yield return null;
        }

        [UnityTest]
        public IEnumerator DisablingAWeaponDoesNotResetOrPauseOtherCooldowns()
        {
            session.BeginRun(); session.StepSimulation(20); var run = session.Run;
            var cannon = run.Space.Weapons.Single(w => w.Definition.Id == "cannon"); int shots = cannon.ShotsFired;
            cannon.Enabled = false; session.StepSimulation(2); yield return null;
            Assert.That(cannon.ShotsFired, Is.EqualTo(shots));
            Assert.That(run.Space.Weapons.Single(w => w.Definition.Id == "laser").ShotsFired, Is.GreaterThan(0));
            Assert.That(cannon.Remaining(run.ElapsedSeconds), Is.Zero);
            cannon.Enabled = true;
        }

        [UnityTest]
        public IEnumerator IdenticalBatteryEmittersHaveIndependentRuntimeTimersAndViews()
        {
            combat.equipmentWeapons.Single(e => e.equipmentId == "basic-cannon").weapon.instances = 2;
            session.ConfigureSpaceCombat(combat, geometry); Assert.That(session.BeginRun(), Is.True);
            var cannons = session.Run.Space.Weapons.Where(w => w.Definition.Id == "cannon").ToArray();
            Assert.That(cannons.Length, Is.EqualTo(2)); Assert.That(cannons[0].Definition, Is.SameAs(cannons[1].Definition));
            cannons[0].Enabled = false; session.StepSimulation(15); motor.Synchronize(); projectiles.Synchronize(); yield return null;
            Assert.That(cannons[0].ShotsFired, Is.Zero); Assert.That(cannons[1].ShotsFired, Is.GreaterThan(0));
            Assert.That(projectiles.GetComponentsInChildren<SpriteRenderer>().Length, Is.EqualTo(combat.maximumProjectiles + 4));
        }
    }
}
