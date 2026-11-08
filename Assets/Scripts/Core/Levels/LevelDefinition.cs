using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LastOutpost.Core.Enemies;

namespace LastOutpost.Core.Levels
{
    public sealed class SpawnGroup
    {
        public EnemyDefinition Enemy { get; }
        public int Count { get; }

        public SpawnGroup(EnemyDefinition enemy, int count)
        {
            Enemy = enemy ?? throw new ArgumentNullException(nameof(enemy));
            Count = Guard.Positive(count, nameof(count));
        }
    }

    public sealed class WaveDefinition
    {
        public string Id { get; }
        public bool IsBossEncounter { get; }
        public IReadOnlyList<SpawnGroup> Groups { get; }
        public int EnemyCount { get; }

        public WaveDefinition(string id, bool isBossEncounter, params SpawnGroup[] groups)
        {
            Id = Guard.Identifier(id, nameof(id));
            if (groups == null) throw new ArgumentNullException(nameof(groups));
            if (groups.Length == 0) throw new ArgumentException("A wave must contain enemies.", nameof(groups));
            var copy = new SpawnGroup[groups.Length];
            int total = 0;
            for (int i = 0; i < groups.Length; i++)
            {
                SpawnGroup group = groups[i] ?? throw new ArgumentException("Null spawn group.", nameof(groups));
                if (group.Enemy.IsBoss != isBossEncounter)
                    throw new ArgumentException("Enemy type must match the encounter type.", nameof(groups));
                total = checked(total + group.Count);
                copy[i] = group;
            }
            if (isBossEncounter && total != 1)
                throw new ArgumentException("The prototype final encounter requires exactly one boss.", nameof(groups));
            IsBossEncounter = isBossEncounter;
            EnemyCount = total;
            Groups = new ReadOnlyCollection<SpawnGroup>(copy);
        }
    }

    public sealed class LevelDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        /// <summary>Ordinary waves followed by exactly one boss encounter.</summary>
        public IReadOnlyList<WaveDefinition> Encounters { get; }

        public LevelDefinition(string id, string displayName, params WaveDefinition[] encounters)
        {
            Id = Guard.Identifier(id, nameof(id));
            DisplayName = Guard.Identifier(displayName, nameof(displayName));
            if (encounters == null) throw new ArgumentNullException(nameof(encounters));
            if (encounters.Length < 2) throw new ArgumentException("Ordinary waves and a final boss are required.", nameof(encounters));
            var copy = new WaveDefinition[encounters.Length];
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < encounters.Length; i++)
            {
                WaveDefinition encounter = encounters[i] ?? throw new ArgumentException("Null encounter.", nameof(encounters));
                if (encounter.IsBossEncounter != (i == encounters.Length - 1))
                    throw new ArgumentException("Only the final encounter may be a boss.", nameof(encounters));
                if (!ids.Add(encounter.Id)) throw new ArgumentException("Duplicate encounter ID.", nameof(encounters));
                copy[i] = encounter;
            }
            Encounters = new ReadOnlyCollection<WaveDefinition>(copy);
        }
    }
}
