using System;
using System.Collections.Generic;
using LastOutpost.Core.Combat;
using LastOutpost.Core.Economy;
using LastOutpost.Core.Modules;
using LastOutpost.Core.Progression;
using LastOutpost.Core.Simulation;
using NUnit.Framework;

namespace LastOutpost.Tests
{
    public sealed class EquipmentTests
    {
        internal static ProgressionConfiguration Config(EquipmentConfiguration equipment = null) => new ProgressionConfiguration(
            ProgressionConfiguration.Prototype().BasePlatform, new[] {
                new UpgradeDefinition(UpgradeKind.MaximumHealth, 200, 1.5m, 10),
                new UpgradeDefinition(UpgradeKind.Armor, 250, 1.5m, 0.03m),
                new UpgradeDefinition(UpgradeKind.WeaponDamage, 300, 1.6m, 0.05m),
                new UpgradeDefinition(UpgradeKind.AmmunitionCapacity, 150, 1.4m, 20)
            }, new[] { new LevelRewardDefinition("level-1", 1, 500) }, equipment: equipment);

        internal static EquipmentDefinition Utility(string id = "reactor-extra", int maximum = 1,
            bool owned = true, params EquipmentModifier[] effects) => new EquipmentDefinition(id, "Test utility",
                EquipmentCategory.SupportModule, ModuleKind.Reactor, new[] { SlotCategory.Utility }, effects,
                maximumInstallations: maximum, availableByDefault: owned);

        internal static EquipmentConfiguration Extended(EquipmentDefinition item = null, bool enabled = true,
            int? slotCapacity = null, int? totalCapacity = null)
        {
            var basis = EquipmentConfiguration.Prototype();
            var definitions = new List<EquipmentDefinition>(basis.Catalog.Definitions); if (item != null) definitions.Add(item);
            var slots = new List<EquipmentSlot>(basis.Slots) {
                new EquipmentSlot("extra", SlotCategory.Utility, new[] { EquipmentCategory.SupportModule }, enabled, capacity: slotCapacity) };
            return new EquipmentConfiguration(new EquipmentCatalog(definitions), slots, totalCapacity);
        }

        [Test]
        public void DefaultsAreFourOwnedItemsWithTypedMechanismsAndAValidPrimary()
        {
            var config = EquipmentConfiguration.Prototype(); var data = config.CreateInitialProfile();
            Assert.That(data.OwnedEquipmentIds.Count, Is.EqualTo(4)); Assert.That(data.Installations.Count, Is.EqualTo(4));
            Assert.That(LoadoutRules.Validate(config, data), Is.EqualTo(LoadoutResult.Succeeded));
            Assert.That(config.Catalog.Get("basic-cannon").Mechanism, Is.EqualTo(ModuleKind.BasicGun));
            Assert.That(config.Catalog.Get("missing"), Is.Null); Assert.That(config.GetSlot("missing"), Is.Null);
            Assert.That(config.GetSlot("primary").Required, Is.True);
        }

        [Test]
        public void CatalogAndSlotsRejectDuplicateStableIds()
        {
            var item = Utility();
            Assert.Throws<ArgumentException>(() => new EquipmentCatalog(new[] { item, item }));
            var config = EquipmentConfiguration.Prototype();
            Assert.Throws<ArgumentException>(() => new EquipmentConfiguration(config.Catalog, new[] { config.Slots[0], config.Slots[0] }));
        }

        [TestCase(-1)]
        [TestCase(100)]
        public void UndefinedCategoriesAreRejected(int value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new EquipmentDefinition("x", "X", (EquipmentCategory)value,
                ModuleKind.BasicGun, new[] { SlotCategory.PrimaryWeapon }));
            Assert.Throws<ArgumentOutOfRangeException>(() => new EquipmentSlot("x", (SlotCategory)value, new[] { EquipmentCategory.BasicWeapon }));
        }

        [Test]
        public void InvalidMechanismsNamesAndCompatibilityDefinitionsAreRejected()
        {
            Assert.Throws<ArgumentException>(() => new EquipmentCatalog(new EquipmentDefinition[] { null }));
            Assert.Throws<ArgumentException>(() => new EquipmentCatalog(Array.Empty<EquipmentDefinition>()));
            Assert.Throws<ArgumentException>(() => Utility(" "));
            Assert.Throws<ArgumentException>(() => new EquipmentDefinition("x", "X", EquipmentCategory.BasicWeapon,
                ModuleKind.Reactor, new[] { SlotCategory.PrimaryWeapon }));
            Assert.Throws<ArgumentException>(() => new EquipmentDefinition("x", "X", EquipmentCategory.BasicWeapon,
                ModuleKind.BasicGun, Array.Empty<SlotCategory>()));
            Assert.Throws<ArgumentException>(() => new EquipmentDefinition("x", "X", EquipmentCategory.BasicWeapon,
                ModuleKind.BasicGun, new[] { SlotCategory.PrimaryWeapon, SlotCategory.PrimaryWeapon }));
            Assert.Throws<ArgumentException>(() => new EquipmentSlot("x", SlotCategory.Utility, Array.Empty<EquipmentCategory>()));
            Assert.Throws<ArgumentOutOfRangeException>(() => new EquipmentSlot("x", SlotCategory.Utility,
                new[] { EquipmentCategory.SupportModule }, enabled: false, required: true));
        }

        [Test]
        public void WeaponOverridesRetainUnlimitedBasicAndLimitedAdvancedAmmoRules()
        {
            var template = RunConfiguration.Prototype();
            Assert.Throws<ArgumentException>(() => new EquipmentDefinition("x", "X", EquipmentCategory.BasicWeapon,
                ModuleKind.BasicGun, new[] { SlotCategory.PrimaryWeapon }, weaponOverride: template.MissileLauncher));
            Assert.Throws<ArgumentException>(() => new EquipmentDefinition("x", "X", EquipmentCategory.AdvancedWeapon,
                ModuleKind.MissileLauncher, new[] { SlotCategory.SecondaryWeapon }, weaponOverride: template.BasicGun));
        }

        [TestCase(EquipmentStat.MaximumHealth, -1)]
        [TestCase(EquipmentStat.AmmunitionCapacity, -1)]
        [TestCase(EquipmentStat.EnergyPerDefeat, -1)]
        [TestCase(EquipmentStat.DamageReduction, 2)]
        [TestCase(EquipmentStat.WeaponDamageMultiplier, 0)]
        [TestCase((EquipmentStat)99, 1)]
        public void InvalidModifierValuesAreRejected(EquipmentStat stat, int value)
        { Assert.Throws<ArgumentOutOfRangeException>(() => new EquipmentModifier(stat, value)); }

        [TestCase(EquipmentStat.MaximumHealth)]
        [TestCase(EquipmentStat.AmmunitionCapacity)]
        [TestCase(EquipmentStat.EnergyPerDefeat)]
        public void IntegerUnitsCannotBeFractional(EquipmentStat stat)
        { Assert.Throws<ArgumentOutOfRangeException>(() => new EquipmentModifier(stat, 0.5m)); }

        [Test]
        public void DefinitionsAndSnapshotsCopyTheirInputs()
        {
            var slots = new[] { SlotCategory.Utility }; var modifiers = new[] { new EquipmentModifier(EquipmentStat.MaximumHealth, 10) };
            var item = new EquipmentDefinition("x", "Renaming does not change behavior", EquipmentCategory.SupportModule,
                ModuleKind.Reactor, slots, modifiers);
            slots[0] = SlotCategory.Defense; modifiers[0] = new EquipmentModifier(EquipmentStat.MaximumHealth, 50);
            Assert.That(item.CompatibleSlots[0], Is.EqualTo(SlotCategory.Utility)); Assert.That(item.Modifiers[0].Value, Is.EqualTo(10));
            var source = EquipmentConfiguration.Prototype().CreateInitialProfile(); var snapshot = new LoadoutSnapshot(source);
            source.OwnedEquipmentIds.Clear(); source.Installations[0].EquipmentId = "changed";
            Assert.That(snapshot.OwnedEquipmentIds.Count, Is.EqualTo(4)); Assert.That(snapshot.At("primary"), Is.EqualTo("basic-cannon"));
        }

        [TestCase(LoadoutOperation.Equip, "missing", "basic-cannon", LoadoutResult.UnknownSlot)]
        [TestCase(LoadoutOperation.Replace, "defense", "missing", LoadoutResult.UnknownEquipment)]
        [TestCase(LoadoutOperation.Replace, "defense", "basic-cannon", LoadoutResult.Incompatible)]
        [TestCase(LoadoutOperation.Unequip, "primary", null, LoadoutResult.RequiredEquipment)]
        [TestCase(LoadoutOperation.Equip, "defense", "reactor-booster", LoadoutResult.SlotOccupied)]
        [TestCase(LoadoutOperation.Replace, "defense", "armor-plating", LoadoutResult.AlreadyInstalled)]
        public void InvalidChangesReturnStructuredResultsAndNoCandidate(LoadoutOperation operation, string slot,
            string item, LoadoutResult expected)
        {
            var config = EquipmentConfiguration.Prototype(); var snapshot = new LoadoutSnapshot(config.CreateInitialProfile());
            Assert.That(LoadoutRules.Change(config, snapshot, operation, slot, item, out var candidate), Is.EqualTo(expected));
            Assert.That(candidate, Is.Null); Assert.That(snapshot.Installations.Count, Is.EqualTo(4));
        }

        [Test]
        public void OwnershipEnabledStatusAndSlotCapacityAreEnforced()
        {
            var item = Utility(owned: false); var config = Extended(item);
            Assert.That(LoadoutRules.Change(config, new LoadoutSnapshot(config.CreateInitialProfile()), LoadoutOperation.Equip,
                "extra", item.Id, out _), Is.EqualTo(LoadoutResult.NotOwned));
            config = Extended(Utility(), enabled: false);
            Assert.That(LoadoutRules.Change(config, new LoadoutSnapshot(config.CreateInitialProfile()), LoadoutOperation.Equip,
                "extra", "reactor-extra", out _), Is.EqualTo(LoadoutResult.DisabledSlot));
            config = Extended(Utility(), slotCapacity: 0);
            Assert.That(LoadoutRules.Change(config, new LoadoutSnapshot(config.CreateInitialProfile()), LoadoutOperation.Equip,
                "extra", "reactor-extra", out _), Is.EqualTo(LoadoutResult.CapacityExceeded));
        }

        [Test]
        public void BothSlotAcceptanceAndEquipmentCompatibilityAreRequired()
        {
            var item = new EquipmentDefinition("wrong-bay", "Utility", EquipmentCategory.SupportModule, ModuleKind.Reactor,
                new[] { SlotCategory.Defense }); var config = Extended(item);
            Assert.That(LoadoutRules.Change(config, new LoadoutSnapshot(config.CreateInitialProfile()), LoadoutOperation.Equip,
                "extra", item.Id, out _), Is.EqualTo(LoadoutResult.Incompatible));
        }

        [Test]
        public void DuplicateLimitsAndTotalCapacityAreEnforced()
        {
            var config = Extended();
            Assert.That(LoadoutRules.Change(config, new LoadoutSnapshot(config.CreateInitialProfile()), LoadoutOperation.Equip,
                "extra", "reactor-booster", out _), Is.EqualTo(LoadoutResult.DuplicateInstallation));
            config = Extended(Utility(), totalCapacity: 4);
            Assert.That(LoadoutRules.Change(config, new LoadoutSnapshot(config.CreateInitialProfile()), LoadoutOperation.Equip,
                "extra", "reactor-extra", out _), Is.EqualTo(LoadoutResult.CapacityExceeded));
        }

        [Test]
        public void AdditionalSlotsInstallAndRemoveWithoutHardcodedFourSlotLimits()
        {
            var config = Extended(Utility()); var current = new LoadoutSnapshot(config.CreateInitialProfile());
            Assert.That(LoadoutRules.Change(config, current, LoadoutOperation.Equip, "extra", "reactor-extra", out var candidate), Is.EqualTo(LoadoutResult.Succeeded));
            Assert.That(candidate.Installations.Count, Is.EqualTo(5));
            Assert.That(LoadoutRules.Change(config, new LoadoutSnapshot(candidate), LoadoutOperation.Unequip, "extra", null, out candidate), Is.EqualTo(LoadoutResult.Succeeded));
            Assert.That(candidate.Installations.Count, Is.EqualTo(4));
            Assert.That(LoadoutRules.Change(config, new LoadoutSnapshot(candidate), LoadoutOperation.Unequip, "extra", null, out _), Is.EqualTo(LoadoutResult.SlotEmpty));
        }

        [Test]
        public void OptionalPrimarySlotStillCannotRemoveTheOnlyBasicWeapon()
        {
            var basis = EquipmentConfiguration.Prototype(); var slots = new List<EquipmentSlot>(basis.Slots);
            slots[0] = new EquipmentSlot("primary", SlotCategory.PrimaryWeapon, new[] { EquipmentCategory.BasicWeapon }, defaultEquipmentId: "basic-cannon");
            var config = new EquipmentConfiguration(basis.Catalog, slots);
            Assert.That(LoadoutRules.Change(config, new LoadoutSnapshot(config.CreateInitialProfile()), LoadoutOperation.Unequip,
                "primary", null, out _), Is.EqualTo(LoadoutResult.RequiredEquipment));
        }

        [Test]
        public void DistinctBasicItemsCannotCreateASecondUnsupportedFiringChannel()
        {
            var basis = EquipmentConfiguration.Prototype(); var definitions = new List<EquipmentDefinition>(basis.Catalog.Definitions) {
                new EquipmentDefinition("other-basic", "Fixture only", EquipmentCategory.BasicWeapon, ModuleKind.BasicGun, new[] { SlotCategory.PrimaryWeapon }) };
            var slots = new List<EquipmentSlot>(basis.Slots) {
                new EquipmentSlot("other-primary", SlotCategory.PrimaryWeapon, new[] { EquipmentCategory.BasicWeapon }) };
            var config = new EquipmentConfiguration(new EquipmentCatalog(definitions), slots);
            Assert.That(LoadoutRules.Change(config, new LoadoutSnapshot(config.CreateInitialProfile()), LoadoutOperation.Equip,
                "other-primary", "other-basic", out _), Is.EqualTo(LoadoutResult.WeaponLimit));
        }

        [Test]
        public void AOneSlotLayoutWithOnlyAMandatoryBasicWeaponIsPlayable()
        {
            var basis = EquipmentConfiguration.Prototype();
            var config = EquipmentTests.Config(new EquipmentConfiguration(basis.Catalog, new[] { basis.Slots[0] }));
            var service = new ProgressionService(config, new MemoryProgressionStore());
            Assert.That(service.CreateRun(RunConfiguration.Prototype(), out var session), Is.EqualTo(ProgressionResult.Succeeded));
            using (session)
            {
                Assert.That(session.Run.Platform.Modules.Capacity, Is.EqualTo(1));
                Assert.That(session.Run.Platform.Modules.GetAt(0).Kind, Is.EqualTo(ModuleKind.BasicGun));
            }
        }

        [Test]
        public void ConfiguredDuplicateAllowanceUsesUniqueSlotInstanceIdentitiesInTheExistingRack()
        {
            var item = Utility(maximum: 2); var config = Config(Extended(item));
            var service = new ProgressionService(config, new MemoryProgressionStore());
            Assert.That(service.Replace("utility", item.Id), Is.EqualTo(LoadoutResult.Succeeded));
            Assert.That(service.Equip("extra", item.Id), Is.EqualTo(LoadoutResult.Succeeded));
            service.CreateRun(RunConfiguration.Prototype(), out var session);
            using (session)
            {
                Assert.That(session.Run.Platform.Modules.Count, Is.EqualTo(5));
                Assert.That(session.Run.Platform.Modules.GetAt(3).Id, Is.Not.EqualTo(session.Run.Platform.Modules.GetAt(4).Id));
            }
        }

        [Test]
        public void WeaponConfigurationIsSelectedByTheDefinitionAndAdvancedFireConsumesAmmo()
        {
            var template = RunConfiguration.Prototype(); var basis = EquipmentConfiguration.Prototype();
            var definitions = new List<EquipmentDefinition>(basis.Catalog.Definitions);
            definitions[0] = new EquipmentDefinition("basic-cannon", "An arbitrary display name", EquipmentCategory.BasicWeapon,
                ModuleKind.BasicGun, new[] { SlotCategory.PrimaryWeapon }, weaponOverride:
                new WeaponConfig("configured-basic", 10, 0.5, 5, 0, new ClosestTargetStrategy()));
            var config = Config(new EquipmentConfiguration(new EquipmentCatalog(definitions), basis.Slots));
            var service = new ProgressionService(config, new MemoryProgressionStore());
            var withAmmo = new RunConfiguration(template.Platform, template.Level, template.BasicGun, template.MissileLauncher,
                template.Purchases, template.Special, template.Route, template.Spawns, initialAmmunition: 20);
            service.CreateRun(withAmmo, out var session);
            using (session)
            {
                Assert.That(session.Run.Configuration.BasicGun.Id, Is.EqualTo("configured-basic"));
                Assert.That(session.Run.Configuration.BasicGun.Damage, Is.EqualTo(10));
                Assert.That(session.Run.Configuration.BasicGun.AmmunitionPerShot, Is.Zero);
                session.Run.Start(); session.Run.Tick(session.Run.Configuration.Route.InitialTravelSeconds + 0.001);
                Assert.That(session.Run.Platform.AdvancedAmmunition.Count, Is.EqualTo(19));
                Assert.That(session.Run.Platform.AdvancedAmmunition.Capacity, Is.EqualTo(100));
            }
        }

        [Test]
        public void PrototypeCannotConfigureAnUnplayableDefault()
        {
            var basis = EquipmentConfiguration.Prototype();
            Assert.Throws<ArgumentException>(() => new EquipmentConfiguration(basis.Catalog, Array.Empty<EquipmentSlot>()));
        }

        [Test]
        public void PermanentThenEquipmentOrderingRoundsWeaponDamageOnlyAtTheEnd()
        {
            var item = Utility(effects: new[] { new EquipmentModifier(EquipmentStat.MaximumHealth, 20),
                new EquipmentModifier(EquipmentStat.WeaponDamageMultiplier, 1.25m), new EquipmentModifier(EquipmentStat.AmmunitionCapacity, 30),
                new EquipmentModifier(EquipmentStat.DamageReduction, 0.5m), new EquipmentModifier(EquipmentStat.EnergyPerDefeat, 3) });
            var config = Config(Extended(item)); var data = ProgressionData.CreateInitial(config);
            data.UpgradeLevels.Health = 3; data.UpgradeLevels.WeaponDamage = 3;
            data.Equipment.Installations.Add(new EquipmentInstallationData { SlotId = "extra", EquipmentId = item.Id });
            var service = new ProgressionService(config, new MemoryProgressionStore(new JsonProgressionCodec().Encode(data)));
            var original = RunConfiguration.Prototype(); service.CreateRun(original, out var session);
            using (session)
            {
                var run = session.Run.Configuration;
                Assert.That(run.Platform.MaximumHealth, Is.EqualTo(150)); Assert.That(session.Run.Platform.Health.Current, Is.EqualTo(150));
                Assert.That(run.Platform.AmmunitionCapacity, Is.EqualTo(130)); Assert.That(run.Platform.DamageReduction, Is.EqualTo(0.30m));
                Assert.That(run.BasicGun.Damage, Is.EqualTo(12)); // 8 * 1.15 * 1.25 = 11.5; avoid 9 * 1.25 = 11.25.
                Assert.That(run.MissileLauncher.Damage, Is.EqualTo(43)); Assert.That(run.Equipment.EnergyPerDefeat, Is.EqualTo(4));
                Assert.That(session.Run.Platform.Modules.Count, Is.EqualTo(5));
                Assert.That(original.Platform.MaximumHealth, Is.EqualTo(100)); Assert.That(original.BasicGun.Damage, Is.EqualTo(8));
                Assert.Throws<ArgumentException>(() => LoadoutRunBuilder.Build(config, service.Snapshot, run));
                Assert.Throws<ArgumentException>(() => new PermanentStatsProvider(config, service.Snapshot).ApplyTo(run));
            }
        }

        [TestCase(EquipmentStat.MaximumHealth)]
        [TestCase(EquipmentStat.AmmunitionCapacity)]
        [TestCase(EquipmentStat.EnergyPerDefeat)]
        public void ModifierOverflowRejectsRegistrationWithoutSaving(EquipmentStat stat)
        {
            var item = Utility(effects: new[] { new EquipmentModifier(stat, int.MaxValue) });
            var config = Config(Extended(item)); var data = ProgressionData.CreateInitial(config);
            data.Equipment.Installations.Add(new EquipmentInstallationData { SlotId = "extra", EquipmentId = item.Id });
            var store = new MemoryProgressionStore(new JsonProgressionCodec().Encode(data)); string before = store.Read();
            var service = new ProgressionService(config, store);
            Assert.That(service.CreateRun(RunConfiguration.Prototype(), out var session), Is.EqualTo(ProgressionResult.Overflow));
            Assert.That(session, Is.Null); Assert.That(store.Read(), Is.EqualTo(before)); Assert.That(service.Snapshot.RegisteredRuns, Is.Zero);
        }

        [Test]
        public void DecimalMultiplierOverflowIsRejectedBeforeAnyRunIsRegistered()
        {
            var config = Config(Extended(Utility(effects: new[] { new EquipmentModifier(EquipmentStat.WeaponDamageMultiplier, decimal.MaxValue) })));
            var data = ProgressionData.CreateInitial(config); data.UpgradeLevels.WeaponDamage = 1;
            data.Equipment.Installations.Add(new EquipmentInstallationData { SlotId = "extra", EquipmentId = "reactor-extra" });
            var service = new ProgressionService(config, new MemoryProgressionStore(new JsonProgressionCodec().Encode(data)));
            Assert.That(service.CreateRun(RunConfiguration.Prototype(), out _), Is.EqualTo(ProgressionResult.Overflow));
        }

        [Test]
        public void ReactorEnergyUsesTheExistingExactlyOnceEnemyDropAndNeverCredits()
        {
            var service = new ProgressionService(Config(), new MemoryProgressionStore()); service.CreateRun(RunConfiguration.Prototype(), out var session);
            using (session)
            {
                var run = session.Run;
                Assert.That(run.Configuration.Platform.DamageReduction, Is.EqualTo(0.02m));
                Assert.That(run.Platform.Health.TakeDamage(50), Is.EqualTo(49));
                run.Start(); run.Tick(run.Configuration.Route.InitialTravelSeconds);
                var enemy = run.Enemies[0]; enemy.State.Health.TakeDamage(int.MaxValue); run.Tick(0.001);
                Assert.That(run.Platform.Resources.GetBalance(ResourceType.Energy), Is.EqualTo(enemy.State.Definition.EnergyDrop + 1));
                Assert.That(enemy.State.TryCollectDrop(run.Platform.Resources, 1), Is.False);
                Assert.That(service.Snapshot.Credits, Is.Zero);
            }
        }

        [Test]
        public void SecondaryRemovalLeavesBasicFireAndRejectsPointlessAmmoPurchases()
        {
            var service = new ProgressionService(Config(), new MemoryProgressionStore()); service.Unequip("secondary");
            service.CreateRun(RunConfiguration.Prototype(), out var session);
            using (session)
            {
                var run = session.Run; int basic = 0, missile = 0;
                run.ShotFired += shot => { if (shot.Kind == ShotKind.BasicGun) basic++; if (shot.Kind == ShotKind.Missile) missile++; };
                run.Start(); run.Platform.Resources.Collect(ResourceType.Energy, 30);
                Assert.That(run.BuyAmmunition(), Is.EqualTo(PurchaseResult.NoEffect));
                run.Tick(run.Configuration.Route.InitialTravelSeconds + 0.001);
                Assert.That(basic, Is.EqualTo(1)); Assert.That(missile, Is.Zero);
                Assert.That(run.Configuration.BasicGun.AmmunitionPerShot, Is.Zero);
                Assert.That(run.Platform.AdvancedAmmunition.Count, Is.Zero);
                Assert.That(run.Platform.Resources.GetBalance(ResourceType.Energy), Is.EqualTo(30));
            }
        }
    }
}
