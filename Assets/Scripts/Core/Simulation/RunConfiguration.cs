using System;
using LastOutpost.Core.Combat;
using LastOutpost.Core.Configuration;
using LastOutpost.Core.Economy;
using LastOutpost.Core.Levels;
using LastOutpost.Core.Modules;
using LastOutpost.Core.Platform;

namespace LastOutpost.Core.Simulation
{
    public sealed class SpawnConfig
    {
        /// <summary>World units ahead of platform and between enemies; all spawns share one route.</summary>
        public double DistanceAhead { get; }
        public double Spacing { get; }

        public SpawnConfig(double distanceAhead = 4.4, double spacing = 0.6)
        {
            DistanceAhead = Guard.PositiveFinite(distanceAhead, nameof(distanceAhead));
            Spacing = Guard.NonNegativeFinite(spacing, nameof(spacing));
        }
    }

    public sealed class RunConfiguration
    {
        public PlatformStats Platform { get; }
        public LevelDefinition Level { get; }
        public WeaponConfig BasicGun { get; }
        public WeaponConfig MissileLauncher { get; }
        public TacticalPurchaseConfig Purchases { get; }
        public SpecialAttackConfig Special { get; }
        public RouteConfig Route { get; }
        public SpawnConfig Spawns { get; }
        public int InitialAmmunition { get; }

        public RunConfiguration(PlatformStats platform, LevelDefinition level, WeaponConfig basicGun,
            WeaponConfig missileLauncher, TacticalPurchaseConfig purchases, SpecialAttackConfig special,
            RouteConfig route, SpawnConfig spawns, int initialAmmunition = 0)
        {
            Platform = platform ?? throw new ArgumentNullException(nameof(platform));
            Level = level ?? throw new ArgumentNullException(nameof(level));
            BasicGun = basicGun ?? throw new ArgumentNullException(nameof(basicGun));
            MissileLauncher = missileLauncher ?? throw new ArgumentNullException(nameof(missileLauncher));
            Purchases = purchases ?? throw new ArgumentNullException(nameof(purchases));
            Special = special ?? throw new ArgumentNullException(nameof(special));
            Route = route ?? throw new ArgumentNullException(nameof(route));
            Spawns = spawns ?? throw new ArgumentNullException(nameof(spawns));
            InitialAmmunition = Guard.NonNegative(initialAmmunition, nameof(initialAmmunition));
            if (basicGun.AmmunitionPerShot != 0) throw new ArgumentException("The basic fallback must have unlimited ammunition.", nameof(basicGun));
            if (missileLauncher.AmmunitionPerShot == 0) throw new ArgumentException("The advanced weapon must consume ammunition.", nameof(missileLauncher));
            if (platform.ModuleSlots < 2) throw new ArgumentException("Prototype requires two module slots.", nameof(platform));
            if (initialAmmunition > platform.AmmunitionCapacity) throw new ArgumentOutOfRangeException(nameof(initialAmmunition));
        }

        public PlatformState CreatePlatform()
        {
            var platform = new PlatformState(Platform, InitialAmmunition);
            platform.Modules.TryAttach(0, new ModuleDefinition("basic-gun", ModuleKind.BasicGun));
            platform.Modules.TryAttach(1, new ModuleDefinition("missile-launcher", ModuleKind.MissileLauncher));
            return platform;
        }

        public static RunConfiguration Prototype()
        {
            var content = new PrototypeContent();
            return new RunConfiguration(content.GetStartingStats(), content.LevelOne, content.BasicGun,
                content.MissileLauncher, content.Purchases, content.SpecialAttack, new RouteConfig(), new SpawnConfig());
        }
    }
}
