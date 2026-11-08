using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using LastOutpost.Core.Simulation;

namespace LastOutpost.Core.Progression
{
    public enum ProgressionLoadError { Corrupt, UnsupportedSchema, InvalidValues, InterruptedWrite, StorageUnavailable }

    public sealed class ProgressionLoadException : Exception
    {
        public ProgressionLoadError Reason { get; }
        public ProgressionLoadException(ProgressionLoadError reason, string message, Exception inner = null) : base(message, inner) => Reason = reason;
    }

    // DTOs contain permanent data only. Public fields support the standard serializer and AOT inspection.
    [DataContract]
    public sealed class UpgradeLevelsData
    {
        [DataMember(Name = "health", IsRequired = true)] public int Health;
        [DataMember(Name = "armor", IsRequired = true)] public int Armor;
        [DataMember(Name = "weaponDamage", IsRequired = true)] public int WeaponDamage;
        [DataMember(Name = "ammunitionCapacity", IsRequired = true)] public int AmmunitionCapacity;

        public int Get(UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.MaximumHealth: return Health;
                case UpgradeKind.Armor: return Armor;
                case UpgradeKind.WeaponDamage: return WeaponDamage;
                case UpgradeKind.AmmunitionCapacity: return AmmunitionCapacity;
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        internal void Increment(UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.MaximumHealth: Health++; break;
                case UpgradeKind.Armor: Armor++; break;
                case UpgradeKind.WeaponDamage: WeaponDamage++; break;
                case UpgradeKind.AmmunitionCapacity: AmmunitionCapacity++; break;
                default: throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }
        internal UpgradeLevelsData Copy() => (UpgradeLevelsData)MemberwiseClone();
    }

    public enum SavedRunOutcome { Active = 0, Victory = 1, Defeat = 2, Abandoned = 3 }

    [DataContract]
    public sealed class RunRecordData
    {
        [DataMember(Name = "runId", IsRequired = true)] public string RunId;
        [DataMember(Name = "levelId", IsRequired = true)] public string LevelId;
        [DataMember(Name = "levelNumber", IsRequired = true)] public int LevelNumber;
        // Snapshot at registration: a later balance change must not change this run's entitlement.
        [DataMember(Name = "victoryCredits", IsRequired = true)] public long VictoryCredits;
        [DataMember(Name = "outcome", IsRequired = true)] public SavedRunOutcome Outcome;
        internal RunRecordData Copy() => (RunRecordData)MemberwiseClone();
    }

    [DataContract]
    public sealed class ProgressionData
    {
        public const int CurrentSchema = 1;
        [DataMember(Name = "schemaVersion", IsRequired = true)] public int SchemaVersion = CurrentSchema;
        [DataMember(Name = "revision", IsRequired = true)] public long Revision;
        [DataMember(Name = "credits", IsRequired = true)] public long Credits;
        [DataMember(Name = "upgradeLevels", IsRequired = true)] public UpgradeLevelsData UpgradeLevels = new UpgradeLevelsData();
        [DataMember(Name = "highestUnlockedLevel", IsRequired = true)] public int HighestUnlockedLevel = 1;
        [DataMember(Name = "completedLevelIds", IsRequired = true)] public List<string> CompletedLevelIds = new List<string>();
        [DataMember(Name = "runs", IsRequired = true)] public List<RunRecordData> Runs = new List<RunRecordData>();

        internal ProgressionData Copy()
        {
            var copy = (ProgressionData)MemberwiseClone();
            copy.UpgradeLevels = UpgradeLevels.Copy();
            copy.CompletedLevelIds = new List<string>(CompletedLevelIds);
            copy.Runs = Runs.ConvertAll(run => run.Copy());
            return copy;
        }

        internal void Validate(ProgressionConfiguration configuration)
        {
            if (SchemaVersion != CurrentSchema)
                throw new ProgressionLoadException(ProgressionLoadError.UnsupportedSchema, "Unsupported progression schema; original save was preserved.");
            if (Revision < 0 || Credits < 0 || UpgradeLevels == null || HighestUnlockedLevel < 1 || CompletedLevelIds == null || Runs == null)
                Invalid();
            foreach (UpgradeKind kind in Enum.GetValues(typeof(UpgradeKind)))
                if (UpgradeLevels.Get(kind) < 0 || UpgradeLevels.Get(kind) > configuration.Get(kind).MaximumLevel) Invalid();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var victories = new HashSet<string>(StringComparer.Ordinal);
            int unlocked = 1;
            foreach (RunRecordData run in Runs)
            {
                if (run == null || !GameRun.IsValidRunId(run.RunId) || !ids.Add(run.RunId) ||
                    !Enum.IsDefined(typeof(SavedRunOutcome), run.Outcome) || run.LevelNumber < 1 ||
                    run.LevelNumber == int.MaxValue || run.VictoryCredits < 0 || string.IsNullOrWhiteSpace(run.LevelId)) Invalid();
                LevelRewardDefinition reward = configuration.GetReward(run.LevelId);
                if (reward == null || reward.LevelNumber != run.LevelNumber) Invalid();
                if (run.Outcome == SavedRunOutcome.Victory)
                {
                    victories.Add(run.LevelId);
                    unlocked = Math.Max(unlocked, run.LevelNumber + 1);
                }
            }
            var completed = new HashSet<string>(StringComparer.Ordinal);
            foreach (string id in CompletedLevelIds)
                if (string.IsNullOrWhiteSpace(id) || !completed.Add(id)) Invalid();
            if (!completed.SetEquals(victories) || HighestUnlockedLevel != unlocked) Invalid();
        }

        private static void Invalid() => throw new ProgressionLoadException(ProgressionLoadError.InvalidValues,
            "Progression contains invalid or inconsistent values; no automatic reset was performed.");
    }

    public sealed class ProgressionSnapshot
    {
        private readonly UpgradeLevelsData levels;
        public long Credits { get; }
        public long Revision { get; }
        public int HighestUnlockedLevel { get; }
        public IReadOnlyList<string> CompletedLevelIds { get; }
        public int RegisteredRuns { get; }
        public int RewardedRuns { get; }
        internal ProgressionSnapshot(ProgressionData data)
        {
            Credits = data.Credits;
            Revision = data.Revision;
            levels = data.UpgradeLevels.Copy();
            HighestUnlockedLevel = data.HighestUnlockedLevel;
            CompletedLevelIds = new ReadOnlyCollection<string>(new List<string>(data.CompletedLevelIds));
            RegisteredRuns = data.Runs.Count;
            RewardedRuns = data.Runs.FindAll(run => run.Outcome == SavedRunOutcome.Victory).Count;
        }
        public int Level(UpgradeKind kind) => levels.Get(kind);
    }

    public interface IProgressionCodec
    {
        ProgressionData Decode(string json, ProgressionConfiguration configuration);
        string Encode(ProgressionData data);
    }

    /// <summary>Schema dispatch belongs here; no invented legacy migrations. Future schemas are rejected before DTO decoding.</summary>
    public sealed class JsonProgressionCodec : IProgressionCodec
    {
        public const int MaximumCharacters = 8 * 1024 * 1024;
        [DataContract]
        private sealed class SchemaHeader
        {
            [DataMember(Name = "schemaVersion", IsRequired = true)] public int Version = 0;
        }

        private static DataContractJsonSerializer Serializer(Type type) => new DataContractJsonSerializer(type,
            new DataContractJsonSerializerSettings { MaxItemsInObjectGraph = 100000 });

        private static System.Xml.XmlDictionaryReader Reader(byte[] bytes) => JsonReaderWriterFactory.CreateJsonReader(bytes,
            new System.Xml.XmlDictionaryReaderQuotas { MaxDepth = 64, MaxArrayLength = 100000,
                MaxStringContentLength = MaximumCharacters, MaxBytesPerRead = 4096, MaxNameTableCharCount = 16384 });

        private static void RequireSingleObject(string json)
        {
            // DataContractJsonSerializer's reader intentionally stops after the first root.
            // Check its boundary independently, respecting quoted/escaped braces; the standard
            // serializer below still validates JSON syntax, types and required members.
            int depth = 0;
            bool started = false, quoted = false, escaped = false;
            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];
                if (!started)
                {
                    if (WhiteSpace(c)) continue;
                    if (c != '{') throw new SerializationException("Expected a JSON save object.");
                    started = true;
                    depth = 1;
                    continue;
                }
                if (quoted)
                {
                    if (escaped) escaped = false;
                    else if (c == '\\') escaped = true;
                    else if (c == '"') quoted = false;
                    continue;
                }
                if (c == '"') quoted = true;
                else if (c == '{' || c == '[')
                {
                    if (++depth > 64) throw new SerializationException("Save nesting limit exceeded.");
                }
                else if ((c == '}' || c == ']') && --depth == 0)
                {
                    for (int tail = i + 1; tail < json.Length; tail++)
                        if (!WhiteSpace(json[tail])) throw new SerializationException("Unexpected content after save object.");
                    return;
                }
            }
            throw new SerializationException("Incomplete save object.");
        }

        private static bool WhiteSpace(char c) => c == ' ' || c == '\t' || c == '\r' || c == '\n';

        public ProgressionData Decode(string json, ProgressionConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (json == null || json.Length == 0 || json.Length > MaximumCharacters)
                throw new ProgressionLoadException(ProgressionLoadError.Corrupt, "Missing/oversized JSON payload; original save preserved.");
            try
            {
                RequireSingleObject(json);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                using (var headerReader = Reader(bytes))
                {
                    var header = (SchemaHeader)Serializer(typeof(SchemaHeader)).ReadObject(headerReader);
                    if (header == null) throw new SerializationException("Null schema header.");
                    if (header.Version != ProgressionData.CurrentSchema)
                        throw new ProgressionLoadException(ProgressionLoadError.UnsupportedSchema, "Unsupported save schema; original save preserved.");
                    // Bounded standard deserialization after the single-object boundary check.
                    using (var reader = Reader(bytes))
                    {
                        var data = (ProgressionData)Serializer(typeof(ProgressionData)).ReadObject(reader);
                        if (data == null || reader.MoveToContent() != System.Xml.XmlNodeType.None)
                            throw new SerializationException("Expected one complete save document.");
                        data.Validate(configuration);
                        return data;
                    }
                }
            }
            catch (Exception error) when (error is SerializationException || error is System.Xml.XmlException || error is FormatException)
            {
                throw new ProgressionLoadException(ProgressionLoadError.Corrupt, "Invalid progression JSON; original save preserved.", error);
            }
        }

        public string Encode(ProgressionData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            using (var stream = new MemoryStream())
            {
                Serializer(typeof(ProgressionData)).WriteObject(stream, data);
                string json = Encoding.UTF8.GetString(stream.ToArray());
                if (json.Length > MaximumCharacters) throw new SerializationException("Progression save exceeds the documented size limit.");
                return json;
            }
        }
    }
}
