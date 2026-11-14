using System;
using LastOutpost.Core.Economy;
using LastOutpost.Core.Progression;
using LastOutpost.Core.Simulation;
using UnityEngine;

namespace LastOutpost.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class OutpostSession : MonoBehaviour
    {
        [SerializeField] private PrototypeSettings settings;
        [SerializeField] private ProgressionHost progressionHost;
        [SerializeField] private SpaceCombatSettings spaceCombatSettings;
        [SerializeField] private SpaceShipSettings spaceShipSettings;
        [SerializeField, Tooltip("Advance with FixedUpdate. Disable for controlled test/tool stepping.")]
        private bool automaticTick = true;
        private ProgressionController injectedController;
        private ProgressionController boundController;
        private ProgressionHost boundHost;
        public ProgressionController Progression => boundController;
        public GameRun Run => boundController?.Run;
        public bool UsesSpaceCombat => spaceCombatSettings != null;
        public bool IsPaused { get; private set; }
        public event Action<GameRun> RunCreated;
        public event Action<RunStatus> Finished;
        public event Action ProgressionChanged;

        private void OnEnable() { if (settings != null) Bind(); }
        private void OnDisable() => ReleaseProgressionBinding();
        private void OnDestroy() => ReleaseProgressionBinding();
        private void FixedUpdate() { if (automaticTick) StepSimulation(Time.fixedDeltaTime); }
        private void OnApplicationPause(bool paused) => IsPaused = paused;

        public void Configure(PrototypeSettings configuration, ProgressionHost owner, bool advanceAutomatically = true)
        {
            ReleaseProgressionBinding();
            settings = configuration != null ? configuration : throw new ArgumentNullException(nameof(configuration));
            progressionHost = owner != null ? owner : throw new ArgumentNullException(nameof(owner));
            injectedController = null;
            automaticTick = advanceAutomatically;
            if (Application.isPlaying && isActiveAndEnabled) Bind();
        }

        /// <summary>Explicit same-owner injection for tests/hosts; never creates a second economy.</summary>
        public void Configure(PrototypeSettings configuration, ProgressionController owner, bool advanceAutomatically = true)
        {
            ReleaseProgressionBinding();
            settings = configuration != null ? configuration : throw new ArgumentNullException(nameof(configuration));
            injectedController = owner ?? throw new ArgumentNullException(nameof(owner));
            progressionHost = null;
            automaticTick = advanceAutomatically;
            if (Application.isPlaying && isActiveAndEnabled) Bind();
        }

        private void Bind()
        {
            ReleaseProgressionBinding();
            boundHost = injectedController == null ? ProgressionHost.Resolve(progressionHost) : null;
            boundController = injectedController ?? boundHost?.Controller;
            if (boundController == null) { ProgressionChanged?.Invoke(); return; }
            boundHost?.ClaimDriver(this);
            boundController.RunChanged += ForwardRun;
            boundController.Finished += ForwardFinished;
            ForwardRun(Run);
            ProgressionChanged?.Invoke();
        }

        public void ConfigureSpaceCombat(SpaceCombatSettings combat, SpaceShipSettings geometry)
        {
            if (Run != null) throw new InvalidOperationException("Configure space combat before starting a run.");
            if (combat == null || geometry == null) throw new ArgumentNullException(nameof(combat));
            combat.Build(geometry); // Validate without registering a run or touching a save.
            spaceCombatSettings = combat; spaceShipSettings = geometry;
        }

        private RunConfiguration BuildTemplate()
        {
            RunConfiguration template = settings.Build();
            return spaceCombatSettings == null ? template : template.WithSpaceCombat(spaceCombatSettings.Build(spaceShipSettings));
        }

        public void ReleaseProgressionBinding()
        {
            if (boundController != null)
            {
                boundController.RunChanged -= ForwardRun;
                boundController.Finished -= ForwardFinished;
            }
            boundHost?.ReleaseDriver(this);
            boundHost = null;
            boundController = null;
            RunCreated?.Invoke(null);
            ProgressionChanged?.Invoke();
        }

        public void RetryInitialization() { if (boundController == null) Bind(); else boundController.RetryLoad(); }
        public bool BeginRun()
        {
            if (settings == null || boundController == null || IsPaused) return false;
            return boundController.StartRun(BuildTemplate()) == ProgressionResult.Succeeded;
        }

        // Validate first, preserving an existing run if Inspector configuration is invalid.
        public void ResetRun()
        {
            if (settings == null) throw new InvalidOperationException("Assign PrototypeSettings before starting Last Outpost.");
            BuildTemplate();
            boundController?.ReturnToMenu();
        }

        public void RestartRun()
        {
            if (settings == null || boundController == null || IsPaused) return;
            RunConfiguration template = BuildTemplate();
            if (boundController.ReturnToMenu() == ProgressionResult.Succeeded) boundController.StartRun(template);
        }

        public void StepSimulation(float deltaSeconds)
        {
            if (!IsPaused && (boundHost == null || boundHost.CanDrive(this))) boundController?.Advance(deltaSeconds);
        }

        public PurchaseResult BuyAmmunition() => CanCommand ? Run.BuyAmmunition() : PurchaseResult.RunNotActive;
        public PurchaseResult Repair() => CanCommand ? Run.Repair() : PurchaseResult.RunNotActive;
        public PurchaseResult ActivateSpecial() => CanCommand ? Run.ActivateSpecial() : PurchaseResult.RunNotActive;
        private bool CanCommand => boundController != null && boundController.Screen == ProgressionScreen.Playing &&
            !boundController.IsBusy && Run != null && !IsPaused;
        private void ForwardRun(GameRun run) => RunCreated?.Invoke(run);
        private void ForwardFinished(RunStatus status) => Finished?.Invoke(status);
    }
}
