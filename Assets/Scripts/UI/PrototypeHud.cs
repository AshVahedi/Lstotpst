using System;
using LastOutpost.Core.Economy;
using LastOutpost.Core.Simulation;
using LastOutpost.Gameplay;
using UnityEngine;

namespace LastOutpost.UI
{
    /// <summary>Minimal IMGUI UI: no Canvas/EventSystem/package dependency. Commands always use the session.</summary>
    public sealed class PrototypeHud : MonoBehaviour
    {
        [SerializeField] private OutpostSession session;
        private GUIStyle labelStyle;
        private GUIStyle titleStyle;
        private GUIStyle buttonStyle;
        private GUIStyle boxStyle;
        private string feedback = "";
        private float feedbackUntil;
        public string LastFeedback => feedback;

        public void Configure(OutpostSession owner) => session = owner != null ? owner : throw new ArgumentNullException(nameof(owner));
        public bool StartRun() => session != null && session.BeginRun();
        public void RestartRun() { feedback = ""; session.RestartRun(); }
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
            if (session == null || session.Run == null) return;
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
                GameRun run = session.Run;
                DrawHud(run);
                if (run.Status == RunStatus.StartScreen) DrawStart(height);
                else if (run.Status == RunStatus.Victory || run.Status == RunStatus.Defeat) DrawResult(run, height);
                else DrawPurchases(run, height);
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
            GUI.Label(new Rect(24, 20, 492, 28), "LAST OUTPOST  /  Industrial Wasteland", labelStyle);
            GUI.Label(new Rect(24, 52, 492, 28), "Health " + run.Platform.Health.Current + "/" + run.Platform.Health.Maximum +
                "    Energy " + run.Platform.Resources.GetBalance(ResourceType.Energy), labelStyle);
            GUI.Label(new Rect(24, 84, 492, 28), "Missiles " + run.Platform.AdvancedAmmunition.Count + "/" +
                run.Platform.AdvancedAmmunition.Capacity + "    Wave " + run.Level.CurrentWave + "/4" +
                (run.Level.BossActive ? "  BOSS" : ""), labelStyle);
            Rect progress = new Rect(24, 124, 492, 28);
            GUI.Box(progress, "", boxStyle);
            Color before = GUI.color;
            GUI.color = new Color(0.2f, 0.85f, 0.65f);
            GUI.Box(new Rect(progress.x, progress.y, progress.width * (float)run.Level.Progress, progress.height), "");
            GUI.color = before;
            GUI.Label(progress, "Level progress " + (run.Level.Progress * 100).ToString("0") + "%", labelStyle);
        }

        private void DrawStart(float height)
        {
            float top = height * 0.35f;
            GUI.Box(new Rect(24, top, 492, 255), "", boxStyle);
            GUI.Label(new Rect(40, top + 12, 460, 45), "LAST OUTPOST", titleStyle);
            GUI.Label(new Rect(40, top + 62, 460, 112),
                "Advance automatically. Defeat three waves and the boss. Spend Energy on missiles, repairs, or a strike.", labelStyle);
            if (GUI.Button(new Rect(40, top + 184, 460, 54), "START RUN", buttonStyle)) StartRun();
        }

        private void DrawResult(GameRun run, float height)
        {
            float top = height * 0.38f;
            GUI.Box(new Rect(24, top, 492, 220), "", boxStyle);
            GUI.Label(new Rect(40, top + 16, 460, 48), run.Status == RunStatus.Victory ? "VICTORY" : "OUTPOST LOST", titleStyle);
            GUI.Label(new Rect(40, top + 72, 460, 52), run.Status == RunStatus.Victory ?
                "Industrial Wasteland cleared." : "Try a different Energy purchase strategy.", labelStyle);
            if (GUI.Button(new Rect(40, top + 146, 460, 54), "RUN AGAIN", buttonStyle)) RestartRun();
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
