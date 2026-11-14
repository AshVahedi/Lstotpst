using System;
using System.Collections.Generic;
using LastOutpost.Core.Modules;

namespace LastOutpost.Core.Combat
{
    /// <summary>Space-specific weapon definition selected by an existing equipment ID; not a second inventory.</summary>
    public sealed class SpaceWeaponDefinition
    {
        public string Id { get; }
        public int Damage { get; }
        public double Cooldown { get; }
        public double Range { get; }
        public double ArcDegrees { get; }
        public double ProjectileSpeed { get; }
        public WeaponFacing Facing { get; }
        public int Instances { get; }
        public SpaceWeaponDefinition(string id, int damage, double cooldown, double range, double arcDegrees,
            double projectileSpeed, WeaponFacing facing, int instances = 1)
        {
            Id = Guard.Identifier(id, nameof(id)); Damage = Guard.Positive(damage, nameof(damage));
            Cooldown = Guard.PositiveFinite(cooldown, nameof(cooldown)); Range = Guard.PositiveFinite(range, nameof(range));
            ProjectileSpeed = Guard.PositiveFinite(projectileSpeed, nameof(projectileSpeed));
            if (ShipPoint.Finite(arcDegrees, nameof(arcDegrees)) <= 0 || arcDegrees > 360 || !Enum.IsDefined(typeof(WeaponFacing), facing))
                throw new ArgumentOutOfRangeException(nameof(arcDegrees));
            ArcDegrees = arcDegrees; Facing = facing;
            if (instances < 1 || instances > 3) throw new ArgumentOutOfRangeException(nameof(instances), "One to three emitters fit inside a module.");
            Instances = instances;
        }
        public SpaceWeaponDefinition Scale(decimal multiplier) => new SpaceWeaponDefinition(Id,
            checked((int)Math.Max(1, decimal.Round(Damage * multiplier, 0, MidpointRounding.AwayFromZero))),
            Cooldown, Range, ArcDegrees, ProjectileSpeed, Facing, Instances);
    }

    public sealed class SpaceCombatConfiguration
    {
        public HorizontalShipLayout Layout { get; }
        public SpaceWeaponDefinition HullLaser { get; }
        public IReadOnlyDictionary<string, SpaceWeaponDefinition> EquipmentWeapons { get; }
        public IReadOnlyList<ArrivalSide> SpawnDistribution { get; }
        public int Seed { get; }
        public int MaximumEnemies { get; }
        public int MaximumProjectiles { get; }
        public double SpawnInterval { get; }
        public double SpawnMargin { get; }
        public double EnemyProjectileSpeed { get; }
        public double EnemyLifetime { get; }
        public double ReferenceHalfWidth { get; }
        public double ReferenceHalfHeight { get; }

        public SpaceCombatConfiguration(HorizontalShipLayout layout, SpaceWeaponDefinition hullLaser,
            IDictionary<string, SpaceWeaponDefinition> equipmentWeapons, IEnumerable<ArrivalSide> spawnDistribution,
            int seed = 17, int maximumEnemies = 8, int maximumProjectiles = 128, double spawnInterval = 0.45,
            double spawnMargin = 0.8, double enemyProjectileSpeed = 4, double enemyLifetime = 90,
            double referenceHalfWidth = 4.5, double referenceHalfHeight = 8)
        {
            Layout = layout ?? throw new ArgumentNullException(nameof(layout));
            HullLaser = hullLaser ?? throw new ArgumentNullException(nameof(hullLaser));
            if (hullLaser.Facing == WeaponFacing.Outboard) throw new ArgumentException("The central hull weapon cannot face outboard.");
            var definitions = new Dictionary<string, SpaceWeaponDefinition>(StringComparer.Ordinal);
            foreach (var entry in equipmentWeapons ?? throw new ArgumentNullException(nameof(equipmentWeapons)))
                definitions.Add(Guard.Identifier(entry.Key, nameof(equipmentWeapons)), entry.Value ?? throw new ArgumentException("Missing weapon definition."));
            EquipmentWeapons = new System.Collections.ObjectModel.ReadOnlyDictionary<string, SpaceWeaponDefinition>(definitions);
            var sides = new List<ArrivalSide>();
            foreach (var side in spawnDistribution ?? throw new ArgumentNullException(nameof(spawnDistribution)))
            { if (!Enum.IsDefined(typeof(ArrivalSide), side)) throw new ArgumentOutOfRangeException(nameof(spawnDistribution)); sides.Add(side); }
            if (sides.Count == 0) throw new ArgumentException("At least one arrival side is required.");
            SpawnDistribution = sides.AsReadOnly(); Seed = seed;
            MaximumEnemies = Guard.Positive(maximumEnemies, nameof(maximumEnemies));
            MaximumProjectiles = Guard.Positive(maximumProjectiles, nameof(maximumProjectiles));
            // Bound memory and reject impractical configuration before a persistent run registration.
            if (maximumEnemies > 128 || maximumProjectiles > 2048) throw new ArgumentOutOfRangeException(nameof(maximumEnemies));
            SpawnInterval = Guard.PositiveFinite(spawnInterval, nameof(spawnInterval));
            SpawnMargin = Guard.PositiveFinite(spawnMargin, nameof(spawnMargin));
            EnemyProjectileSpeed = Guard.PositiveFinite(enemyProjectileSpeed, nameof(enemyProjectileSpeed));
            EnemyLifetime = Guard.PositiveFinite(enemyLifetime, nameof(enemyLifetime));
            ReferenceHalfWidth = Guard.PositiveFinite(referenceHalfWidth, nameof(referenceHalfWidth));
            ReferenceHalfHeight = Guard.PositiveFinite(referenceHalfHeight, nameof(referenceHalfHeight));
            if (referenceHalfWidth <= layout.MaximumHalfExtent || referenceHalfHeight <= 1.2)
                throw new ArgumentException("Camera must leave room beyond the ship.");
        }

        public SpaceCombatConfiguration Scale(decimal multiplier)
        {
            if (multiplier <= 0) throw new ArgumentOutOfRangeException(nameof(multiplier));
            var weapons = new Dictionary<string, SpaceWeaponDefinition>();
            foreach (var item in EquipmentWeapons) weapons.Add(item.Key, item.Value.Scale(multiplier));
            return new SpaceCombatConfiguration(Layout, HullLaser.Scale(multiplier), weapons, SpawnDistribution,
                Seed, MaximumEnemies, MaximumProjectiles, SpawnInterval, SpawnMargin, EnemyProjectileSpeed,
                EnemyLifetime, ReferenceHalfWidth, ReferenceHalfHeight);
        }

        public static SpaceCombatConfiguration Prototype(HorizontalShipLayout layout = null) => new SpaceCombatConfiguration(
            layout ?? new HorizontalShipLayout(EquipmentConfiguration.Prototype(), HorizontalShipLayout.DefaultPoints()),
            new SpaceWeaponDefinition("laser", 5, 0.25, 7, 100, 24, WeaponFacing.Forward),
            new Dictionary<string, SpaceWeaponDefinition> {
                ["basic-cannon"] = new SpaceWeaponDefinition("cannon", 24, 0.9, 6, 220, 13, WeaponFacing.Outboard),
                ["missile-launcher"] = new SpaceWeaponDefinition("missile", 55, 2.4, 8, 220, 9, WeaponFacing.Outboard) },
            new[] { ArrivalSide.Front, ArrivalSide.Left, ArrivalSide.Right, ArrivalSide.Rear });
    }
}
