using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LastOutpost.Core.Combat;

namespace LastOutpost.Core.Modules
{
    public enum SlotCategory { PrimaryWeapon, SecondaryWeapon, Defense, Utility }
    public enum EquipmentCategory { BasicWeapon, AdvancedWeapon, DefensiveModule, SupportModule }
    public enum EquipmentStat { MaximumHealth, DamageReduction, WeaponDamageMultiplier, AmmunitionCapacity, EnergyPerDefeat }

    /// <summary>HP/rounds/Energy and armor fractions add; weapon factors multiply. Currency is unaffected.</summary>
    public sealed class EquipmentModifier
    {
        public EquipmentStat Stat { get; }
        public decimal Value { get; }
        public EquipmentModifier(EquipmentStat stat, decimal value)
        {
            if (!Enum.IsDefined(typeof(EquipmentStat), stat) || value < 0 ||
                (stat == EquipmentStat.WeaponDamageMultiplier && value == 0) ||
                (stat == EquipmentStat.DamageReduction && value > 1)) throw new ArgumentOutOfRangeException(nameof(value));
            if ((stat == EquipmentStat.MaximumHealth || stat == EquipmentStat.AmmunitionCapacity || stat == EquipmentStat.EnergyPerDefeat) &&
                (value != decimal.Truncate(value) || value > int.MaxValue)) throw new ArgumentOutOfRangeException(nameof(value));
            Stat = stat; Value = value;
        }
    }

    public sealed class EquipmentDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string Description { get; }
        public EquipmentCategory Category { get; }
        public ModuleKind Mechanism { get; }
        public IReadOnlyList<SlotCategory> CompatibleSlots { get; }
        public IReadOnlyList<EquipmentModifier> Modifiers { get; }
        public WeaponConfig WeaponOverride { get; }
        public int InstallationSize { get; }
        public int MaximumInstallations { get; }
        public bool AvailableByDefault { get; }

        public EquipmentDefinition(string id, string displayName, EquipmentCategory category, ModuleKind mechanism,
            IEnumerable<SlotCategory> compatibleSlots, IEnumerable<EquipmentModifier> modifiers = null,
            string description = "", int installationSize = 1, int maximumInstallations = 1,
            bool availableByDefault = true, WeaponConfig weaponOverride = null)
        {
            Id = Guard.Identifier(id, nameof(id)); DisplayName = Guard.Identifier(displayName, nameof(displayName));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            if (!Enum.IsDefined(typeof(EquipmentCategory), category) || !Enum.IsDefined(typeof(ModuleKind), mechanism))
                throw new ArgumentOutOfRangeException(nameof(category));
            // Only existing prototype mechanisms are supported. Names never select behavior.
            bool valid = (category == EquipmentCategory.BasicWeapon && mechanism == ModuleKind.BasicGun) ||
                (category == EquipmentCategory.AdvancedWeapon && mechanism == ModuleKind.MissileLauncher) ||
                (category == EquipmentCategory.DefensiveModule && mechanism == ModuleKind.Armor) ||
                (category == EquipmentCategory.SupportModule && mechanism == ModuleKind.Reactor);
            if (!valid || (weaponOverride != null && (category != EquipmentCategory.BasicWeapon && category != EquipmentCategory.AdvancedWeapon)))
                throw new ArgumentException("Category and existing mechanism must agree.");
            if (weaponOverride != null && ((category == EquipmentCategory.BasicWeapon) != (weaponOverride.AmmunitionPerShot == 0)))
                throw new ArgumentException("Basic weapons require unlimited ammo; advanced weapons consume ammo.");
            Category = category; Mechanism = mechanism; WeaponOverride = weaponOverride;
            InstallationSize = Guard.Positive(installationSize, nameof(installationSize));
            MaximumInstallations = Guard.Positive(maximumInstallations, nameof(maximumInstallations));
            AvailableByDefault = availableByDefault;
            var slots = new List<SlotCategory>();
            foreach (SlotCategory slot in compatibleSlots ?? throw new ArgumentNullException(nameof(compatibleSlots)))
            {
                if (!Enum.IsDefined(typeof(SlotCategory), slot) || slots.Contains(slot)) throw new ArgumentException("Invalid/duplicate slot category.");
                slots.Add(slot);
            }
            if (slots.Count == 0) throw new ArgumentException("Equipment needs a compatible slot category.");
            CompatibleSlots = slots.AsReadOnly();
            var effects = new List<EquipmentModifier>();
            if (modifiers != null) foreach (var effect in modifiers) effects.Add(effect ?? throw new ArgumentException("Null modifier."));
            Modifiers = effects.AsReadOnly();
        }
    }

    public sealed class EquipmentCatalog
    {
        private readonly Dictionary<string, EquipmentDefinition> byId = new Dictionary<string, EquipmentDefinition>(StringComparer.Ordinal);
        public IReadOnlyList<EquipmentDefinition> Definitions { get; }
        public EquipmentCatalog(IEnumerable<EquipmentDefinition> definitions)
        {
            var ordered = new List<EquipmentDefinition>();
            foreach (var item in definitions ?? throw new ArgumentNullException(nameof(definitions)))
            {
                if (item == null || byId.ContainsKey(item.Id)) throw new ArgumentException("Null/duplicate equipment ID.");
                byId.Add(item.Id, item); ordered.Add(item);
            }
            if (ordered.Count == 0) throw new ArgumentException("An equipment catalog cannot be empty.");
            Definitions = ordered.AsReadOnly();
        }
        public EquipmentDefinition Get(string id) => id != null && byId.TryGetValue(id, out var item) ? item : null;
    }

    public sealed class EquipmentSlot
    {
        public string Id { get; }
        public SlotCategory Category { get; }
        public IReadOnlyList<EquipmentCategory> AllowedCategories { get; }
        public bool Enabled { get; }
        public bool Required { get; }
        /// <summary>Optional installation-size budget, in configured capacity units.</summary>
        public int? Capacity { get; }
        public string DefaultEquipmentId { get; }
        public EquipmentSlot(string id, SlotCategory category, IEnumerable<EquipmentCategory> allowedCategories,
            bool enabled = true, bool required = false, int? capacity = null, string defaultEquipmentId = null)
        {
            Id = Guard.Identifier(id, nameof(id));
            if (!Enum.IsDefined(typeof(SlotCategory), category) || capacity < 0 || (required && !enabled))
                throw new ArgumentOutOfRangeException(nameof(category));
            Category = category; Enabled = enabled; Required = required; Capacity = capacity;
            DefaultEquipmentId = defaultEquipmentId == null ? null : Guard.Identifier(defaultEquipmentId, nameof(defaultEquipmentId));
            var allowed = new List<EquipmentCategory>();
            foreach (var item in allowedCategories ?? throw new ArgumentNullException(nameof(allowedCategories)))
            {
                if (!Enum.IsDefined(typeof(EquipmentCategory), item) || allowed.Contains(item)) throw new ArgumentException("Invalid/duplicate allowed category.");
                allowed.Add(item);
            }
            if (allowed.Count == 0) throw new ArgumentException("A slot must accept a category.");
            AllowedCategories = allowed.AsReadOnly();
        }

        public bool Accepts(EquipmentDefinition item)
        {
            if (item == null) return false;
            bool category = false, bay = false;
            foreach (var allowed in AllowedCategories) if (allowed == item.Category) category = true;
            foreach (var compatible in item.CompatibleSlots) if (compatible == Category) bay = true;
            return category && bay;
        }
    }

    public sealed class EquipmentConfiguration
    {
        private readonly Dictionary<string, EquipmentSlot> byId = new Dictionary<string, EquipmentSlot>(StringComparer.Ordinal);
        public EquipmentCatalog Catalog { get; }
        public IReadOnlyList<EquipmentSlot> Slots { get; }
        public int? TotalCapacity { get; }
        public EquipmentConfiguration(EquipmentCatalog catalog, IEnumerable<EquipmentSlot> slots, int? totalCapacity = null)
        {
            Catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            if (totalCapacity < 0) throw new ArgumentOutOfRangeException(nameof(totalCapacity));
            TotalCapacity = totalCapacity;
            var ordered = new List<EquipmentSlot>();
            foreach (var slot in slots ?? throw new ArgumentNullException(nameof(slots)))
            {
                if (slot == null || byId.ContainsKey(slot.Id)) throw new ArgumentException("Null/duplicate slot ID.");
                byId.Add(slot.Id, slot); ordered.Add(slot);
            }
            Slots = ordered.AsReadOnly();
            if (LoadoutRules.Validate(this, CreateInitialProfile()) != LoadoutResult.Succeeded)
                throw new ArgumentException("Default loadout must be owned, compatible and combat-ready.");
        }
        public EquipmentSlot GetSlot(string id) => id != null && byId.TryGetValue(id, out var slot) ? slot : null;
        public EquipmentProfileData CreateInitialProfile()
        {
            var result = new EquipmentProfileData();
            foreach (var item in Catalog.Definitions) if (item.AvailableByDefault) result.OwnedEquipmentIds.Add(item.Id);
            foreach (var slot in Slots) if (slot.DefaultEquipmentId != null)
                result.Installations.Add(new EquipmentInstallationData { SlotId = slot.Id, EquipmentId = slot.DefaultEquipmentId });
            return result;
        }
        public static EquipmentConfiguration Prototype()
        {
            var catalog = new EquipmentCatalog(new[] {
                new EquipmentDefinition("basic-cannon", "Basic Cannon", EquipmentCategory.BasicWeapon, ModuleKind.BasicGun,
                    new[] { SlotCategory.PrimaryWeapon }, description: "Unlimited ammunition; uses configured basic gun."),
                new EquipmentDefinition("missile-launcher", "Missile Launcher", EquipmentCategory.AdvancedWeapon, ModuleKind.MissileLauncher,
                    new[] { SlotCategory.SecondaryWeapon }, description: "Uses configured advanced weapon and ammunition."),
                new EquipmentDefinition("armor-plating", "Armor Plating", EquipmentCategory.DefensiveModule, ModuleKind.Armor,
                    new[] { SlotCategory.Defense }, new[] { new EquipmentModifier(EquipmentStat.DamageReduction, 0.02m) },
                    "Adds 2 percentage points of armor, within the configured armor ceiling."),
                new EquipmentDefinition("reactor-booster", "Reactor Booster", EquipmentCategory.SupportModule, ModuleKind.Reactor,
                    new[] { SlotCategory.Utility }, new[] { new EquipmentModifier(EquipmentStat.EnergyPerDefeat, 1m) },
                    "Adds 1 tactical Energy to each defeated enemy's once-only drop.")
            });
            return new EquipmentConfiguration(catalog, new[] {
                new EquipmentSlot("primary", SlotCategory.PrimaryWeapon, new[] { EquipmentCategory.BasicWeapon }, required: true, defaultEquipmentId: "basic-cannon"),
                new EquipmentSlot("secondary", SlotCategory.SecondaryWeapon, new[] { EquipmentCategory.AdvancedWeapon }, defaultEquipmentId: "missile-launcher"),
                new EquipmentSlot("defense", SlotCategory.Defense, new[] { EquipmentCategory.DefensiveModule }, defaultEquipmentId: "armor-plating"),
                new EquipmentSlot("utility", SlotCategory.Utility, new[] { EquipmentCategory.SupportModule }, defaultEquipmentId: "reactor-booster")
            });
        }
    }
}
