using System;
using System.Collections.Generic;

namespace LastOutpost.Core.Economy
{
    // Credits belong to future permanent progression, not this tactical wallet.
    public enum ResourceType { Energy }

    public sealed class ResourceWallet
    {
        private readonly Dictionary<ResourceType, int> balances = new Dictionary<ResourceType, int>();

        public int GetBalance(ResourceType type)
        {
            ValidateType(type);
            return balances.TryGetValue(type, out int value) ? value : 0;
        }

        public void Collect(ResourceType type, int amount)
        {
            Guard.NonNegative(amount, nameof(amount));
            // Overflow is detected before assigning, preserving the old balance.
            int next = checked(GetBalance(type) + amount);
            balances[type] = next;
        }

        public bool TrySpend(ResourceType type, int amount)
        {
            Guard.NonNegative(amount, nameof(amount));
            int balance = GetBalance(type);
            if (balance < amount) return false;
            balances[type] = balance - amount;
            return true;
        }

        private static void ValidateType(ResourceType type)
        {
            if (!Enum.IsDefined(typeof(ResourceType), type))
                throw new ArgumentOutOfRangeException(nameof(type));
        }
    }
}
