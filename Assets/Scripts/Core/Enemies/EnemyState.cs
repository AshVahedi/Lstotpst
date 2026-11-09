using System;
using LastOutpost.Core.Combat;
using LastOutpost.Core.Economy;

namespace LastOutpost.Core.Enemies
{
    public sealed class EnemyDefinition
    {
        public string Id { get; }
        public int MaximumHealth { get; }
        /// <summary>World units per second. Unity movement will consume this value.</summary>
        public double MovementSpeed { get; }
        public WeaponConfig Attack { get; }
        public int EnergyDrop { get; }
        public bool IsBoss { get; }

        public EnemyDefinition(string id, int maximumHealth, double movementSpeed, WeaponConfig attack,
            int energyDrop, bool isBoss = false)
        {
            Id = Guard.Identifier(id, nameof(id));
            MaximumHealth = Guard.Positive(maximumHealth, nameof(maximumHealth));
            MovementSpeed = Guard.NonNegativeFinite(movementSpeed, nameof(movementSpeed));
            Attack = attack ?? throw new ArgumentNullException(nameof(attack));
            if (attack.AmmunitionPerShot != 0)
                throw new ArgumentException("Prototype enemy attacks must not require ammunition.", nameof(attack));
            EnergyDrop = Guard.NonNegative(energyDrop, nameof(energyDrop));
            IsBoss = isBoss;
        }
    }

    public sealed class EnemyState
    {
        public int Id { get; }
        public EnemyDefinition Definition { get; }
        public Health Health { get; }
        public Weapon Attack { get; }
        public bool DropCollected { get; private set; }

        public EnemyState(int id, EnemyDefinition definition)
        {
            Id = Guard.Positive(id, nameof(id));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Health = new Health(definition.MaximumHealth);
            Attack = new Weapon(definition.Attack);
        }

        /// <summary>Exactly-once reward per enemy instance, independent of collision/death callbacks.</summary>
        public bool TryCollectDrop(ResourceWallet wallet, int energyBonus = 0)
        {
            if (wallet == null) throw new ArgumentNullException(nameof(wallet));
            Guard.NonNegative(energyBonus, nameof(energyBonus));
            if (!Health.IsDead || DropCollected) return false;
            wallet.Collect(ResourceType.Energy, checked(Definition.EnergyDrop + energyBonus));
            // Mark only after a successful credit, so overflow cannot destroy a reward.
            DropCollected = true;
            return true;
        }
    }
}
