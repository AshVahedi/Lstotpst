using System;
using System.Collections.Generic;

namespace LastOutpost.Core.Combat
{
    public sealed class WeaponConfig
    {
        public string Id { get; }
        public int Damage { get; }
        /// <summary>Seconds between shots; the first shot is immediately available.</summary>
        public double AttackIntervalSeconds { get; }
        public double Range { get; }
        /// <summary>Zero means unlimited firing, independent of any ammunition inventory.</summary>
        public int AmmunitionPerShot { get; }
        public ITargetingStrategy Targeting { get; }

        public WeaponConfig(string id, int damage, double attackIntervalSeconds, double range,
            int ammunitionPerShot, ITargetingStrategy targeting)
        {
            Id = Guard.Identifier(id, nameof(id));
            Damage = Guard.Positive(damage, nameof(damage));
            AttackIntervalSeconds = Guard.PositiveFinite(attackIntervalSeconds, nameof(attackIntervalSeconds));
            Range = Guard.PositiveFinite(range, nameof(range));
            AmmunitionPerShot = Guard.NonNegative(ammunitionPerShot, nameof(ammunitionPerShot));
            Targeting = targeting ?? throw new ArgumentNullException(nameof(targeting));
        }
    }

    public sealed class Weapon
    {
        private readonly Ammunition ammunition;
        public WeaponConfig Config { get; }
        public double NextAttackTimeSeconds { get; private set; }

        public Weapon(WeaponConfig config, Ammunition ammunition = null)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            if (config.AmmunitionPerShot > 0 && ammunition == null)
                throw new ArgumentNullException(nameof(ammunition));
            this.ammunition = ammunition;
        }

        /// <summary>
        /// Use elapsed level time in seconds, not wall-clock time. One call fires at most one shot;
        /// missed frames do not accumulate a burst. Rendering must follow this result, never cause damage.
        /// </summary>
        public bool TryFire(IReadOnlyList<Target> targets, double elapsedSeconds, out Target hit)
        {
            if (targets == null) throw new ArgumentNullException(nameof(targets));
            Guard.NonNegativeFinite(elapsedSeconds, nameof(elapsedSeconds));
            hit = null;
            if (elapsedSeconds < NextAttackTimeSeconds) return false;
            Target selected = Config.Targeting.Select(targets, Config.Range);
            // Defend the weapon's invariants even when a future strategy returns an invalid target.
            if (selected == null || selected.Health.IsDead || selected.Distance > Config.Range) return false;
            double nextTime = Guard.NonNegativeFinite(elapsedSeconds + Config.AttackIntervalSeconds, nameof(elapsedSeconds));
            if (Config.AmmunitionPerShot > 0 && !ammunition.TryConsume(Config.AmmunitionPerShot)) return false;

            selected.Health.TakeDamage(Config.Damage);
            NextAttackTimeSeconds = nextTime;
            hit = selected;
            return true;
        }
    }
}
