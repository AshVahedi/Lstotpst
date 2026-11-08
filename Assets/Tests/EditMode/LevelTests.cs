using System;
using System.Collections.Generic;
using LastOutpost.Core;
using LastOutpost.Core.Combat;
using LastOutpost.Core.Configuration;
using LastOutpost.Core.Economy;
using LastOutpost.Core.Enemies;
using LastOutpost.Core.Levels;
using NUnit.Framework;

namespace LastOutpost.Tests
{
    public sealed class LevelTests
    {
        private PrototypeContent content;
        private Health platformHealth;
        private LevelProgression level;

        [SetUp]
        public void SetUp()
        {
            content = new PrototypeContent();
            platformHealth = new Health(100);
            level = new LevelProgression(content.LevelOne, platformHealth);
        }

        [Test]
        public void InitialStateDoesNotSpawnBeforeStart()
        {
            Assert.That(level.State, Is.EqualTo(LevelState.Ready));
            Assert.That(level.CurrentWave, Is.Zero);
            Assert.That(level.RemainingEnemies, Is.Zero);
            Assert.That(level.Progress, Is.Zero);
            Assert.That(level.BossActive, Is.False);
            level.Refresh();
            Assert.That(level.State, Is.EqualTo(LevelState.Ready));
        }

        [Test]
        public void WavesActivateInConfiguredOrderWithBossOnlyLast()
        {
            string[][] expected = {
                new[] { "basic", "basic", "basic" },
                new[] { "fast", "fast", "fast", "fast" },
                new[] { "basic", "basic", "basic", "fast", "fast", "fast" },
                new[] { "wasteland-siege-engine" }
            };
            var ids = new HashSet<int>();
            for (int wave = 0; wave < expected.Length; wave++)
            {
                Assert.That(level.TryBeginNextEncounter(), Is.True);
                Assert.That(level.CurrentWave, Is.EqualTo(wave + 1));
                Assert.That(level.BossActive, Is.EqualTo(wave == 3));
                Assert.That(level.RemainingEnemies, Is.EqualTo(expected[wave].Length));
                for (int i = 0; i < level.ActiveEnemies.Count; i++)
                {
                    Assert.That(level.ActiveEnemies[i].Definition.Id, Is.EqualTo(expected[wave][i]));
                    Assert.That(ids.Add(level.ActiveEnemies[i].Id), Is.True);
                }
                DefeatCurrentEnemies();
            }
            Assert.That(level.State, Is.EqualTo(LevelState.Victory));
            Assert.That(level.Progress, Is.EqualTo(1));
            Assert.That(level.TryBeginNextEncounter(), Is.False);
        }

        [Test]
        public void NextWaveCannotStartUntilEveryEnemyIsDead()
        {
            level.TryBeginNextEncounter();
            Assert.That(level.TryBeginNextEncounter(), Is.False);
            level.ActiveEnemies[0].Health.TakeDamage(1000);
            level.ActiveEnemies[1].Health.TakeDamage(1000);
            Assert.That(level.TryBeginNextEncounter(), Is.False);
            Assert.That(level.RemainingEnemies, Is.EqualTo(1));
            Assert.That(level.CurrentWave, Is.EqualTo(1));
            Assert.That(level.Progress, Is.EqualTo(1.0 / 6).Within(0.00001));
            Assert.That(level.BossActive, Is.False);
            level.ActiveEnemies[2].Health.TakeDamage(1000);
            Assert.That(level.TryBeginNextEncounter(), Is.True);
            Assert.That(level.CurrentWave, Is.EqualTo(2));
        }

        [Test]
        public void RepeatedRefreshDoesNotAdvanceOrCountDeathsTwice()
        {
            level.TryBeginNextEncounter();
            DefeatCurrentEnemies();
            for (int i = 0; i < 10; i++) level.Refresh();
            Assert.That(level.State, Is.EqualTo(LevelState.BetweenEncounters));
            Assert.That(level.CurrentWave, Is.EqualTo(1));
            Assert.That(level.Progress, Is.EqualTo(0.25));
            Assert.That(level.RemainingEnemies, Is.Zero);
        }

        [Test]
        public void BossMustDieForVictory()
        {
            AdvanceToBoss();
            Assert.That(level.State, Is.EqualTo(LevelState.EncounterActive));
            level.ActiveEnemies[0].Health.TakeDamage(299);
            level.Refresh();
            Assert.That(level.State, Is.EqualTo(LevelState.EncounterActive));
            Assert.That(level.TryBeginNextEncounter(), Is.False);
            level.ActiveEnemies[0].Health.TakeDamage(1);
            level.Refresh();
            Assert.That(level.State, Is.EqualTo(LevelState.Victory));
            Assert.That(platformHealth.IsDead, Is.False);
        }

        [Test]
        public void PlatformDestructionTriggersDefeatAndPreventsSpawning()
        {
            level.TryBeginNextEncounter();
            platformHealth.TakeDamage(100);
            Assert.That(level.TryBeginNextEncounter(), Is.False);
            Assert.That(level.State, Is.EqualTo(LevelState.Defeat));
            Assert.That(level.CurrentWave, Is.EqualTo(1));
        }

        [Test]
        public void DestroyedPlatformCannotStartLevel()
        {
            platformHealth.TakeDamage(100);
            Assert.That(level.TryBeginNextEncounter(), Is.False);
            Assert.That(level.State, Is.EqualTo(LevelState.Defeat));
            Assert.That(level.ActiveEnemies, Is.Empty);
        }

        [Test]
        public void DestructionBetweenWavesPreventsNextEncounter()
        {
            level.TryBeginNextEncounter();
            DefeatCurrentEnemies();
            platformHealth.TakeDamage(100);
            Assert.That(level.TryBeginNextEncounter(), Is.False);
            Assert.That(level.State, Is.EqualTo(LevelState.Defeat));
        }

        [Test]
        public void SimultaneousBossAndPlatformDeathIsDefeat()
        {
            AdvanceToBoss();
            level.ActiveEnemies[0].Health.TakeDamage(1000);
            platformHealth.TakeDamage(100);
            level.Refresh();
            Assert.That(level.State, Is.EqualTo(LevelState.Defeat));
            Assert.That(level.TryBeginNextEncounter(), Is.False);
        }

        [Test]
        public void DefeatCannotBeOverwrittenByLaterEnemyDeaths()
        {
            AdvanceToBoss();
            platformHealth.TakeDamage(100);
            level.Refresh();
            DefeatCurrentEnemies();
            Assert.That(level.State, Is.EqualTo(LevelState.Defeat));
        }

        [Test]
        public void LevelDefinitionRejectsPrematureOrMissingBoss()
        {
            var ordinary = new WaveDefinition("ordinary", false, new SpawnGroup(content.BasicEnemy, 1));
            var boss = new WaveDefinition("boss", true, new SpawnGroup(content.Boss, 1));
            Assert.Throws<ArgumentException>(() => new LevelDefinition("id", "name", boss, ordinary));
            Assert.Throws<ArgumentException>(() => new LevelDefinition("id", "name", ordinary, ordinary));
            Assert.Throws<ArgumentException>(() => new LevelDefinition("id", "name", boss));
            Assert.Throws<ArgumentException>(() => new WaveDefinition("bad", false, new SpawnGroup(content.Boss, 1)));
            Assert.Throws<ArgumentException>(() => new WaveDefinition("bad", true, new SpawnGroup(content.Boss, 2)));
            Assert.Throws<ArgumentException>(() => new WaveDefinition("empty", false));
        }

        [Test]
        public void ConfigurationCopiesInputArraysAndRejectsDuplicateEncounterIds()
        {
            var groups = new[] { new SpawnGroup(content.BasicEnemy, 2) };
            var wave = new WaveDefinition("wave", false, groups);
            groups[0] = new SpawnGroup(content.FastEnemy, 5);
            Assert.That(wave.EnemyCount, Is.EqualTo(2));
            Assert.That(wave.Groups[0].Enemy, Is.SameAs(content.BasicEnemy));
            var boss = new WaveDefinition("boss", true, new SpawnGroup(content.Boss, 1));
            var encounters = new[] { wave, boss };
            var definition = new LevelDefinition("id", "name", encounters);
            encounters[0] = boss;
            Assert.That(definition.Encounters[0], Is.SameAs(wave));
            Assert.Throws<ArgumentException>(() => new LevelDefinition("id", "name", wave, wave, boss));
        }

        [Test]
        public void EnemyAttackUsesItsOwnDamageRangeAndInterval()
        {
            var enemy = new EnemyState(1, content.FastEnemy);
            Assert.That(enemy.Attack.TryFire(new[] { new Target(1, platformHealth, 1) }, 0, out _), Is.False);
            var targets = new[] { new Target(1, platformHealth, 0.8) };
            Assert.That(enemy.Attack.TryFire(targets, 0, out _), Is.True);
            Assert.That(platformHealth.Current, Is.EqualTo(97));
            Assert.That(enemy.Attack.TryFire(targets, 0.699, out _), Is.False);
            Assert.That(enemy.Attack.TryFire(targets, 0.7, out _), Is.True);
            Assert.That(platformHealth.Current, Is.EqualTo(94));
        }

        [Test]
        public void FullCoreRunCanPurchaseAmmoDefeatBossAndReplayDeterministically()
        {
            // Headless rules integration only: no movement, Unity scene, or playability claim.
            Assert.That(RunHeadlessLevel(), Is.EqualTo(RunHeadlessLevel()));
        }

        private static string RunHeadlessLevel()
        {
            var defaults = new PrototypeContent();
            var platform = defaults.CreatePlatform();
            var progression = new LevelProgression(defaults.LevelOne, platform.Health);
            var basic = new Weapon(defaults.BasicGun);
            var missile = new Weapon(defaults.MissileLauncher, platform.AdvancedAmmunition);
            var shop = new TacticalPurchases(platform, defaults.Purchases, defaults.SpecialAttack);
            var targets = new List<Target>();
            int shots = 0;
            for (int tick = 0; tick < 10000 && progression.State != LevelState.Victory; tick++)
            {
                if (progression.State == LevelState.Ready || progression.State == LevelState.BetweenEncounters)
                {
                    if (platform.Resources.GetBalance(ResourceType.Energy) >= defaults.Purchases.AmmunitionCost)
                        shop.BuyAmmunition();
                    Assert.That(progression.TryBeginNextEncounter(), Is.True);
                }
                targets.Clear();
                for (int i = 0; i < progression.ActiveEnemies.Count; i++)
                {
                    EnemyState enemy = progression.ActiveEnemies[i];
                    targets.Add(new Target(enemy.Id, enemy.Health, 2 + i * 0.1));
                }
                double now = tick * 0.1;
                if (basic.TryFire(targets, now, out _)) shots++;
                if (missile.TryFire(targets, now, out _)) shots++;
                for (int i = 0; i < progression.ActiveEnemies.Count; i++)
                    progression.ActiveEnemies[i].TryCollectDrop(platform.Resources);
                progression.Refresh();
            }
            Assert.That(progression.State, Is.EqualTo(LevelState.Victory));
            Assert.That(progression.Progress, Is.EqualTo(1));
            Assert.That(shots, Is.GreaterThan(0));
            Assert.That(platform.AdvancedAmmunition.Count, Is.LessThan(60));
            Assert.That(platform.Resources.GetBalance(ResourceType.Energy), Is.GreaterThan(0));
            return shots + ":" + platform.Resources.GetBalance(ResourceType.Energy) + ":" + platform.AdvancedAmmunition.Count;
        }

        private void AdvanceToBoss()
        {
            for (int i = 0; i < 3; i++)
            {
                Assert.That(level.TryBeginNextEncounter(), Is.True);
                DefeatCurrentEnemies();
            }
            Assert.That(level.TryBeginNextEncounter(), Is.True);
        }

        private void DefeatCurrentEnemies()
        {
            for (int i = 0; i < level.ActiveEnemies.Count; i++)
                level.ActiveEnemies[i].Health.TakeDamage(int.MaxValue);
            level.Refresh();
        }
    }
}
