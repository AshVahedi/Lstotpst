using System;
using LastOutpost.Core.Economy;
using LastOutpost.Core.Simulation;
using UnityEngine;

namespace LastOutpost.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class OutpostSession : MonoBehaviour
    {
        [SerializeField] private PrototypeSettings settings;
        [SerializeField, Tooltip("Advance with FixedUpdate. Disable for controlled test/tool stepping.")]
        private bool automaticTick = true;
        public GameRun Run { get; private set; }
        public bool IsPaused { get; private set; }
        public event Action<GameRun> RunCreated;
        public event Action<RunStatus> Finished;

        private void Awake()
        {
            if (settings != null) ResetRun();
        }

        private void FixedUpdate()
        {
            if (automaticTick) StepSimulation(Time.fixedDeltaTime);
        }

        private void OnApplicationPause(bool paused) => IsPaused = paused;

        private void OnDestroy()
        {
            if (Run != null) Run.Finished -= OnFinished;
        }

        public void Configure(PrototypeSettings configuration, bool advanceAutomatically = true)
        {
            settings = configuration != null ? configuration : throw new ArgumentNullException(nameof(configuration));
            automaticTick = advanceAutomatically;
            if (Application.isPlaying) ResetRun();
        }

        public void ResetRun()
        {
            if (settings == null) throw new InvalidOperationException("Assign PrototypeSettings before starting Last Outpost.");
            // Build first so invalid inspector data cannot discard a valid existing run.
            var next = new GameRun(settings.Build());
            if (Run != null) Run.Finished -= OnFinished;
            Run = next;
            Run.Finished += OnFinished;
            RunCreated?.Invoke(Run);
        }

        public bool BeginRun()
        {
            if (Run == null) ResetRun();
            return Run.Start();
        }

        public void RestartRun()
        {
            ResetRun();
            BeginRun();
        }

        public void StepSimulation(float deltaSeconds)
        {
            if (!IsPaused) Run?.Tick(deltaSeconds);
        }

        public PurchaseResult BuyAmmunition() => CanCommand ? Run.BuyAmmunition() : PurchaseResult.RunNotActive;
        public PurchaseResult Repair() => CanCommand ? Run.Repair() : PurchaseResult.RunNotActive;
        public PurchaseResult ActivateSpecial() => CanCommand ? Run.ActivateSpecial() : PurchaseResult.RunNotActive;
        private bool CanCommand => Run != null && !IsPaused;
        private void OnFinished(RunStatus result) => Finished?.Invoke(result);
    }
}
