namespace LastOutpost.Core
{
    public sealed class Health
    {
        public int Maximum { get; }
        public int Current { get; private set; }
        public bool IsDead => Current == 0;
        public int Missing => Maximum - Current;

        public Health(int maximum)
        {
            Maximum = Guard.Positive(maximum, nameof(maximum));
            Current = maximum;
        }

        /// <returns>Actual health lost, clamped at zero health.</returns>
        public int TakeDamage(int damage)
        {
            Guard.NonNegative(damage, nameof(damage));
            int applied = System.Math.Min(Current, damage);
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
