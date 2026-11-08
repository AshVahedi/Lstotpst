using System;
using System.IO;
using System.Runtime.Serialization;
using LastOutpost.Core.Levels;
using LastOutpost.Core.Simulation;

namespace LastOutpost.Core.Progression
{
    public enum ProgressionResult
    {
        Succeeded, InvalidUpgrade, MaximumLevel, InsufficientCredits, Overflow,
        PersistenceFailed, PersistenceUncertain, Conflict, ReloadRequired, UnknownLevel, LevelLocked,
        InvalidRun, RunNotFinished, AlreadyProcessed, InvalidRunConfiguration
    }

    public interface IRunIdGenerator { string Create(); }
    public sealed class GuidRunIdGenerator : IRunIdGenerator { public string Create() => Guid.NewGuid().ToString("N"); }

    /// <summary>
    /// Copy/validate/persist/publish transactions over a whole permanent profile. No Energy access.
    /// Memory is published only after CAS succeeds; any uncertain I/O blocks further writes until Reload.
    /// </summary>
    public sealed class ProgressionService
    {
        private readonly object gate = new object();
        private readonly IProgressionStore store;
        private readonly IProgressionCodec codec;
        private readonly IRunIdGenerator ids;
        private ProgressionData data;
        private string persisted;
        public ProgressionConfiguration Configuration { get; }
        public bool RequiresReload { get; private set; }
        public ProgressionSnapshot Snapshot { get { lock (gate) return new ProgressionSnapshot(data); } }

        public ProgressionService(ProgressionConfiguration configuration, IProgressionStore store,
            IProgressionCodec codec = null, IRunIdGenerator ids = null)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.store = store ?? throw new ArgumentNullException(nameof(store));
            this.codec = codec ?? new JsonProgressionCodec();
            this.ids = ids ?? new GuidRunIdGenerator();
            Reload();
        }

        /// <summary>Preserves the last in-memory snapshot if load fails. Never resets over an invalid save.</summary>
        public void Reload()
        {
            lock (gate)
            {
                RequiresReload = true;
                try
                {
                    string loaded = store.Read();
                    ProgressionData next = loaded == null ? new ProgressionData() : codec.Decode(loaded, Configuration);
                    next.Validate(Configuration);
                    data = next;
                    persisted = loaded;
                    RequiresReload = false;
                }
                catch (Exception error) when (IsStorageError(error))
                {
                    throw new ProgressionLoadException(ProgressionLoadError.StorageUnavailable,
                        "Progression could not be loaded; preserve the save and retry explicitly.", error);
                }
            }
        }

        public bool TryGetPrice(UpgradeKind kind, out long credits)
        {
            lock (gate)
            {
                credits = 0;
                UpgradeDefinition definition = Configuration.Get(kind);
                if (definition == null || data.UpgradeLevels.Get(kind) >= definition.MaximumLevel) return false;
                credits = definition.CostAt(data.UpgradeLevels.Get(kind));
                return true;
            }
        }

        public ProgressionResult BuyUpgrade(UpgradeKind kind)
        {
            lock (gate)
            {
                if (RequiresReload) return ProgressionResult.ReloadRequired;
                UpgradeDefinition definition = Configuration.Get(kind);
                if (definition == null) return ProgressionResult.InvalidUpgrade;
                int level = data.UpgradeLevels.Get(kind);
                if (level >= definition.MaximumLevel) return ProgressionResult.MaximumLevel;
                long price = definition.CostAt(level);
                if (data.Credits < price) return ProgressionResult.InsufficientCredits;
                ProgressionData candidate = data.Copy();
                candidate.Credits -= price;
                candidate.UpgradeLevels.Increment(kind);
                return Commit(candidate);
            }
        }

        /// <summary>Persists the unique run registration before returning a usable gameplay session.</summary>
        public ProgressionResult CreateRun(RunConfiguration template, out ProgressionRunSession session)
        {
            lock (gate)
            {
                session = null;
                if (RequiresReload) return ProgressionResult.ReloadRequired;
                if (template == null) return ProgressionResult.InvalidRunConfiguration;
                LevelRewardDefinition reward = Configuration.GetReward(template.Level.Id);
                if (reward == null) return ProgressionResult.UnknownLevel;
                if (reward.LevelNumber > data.HighestUnlockedLevel) return ProgressionResult.LevelLocked;
                string id = ids.Create();
                if (!GameRun.IsValidRunId(id) || data.Runs.Exists(run => run.RunId == id)) return ProgressionResult.InvalidRun;
                GameRun run;
                try
                {
                    var stats = new PermanentStatsProvider(Configuration, new ProgressionSnapshot(data));
                    run = new GameRun(stats.ApplyTo(template), id);
                }
                catch (OverflowException) { return ProgressionResult.Overflow; }
                catch (ArgumentException) { return ProgressionResult.InvalidRunConfiguration; }
                ProgressionData candidate = data.Copy();
                candidate.Runs.Add(new RunRecordData { RunId = id, LevelId = reward.LevelId,
                    LevelNumber = reward.LevelNumber, VictoryCredits = reward.VictoryCredits });
                ProgressionResult result = Commit(candidate);
                if (result == ProgressionResult.Succeeded) session = new ProgressionRunSession(this, run);
                return result;
            }
        }

        public ProgressionResult CompleteRun(GameRun run)
        {
            lock (gate)
            {
                if (RequiresReload) return ProgressionResult.ReloadRequired;
                int index = Find(run);
                if (index < 0) return ProgressionResult.InvalidRun;
                if (data.Runs[index].Outcome != SavedRunOutcome.Active) return ProgressionResult.AlreadyProcessed;
                bool victory = run.Status == RunStatus.Victory && run.Level.State == LevelState.Victory && !run.Platform.Health.IsDead;
                bool defeat = run.Status == RunStatus.Defeat && run.Platform.Health.IsDead;
                if (!victory && !defeat) return ProgressionResult.RunNotFinished;
                ProgressionData candidate = data.Copy();
                RunRecordData record = candidate.Runs[index];
                record.Outcome = victory ? SavedRunOutcome.Victory : SavedRunOutcome.Defeat;
                if (victory)
                {
                    try { candidate.Credits = checked(candidate.Credits + record.VictoryCredits); }
                    catch (OverflowException) { return ProgressionResult.Overflow; }
                    candidate.HighestUnlockedLevel = Math.Max(candidate.HighestUnlockedLevel, record.LevelNumber + 1);
                    if (!candidate.CompletedLevelIds.Contains(record.LevelId)) candidate.CompletedLevelIds.Add(record.LevelId);
                }
                return Commit(candidate);
            }
        }

        public ProgressionResult AbandonRun(GameRun run)
        {
            lock (gate)
            {
                if (RequiresReload) return ProgressionResult.ReloadRequired;
                int index = Find(run);
                if (index < 0) return ProgressionResult.InvalidRun;
                if (data.Runs[index].Outcome != SavedRunOutcome.Active) return ProgressionResult.AlreadyProcessed;
                // A pending victory save must be retried, never converted to an abandoned run.
                if (run.Status == RunStatus.Victory) return ProgressionResult.RunNotFinished;
                ProgressionData candidate = data.Copy();
                candidate.Runs[index].Outcome = SavedRunOutcome.Abandoned;
                return Commit(candidate);
            }
        }

        private int Find(GameRun run) => run == null ? -1 : data.Runs.FindIndex(record =>
            record.RunId == run.RunId && record.LevelId == run.Configuration.Level.Id);

        private ProgressionResult Commit(ProgressionData candidate)
        {
            string replacement;
            try
            {
                candidate.Revision = checked(data.Revision + 1);
                candidate.Validate(Configuration);
                replacement = codec.Encode(candidate);
            }
            catch (OverflowException) { return ProgressionResult.Overflow; }
            catch (Exception error) when (error is SerializationException || error is InvalidDataContractException || error is NotSupportedException)
            {
                // Serialization happens before any store write or in-memory publication.
                return ProgressionResult.PersistenceFailed;
            }
            try
            {
                if (store.Commit(persisted, replacement) == StoreCommitResult.Conflict)
                {
                    RequiresReload = true;
                    return ProgressionResult.Conflict;
                }
                data = candidate;
                persisted = replacement;
                return ProgressionResult.Succeeded;
            }
            catch (Exception error) when (IsStorageError(error) || error is ProgressionLoadException)
            {
                RequiresReload = true;
                // A close/acknowledgment can fail after the atomic rename. Reconcile once
                // instead of telling UI a committed purchase failed and inviting a double buy.
                try
                {
                    string authoritative = store.Read();
                    if (string.Equals(authoritative, replacement, StringComparison.Ordinal))
                    {
                        data = candidate;
                        persisted = replacement;
                        RequiresReload = false;
                        return ProgressionResult.Succeeded;
                    }
                    if (string.Equals(authoritative, persisted, StringComparison.Ordinal)) return ProgressionResult.PersistenceFailed;
                }
                catch (Exception readError) when (IsStorageError(readError) || readError is ProgressionLoadException) { }
                return ProgressionResult.PersistenceUncertain;
            }
        }

        private static bool IsStorageError(Exception error) => error is IOException || error is UnauthorizedAccessException || error is NotSupportedException;
    }

    /// <summary>Unity-neutral lifecycle adapter. UI can retry failed completion without rerunning combat.</summary>
    public sealed class ProgressionRunSession : IDisposable
    {
        private readonly ProgressionService progression;
        public GameRun Run { get; }
        public ProgressionResult LastCompletionResult { get; private set; } = ProgressionResult.RunNotFinished;
        internal ProgressionRunSession(ProgressionService progression, GameRun run)
        {
            this.progression = progression;
            Run = run;
            Run.Finished += OnFinished;
        }
        private void OnFinished(RunStatus status) => LastCompletionResult = progression.CompleteRun(Run);
        public ProgressionResult RetryCompletion() => LastCompletionResult = progression.CompleteRun(Run);
        public ProgressionResult Abandon() => progression.AbandonRun(Run);
        // Dispose only disconnects callbacks. Call Abandon and inspect its result before leaving an active run.
        public void Dispose() => Run.Finished -= OnFinished;
    }
}
