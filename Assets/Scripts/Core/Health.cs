namespace LastOutpost.Core
{
    public sealed class Health
    {
        public int Maximum { get; }
        public int Current { get; private set; }
        public bool IsDead => Current == 0;
        public int Missing => Maximum - Current;
        /// <summary>Fraction [0,1] removed from incoming damage; remaining damage rounds up.</summary>
        public decimal DamageReduction { get; }

        public Health(int maximum, decimal damageReduction = 0m)
        {
            Maximum = Guard.Positive(maximum, nameof(maximum));
            if (damageReduction < 0m || damageReduction > 1m)
                throw new System.ArgumentOutOfRangeException(nameof(damageReduction));
            DamageReduction = damageReduction;
            Current = maximum;
        }

        /// <returns>Actual health lost, clamped at zero health.</returns>
        public int TakeDamage(int damage)
        {
            Guard.NonNegative(damage, nameof(damage));
            int reduced = (int)System.Math.Ceiling(damage * (1m - DamageReduction));
            int applied = System.Math.Min(Current, reduced);
            Current -= applied;
            return applied;
        }

        /// <summary>Healing cannot revive a destroyed combatant.</summary>
        public int Heal(int amount)
        {
            Guard.NonNegative(amount, nameof(amount));
            if (IsDead) return 0;
            int applied = System.Math.Min(Missing, amount);
            Current += applied;
            return applied;
        }
    }
}
