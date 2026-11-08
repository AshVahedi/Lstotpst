using System;
using LastOutpost.Core.Combat;
using LastOutpost.Core.Economy;
using LastOutpost.Core.Modules;

namespace LastOutpost.Core.Platform
{
    /// <summary>Future permanent upgrades can supply starting stats through this seam.</summary>
    public interface IPlatformStatsProvider
    {
        PlatformStats GetStartingStats();
    }

    public sealed class PlatformStats
    {
        public int MaximumHealth { get; }
        public int AmmunitionCapacity { get; }
        public int ModuleSlots { get; }

        public PlatformStats(int maximumHealth, int ammunitionCapacity, int moduleSlots)
        {
            MaximumHealth = Guard.Positive(maximumHealth, nameof(maximumHealth));
            AmmunitionCapacity = Guard.NonNegative(ammunitionCapacity, nameof(ammunitionCapacity));
            ModuleSlots = Guard.NonNegative(moduleSlots, nameof(moduleSlots));
        }
    }

    public sealed class PlatformState
    {
        public Health Health { get; }
        public Ammunition AdvancedAmmunition { get; }
        public ResourceWallet Resources { get; } = new ResourceWallet();
        public ModuleRack Modules { get; }

        public PlatformState(PlatformStats stats, int initialAmmunition = 0)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));
            Health = new Health(stats.MaximumHealth);
            AdvancedAmmunition = new Ammunition(initialAmmunition, stats.AmmunitionCapacity);
            Modules = new ModuleRack(stats.ModuleSlots);
        }
    }
}
