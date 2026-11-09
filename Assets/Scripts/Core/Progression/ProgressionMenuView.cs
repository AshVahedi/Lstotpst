using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace LastOutpost.Core.Progression
{
    /// <summary>Disposable view subscription; has no profile mutation authority.</summary>
    public sealed class ProgressionMenuBinding : IDisposable
    {
        private ProgressionController owner;
        private Action<ProgressionMenuView> render;
        public ProgressionMenuBinding(ProgressionController owner, Action<ProgressionMenuView> render)
        {
            this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
            this.render = render ?? throw new ArgumentNullException(nameof(render));
            owner.Changed += Update;
            Update();
        }
        private void Update() => render?.Invoke(owner.View);
        public void Dispose()
        {
            if (owner == null) return;
            owner.Changed -= Update;
            owner = null; render = null;
        }
    }

    public sealed class UpgradeMenuRow
    {
        public UpgradeKind Kind { get; }
        public string Name { get; }
        public int Level { get; }
        public int MaximumLevel { get; }
        public decimal CurrentEffect { get; }
        public decimal? NextEffect { get; }
        public long? Price { get; }
        public bool CanPurchase { get; }
        public bool IsMaximum => Level == MaximumLevel;
        public string CurrentText => Format(CurrentEffect);
        public string NextText => NextEffect.HasValue ? Format(NextEffect.Value) : "MAXIMUM";

        internal UpgradeMenuRow(ProgressionConfiguration config, ProgressionSnapshot profile, UpgradeKind kind, bool canTransact)
        {
            Kind = kind;
            var definition = config.Get(kind);
            Level = profile.Level(kind); MaximumLevel = definition.MaximumLevel;
            CurrentEffect = config.EffectAt(kind, Level);
            if (!IsMaximum) { NextEffect = config.EffectAt(kind, Level + 1); Price = definition.CostAt(Level); }
            CanPurchase = canTransact && Price.HasValue && profile.Credits >= Price.Value;
            switch (kind)
            {
                case UpgradeKind.MaximumHealth: Name = "Maximum Health"; break;
                case UpgradeKind.Armor: Name = "Armor"; break;
                case UpgradeKind.WeaponDamage: Name = "Weapon Damage"; break;
                default: Name = "Ammunition Capacity"; break;
            }
        }

        private string Format(decimal effect)
        {
            if (Kind == UpgradeKind.Armor) return (effect * 100).ToString("0.##", CultureInfo.InvariantCulture) + "% reduction";
            if (Kind == UpgradeKind.WeaponDamage) return effect.ToString("0.00", CultureInfo.InvariantCulture) + "x damage";
            return effect.ToString("0", CultureInfo.InvariantCulture) + (Kind == UpgradeKind.MaximumHealth ? " HP" : " rounds");
        }
    }

    public sealed class ProgressionMenuView
    {
        public ProgressionScreen Screen { get; }
        public long? Credits { get; }
        public int? HighestUnlockedLevel { get; }
        public long? CommittedReward { get; }
        public bool IsBusy { get; }
        public bool CreditsAreCurrent { get; }
        public bool CanStart { get; }
        public bool CanReload { get; }
        public bool CanRetrySave { get; }
        public bool ConfirmingReset { get; }
        public string Message { get; }
        public IReadOnlyList<UpgradeMenuRow> Upgrades { get; }

        internal ProgressionMenuView(ProgressionConfiguration config, ProgressionSnapshot profile,
            ProgressionScreen screen, bool busy, bool reloadRequired, bool confirmingReset, bool hasRun,
            RunReceipt receipt, string message)
        {
            Screen = screen; IsBusy = busy; ConfirmingReset = confirmingReset; Message = message;
            Credits = profile?.Credits; HighestUnlockedLevel = profile?.HighestUnlockedLevel;
            CreditsAreCurrent = profile != null && !reloadRequired;
            CommittedReward = receipt?.IsTerminal == true ? receipt.AwardedCredits : (long?)null;
            bool between = screen == ProgressionScreen.Start || screen == ProgressionScreen.Upgrades;
            CanStart = between && profile != null && !busy && !reloadRequired;
            CanReload = screen == ProgressionScreen.Recovery && !busy && !confirmingReset;
            CanRetrySave = CanReload && hasRun && !reloadRequired;
            var rows = new List<UpgradeMenuRow>(4);
            if (profile != null)
                foreach (UpgradeKind kind in Enum.GetValues(typeof(UpgradeKind))) rows.Add(new UpgradeMenuRow(config, profile, kind, CanStart));
            Upgrades = new ReadOnlyCollection<UpgradeMenuRow>(rows);
        }
    }
}
