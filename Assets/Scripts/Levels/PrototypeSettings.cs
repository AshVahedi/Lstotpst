using System;
using LastOutpost.Core.Combat;
using LastOutpost.Core.Configuration;
using LastOutpost.Core.Economy;
using LastOutpost.Core.Enemies;
using LastOutpost.Core.Levels;
using LastOutpost.Core.Platform;
using LastOutpost.Core.Simulation;
using UnityEngine;

namespace LastOutpost.Gameplay
{
    [Serializable]
    public sealed class WeaponTuning
    {
        [Min(1)] public int damage;
        [Min(0.01f), Tooltip("Seconds between attacks.")] public float intervalSeconds;
        [Min(0.01f), Tooltip("World units along the horizontal route.")] public float range;
        [Min(0), Tooltip("Zero is unlimited. Only the basic gun and enemies may use zero.")] public int ammunitionPerShot;

        public WeaponConfig Build(string id) => new WeaponConfig(id, damage, intervalSeconds, range,
            ammunitionPerShot, new ClosestTargetStrategy());

        public static WeaponTuning From(WeaponConfig config) => new WeaponTuning
        {
            damage = config.Damage, intervalSeconds = (float)config.AttackIntervalSeconds,
            range = (float)config.Range, ammunitionPerShot = config.AmmunitionPerShot
        };
    }

    [Serializable]
    public sealed class EnemyTuning
    {
        [Min(1)] public int health;
        [Min(0), Tooltip("World units per second toward the platform.")] public float speed;
        [Min(0)] public int energyReward;
        public WeaponTuning attack;

        public EnemyDefinition Build(string id, bool boss = false) => new EnemyDefinition(id, health, speed,
            attack.Build(id + "-attack"), energyReward, boss);

        public static EnemyTuning From(EnemyDefinition config) => new EnemyTuning
        {
            health = config.MaximumHealth, speed = (float)config.MovementSpeed,
            energyReward = config.EnergyDrop, attack = WeaponTuning.From(config.Attack)
        };
    }

    [CreateAssetMenu(menuName = "Last Outpost/Prototype Settings", fileName = "PrototypeSettings")]
    public sealed class PrototypeSettings : ScriptableObject
    {
        [SerializeField, HideInInspector] private bool initialized;
        [Header("Platform")]
        [Min(1)] public int maximumHealth = 100;
        [Min(0)] public int ammunitionCapacity = 60;
        [Min(0)] public int initialAmmunition;
        [Header("Weapons")]
        public WeaponTuning basicGun;
        public WeaponTuning missileLauncher;
        [Header("Enemies")]
        public EnemyTuning basicEnemy;
        public EnemyTuning fastEnemy;
        public EnemyTuning boss;
        [Header("Exactly three waves, then one boss")]
        [Min(1)] public int waveOneBasicCount = 3;
        [Min(1)] public int waveTwoFastCount = 4;
        [Min(1)] public int waveThreeBasicCount = 3;
        [Min(1)] public int waveThreeFastCount = 3;
        [Header("Travel (world units/second; delays in seconds)")]
        [Min(0)] public float travelSpeed = 0.9f;
        [Min(0)] public float encounterSpeed = 0.1f;
        [Min(0)] public float initialTravelSeconds = 1.5f;
        [Min(0)] public float betweenEncounterSeconds = 2;
        [Min(0.01f)] public float spawnDistanceAhead = 4.4f;
        [Min(0)] public float spawnSpacing = 0.6f;
        [Header("Tactical purchases (Energy and integer effect units)")]
        [Min(0)] public int ammunitionCost = 30;
        [Min(1)] public int ammunitionRefill = 20;
        [Min(0)] public int repairCost = 50;
        [Min(1)] public int repairHealth = 25;
        [Min(0)] public int specialCost = 80;
        [Min(1)] public int specialDamage = 120;
        [Min(0.01f)] public float specialRange = 10;
        [Min(0), Tooltip("Seconds; zero disables the optional cooldown.")] public float specialCooldownSeconds;

        private void OnEnable()
        {
            if (initialized) return;
            // Seed only new assets from Task 001's balance data; never overwrite saved inspector tuning.
            var defaults = new PrototypeContent();
            basicGun = WeaponTuning.From(defaults.BasicGun);
            missileLauncher = WeaponTuning.From(defaults.MissileLauncher);
            basicEnemy = EnemyTuning.From(defaults.BasicEnemy);
            fastEnemy = EnemyTuning.From(defaults.FastEnemy);
            boss = EnemyTuning.From(defaults.Boss);
            initialized = true;
        }

        public RunConfiguration Build()
        {
            if (basicGun == null || missileLauncher == null || basicEnemy == null || fastEnemy == null || boss == null)
                throw new InvalidOperationException("Prototype weapon/enemy tuning is missing.");
            EnemyDefinition basic = basicEnemy.Build("basic");
            EnemyDefinition fast = fastEnemy.Build("fast");
            EnemyDefinition finalBoss = boss.Build("wasteland-siege-engine", true);
            var level = new LevelDefinition("level-1", "Industrial Wasteland",
                new WaveDefinition("wave-1", false, new SpawnGroup(basic, waveOneBasicCount)),
                new WaveDefinition("wave-2", false, new SpawnGroup(fast, waveTwoFastCount)),
                new WaveDefinition("wave-3", false, new SpawnGroup(basic, waveThreeBasicCount), new SpawnGroup(fast, waveThreeFastCount)),
                new WaveDefinition("final-boss", true, new SpawnGroup(finalBoss, 1)));
            return new RunConfiguration(new PlatformStats(maximumHealth, ammunitionCapacity, 2), level,
                basicGun.Build("basic-gun"), missileLauncher.Build("missile-launcher"),
                new TacticalPurchaseConfig(ammunitionCost, ammunitionRefill, repairCost, repairHealth),
                new SpecialAttackConfig(specialCost, specialDamage, specialRange, specialCooldownSeconds, new ClosestTargetStrategy()),
                new RouteConfig(travelSpeed, encounterSpeed, initialTravelSeconds, betweenEncounterSeconds),
                new SpawnConfig(spawnDistanceAhead, spawnSpacing), initialAmmunition);
        }
    }
}
