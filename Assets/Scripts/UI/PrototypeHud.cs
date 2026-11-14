using System;
using LastOutpost.Core.Economy;
using LastOutpost.Core.Progression;
using LastOutpost.Core.Modules;
using LastOutpost.Core.Simulation;
using LastOutpost.Gameplay;
using UnityEngine;

namespace LastOutpost.UI
{
    /// <summary>Minimal IMGUI UI: no Canvas/EventSystem/package dependency. Commands always use the session.</summary>
    public sealed class PrototypeHud : MonoBehaviour
    {
        [SerializeField] private OutpostSession session;
        [SerializeField] private string levelTitle = "Industrial Wasteland";
        [SerializeField] private string introduction = "Industrial Wasteland. Three waves and one boss. Permanent upgrades apply when this run starts.";
        private GUIStyle labelStyle;
        private GUIStyle titleStyle;
        private GUIStyle buttonStyle;
        private GUIStyle boxStyle;
        private string feedback = "";
        private float feedbackUntil;
        private ProgressionMenuBinding progressionBinding;
        private ProgressionMenuView progressionView;
        private Vector2 upgradeScroll;
        private Vector2 loadoutScroll;
        private bool confirmDefaultLoadout;
        public string LastFeedback => feedback;
        public ProgressionMenuView ProgressionView => progressionView;

        public void Configure(OutpostSession owner, string title = null, string intro = null)
        {
            OnDisable();
            session = owner != null ? owner : throw new ArgumentNullException(nameof(owner));
            if (title != null) levelTitle = title;
            if (intro != null) introduction = intro;
            if (isActiveAndEnabled) OnEnable();
        }
        private void OnEnable()
        {
            if (session == null) return;
            session.ProgressionChanged -= Rebind;
            session.ProgressionChanged += Rebind;
            Rebind();
        }
        private void OnDisable()
        {
            if (session != null) session.ProgressionChanged -= Rebind;
            progressionBinding?.Dispose(); progressionBinding = null; progressionView = null;
            confirmDefaultLoadout = false;
        }
        private void Rebind()
        {
            progressionBinding?.Dispose(); progressionBinding = null; progressionView = null;
            if (session.Progression != null)
                progressionBinding = new ProgressionMenuBinding(session.Progression, view => progressionView = view);
        }
        public bool StartRun() => session != null && session.BeginRun();
        public void RestartRun() { feedback = ""; session.RestartRun(); }
        public ProgressionResult BuyPermanent(UpgradeKind kind) => session != null && !session.IsPaused && session.Progression != null
            ? session.Progression.BuyUpgrade(kind) : ProgressionResult.NotInitialized;
        public LoadoutResult ChangeEquipment(LoadoutOperation operation, string slotId = null, string equipmentId = null) =>
            session != null && !session.IsPaused && session.Progression != null
                ? session.Progression.ChangeLoadout(operation, slotId, equipmentId) : LoadoutResult.NotInitialized;
        public bool ReturnToMenu() => session != null && session.Progression != null &&
            session.Progression.ReturnToMenu() == ProgressionResult.Succeeded;
        public PurchaseResult BuyAmmunition() => Report(session != null ? session.BuyAmmunition() : PurchaseResult.RunNotActive);
        public PurchaseResult Repair() => Report(session != null ? session.Repair() : PurchaseResult.RunNotActive);
        public PurchaseResult ActivateSpecial() => Report(session != null ? session.ActivateSpecial() : PurchaseResult.RunNotActive);

        private PurchaseResult Report(PurchaseResult result)
        {
            switch (result)
            {
                case PurchaseResult.Succeeded: feedback = "Ready."; break;
                case PurchaseResult.InsufficientEnergy: feedback = "Not enough Energy."; break;
                case PurchaseResult.PlatformDestroyed: feedback = "Platform destroyed."; break;
                case PurchaseResult.CoolingDown: feedback = "Strike is recharging."; break;
                case PurchaseResult.NoEffect: feedback = "Already full, or no target in range."; break;
                default: feedback = "Start a run to purchase."; break;
            }
            feedbackUntil = Time.unscaledTime + 3;
            return result;
        }

        private void OnGUI()
        {
            if (session == null) return;
            EnsureStyles();
            Rect safe = Screen.safeArea;
            if (safe.width <= 0 || safe.height <= 0) return;
            float scale = safe.width / 540;
            float height = safe.height / scale;
            Matrix4x4 previous = GUI.matrix;
            bool wasEnabled = GUI.enabled;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            GUI.BeginGroup(new Rect(safe.x / scale, (Screen.height - safe.yMax) / scale, 540, height));
            try
            {
                GUI.enabled = !session.IsPaused;
                if (progressionView == null)
                {
                    GUI.Label(new Rect(24, 80, 492, 110), "Permanent progression is not initialized. Check the assigned progression host.", labelStyle);
                    if (GUI.Button(new Rect(24, 210, 492, 56), "RETRY STARTUP", buttonStyle)) session.RetryInitialization();
                    return;
                }
                if (progressionView.Screen == ProgressionScreen.Recovery) { DrawRecovery(height); return; }
                GameRun run = session.Run;
                if (run == null) { DrawBetweenRuns(height); return; }
                DrawHud(run);
                if (run.Status == RunStatus.Victory || run.Status == RunStatus.Defeat) DrawResult(run, height);
                else
                {
                    if (run.Space == null) DrawPurchases(run, height);
                    else DrawSpaceCooldowns(run, height);
                    if (GUI.Button(new Rect(24, 184, 492, 40), "ABANDON TO MENU", buttonStyle)) ReturnToMenu();
                }
            }
            finally
            {
                GUI.EndGroup();
                GUI.matrix = previous;
                GUI.enabled = wasEnabled;
            }
        }

        private void DrawHud(GameRun run)
        {
            GUI.Box(new Rect(12, 12, 516, 164), "", boxStyle);
            GUI.Label(new Rect(24, 20, 492, 28), "LAST OUTPOST  /  " + levelTitle, labelStyle);
            GUI.Label(new Rect(24, 52, 492, 28), "Health " + run.Platform.Health.Current + "/" + run.Platform.Health.Maximum +
                (run.Space == null ? "    Energy " + run.Platform.Resources.GetBalance(ResourceType.Energy) : "    Automatic weapons"), labelStyle);
            GUI.Label(new Rect(24, 84, 492, 28), (run.Space == null ? "Missiles " + run.Platform.AdvancedAmmunition.Count + "/" +
                run.Platform.AdvancedAmmunition.Capacity + "    " : "Weapons " + run.Space.Weapons.Count + "    ") + "Wave " + run.Level.CurrentWave + "/4" +
                (run.Level.BossActive ? "  BOSS" : ""), labelStyle);
            Rect progress = new Rect(24, 124, 492, 28);
            GUI.Box(progress, "", boxStyle);
            Color before = GUI.color;
            GUI.color = new Color(0.2f, 0.85f, 0.65f);
            GUI.Box(new Rect(progress.x, progress.y, progress.width * (float)run.Level.Progress, progress.height), "");
            GUI.color = before;
            GUI.Label(progress, "Level progress " + (run.Level.Progress * 100).ToString("0") + "%", labelStyle);
        }

        private void DrawBetweenRuns(float height)
        {
            GUI.Label(new Rect(24, 18, 492, 45), "LAST OUTPOST", titleStyle);
            GUI.Label(new Rect(24, 68, 492, 60), "Credits " + progressionView.Credits +
                "   Unlocked level " + progressionView.HighestUnlockedLevel + " (only Level 1 available)", labelStyle);
            if (progressionView.Screen == ProgressionScreen.Upgrades)
            {
                float top = 0;
                upgradeScroll = GUI.BeginScrollView(new Rect(24, 136, 492, Mathf.Max(100, height - 304)),
                    upgradeScroll, new Rect(0, 0, 468, 576));
                foreach (UpgradeMenuRow row in progressionView.Upgrades)
                {
                    GUI.Box(new Rect(0, top, 468, 120), "", boxStyle);
                    GUI.Label(new Rect(12, top + 4, 444, 28), row.Name + "  " + row.Level + "/" + row.MaximumLevel, labelStyle);
                    GUI.Label(new Rect(12, top + 32, 444, 28), row.CurrentText + " → " + row.NextText, labelStyle);
                    GUI.enabled = !session.IsPaused && row.CanPurchase;
                    if (GUI.Button(new Rect(12, top + 66, 444, 44), row.IsMaximum ? "MAXIMUM" : "BUY / " + row.Price + " Credits", buttonStyle))
                        BuyPermanent(row.Kind);
                    GUI.enabled = !session.IsPaused;
                    top += 128;
                }
                if (GUI.Button(new Rect(0, top + 4, 468, 48), "BACK", buttonStyle)) session.Progression.ShowUpgrades(false);
                GUI.EndScrollView();
            }
            else if (progressionView.Screen == ProgressionScreen.Loadout) DrawLoadout(height);
            else
            {
                GUI.Label(new Rect(24, 160, 492, 100), introduction, labelStyle);
                if (GUI.Button(new Rect(24, 280, 492, 56), "UPGRADES", buttonStyle)) session.Progression.ShowUpgrades(true);
                if (GUI.Button(new Rect(24, 348, 492, 56), "LOADOUT", buttonStyle)) session.Progression.ShowLoadout(true);
            }
            GUI.Label(new Rect(24, height - 148, 492, 60), progressionView.Message +
                (progressionView.Loadout.Validation == LoadoutResult.Succeeded ? "" : " Loadout: " + progressionView.Loadout.Validation), labelStyle);
            GUI.enabled = !session.IsPaused && progressionView.CanStart;
            if (GUI.Button(new Rect(24, height - 76, 492, 56), "START LEVEL 1", buttonStyle)) StartRun();
        }

        private void DrawLoadout(float height)
        {
            LoadoutMenuView view = progressionView.Loadout;
            float contentHeight = 220;
            foreach (var row in view.Slots) contentHeight += 126 + row.Options.Count * 132;
            if (view.UnknownOwnedIds.Count > 0) contentHeight += 96;
            loadoutScroll = GUI.BeginScrollView(new Rect(24, 136, 492, Mathf.Max(100, height - 304)),
                loadoutScroll, new Rect(0, 0, 468, contentHeight));
            try
            {
                float top = 0;
                foreach (var row in view.Slots)
                {
                    GUI.Label(new Rect(0, top, 468, 66), row.SlotId + " / " + row.Category +
                        (row.Enabled ? "" : " / DISABLED") + (row.Required ? " / REQUIRED" : "") + "\nInstalled: " + row.EquipmentName, labelStyle);
                    GUI.enabled = !session.IsPaused && row.CanUnequip;
                    if (GUI.Button(new Rect(0, top + 70, 468, 44), "UNEQUIP / " + row.UnequipAvailability, buttonStyle))
                        ChangeEquipment(LoadoutOperation.Unequip, row.SlotId);
                    top += 126;
                    foreach (var item in row.Options)
                    {
                        GUI.Label(new Rect(8, top, 452, 80), item.Description, labelStyle);
                        GUI.enabled = !session.IsPaused && item.CanInstall;
                        if (GUI.Button(new Rect(8, top + 84, 452, 44), item.Name + " / " +
                            (item.Owned ? item.Availability.ToString() : "NOT OWNED"), buttonStyle))
                            ChangeEquipment(item.Operation, row.SlotId, item.EquipmentId);
                        top += 132;
                    }
                    GUI.enabled = !session.IsPaused;
                }
                if (view.UnknownOwnedIds.Count > 0)
                {
                    GUI.Label(new Rect(0, top, 468, 88), "Unrecognized owned IDs are retained: " +
                        string.Join(", ", view.UnknownOwnedIds), labelStyle);
                    top += 96;
                }
                GUI.enabled = !session.IsPaused && view.CanRestoreDefaults;
                if (confirmDefaultLoadout)
                {
                    GUI.Label(new Rect(0, top, 468, 58), "Replace ALL slot assignments with defaults? Owned IDs are retained.", labelStyle);
                    if (GUI.Button(new Rect(0, top + 62, 226, 44), "CONFIRM DEFAULTS", buttonStyle))
                    { ChangeEquipment(LoadoutOperation.RestoreDefaults); confirmDefaultLoadout = false; }
                    if (GUI.Button(new Rect(238, top + 62, 230, 44), "CANCEL", buttonStyle)) confirmDefaultLoadout = false;
                }
                else if (GUI.Button(new Rect(0, top, 468, 52), "RESTORE DEFAULT LOADOUT...", buttonStyle)) confirmDefaultLoadout = true;
                GUI.enabled = !session.IsPaused;
                if (GUI.Button(new Rect(0, top + 120, 468, 48), "BACK", buttonStyle))
                { confirmDefaultLoadout = false; session.Progression.ShowLoadout(false); }
            }
            finally { GUI.EndScrollView(); }
        }

        private void DrawRecovery(float height)
        {
            GUI.Label(new Rect(24, 36, 492, 48), "SAVE RECOVERY", titleStyle);
            GUI.Label(new Rect(24, 100, 492, 170), progressionView.Message, labelStyle);
            GUI.Label(new Rect(24, 278, 492, 70), (progressionView.CreditsAreCurrent ? "Saved Credits: " : "Last known Credits: ") + (progressionView.Credits?.ToString() ?? "unavailable") +
                ". A victory reward is permanent only after it is saved.", labelStyle);
            if (progressionView.ConfirmingReset)
            {
                GUI.enabled = !session.IsPaused && !progressionView.IsBusy;
                if (GUI.Button(new Rect(24, 380, 492, 60), "CONFIRM BACKUP AND NEW PROFILE", buttonStyle)) session.Progression.ConfirmNewProfile(true);
                if (GUI.Button(new Rect(24, 454, 492, 56), "CANCEL", buttonStyle)) session.Progression.CancelNewProfile();
                return;
            }
            GUI.enabled = !session.IsPaused && progressionView.CanReload;
            if (GUI.Button(new Rect(24, 380, 492, 56), "RETRY LOADING", buttonStyle)) session.Progression.RetryLoad();
            GUI.enabled = !session.IsPaused && progressionView.CanRetrySave;
            if (GUI.Button(new Rect(24, 448, 492, 56), "RETRY SAVING RUN OUTCOME", buttonStyle)) session.Progression.RetrySave();
            GUI.enabled = !session.IsPaused && !progressionView.IsBusy;
            if (GUI.Button(new Rect(24, 540, 492, 56), "REQUEST NEW PROFILE...", buttonStyle)) session.Progression.RequestNewProfile();
        }

        private void DrawResult(GameRun run, float height)
        {
            float top = height * 0.38f;
            GUI.Box(new Rect(24, top, 492, 220), "", boxStyle);
            GUI.Label(new Rect(40, top + 16, 460, 48), run.Status == RunStatus.Victory ? "VICTORY" : run.Space != null ? "SHIP LOST" : "OUTPOST LOST", titleStyle);
            GUI.Label(new Rect(40, top + 72, 460, 60), "Saved reward: " + (progressionView.CommittedReward?.ToString() ?? "PENDING") +
                " Credits. Balance: " + progressionView.Credits, labelStyle);
            if (GUI.Button(new Rect(40, top + 146, 460, 54), "BETWEEN-RUN MENU", buttonStyle)) ReturnToMenu();
        }

        private void DrawPurchases(GameRun run, float height)
        {
            var config = run.Configuration;
            float top = height - 218;
            GUI.Label(new Rect(24, top - 40, 492, 34), Time.unscaledTime <= feedbackUntil ? feedback : "", labelStyle);
            GUI.enabled = !session.IsPaused;
            if (GUI.Button(new Rect(24, top, 492, 56), "Missiles +" + config.Purchases.AmmunitionAmount + "  /  " +
                config.Purchases.AmmunitionCost + " Energy", buttonStyle)) BuyAmmunition();
            if (GUI.Button(new Rect(24, top + 64, 492, 56), "Repair +" + config.Purchases.RepairHealth + "  /  " +
                config.Purchases.RepairCost + " Energy", buttonStyle)) Repair();
            if (GUI.Button(new Rect(24, top + 128, 492, 56), "Special strike  /  " + config.Special.EnergyCost + " Energy", buttonStyle)) ActivateSpecial();
        }

        private void DrawSpaceCooldowns(GameRun run, float height)
        {
            float panelHeight = 42 + run.Space.Weapons.Count * 34;
            float top = height - panelHeight - 12;
            GUI.Box(new Rect(12, top - 8, 516, panelHeight + 8), "", boxStyle);
            int row = 0;
            foreach (var weapon in run.Space.Weapons)
            {
                double remaining = weapon.Remaining(run.ElapsedSeconds);
                GUI.Label(new Rect(24, top + row * 34, 492, 32), weapon.InstanceId + "  " +
                    (!weapon.Enabled ? "DISABLED" : remaining <= 0.001 ? "READY" : remaining.ToString("0.00") + " s"), labelStyle);
                row++;
            }
            GUI.Label(new Rect(24, top + row * 34 + 4, 492, 28), "Automatic fire / no Energy or ammunition cost", labelStyle);
        }

        private void EnsureStyles()
        {
            if (labelStyle != null) return;
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 19, wordWrap = true };
            titleStyle = new GUIStyle(labelStyle) { fontSize = 30, fontStyle = FontStyle.Bold };
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 22, wordWrap = true };
            boxStyle = new GUIStyle(GUI.skin.box);
        }
    }
}
