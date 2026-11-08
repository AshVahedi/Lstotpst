using System;
using System.Collections.Generic;
using LastOutpost.Core.Combat;
using LastOutpost.Core.Platform;

namespace LastOutpost.Core.Economy
{
    public enum PurchaseResult { Succeeded, InsufficientEnergy, NoEffect, PlatformDestroyed, CoolingDown }

    public sealed class TacticalPurchaseConfig
    {
        public int AmmunitionCost { get; }
        public int AmmunitionAmount { get; }
        public int RepairCost { get; }
        public int RepairHealth { get; }

        public TacticalPurchaseConfig(int ammunitionCost, int ammunitionAmount, int repairCost, int repairHealth)
        {
            AmmunitionCost = Guard.NonNegative(ammunitionCost, nameof(ammunitionCost));
            AmmunitionAmount = Guard.Positive(ammunitionAmount, nameof(ammunitionAmount));
            RepairCost = Guard.NonNegative(repairCost, nameof(repairCost));
            RepairHealth = Guard.Positive(repairHealth, nameof(repairHealth));
        }
    }

    public sealed class SpecialAttackConfig
    {
        public int EnergyCost { get; }
        public int Damage { get; }
        public double Range { get; }
        public double CooldownSeconds { get; }
        public ITargetingStrategy Targeting { get; }

        public SpecialAttackConfig(int energyCost, int damage, double range, double cooldownSeconds,
            ITargetingStrategy targeting)
        {
            EnergyCost = Guard.NonNegative(energyCost, nameof(energyCost));
            Damage = Guard.Positive(damage, nameof(damage));
            Range = Guard.PositiveFinite(range, nameof(range));
            CooldownSeconds = Guard.NonNegativeFinite(cooldownSeconds, nameof(cooldownSeconds));
            Targeting = targeting ?? throw new ArgumentNullException(nameof(targeting));
        }
    }

    /// <summary>
    /// Synchronous gameplay-thread transactions. All validation precedes spending; commit paths have
    /// no callbacks or fallible external effects. Do not call these models concurrently from UI/tasks.
    /// </summary>
    public sealed class TacticalPurchases
    {
        private readonly PlatformState platform;
        public TacticalPurchaseConfig Config { get; }
        public SpecialAttackConfig SpecialAttack { get; }
        public double NextSpecialTimeSeconds { get; private set; }

        public TacticalPurchases(PlatformState platform, TacticalPurchaseConfig config, SpecialAttackConfig specialAttack)
        {
            this.platform = platform ?? throw new ArgumentNullException(nameof(platform));
            Config = config ?? throw new ArgumentNullException(nameof(config));
            SpecialAttack = specialAttack ?? throw new ArgumentNullException(nameof(specialAttack));
        }

        public PurchaseResult BuyAmmunition()
        {
            if (platform.Health.IsDead) return PurchaseResult.PlatformDestroyed;
            int added = platform.AdvancedAmmunition.GetAddableAmount(Config.AmmunitionAmount);
            if (added == 0) return PurchaseResult.NoEffect;
            if (!platform.Resources.TrySpend(ResourceType.Energy, Config.AmmunitionCost))
                return PurchaseResult.InsufficientEnergy;
            // A partially full inventory receives the remaining capacity at the listed full price.
            platform.AdvancedAmmunition.Add(added);
            return PurchaseResult.Succeeded;
        }

        public PurchaseResult Repair()
        {
            if (platform.Health.IsDead) return PurchaseResult.PlatformDestroyed;
            if (platform.Health.Missing == 0) return PurchaseResult.NoEffect;
            if (!platform.Resources.TrySpend(ResourceType.Energy, Config.RepairCost))
                return PurchaseResult.InsufficientEnergy;
            platform.Health.Heal(Config.RepairHealth);
            return PurchaseResult.Succeeded;
        }

        public PurchaseResult ActivateSpecial(IReadOnlyList<Target> targets, double elapsedSeconds, out Target hit)
        {
            if (targets == null) throw new ArgumentNullException(nameof(targets));
            Guard.NonNegativeFinite(elapsedSeconds, nameof(elapsedSeconds));
            hit = null;
            if (platform.Health.IsDead) return PurchaseResult.PlatformDestroyed;
            if (elapsedSeconds < NextSpecialTimeSeconds) return PurchaseResult.CoolingDown;
            Target selected = SpecialAttack.Targeting.Select(targets, SpecialAttack.Range);
            if (selected == null || selected.Health.IsDead || selected.Distance > SpecialAttack.Range)
                return PurchaseResult.NoEffect;
            double nextTime = Guard.NonNegativeFinite(elapsedSeconds + SpecialAttack.CooldownSeconds, nameof(elapsedSeconds));
            if (!platform.Resources.TrySpend(ResourceType.Energy, SpecialAttack.EnergyCost))
                return PurchaseResult.InsufficientEnergy;
            selected.Health.TakeDamage(SpecialAttack.Damage);
            NextSpecialTimeSeconds = nextTime;
            hit = selected;
            return PurchaseResult.Succeeded;
        }
    }
}
