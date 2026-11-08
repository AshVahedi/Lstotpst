using LastOutpost.Core.Combat;
using LastOutpost.Core.Economy;
using LastOutpost.Core.Enemies;
using LastOutpost.Core.Levels;
using LastOutpost.Core.Modules;
using LastOutpost.Core.Platform;

namespace LastOutpost.Core.Configuration
{
    /// <summary>
    /// Version 0.1 balancing defaults, deliberately kept outside rule implementations.
    /// Damage/health/Energy/ammunition are integers, distances world units, intervals seconds.
    /// Future Unity ScriptableObjects can construct these same validated models.
    /// </summary>
    public sealed class PrototypeContent : IPlatformStatsProvider
    {
        public WeaponConfig BasicGun { get; }
        public WeaponConfig MissileLauncher { get; }
        public TacticalPurchaseConfig Purchases { get; }
        public SpecialAttackConfig SpecialAttack { get; }
        public EnemyDefinition BasicEnemy { get; }
        public EnemyDefinition FastEnemy { get; }
        public EnemyDefinition Boss { get; }
        public LevelDefinition LevelOne { get; }

        public PrototypeContent()
        {
            var closest = new ClosestTargetStrategy();
            BasicGun = new WeaponConfig("basic-gun", 8, 0.6, 6, 0, closest);
            MissileLauncher = new WeaponConfig("missile-launcher", 30, 1.8, 7, 1, closest);
            Purchases = new TacticalPurchaseConfig(30, 20, 50, 25);
            // Single-target high-damage strike; no cooldown in the initial balance.
            SpecialAttack = new SpecialAttackConfig(80, 120, 10, 0, closest);
            BasicEnemy = new EnemyDefinition("basic", 32, 0.7,
                new WeaponConfig("basic-contact", 4, 1.5, 0.8, 0, closest), 12);
            FastEnemy = new EnemyDefinition("fast", 20, 1.5,
                new WeaponConfig("fast-contact", 3, 0.7, 0.8, 0, closest), 10);
            Boss = new EnemyDefinition("wasteland-siege-engine", 300, 0.4,
                new WeaponConfig("boss-cannon", 12, 2.5, 3, 0, closest), 60, true);
            LevelOne = new LevelDefinition("level-1", "Industrial Wasteland",
                new WaveDefinition("wave-1", false, new SpawnGroup(BasicEnemy, 3)),
                new WaveDefinition("wave-2", false, new SpawnGroup(FastEnemy, 4)),
                new WaveDefinition("wave-3", false, new SpawnGroup(BasicEnemy, 3), new SpawnGroup(FastEnemy, 3)),
                new WaveDefinition("final-boss", true, new SpawnGroup(Boss, 1)));
        }

        public PlatformStats GetStartingStats() => new PlatformStats(100, 60, 2);

        public PlatformState CreatePlatform()
        {
            var platform = new PlatformState(GetStartingStats());
            platform.Modules.TryAttach(0, new ModuleDefinition("basic-gun", ModuleKind.BasicGun));
            platform.Modules.TryAttach(1, new ModuleDefinition("missile-launcher", ModuleKind.MissileLauncher));
            return platform;
        }
    }
}
