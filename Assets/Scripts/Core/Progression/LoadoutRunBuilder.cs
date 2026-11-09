using System;
using System.Collections.Generic;
using LastOutpost.Core.Modules;
using LastOutpost.Core.Platform;
using LastOutpost.Core.Simulation;

namespace LastOutpost.Core.Progression
{
    /// <summary>Base -> permanent snapshot -> equipment -> clamp/round once -> immutable run config.</summary>
    public static class LoadoutRunBuilder
    {
        public static RunConfiguration Build(ProgressionConfiguration config, ProgressionSnapshot profile, RunConfiguration template)
        {
            if (config == null || profile == null || template == null) throw new ArgumentNullException(nameof(config));
            if (template.Equipment != null) throw new ArgumentException("Use the original template; equipment is already applied.");
            var layout = config.Equipment;
            if (LoadoutRules.Validate(layout, profile.Loadout.ToData()) != LoadoutResult.Succeeded)
                throw new ArgumentException("The selected loadout is not combat-ready.");
            var permanent = new PermanentStatsProvider(config, profile);
            PlatformStats basis = permanent.GetStartingStats();
            decimal health = basis.MaximumHealth, ammo = basis.AmmunitionCapacity, armor = basis.DamageReduction;
            decimal multiplier = permanent.WeaponMultiplier, energy = 0;
            var basic = template.BasicGun; var advanced = template.MissileLauncher;
            var installed = new List<ResolvedInstallation>();
            checked
            {
                // Configured slot order gives deterministic modifier order, never dictionary iteration.
                for (int index = 0; index < layout.Slots.Count; index++)
                {
                    var slot = layout.Slots[index];
                    var definition = layout.Catalog.Get(profile.Loadout.At(slot.Id));
                    if (definition == null) continue;
                    installed.Add(new ResolvedInstallation(slot.Id, index, definition));
                    if (definition.WeaponOverride != null)
                    {
                        if (definition.Mechanism == ModuleKind.BasicGun) basic = definition.WeaponOverride;
                        else advanced = definition.WeaponOverride;
                    }
                    foreach (var effect in definition.Modifiers)
                        switch (effect.Stat)
                        {
                            case EquipmentStat.MaximumHealth: health += effect.Value; break;
                            case EquipmentStat.DamageReduction: armor += effect.Value; break;
                            case EquipmentStat.WeaponDamageMultiplier: multiplier *= effect.Value; break;
                            case EquipmentStat.AmmunitionCapacity: ammo += effect.Value; break;
                            case EquipmentStat.EnergyPerDefeat: energy += effect.Value; break;
                        }
                }
                var stats = new PlatformStats((int)health, (int)ammo, layout.Slots.Count,
                    Math.Min(config.MaximumDamageReduction, armor));
                int bonus = (int)energy;
                // Fail before registration, rather than overflowing an individual reward mid-combat.
                foreach (var encounter in template.Level.Encounters)
                    foreach (var group in encounter.Groups) _ = group.Enemy.EnergyDrop + bonus;
                return new RunConfiguration(stats, template.Level, PermanentStatsProvider.Scale(basic, multiplier),
                    PermanentStatsProvider.Scale(advanced, multiplier), template.Purchases, template.Special,
                    template.Route, template.Spawns, Math.Min(template.InitialAmmunition, stats.AmmunitionCapacity),
                    new RunLoadout(layout.Slots.Count, bonus, installed));
            }
        }
    }
}
