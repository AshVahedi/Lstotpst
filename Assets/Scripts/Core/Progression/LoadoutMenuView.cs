using System.Collections.Generic;
using LastOutpost.Core.Modules;

namespace LastOutpost.Core.Progression
{
    public sealed class LoadoutMenuOption
    {
        public string EquipmentId { get; }
        public string Name { get; }
        public string Description { get; }
        public bool Owned { get; }
        public LoadoutResult Availability { get; }
        public bool CanInstall { get; }
        public LoadoutOperation Operation { get; }
        internal LoadoutMenuOption(EquipmentConfiguration config, LoadoutSnapshot profile, string slot,
            EquipmentDefinition item, bool canEdit)
        {
            EquipmentId = item.Id; Name = item.DisplayName; Description = item.Description;
            foreach (string id in profile.OwnedEquipmentIds) if (id == item.Id) Owned = true;
            Operation = profile.At(slot) == null ? LoadoutOperation.Equip : LoadoutOperation.Replace;
            Availability = LoadoutRules.Change(config, profile, Operation, slot, item.Id, out _);
            CanInstall = canEdit && Availability == LoadoutResult.Succeeded;
        }
    }

    public sealed class LoadoutMenuRow
    {
        public string SlotId { get; }
        public string Category { get; }
        public string EquipmentId { get; }
        public string EquipmentName { get; }
        public bool Enabled { get; }
        public bool Required { get; }
        public bool CanUnequip { get; }
        public LoadoutResult UnequipAvailability { get; }
        public IReadOnlyList<LoadoutMenuOption> Options { get; }
        internal LoadoutMenuRow(EquipmentConfiguration config, LoadoutSnapshot profile, string slotId, bool canEdit)
        {
            var slot = config.GetSlot(slotId);
            SlotId = slotId; Category = slot?.Category.ToString() ?? "UNRECOGNIZED SLOT";
            EquipmentId = profile.At(slotId);
            EquipmentName = config.Catalog.Get(EquipmentId)?.DisplayName ?? EquipmentId ?? "Empty";
            Enabled = slot?.Enabled ?? false; Required = slot?.Required ?? false;
            UnequipAvailability = LoadoutRules.Change(config, profile, LoadoutOperation.Unequip, slotId, null, out _);
            CanUnequip = canEdit && UnequipAvailability == LoadoutResult.Succeeded;
            var options = new List<LoadoutMenuOption>();
            if (slot != null) foreach (var item in config.Catalog.Definitions)
                if (slot.Accepts(item)) options.Add(new LoadoutMenuOption(config, profile, slotId, item, canEdit));
            Options = options.AsReadOnly();
        }
    }

    public sealed class LoadoutMenuView
    {
        public LoadoutResult Validation { get; }
        public bool CanEdit { get; }
        public bool CanRestoreDefaults { get; }
        public IReadOnlyList<LoadoutMenuRow> Slots { get; }
        public IReadOnlyList<string> UnknownOwnedIds { get; }
        internal LoadoutMenuView(EquipmentConfiguration config, LoadoutSnapshot profile, bool canEdit)
        {
            CanEdit = canEdit && profile != null;
            var rows = new List<LoadoutMenuRow>(); var unknown = new List<string>();
            Validation = profile == null ? LoadoutResult.NotInitialized : LoadoutRules.Validate(config, profile.ToData());
            if (profile != null)
            {
                foreach (var slot in config.Slots) rows.Add(new LoadoutMenuRow(config, profile, slot.Id, CanEdit));
                foreach (var item in profile.Installations)
                    if (config.GetSlot(item.SlotId) == null) rows.Add(new LoadoutMenuRow(config, profile, item.SlotId, false));
                foreach (string id in profile.OwnedEquipmentIds) if (config.Catalog.Get(id) == null) unknown.Add(id);
                CanRestoreDefaults = CanEdit && LoadoutRules.Change(config, profile, LoadoutOperation.RestoreDefaults, null, null, out _) == LoadoutResult.Succeeded;
            }
            Slots = rows.AsReadOnly(); UnknownOwnedIds = unknown.AsReadOnly();
        }
    }
}
