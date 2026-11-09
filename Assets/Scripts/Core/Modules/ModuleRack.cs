using System;

namespace LastOutpost.Core.Modules
{
    public enum ModuleKind { BasicGun, MissileLauncher, Shield, Reactor, Armor, Repair }

    public sealed class ModuleDefinition
    {
        public string Id { get; }
        public ModuleKind Kind { get; }

        public ModuleDefinition(string id, ModuleKind kind)
        {
            Id = Guard.Identifier(id, nameof(id));
            if (!Enum.IsDefined(typeof(ModuleKind), kind)) throw new ArgumentOutOfRangeException(nameof(kind));
            Kind = kind;
        }
    }

    /// <summary>Module IDs identify unique installed instances; slot indices are zero-based.</summary>
    public sealed class ModuleRack
    {
        private readonly ModuleDefinition[] slots;
        public int Capacity => slots.Length;
        public int Count { get; private set; }
        public bool IsReadOnly { get; private set; }
        internal void Freeze() => IsReadOnly = true;

        public ModuleRack(int capacity)
        {
            slots = new ModuleDefinition[Guard.NonNegative(capacity, nameof(capacity))];
        }

        public ModuleDefinition GetAt(int slot)
        {
            if (slot < 0 || slot >= Capacity) throw new ArgumentOutOfRangeException(nameof(slot));
            return slots[slot];
        }

        public bool TryAttach(int slot, ModuleDefinition module)
        {
            if (IsReadOnly || module == null || slot < 0 || slot >= Capacity || slots[slot] != null) return false;
            for (int i = 0; i < Capacity; i++)
                if (slots[i] != null && slots[i].Id == module.Id) return false;
            slots[slot] = module;
            Count++;
            return true;
        }

        public bool TryDetach(int slot)
        {
            if (IsReadOnly || slot < 0 || slot >= Capacity || slots[slot] == null) return false;
            slots[slot] = null;
            Count--;
            return true;
        }
    }
}
