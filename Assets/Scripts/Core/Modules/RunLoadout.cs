using System.Collections.Generic;

namespace LastOutpost.Core.Modules
{
    public sealed class ResolvedInstallation
    {
        public string SlotId { get; }
        public int SlotIndex { get; }
        public EquipmentDefinition Definition { get; }
        internal ResolvedInstallation(string slotId, int slotIndex, EquipmentDefinition definition)
        { SlotId = slotId; SlotIndex = slotIndex; Definition = definition; }
    }

    /// <summary>Resolved immutable gear snapshot. Contains no live profile or mutable DTO references.</summary>
    public sealed class RunLoadout
    {
        public int SlotCount { get; }
        public int EnergyPerDefeat { get; }
        public bool HasAdvancedWeapon { get; }
        public IReadOnlyList<ResolvedInstallation> Installations { get; }
        internal RunLoadout(int slotCount, int energyPerDefeat, List<ResolvedInstallation> installations)
        {
            SlotCount = slotCount; EnergyPerDefeat = energyPerDefeat;
            Installations = new List<ResolvedInstallation>(installations).AsReadOnly();
            HasAdvancedWeapon = installations.Exists(item => item.Definition.Mechanism == ModuleKind.MissileLauncher);
        }
    }
}
