using System;

namespace LastOutpost.Core.Combat
{
    public sealed class Ammunition
    {
        /// <summary>Null means no upper capacity; counts are always nonnegative.</summary>
        public int? Capacity { get; }
        public int Count { get; private set; }

        public Ammunition(int initialCount, int? capacity = null)
        {
            Guard.NonNegative(initialCount, nameof(initialCount));
            if (capacity.HasValue)
            {
                Guard.NonNegative(capacity.Value, nameof(capacity));
                if (initialCount > capacity.Value) throw new ArgumentOutOfRangeException(nameof(initialCount));
            }
            Capacity = capacity;
            Count = initialCount;
        }

        public int GetAddableAmount(int requested)
        {
            Guard.NonNegative(requested, nameof(requested));
            int limit = Capacity ?? int.MaxValue;
            return Math.Min(requested, limit - Count);
        }

        public int Add(int amount)
        {
            int added = GetAddableAmount(amount);
            Count += added;
            return added;
        }

        public bool TryConsume(int amount)
        {
            Guard.NonNegative(amount, nameof(amount));
            if (Count < amount) return false;
            Count -= amount;
            return true;
        }
    }
}
