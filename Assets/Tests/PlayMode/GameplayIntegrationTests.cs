using System;
using System.Collections;
using System.Collections.Generic;
using LastOutpost.Core.Economy;
using LastOutpost.Core.Simulation;
using LastOutpost.Gameplay;
using LastOutpost.Presentation;
using LastOutpost.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace LastOutpost.Tests.PlayMode
{
    public sealed class GameplayIntegrationTests
    {
        private GameObject root;
        private GameObject vehicle;
        private PrototypeSettings settings;
        private OutpostSession session;
        private EnemySpawner spawner;
        private PlatformMotor motor;
        private PrototypeHud hud;
        private Material material;

        [SetUp]
        public void SetUp()
        {
            settings = ScriptableObject.CreateInstance<PrototypeSettings>();
            settings.travelSpeed = 1;
            settings.encounterSpeed = 0;
            settings.initialTravelSeconds = 1;
            settings.betweenEncounterSeconds = 0;
            root = new GameObject("Last Outpost test");
            session = root.AddComponent<OutpostSession>();
            session.Configure(settings, false);
            vehicle = Child("Vehicle");
            motor = root.AddComponent<PlatformMotor>();
            motor.Configure(session, vehicle.transform, Vector3.zero);
            EnemyActor basic = Template("Basic template");
            EnemyActor fast = Template("Fast template");
            EnemyActor boss = Template("Boss template");
            spawner = root.AddComponent<EnemySpawner>();
            spawner.Configure(session, basic, fast, boss, Child("Actors").transform, Vector3.zero);
            hud = root.AddComponent<PrototypeHud>();
            hud.Configure(session);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            UnityEngine.Object.Destroy(root);
            UnityEngine.Object.Destroy(settings);
            if (material != null) UnityEngine.Object.Destroy(material);
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartActionAdvancesPlatformTransformAndEncounterChangesMode()
        {
            Assert.That(hud.StartRun(), Is.True);
            session.StepSimulation(0.5f);
            yield return null;
            Assert.That(vehicle.transform.position.x, Is.EqualTo(0.5f).Within(0.0001f));
            session.StepSimulation(0.5f);
            Assert.That(motor.Mode, Is.EqualTo(MovementMode.Encounter));
            Assert.That(hud.StartRun(), Is.False);
        }

        [Test]
        public void SpawnedObjectsBindExactlyToTheCoreRoster()
        {
            StartEncounter();
            Assert.That(spawner.ActiveCount, Is.EqualTo(3));
            foreach (EnemyAgent agent in session.Run.Enemies)
            {
                EnemyActor actor = spawner.FindActor(agent.State.Id);
                Assert.That(actor.Agent, Is.SameAs(agent));
                Assert.That(actor.transform.position.x, Is.EqualTo((float)agent.PositionX).Within(0.0001));
            }
        }

        [UnityTest]
        public IEnumerator EnemyTransformsFollowApproachMovement()
        {
            StartEncounter();
            EnemyAgent enemy = session.Run.Enemies[0];
            float before = spawner.FindActor(enemy.State.Id).transform.position.x;
            session.StepSimulation(0.5f);
            yield return null;
            Assert.That(spawner.FindActor(enemy.State.Id).transform.position.x, Is.LessThan(before));
        }

        [Test]
        public void AutomaticShotsChooseClosestEnemyAndHonorInterval()
        {
            StartEncounter();
            var hits = new List<ShotEvent>();
            session.Run.ShotFired += hits.Add;
            session.StepSimulation(0.1f);
            Assert.That(hits.Count, Is.EqualTo(1));
            Assert.That(hits[0].TargetEnemyId, Is.EqualTo(session.Run.Enemies[0].State.Id));
            session.StepSimulation(0.5f);
            Assert.That(hits.Count, Is.EqualTo(1));
            session.StepSimulation(0.11f);
            Assert.That(hits.Count, Is.EqualTo(2));
            Assert.That(session.Run.Enemies[0].State.Health.Current, Is.EqualTo(16));
        }

        [UnityTest]
        public IEnumerator VisibleTracerNeverAppliesDamageAgain()
        {
            Shader shader = Shader.Find("Sprites/Default");
            Assert.That(shader, Is.Not.Null, "Generated prototype requires the built-in sprite shader.");
            material = new Material(shader);
            Transform effects = Child("Effects").transform;
            root.AddComponent<CombatFeedback>().Configure(session, material, effects, Vector3.zero);
            StartEncounter();
            session.StepSimulation(0.01f);
            Assert.That(effects.childCount, Is.EqualTo(1));
            Assert.That(effects.GetChild(0).GetComponent<ShotTrace>(), Is.Not.Null);
            int afterShot = session.Run.Enemies[0].State.Health.Current;
            Assert.That(afterShot, Is.EqualTo(24));
            yield return null;
            yield return null;
            Assert.That(session.Run.Enemies[0].State.Health.Current, Is.EqualTo(afterShot));
        }

        [Test]
        public void BoughtAmmunitionFiresMissileAndBasicFallbackWorksWhenEmpty()
        {
            StartEncounter();
            session.Run.Platform.Resources.Collect(ResourceType.Energy, 30);
            Assert.That(hud.BuyAmmunition(), Is.EqualTo(PurchaseResult.Succeeded));
            session.StepSimulation(0.01f);
            Assert.That(session.Run.Platform.AdvancedAmmunition.Count, Is.EqualTo(19));
            session.Run.Platform.AdvancedAmmunition.TryConsume(19);
            var kinds = new List<ShotKind>();
            session.Run.ShotFired += shot => kinds.Add(shot.Kind);
            session.StepSimulation(2);
            Assert.That(kinds, Does.Contain(ShotKind.BasicGun));
            Assert.That(kinds, Does.Not.Contain(ShotKind.Missile));
        }

        [UnityTest]
        public IEnumerator EnemyDestructionCreditsEnergyOnlyOnce()
        {
            StartEncounter();
            EnemyAgent enemy = session.Run.Enemies[0];
            EnemyActor actor = spawner.FindActor(enemy.State.Id);
            enemy.State.Health.TakeDamage(1000);
            session.StepSimulation(0.01f);
            Assert.That(spawner.ActiveCount, Is.EqualTo(2));
            Assert.That(actor.gameObject.activeSelf, Is.False);
            yield return null;
            Assert.That(actor == null, Is.True);
            session.StepSimulation(0.01f);
            Assert.That(session.Run.Platform.Resources.GetBalance(ResourceType.Energy), Is.EqualTo(12));
        }

        [Test]
        public void SpawnerCannotBeginNextWaveWhileOneRosterMemberLives()
        {
            StartEncounter();
            session.Run.Enemies[0].State.Health.TakeDamage(1000);
            session.Run.Enemies[1].State.Health.TakeDamage(1000);
            session.StepSimulation(0.01f);
            Assert.That(session.Run.Level.CurrentWave, Is.EqualTo(1));
            Assert.That(spawner.ActiveCount, Is.EqualTo(1));
            session.Run.Enemies[2].State.Health.TakeDamage(1000);
            session.StepSimulation(0.01f);
            Assert.That(session.Run.Level.CurrentWave, Is.EqualTo(2));
            Assert.That(spawner.ActiveCount, Is.EqualTo(4));
        }

        [UnityTest]
        public IEnumerator BossSpawnsAfterThreeWavesAndPlatformTransformStops()
        {
            StartEncounter();
            AdvanceToBoss();
            Assert.That(session.Run.Level.BossActive, Is.True);
            Assert.That(spawner.ActiveCount, Is.EqualTo(1));
            Assert.That(spawner.FindActor(session.Run.Enemies[0].State.Id).name, Does.StartWith("wasteland-siege-engine"));
            motor.Synchronize();
            float before = vehicle.transform.position.x;
            session.StepSimulation(0.5f);
            yield return null;
            Assert.That(vehicle.transform.position.x, Is.EqualTo(before));
            Assert.That(motor.Mode, Is.EqualTo(MovementMode.Boss));
        }

        [Test]
        public void BossDeathForwardsVictoryEventOnceAndStopsSimulation()
        {
            StartEncounter();
            AdvanceToBoss();
            int victories = 0;
            session.Finished += result => { Assert.That(result, Is.EqualTo(RunStatus.Victory)); victories++; };
            session.Run.Enemies[0].State.Health.TakeDamage(1000);
            session.StepSimulation(0.1f);
            double elapsed = session.Run.ElapsedSeconds;
            session.StepSimulation(100);
            Assert.That(victories, Is.EqualTo(1));
            Assert.That(session.Run.ElapsedSeconds, Is.EqualTo(elapsed));
            Assert.That(session.Run.Status, Is.EqualTo(RunStatus.Victory));
            Assert.That(spawner.ActiveCount, Is.Zero);
        }

        [Test]
        public void PlatformDeathForwardsDefeatOnceAndRejectsRepair()
        {
            StartEncounter();
            int defeats = 0;
            session.Finished += result => { Assert.That(result, Is.EqualTo(RunStatus.Defeat)); defeats++; };
            session.Run.Platform.Health.TakeDamage(100);
            session.StepSimulation(0.1f);
            session.StepSimulation(100);
            Assert.That(defeats, Is.EqualTo(1));
            Assert.That(motor.Mode, Is.EqualTo(MovementMode.Stopped));
            Assert.That(hud.Repair(), Is.EqualTo(PurchaseResult.RunNotActive));
        }

        [Test]
        public void HudPurchasesCannotBypassFundsAndRepairValidation()
        {
            Assert.That(hud.BuyAmmunition(), Is.EqualTo(PurchaseResult.RunNotActive));
            StartEncounter();
            session.Run.Platform.Resources.Collect(ResourceType.Energy, 29);
            Assert.That(hud.BuyAmmunition(), Is.EqualTo(PurchaseResult.InsufficientEnergy));
            Assert.That(session.Run.Platform.AdvancedAmmunition.Count, Is.Zero);
            Assert.That(session.Run.Platform.Resources.GetBalance(ResourceType.Energy), Is.EqualTo(29));
            session.Run.Platform.Resources.Collect(ResourceType.Energy, 1);
            Assert.That(hud.BuyAmmunition(), Is.EqualTo(PurchaseResult.Succeeded));
            session.Run.Platform.Health.TakeDamage(40);
            session.Run.Platform.Resources.Collect(ResourceType.Energy, 49);
            Assert.That(hud.Repair(), Is.EqualTo(PurchaseResult.InsufficientEnergy));
            Assert.That(session.Run.Platform.Health.Current, Is.EqualTo(60));
            session.Run.Platform.Resources.Collect(ResourceType.Energy, 1);
            Assert.That(hud.Repair(), Is.EqualTo(PurchaseResult.Succeeded));
            Assert.That(session.Run.Platform.Health.Current, Is.EqualTo(85));
        }

        [Test]
        public void HudSpecialRejectsInsufficientFundsThenKillsAndCollectsReward()
        {
            StartEncounter();
            session.Run.Platform.Resources.Collect(ResourceType.Energy, 79);
            Assert.That(hud.ActivateSpecial(), Is.EqualTo(PurchaseResult.InsufficientEnergy));
            Assert.That(session.Run.Enemies[0].State.Health.Current, Is.EqualTo(32));
            session.Run.Platform.Resources.Collect(ResourceType.Energy, 1);
            Assert.That(hud.ActivateSpecial(), Is.EqualTo(PurchaseResult.Succeeded));
            Assert.That(session.Run.Enemies[0].State.Health.IsDead, Is.True);
            Assert.That(session.Run.Platform.Resources.GetBalance(ResourceType.Energy), Is.EqualTo(12));
            Assert.That(spawner.ActiveCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator RestartResetsViewsInventoryClockAndOldEventSubscriptions()
        {
            StartEncounter();
            GameRun oldRun = session.Run;
            EnemyActor oldActor = spawner.FindActor(oldRun.Enemies[0].State.Id);
            oldRun.Platform.Resources.Collect(ResourceType.Energy, 30);
            hud.BuyAmmunition();
            hud.RestartRun();
            Assert.That(spawner.ActiveCount, Is.Zero);
            Assert.That(session.Run.ElapsedSeconds, Is.Zero);
            Assert.That(session.Run.Platform.AdvancedAmmunition.Count, Is.Zero);
            Assert.That(session.Run.Platform.Resources.GetBalance(ResourceType.Energy), Is.Zero);
            yield return null;
            Assert.That(oldActor == null, Is.True);
            session.StepSimulation(1);
            Assert.That(spawner.ActiveCount, Is.EqualTo(3));
            int finishes = 0;
            session.Finished += _ => finishes++;
            oldRun.Platform.Health.TakeDamage(100);
            oldRun.Tick(0.1);
            Assert.That(finishes, Is.Zero);
            session.Run.Platform.Health.TakeDamage(100);
            session.StepSimulation(0.1f);
            Assert.That(finishes, Is.EqualTo(1));
        }

        [Test]
        public void ApplicationPauseStopsClockMovementAndPurchaseCommands()
        {
            hud.StartRun();
            session.SendMessage("OnApplicationPause", true, SendMessageOptions.RequireReceiver);
            session.StepSimulation(0.5f);
            Assert.That(session.Run.ElapsedSeconds, Is.Zero);
            Assert.That(hud.BuyAmmunition(), Is.EqualTo(PurchaseResult.RunNotActive));
            session.SendMessage("OnApplicationPause", false, SendMessageOptions.RequireReceiver);
            session.StepSimulation(0.5f);
            Assert.That(session.Run.ElapsedSeconds, Is.EqualTo(0.5));
        }

        [Test]
        public void InspectorEnemySettingsBecomeAuthoritativeSpawnStats()
        {
            settings.basicEnemy.health = 70;
            settings.basicEnemy.speed = 2;
            settings.basicEnemy.attack.damage = 7;
            settings.basicEnemy.attack.intervalSeconds = 0.4f;
            settings.basicEnemy.energyReward = 18;
            session.ResetRun();
            StartEncounter();
            var definition = session.Run.Enemies[0].State.Definition;
            Assert.That(definition.MaximumHealth, Is.EqualTo(70));
            Assert.That(definition.MovementSpeed, Is.EqualTo(2));
            Assert.That(definition.Attack.Damage, Is.EqualTo(7));
            Assert.That(definition.Attack.AttackIntervalSeconds, Is.EqualTo(0.4).Within(0.00001));
            Assert.That(definition.EnergyDrop, Is.EqualTo(18));
        }

        [Test]
        public void InvalidInspectorConfigurationDoesNotDiscardAnExistingRun()
        {
            GameRun current = session.Run;
            settings.maximumHealth = 0;
            Assert.Throws<ArgumentOutOfRangeException>(() => session.ResetRun());
            Assert.That(session.Run, Is.SameAs(current));
        }

        private GameObject Child(string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(root.transform, false);
            return child;
        }

        private EnemyActor Template(string name)
        {
            GameObject template = Child(name);
            EnemyActor actor = template.AddComponent<EnemyActor>();
            template.SetActive(false);
            return actor;
        }

        private void StartEncounter()
        {
            hud.StartRun();
            session.StepSimulation(1);
        }

        private void AdvanceToBoss()
        {
            for (int i = 0; i < 3; i++)
            {
                foreach (EnemyAgent enemy in session.Run.Enemies) enemy.State.Health.TakeDamage(1000);
                session.StepSimulation(0.01f);
            }
        }
    }
}
