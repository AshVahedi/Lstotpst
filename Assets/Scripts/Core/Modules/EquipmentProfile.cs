using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace LastOutpost.Core.Modules
{
    public enum LoadoutResult
    {
        Succeeded, UnknownSlot, UnknownEquipment, NotOwned, Incompatible, DisabledSlot,
        DuplicateInstallation, CapacityExceeded, RequiredEquipment, SlotOccupied, SlotEmpty,
        AlreadyInstalled, WeaponLimit, InvalidData, RunActive, Busy, NotInitialized, RecoveryRequired,
        PersistenceFailed, PersistenceUncertain, Conflict, Overflow
    }
    public enum LoadoutOperation { Equip, Replace, Unequip, RestoreDefaults }

    [DataContract]
    public sealed class EquipmentInstallationData
    {
        [DataMember(Name = "slotId", IsRequired = true)] public string SlotId;
        [DataMember(Name = "equipmentId", IsRequired = true)] public string EquipmentId;
        internal EquipmentInstallationData Copy() => (EquipmentInstallationData)MemberwiseClone();
    }
    [DataContract]
    public sealed class EquipmentProfileData
    {
        [DataMember(Name = "ownedEquipmentIds", IsRequired = true)] public List<string> OwnedEquipmentIds = new List<string>();
        [DataMember(Name = "installations", IsRequired = true)] public List<EquipmentInstallationData> Installations = new List<EquipmentInstallationData>();
        internal EquipmentProfileData Copy() => new EquipmentProfileData {
            OwnedEquipmentIds = new List<string>(OwnedEquipmentIds), Installations = Installations.ConvertAll(item => item.Copy()) };
        internal bool IsStructurallyValid()
        {
            if (OwnedEquipmentIds == null || Installations == null) return false;
            var owned = new HashSet<string>(StringComparer.Ordinal); var slots = new HashSet<string>(StringComparer.Ordinal);
            foreach (string id in OwnedEquipmentIds) if (string.IsNullOrWhiteSpace(id) || !owned.Add(id)) return false;
            foreach (var item in Installations)
                if (item == null || string.IsNullOrWhiteSpace(item.SlotId) || string.IsNullOrWhiteSpace(item.EquipmentId) || !slots.Add(item.SlotId)) return false;
            return true; // Unknown catalog IDs are retained and reported by loadout validation, not erased.
        }
    }

    public sealed class InstalledEquipment
    {
        public string SlotId { get; }
        public string EquipmentId { get; }
        internal InstalledEquipment(string slot, string equipment) { SlotId = slot; EquipmentId = equipment; }
    }
    public sealed class LoadoutSnapshot
    {
        public IReadOnlyList<string> OwnedEquipmentIds { get; }
        public IReadOnlyList<InstalledEquipment> Installations { get; }
        public LoadoutSnapshot(EquipmentProfileData data)
        {
            if (data == null || !data.IsStructurallyValid()) throw new ArgumentException("Malformed equipment state.");
            OwnedEquipmentIds = new ReadOnlyCollection<string>(new List<string>(data.OwnedEquipmentIds));
            Installations = data.Installations.ConvertAll(item => new InstalledEquipment(item.SlotId, item.EquipmentId)).AsReadOnly();
        }
        public string At(string slotId)
        { foreach (var item in Installations) if (item.SlotId == slotId) return item.EquipmentId; return null; }
        internal EquipmentProfileData ToData()
        {
            var data = new EquipmentProfileData { OwnedEquipmentIds = new List<string>(OwnedEquipmentIds) };
            foreach (var item in Installations) data.Installations.Add(new EquipmentInstallationData { SlotId = item.SlotId, EquipmentId = item.EquipmentId });
            return data;
        }
    }

    public static class LoadoutRules
    {
        public static LoadoutResult Validate(EquipmentConfiguration config, EquipmentProfileData data)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (data == null || !data.IsStructurallyValid()) return LoadoutResult.InvalidData;
            var counts = new Dictionary<string, int>(StringComparer.Ordinal);
            long total = 0; int basic = 0, advanced = 0;
            foreach (var installation in data.Installations)
            {
                var slot = config.GetSlot(installation.SlotId); var item = config.Catalog.Get(installation.EquipmentId);
                if (slot == null) return LoadoutResult.UnknownSlot;
                if (item == null) return LoadoutResult.UnknownEquipment;
                if (!data.OwnedEquipmentIds.Contains(item.Id)) return LoadoutResult.NotOwned;
                if (!slot.Enabled) return LoadoutResult.DisabledSlot;
                if (!slot.Accepts(item)) return LoadoutResult.Incompatible;
                if (slot.Capacity.HasValue && item.InstallationSize > slot.Capacity) return LoadoutResult.CapacityExceeded;
                counts.TryGetValue(item.Id, out int count); counts[item.Id] = count + 1;
                if (count + 1 > item.MaximumInstallations) return LoadoutResult.DuplicateInstallation;
                total += item.InstallationSize;
                if (item.Mechanism == ModuleKind.BasicGun) basic++;
                if (item.Mechanism == ModuleKind.MissileLauncher) advanced++;
            }
            if (config.TotalCapacity.HasValue && total > config.TotalCapacity) return LoadoutResult.CapacityExceeded;
            foreach (var slot in config.Slots)
                if (slot.Required && !data.Installations.Exists(item => item.SlotId == slot.Id)) return LoadoutResult.RequiredEquipment;
            if (basic == 0) return LoadoutResult.RequiredEquipment;
            // The current combat engine has one firing channel per existing weapon mechanism.
            if (basic > 1 || advanced > 1) return LoadoutResult.WeaponLimit;
            return LoadoutResult.Succeeded;
        }

        public static LoadoutResult Change(EquipmentConfiguration config, LoadoutSnapshot current,
            LoadoutOperation operation, string slotId, string equipmentId, out EquipmentProfileData candidate)
        {
            candidate = null;
            if (current == null || !Enum.IsDefined(typeof(LoadoutOperation), operation)) return LoadoutResult.InvalidData;
            EquipmentProfileData next = current.ToData();
            if (operation == LoadoutOperation.RestoreDefaults) next.Installations = config.CreateInitialProfile().Installations;
            else
            {
                var slot = config.GetSlot(slotId);
                if (slot == null) return LoadoutResult.UnknownSlot;
                if (!slot.Enabled) return LoadoutResult.DisabledSlot;
                int index = next.Installations.FindIndex(item => item.SlotId == slotId);
                if (operation == LoadoutOperation.Unequip)
                {
                    if (index < 0) return LoadoutResult.SlotEmpty;
                    next.Installations.RemoveAt(index);
                }
                else
                {
                    var item = config.Catalog.Get(equipmentId);
                    if (item == null) return LoadoutResult.UnknownEquipment;
                    if (!next.OwnedEquipmentIds.Contains(item.Id)) return LoadoutResult.NotOwned;
                    if (index >= 0 && next.Installations[index].EquipmentId == equipmentId) return LoadoutResult.AlreadyInstalled;
                    if (index >= 0 && operation == LoadoutOperation.Equip) return LoadoutResult.SlotOccupied;
                    if (index < 0 && operation == LoadoutOperation.Replace) return LoadoutResult.SlotEmpty;
                    var entry = new EquipmentInstallationData { SlotId = slotId, EquipmentId = item.Id };
                    if (index < 0) next.Installations.Add(entry); else next.Installations[index] = entry;
                }
            }
            LoadoutResult result = Validate(config, next);
            if (result == LoadoutResult.Succeeded) candidate = next;
            return result;
        }
    }
}
