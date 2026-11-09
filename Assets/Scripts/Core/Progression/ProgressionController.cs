using System;
using System.IO;
using System.Runtime.Serialization;

using LastOutpost.Core.Simulation;

namespace LastOutpost.Core.Progression
{
    public enum ProgressionScreen { Loading, Start, Upgrades, Playing, Victory, Defeat, Recovery, Closed }

    /// <summary>
    /// Application-lifetime coordinator, independent of scene/UI lifetimes. Call on the gameplay
    /// thread. Synchronous operations reject overlap/reentrant commands; they never queue purchases.
    /// </summary>
    public sealed class ProgressionController : IDisposable
    {
        private readonly ProgressionConfiguration configuration;
        private readonly IProgressionStore store;
        private readonly IProgressionCodec codec;
        private readonly IRunIdGenerator ids;
        private ProgressionService service;
        private ProgressionRunSession session;
        private bool attempted, busy, publishing, disposed;
        private string resetToken;
        private bool pendingAbandon;
        private bool confirmedResetPending;
        public ProgressionScreen Screen { get; private set; } = ProgressionScreen.Loading;
        public ProgressionMenuView View { get; private set; }
        public GameRun Run => session?.Run;
        public bool IsBusy => busy;
        public bool IsInitialized => service != null;
        public bool RequiresReload => service == null || service.RequiresReload;
        public ProgressionResult LastResult { get; private set; } = ProgressionResult.NotInitialized;
        public string Message { get; private set; } = "Loading permanent progression.";
        public string LastBackup { get; private set; }
        public event Action Changed;
        public event Action<GameRun> RunChanged;
        public event Action<RunStatus> Finished;

        public ProgressionController(ProgressionConfiguration configuration, IProgressionStore store,
            IProgressionCodec codec = null, IRunIdGenerator ids = null)
        {
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.codec = codec ?? new JsonProgressionCodec();
            this.ids = ids;
            Refresh();
        }

        public ProgressionResult Initialize()
        {
            if (disposed) return ProgressionResult.NotInitialized;
            if (attempted) return IsInitialized && Screen != ProgressionScreen.Recovery ? ProgressionResult.Succeeded : LastResult;
            if (!Enter()) return ProgressionResult.Busy;
            try { attempted = true; return Load(); }
            finally { Leave(); }
        }

        public ProgressionResult RetryLoad()
        {
            if (!Enter()) return ProgressionResult.Busy;
            try
            {
                if (Screen != ProgressionScreen.Recovery) return ProgressionResult.NotBetweenRuns;
                resetToken = null;
                return Load();
            }
            finally { Leave(); }
        }

        private ProgressionResult Load()
        {
            try
            {
                if (service == null) service = new ProgressionService(configuration, store, codec, ids);
                else service.Reload();
                if (session != null && confirmedResetPending && service.GetRunReceipt(Run.RunId) == null) ClearRun();
                confirmedResetPending = false;
                if (session == null) { Screen = ProgressionScreen.Start; Message = "Progression ready."; }
                else ResolveOutcome(); // A reload never invents a reward or restarts a run.
                return LastResult = ProgressionResult.Succeeded;
            }
            catch (ProgressionLoadException error)
            {
                Screen = ProgressionScreen.Recovery;
                Message = LoadMessage(error.Reason);
                return LastResult = ProgressionResult.PersistenceFailed;
            }
            catch (Exception error) when (error is SerializationException || error is InvalidDataContractException || error is NotSupportedException)
            {
                Recover("The save reader is unavailable on this runtime. Original files are preserved.");
                return LastResult = ProgressionResult.PersistenceFailed;
            }
        }

        public bool ShowUpgrades(bool visible)
        {
            if (!CanUseMenu) return false;
            Screen = visible ? ProgressionScreen.Upgrades : ProgressionScreen.Start;
            Refresh();
            return true;
        }

        private bool CanUseMenu => !disposed && !busy && !publishing && service != null && !service.RequiresReload &&
            (Screen == ProgressionScreen.Start || Screen == ProgressionScreen.Upgrades);

        public ProgressionResult BuyUpgrade(UpgradeKind kind)
        {
            if (!CanUseMenu) return busy || publishing ? ProgressionResult.Busy : ProgressionResult.NotBetweenRuns;
            if (!Enter()) return ProgressionResult.Busy;
            try
            {
                LastResult = service.BuyUpgrade(kind);
                if (SaveProblem(LastResult)) Recover("Upgrade was not confirmed. Reload before making another purchase.");
                else Message = LastResult == ProgressionResult.Succeeded ? "Upgrade saved." : "Upgrade unavailable: " + LastResult;
                return LastResult;
            }
            finally { Leave(); }
        }

        public ProgressionResult StartRun(RunConfiguration originalTemplate)
        {
            if (!CanUseMenu) return busy || publishing ? ProgressionResult.Busy : ProgressionResult.NotBetweenRuns;
            if (!Enter()) return ProgressionResult.Busy;
            try
            {
                LastResult = service.CreateRun(originalTemplate, out ProgressionRunSession created);
                if (LastResult != ProgressionResult.Succeeded)
                {
                    if (SaveProblem(LastResult)) Recover("Run registration was not confirmed. Reload before starting again.");
                    else Message = "Run could not start: " + LastResult;
                    return LastResult;
                }
                session = created;
                pendingAbandon = false;
                // ProgressionRunSession subscribed first, so durable outcome processing precedes UI notification.
                Run.Finished += OnFinished;
                Run.Start();
                Screen = ProgressionScreen.Playing;
                Message = "";
                RunChanged?.Invoke(Run);
                return LastResult;
            }
            finally { Leave(); }
        }

        public void Advance(double seconds)
        {
            if (disposed || busy || Screen != ProgressionScreen.Playing) return;
            busy = true; // No per-frame UI allocation; only terminal transitions refresh the view.
            try { Run.Tick(seconds); }
            finally { busy = false; if (Screen != ProgressionScreen.Playing) Refresh(); }
        }

        private void OnFinished(RunStatus status)
        {
            LastResult = session.LastCompletionResult;
            ResolveOutcome();
            Refresh();
            Finished?.Invoke(status);
        }

        private void ResolveOutcome()
        {
            RunReceipt receipt = service.GetRunReceipt(Run.RunId);
            if (!service.RequiresReload && receipt != null && receipt.IsTerminal)
            {
                if (pendingAbandon) { ClearRun(); Screen = ProgressionScreen.Start; Message = "Run abandoned."; }
                else
                {
                    Screen = Run.Status == RunStatus.Victory ? ProgressionScreen.Victory : ProgressionScreen.Defeat;
                    Message = receipt.Outcome == SavedRunOutcome.Victory ? "Victory reward saved." : "Run ended. No victory Credits awarded.";
                }
            }
            else Recover("Run outcome is not saved. Keep this session open; reload if required, then retry saving.");
        }

        public ProgressionResult ReturnToMenu()
        {
            if (!Enter()) return ProgressionResult.Busy;
            try
            {
                if (session == null)
                {
                    if (Screen != ProgressionScreen.Start && Screen != ProgressionScreen.Upgrades) return ProgressionResult.NotInitialized;
                    Screen = ProgressionScreen.Start;
                    return ProgressionResult.Succeeded;
                }
                if (Screen == ProgressionScreen.Recovery) return ProgressionResult.ReloadRequired;
                if (Run.Status == RunStatus.Playing || Run.Status == RunStatus.StartScreen)
                {
                    pendingAbandon = true;
                    LastResult = session.Abandon();
                    if (!Accepted(LastResult)) { Recover("Abandon was not saved. Reload, then retry saving."); return LastResult; }
                }
                else if (service.RequiresReload || service.GetRunReceipt(Run.RunId)?.IsTerminal != true)
                    return ProgressionResult.ReloadRequired;
                ClearRun();
                Screen = ProgressionScreen.Start;
                Message = "Progression ready.";
                return LastResult = ProgressionResult.Succeeded;
            }
            finally { Leave(); }
        }

        public ProgressionResult RetrySave()
        {
            if (!Enter()) return ProgressionResult.Busy;
            try
            {
                if (Screen != ProgressionScreen.Recovery || session == null) return ProgressionResult.RunNotFinished;
                if (service.RequiresReload) return ProgressionResult.ReloadRequired;
                LastResult = pendingAbandon ? session.Abandon() : session.RetryCompletion();
                ResolveOutcome();
                return LastResult;
            }
            finally { Leave(); }
        }

        public ProgressionResult RequestNewProfile()
        {
            if (!Enter()) return ProgressionResult.Busy;
            try
            {
                if (Screen != ProgressionScreen.Recovery || !(store is IProgressionRecoveryStore recovery))
                    return ProgressionResult.RecoveryNotSupported;
                try { resetToken = recovery.CaptureRecoveryToken(); }
                catch (Exception error) when (StorageFailure(error)) { Message = "Cannot prepare recovery. Original files remain unchanged."; return ProgressionResult.PersistenceFailed; }
                Message = "Confirm a new profile? Existing files will be backed up. Pending run rewards will be discarded.";
                return LastResult = ProgressionResult.ConfirmationRequired;
            }
            finally { Leave(); }
        }

        public void CancelNewProfile() { if (busy || disposed) return; resetToken = null; Message = "Recovery cancelled. Original profile retained."; Refresh(); }

        public ProgressionResult ConfirmNewProfile(bool confirmed)
        {
            if (!confirmed || resetToken == null || Screen != ProgressionScreen.Recovery) return ProgressionResult.ConfirmationRequired;
            if (!Enter()) return ProgressionResult.Busy;
            try
            {
                string replacement = codec.Encode(new ProgressionData());
                string backup = null;
                confirmedResetPending = true;
                try
                {
                    var recovery = (IProgressionRecoveryStore)store;
                    LastResult = recovery.ResetWithBackup(resetToken, replacement, out backup) == StoreCommitResult.Committed
                        ? ProgressionResult.Succeeded : ProgressionResult.Conflict;
                    if (LastResult == ProgressionResult.Conflict)
                    { confirmedResetPending = false; resetToken = null; Message = "Save changed after confirmation was requested. Reload; nothing was replaced."; return LastResult; }
                }
                catch (Exception error) when (StorageFailure(error))
                {
                    // A lost acknowledgment after replacement can still mean the reset committed.
                    try { if (store.Read() != replacement) throw new IOException("Reset not confirmed."); }
                    catch (Exception readError) when (StorageFailure(readError))
                    { resetToken = null; Message = "Reset could not be confirmed. Files/backups are preserved; retry loading."; return LastResult = ProgressionResult.PersistenceUncertain; }
                }
                finally { if (backup != null) LastBackup = backup; }
                ClearRun();
                resetToken = null;
                return Load();
            }
            catch (Exception error) when (error is SerializationException || error is InvalidDataContractException || error is NotSupportedException)
            { Message = "New profile could not be serialized. Nothing was replaced."; return LastResult = ProgressionResult.PersistenceFailed; }
            finally { Leave(); }
        }

        private void ClearRun()
        {
            if (session == null) return;
            Run.Finished -= OnFinished;
            session.Dispose();
            session = null;
            pendingAbandon = false;
            RunChanged?.Invoke(null);
        }

        private static bool Accepted(ProgressionResult result) => result == ProgressionResult.Succeeded || result == ProgressionResult.AlreadyProcessed;
        private static bool SaveProblem(ProgressionResult result) => result == ProgressionResult.PersistenceFailed ||
            result == ProgressionResult.PersistenceUncertain || result == ProgressionResult.Conflict ||
            result == ProgressionResult.ReloadRequired || result == ProgressionResult.Overflow;
        private static bool StorageFailure(Exception error) => error is IOException || error is UnauthorizedAccessException ||
            error is NotSupportedException || error is ProgressionLoadException;
        private void Recover(string message) { Screen = ProgressionScreen.Recovery; Message = message; }
        private bool Enter() { if (busy || publishing || disposed) return false; busy = true; Refresh(); return true; }
        private void Leave() { busy = false; Refresh(); }
        private void Refresh()
        {
            View = new ProgressionMenuView(configuration, service?.Snapshot, Screen, busy, service?.RequiresReload ?? true,
                resetToken != null, session != null, session == null || service == null ? null : service.GetRunReceipt(Run.RunId), Message);
            if (publishing) return;
            publishing = true;
            try { Changed?.Invoke(); }
            finally { publishing = false; }
        }

        private static string LoadMessage(ProgressionLoadError reason)
        {
            switch (reason)
            {
                case ProgressionLoadError.Corrupt: return "Save is damaged. Original files are preserved. Retry loading or confirm a backed-up new profile.";
                case ProgressionLoadError.UnsupportedSchema: return "Save needs another game version. Original files are preserved.";
                case ProgressionLoadError.InterruptedWrite: return "An interrupted save was found. Original files are preserved; choose recovery explicitly.";
                case ProgressionLoadError.InvalidValues: return "Save contains invalid values. Original files are preserved.";
                default: return "Storage is unavailable. Retry loading; no profile was reset.";
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            ClearRun();
            disposed = true;
            Screen = ProgressionScreen.Closed;
            Changed = null; RunChanged = null; Finished = null;
            Refresh();
        }
    }
}
