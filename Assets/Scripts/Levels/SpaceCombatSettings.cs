using System;
using System.Collections.Generic;
using LastOutpost.Core.Combat;
using LastOutpost.Core.Modules;
using UnityEngine;

namespace LastOutpost.Gameplay
{
    [Serializable]
    public sealed class SpaceWeaponTuning
    {
        public string id;
        public int damage;
        public float cooldownSeconds, range, arcDegrees, projectileSpeed;
        public WeaponFacing facing;
        [Range(1, 3), Tooltip("Independent emitters on this installed module; ownership is not duplicated.")]
        public int instances = 1;
        public SpaceWeaponDefinition Build() => new SpaceWeaponDefinition(id, damage, cooldownSeconds, range, arcDegrees, projectileSpeed, facing, instances);
        public static SpaceWeaponTuning From(SpaceWeaponDefinition definition) => new SpaceWeaponTuning {
            id = definition.Id, damage = definition.Damage, cooldownSeconds = (float)definition.Cooldown,
            range = (float)definition.Range, arcDegrees = (float)definition.ArcDegrees,
            projectileSpeed = (float)definition.ProjectileSpeed, facing = definition.Facing, instances = definition.Instances };
    }
    [Serializable]
    public sealed class SpaceEquipmentWeapon
    {
        public string equipmentId;
        public SpaceWeaponTuning weapon;
    }

    [CreateAssetMenu(menuName = "Last Outpost/Space Combat Settings")]
    public sealed class SpaceCombatSettings : ScriptableObject
    {
        [SerializeField, HideInInspector] private bool initialized;
        [Tooltip("Mandatory built-in hull weapon; never a new owned inventory item.")]
        public SpaceWeaponTuning hullLaser;
        [Tooltip("Space weapon definitions keyed by the existing owned equipment ID.")]
        public SpaceEquipmentWeapon[] equipmentWeapons;
        public ArrivalSide[] spawnDistribution;
        public int seed, maximumEnemies, maximumProjectiles;
        public float spawnInterval, spawnMargin, enemyProjectileSpeed, enemyLifetime;

        private void OnEnable()
        {
            if (initialized) return;
            var defaults = SpaceCombatConfiguration.Prototype();
            hullLaser = SpaceWeaponTuning.From(defaults.HullLaser);
            var weapons = new List<SpaceEquipmentWeapon>();
            foreach (var item in defaults.EquipmentWeapons)
                weapons.Add(new SpaceEquipmentWeapon { equipmentId = item.Key, weapon = SpaceWeaponTuning.From(item.Value) });
            equipmentWeapons = weapons.ToArray(); spawnDistribution = new List<ArrivalSide>(defaults.SpawnDistribution).ToArray();
            seed = defaults.Seed; maximumEnemies = defaults.MaximumEnemies; maximumProjectiles = defaults.MaximumProjectiles;
            spawnInterval = (float)defaults.SpawnInterval; spawnMargin = (float)defaults.SpawnMargin;
            enemyProjectileSpeed = (float)defaults.EnemyProjectileSpeed; enemyLifetime = (float)defaults.EnemyLifetime;
            initialized = true;
        }

        public SpaceCombatConfiguration Build(SpaceShipSettings geometry)
        {
            if (geometry == null || hullLaser == null || equipmentWeapons == null) throw new ArgumentException("Space combat settings are incomplete.");
            HorizontalShipLayout layout = geometry.Build();
            var weapons = new Dictionary<string, SpaceWeaponDefinition>(StringComparer.Ordinal);
            foreach (var entry in equipmentWeapons)
            {
                if (entry == null || entry.weapon == null || EquipmentConfiguration.Prototype().Catalog.Get(entry.equipmentId) == null)
                    throw new ArgumentException("A space weapon must reference an existing equipment definition.");
                var category = EquipmentConfiguration.Prototype().Catalog.Get(entry.equipmentId).Category;
                if (category != EquipmentCategory.BasicWeapon && category != EquipmentCategory.AdvancedWeapon)
                    throw new ArgumentException("Support/defense equipment is not a weapon.");
                weapons.Add(entry.equipmentId, entry.weapon.Build());
            }
            foreach (var equipment in EquipmentConfiguration.Prototype().Catalog.Definitions)
                if ((equipment.Category == EquipmentCategory.BasicWeapon || equipment.Category == EquipmentCategory.AdvancedWeapon) && !weapons.ContainsKey(equipment.Id))
                    throw new ArgumentException("Missing space weapon definition for " + equipment.Id);
            double height = ShipPortraitFraming.HalfHeight(geometry.widthLimit, geometry.threatMargin, geometry.minimumCameraHalfHeight, 9d / 16);
            return new SpaceCombatConfiguration(layout, hullLaser.Build(), weapons, spawnDistribution, seed,
                maximumEnemies, maximumProjectiles, spawnInterval, spawnMargin, enemyProjectileSpeed, enemyLifetime,
                height * 9 / 16, height);
        }
    }
}
