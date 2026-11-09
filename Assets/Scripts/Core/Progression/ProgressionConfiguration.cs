using System;
using System.Collections.Generic;
using System.Numerics;
using LastOutpost.Core.Combat;
using LastOutpost.Core.Platform;
using LastOutpost.Core.Simulation;

namespace LastOutpost.Core.Progression
{
    // Stable schema identifiers. Never renumber existing values.
    public enum UpgradeKind { MaximumHealth = 1, Armor = 2, WeaponDamage = 3, AmmunitionCapacity = 4 }

    public sealed class UpgradeDefinition
    {
        private readonly long[] costs;
        public UpgradeKind Kind { get; }
        public int MaximumLevel => costs.Length;
        public long BaseCost { get; }
        public decimal GrowthFactor { get; }
        /// <summary>HP/rounds, fraction for armor, additive multiplier for weapon damage.</summary>
        public decimal IncreasePerLevel { get; }

        public UpgradeDefinition(UpgradeKind kind, long baseCost, decimal growthFactor,
            decimal increasePerLevel, int maximumLevel = 10)
        {
            if (!Enum.IsDefined(typeof(UpgradeKind), kind)) throw new ArgumentOutOfRangeException(nameof(kind));
            if (baseCost < 0 || growthFactor < 1m || increasePerLevel < 0m || maximumLevel < 1 || maximumLevel > 1000)
                throw new ArgumentOutOfRangeException(nameof(baseCost), "Nonnegative costs/effects, growth >= 1, and 1..1000 levels required.");
            if ((kind == UpgradeKind.MaximumHealth || kind == UpgradeKind.AmmunitionCapacity) &&
                increasePerLevel != decimal.Truncate(increasePerLevel))
                throw new ArgumentException("Health and ammunition increases must be whole units.", nameof(increasePerLevel));
            Kind = kind;
            BaseCost = baseCost;
            GrowthFactor = growthFactor;
            IncreasePerLevel = increasePerLevel;
            costs = new long[maximumLevel];
            int[] bits = decimal.GetBits(growthFactor);
            BigInteger numerator = (uint)bits[0] + ((BigInteger)(uint)bits[1] << 32) + ((BigInteger)(uint)bits[2] << 64);
            BigInteger denominator = BigInteger.Pow(10, (bits[3] >> 16) & 0xff);
            BigInteger divisor = BigInteger.GreatestCommonDivisor(numerator, denominator);
            numerator /= divisor;
            denominator /= divisor;
            BigInteger priceNumerator = baseCost;
            BigInteger priceDenominator = BigInteger.One;
            // Exact rational powers of the configured decimal; round only the final price.
            // Validate every purchasable price before this catalog can be used in a transaction.
            for (int n = 0; n < maximumLevel; n++)
            {
                costs[n] = (long)((priceNumerator + priceDenominator - 1) / priceDenominator);
                if (baseCost != 0 && n + 1 < maximumLevel)
                {
                    priceNumerator *= numerator;
                    priceDenominator *= denominator;
                }
            }
        }

        public long CostAt(int currentLevel)
        {
            if (currentLevel < 0 || currentLevel >= MaximumLevel) throw new ArgumentOutOfRangeException(nameof(currentLevel));
            return costs[currentLevel];
        }
    }

    public sealed class LevelRewardDefinition
    {
        public string LevelId { get; }
        public int LevelNumber { get; }
        public long VictoryCredits { get; }
        public LevelRewardDefinition(string levelId, int levelNumber, long victoryCredits)
        {
            LevelId = Guard.Identifier(levelId, nameof(levelId));
            LevelNumber = Guard.Positive(levelNumber, nameof(levelNumber));
            if (victoryCredits < 0 || levelNumber == int.MaxValue) throw new ArgumentOutOfRangeException(nameof(victoryCredits));
            VictoryCredits = victoryCredits;
        }
    }

    public sealed class ProgressionConfiguration
    {
        private readonly Dictionary<UpgradeKind, UpgradeDefinition> upgrades = new Dictionary<UpgradeKind, UpgradeDefinition>();
        private readonly Dictionary<string, LevelRewardDefinition> rewards = new Dictionary<string, LevelRewardDefinition>(StringComparer.Ordinal);
        public PlatformStats BasePlatform { get; }
        public decimal BaseWeaponMultiplier { get; }
        public decimal MaximumDamageReduction { get; }

        public ProgressionConfiguration(PlatformStats basePlatform, IEnumerable<UpgradeDefinition> definitions,
            IEnumerable<LevelRewardDefinition> levelRewards, decimal maximumDamageReduction = 0.30m,
            decimal baseWeaponMultiplier = 1m)
        {
            BasePlatform = basePlatform ?? throw new ArgumentNullException(nameof(basePlatform));
            if (definitions == null || levelRewards == null) throw new ArgumentNullException(nameof(definitions));
            if (maximumDamageReduction < basePlatform.DamageReduction || maximumDamageReduction > 1m || baseWeaponMultiplier < 1m)
                throw new ArgumentOutOfRangeException(nameof(maximumDamageReduction));
            MaximumDamageReduction = maximumDamageReduction;
            BaseWeaponMultiplier = baseWeaponMultiplier;
            foreach (UpgradeDefinition definition in definitions)
            {
                if (definition == null || upgrades.ContainsKey(definition.Kind)) throw new ArgumentException("Duplicate/null upgrade.");
                upgrades.Add(definition.Kind, definition);
            }
            foreach (UpgradeKind kind in Enum.GetValues(typeof(UpgradeKind)))
                if (!upgrades.ContainsKey(kind)) throw new ArgumentException("All four upgrades are required.");
            var numbers = new HashSet<int>();
            foreach (LevelRewardDefinition reward in levelRewards)
            {
                if (reward == null || rewards.ContainsKey(reward.LevelId) || !numbers.Add(reward.LevelNumber))
                    throw new ArgumentException("Duplicate/null level reward.");
                rewards.Add(reward.LevelId, reward);
            }
            if (rewards.Count == 0) throw new ArgumentException("At least one level reward is required.");
            // Effects must also remain representable at every allowed level.
            checked
            {
                _ = (int)(basePlatform.MaximumHealth + Get(UpgradeKind.MaximumHealth).IncreasePerLevel * Get(UpgradeKind.MaximumHealth).MaximumLevel);
                _ = (int)(basePlatform.AmmunitionCapacity + Get(UpgradeKind.AmmunitionCapacity).IncreasePerLevel * Get(UpgradeKind.AmmunitionCapacity).MaximumLevel);
                _ = baseWeaponMultiplier + Get(UpgradeKind.WeaponDamage).IncreasePerLevel * Get(UpgradeKind.WeaponDamage).MaximumLevel;
                _ = basePlatform.DamageReduction + Get(UpgradeKind.Armor).IncreasePerLevel * Get(UpgradeKind.Armor).MaximumLevel;
            }
        }

        public UpgradeDefinition Get(UpgradeKind kind) => upgrades.TryGetValue(kind, out UpgradeDefinition definition) ? definition : null;
        public LevelRewardDefinition GetReward(string id) => id != null && rewards.TryGetValue(id, out LevelRewardDefinition reward) ? reward : null;

        /// <summary>Authoritative numeric effect for gameplay snapshots and menu previews.</summary>
        public decimal EffectAt(UpgradeKind kind, int level)
        {
            UpgradeDefinition definition = Get(kind);
            if (definition == null || level < 0 || level > definition.MaximumLevel) throw new ArgumentOutOfRangeException(nameof(level));
            decimal increase = checked(level * definition.IncreasePerLevel);
            switch (kind)
            {
                case UpgradeKind.MaximumHealth: return checked(BasePlatform.MaximumHealth + increase);
                case UpgradeKind.Armor: return Math.Min(MaximumDamageReduction, checked(BasePlatform.DamageReduction + increase));
                case UpgradeKind.WeaponDamage: return checked(BaseWeaponMultiplier + increase);
                case UpgradeKind.AmmunitionCapacity: return checked(BasePlatform.AmmunitionCapacity + increase);
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        public static ProgressionConfiguration Prototype() => new ProgressionConfiguration(new PlatformStats(100, 100, 2),
            new[] {
                new UpgradeDefinition(UpgradeKind.MaximumHealth, 200, 1.5m, 10m),
                new UpgradeDefinition(UpgradeKind.Armor, 250, 1.5m, 0.03m),
                new UpgradeDefinition(UpgradeKind.WeaponDamage, 300, 1.6m, 0.05m),
                new UpgradeDefinition(UpgradeKind.AmmunitionCapacity, 150, 1.4m, 20m)
            }, new[] { new LevelRewardDefinition("level-1", 1, 500) });
    }

    /// <summary>Immutable run-start snapshot; purchases during a run affect subsequent runs only.</summary>
    public sealed class PermanentStatsProvider : IPlatformStatsProvider
    {
        private readonly PlatformStats stats;
        public decimal WeaponMultiplier { get; }

        public PermanentStatsProvider(ProgressionConfiguration config, ProgressionSnapshot profile)
        {
            if (config == null || profile == null) throw new ArgumentNullException(nameof(config));
            // A snapshot can come from another service/catalog. Reject levels that
            // exceed this catalog rather than applying an uncapped health/ammo perk.
            foreach (UpgradeKind kind in Enum.GetValues(typeof(UpgradeKind)))
                if (profile.Level(kind) < 0 || profile.Level(kind) > config.Get(kind).MaximumLevel)
                    throw new ArgumentOutOfRangeException(nameof(profile), "Upgrade levels exceed this catalog.");
            int hp = checked((int)config.EffectAt(UpgradeKind.MaximumHealth, profile.Level(UpgradeKind.MaximumHealth)));
            int ammo = checked((int)config.EffectAt(UpgradeKind.AmmunitionCapacity, profile.Level(UpgradeKind.AmmunitionCapacity)));
            decimal armor = config.EffectAt(UpgradeKind.Armor, profile.Level(UpgradeKind.Armor));
            stats = new PlatformStats(hp, ammo, config.BasePlatform.ModuleSlots, armor);
            WeaponMultiplier = config.EffectAt(UpgradeKind.WeaponDamage, profile.Level(UpgradeKind.WeaponDamage));
        }

        public PlatformStats GetStartingStats() => stats;

        public RunConfiguration ApplyTo(RunConfiguration template)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            return new RunConfiguration(stats, template.Level, Scale(template.BasicGun), Scale(template.MissileLauncher),
                template.Purchases, template.Special, template.Route, template.Spawns,
                Math.Min(template.InitialAmmunition, stats.AmmunitionCapacity));
        }

        private WeaponConfig Scale(WeaponConfig weapon) => new WeaponConfig(weapon.Id,
            checked((int)decimal.Round(weapon.Damage * WeaponMultiplier, 0, MidpointRounding.AwayFromZero)),
            weapon.AttackIntervalSeconds, weapon.Range, weapon.AmmunitionPerShot, weapon.Targeting);
    }
}
