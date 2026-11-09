using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LastOutpost.Core.Progression
{
    /// <summary>Exact required v1 fields. Reading a v1 profile never rewrites its file.</summary>
    [DataContract]
    internal sealed class LegacyProgressionData
    {
        [DataMember(Name = "schemaVersion", IsRequired = true)] public int SchemaVersion = 1;
        [DataMember(Name = "revision", IsRequired = true)] public long Revision = 0;
        [DataMember(Name = "credits", IsRequired = true)] public long Credits = 0;
        [DataMember(Name = "upgradeLevels", IsRequired = true)] public UpgradeLevelsData UpgradeLevels = null;
        [DataMember(Name = "highestUnlockedLevel", IsRequired = true)] public int HighestUnlockedLevel = 1;
        [DataMember(Name = "completedLevelIds", IsRequired = true)] public List<string> CompletedLevelIds = null;
        [DataMember(Name = "runs", IsRequired = true)] public List<RunRecordData> Runs = null;

        internal ProgressionData Migrate(ProgressionConfiguration config)
        {
            if (SchemaVersion != 1) throw new ProgressionLoadException(ProgressionLoadError.UnsupportedSchema, "Expected schema v1.");
            var next = ProgressionData.CreateInitial(config);
            next.Revision = Revision; next.Credits = Credits; next.UpgradeLevels = UpgradeLevels;
            next.HighestUnlockedLevel = HighestUnlockedLevel; next.CompletedLevelIds = CompletedLevelIds; next.Runs = Runs;
            next.Validate(config);
            return next;
        }
    }
}
