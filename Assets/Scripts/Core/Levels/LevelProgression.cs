using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LastOutpost.Core.Enemies;

namespace LastOutpost.Core.Levels
{
    public enum LevelState { Ready, EncounterActive, BetweenEncounters, Victory, Defeat }

    public sealed class LevelProgression
    {
        private readonly Health platformHealth;
        private readonly List<EnemyState> enemies = new List<EnemyState>();
        private int encounterIndex = -1;
        private int nextEnemyId = 1;
        public LevelDefinition Definition { get; }
        public LevelState State { get; private set; } = LevelState.Ready;
        public IReadOnlyList<EnemyState> ActiveEnemies { get; }
        /// <summary>Zero before start; one-based during play, including the final boss.</summary>
        public int CurrentWave => encounterIndex + 1;
        public bool BossActive => State == LevelState.EncounterActive && Definition.Encounters[encounterIndex].IsBossEncounter;
        public int RemainingEnemies { get; private set; }
        public double Progress { get; private set; }

        public LevelProgression(LevelDefinition definition, Health platformHealth)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            this.platformHealth = platformHealth ?? throw new ArgumentNullException(nameof(platformHealth));
            ActiveEnemies = new ReadOnlyCollection<EnemyState>(enemies);
        }

        /// <summary>The caller controls travel time between encounters; no random spawns.</summary>
        public bool TryBeginNextEncounter()
        {
            Refresh();
            if (State != LevelState.Ready && State != LevelState.BetweenEncounters) return false;
            int nextIndex = encounterIndex + 1;
            WaveDefinition wave = Definition.Encounters[nextIndex];
            // Validate ID space before mutating the roster.
            checked { _ = nextEnemyId + wave.EnemyCount; }
            enemies.Clear();
            for (int i = 0; i < wave.Groups.Count; i++)
                for (int j = 0; j < wave.Groups[i].Count; j++)
                    enemies.Add(new EnemyState(nextEnemyId++, wave.Groups[i].Enemy));
            encounterIndex = nextIndex;
            RemainingEnemies = enemies.Count;
            State = LevelState.EncounterActive;
            return true;
        }

        /// <summary>
        /// Call once after all combat for a simulation tick, before presenting state or spawning.
        /// Defeat wins when platform and boss die on the same tick. Terminal results are latched.
        /// </summary>
        public void Refresh()
        {
            if (State == LevelState.Victory || State == LevelState.Defeat) return;
            int remaining = 0;
            for (int i = 0; i < enemies.Count; i++)
                if (!enemies[i].Health.IsDead) remaining++;
            RemainingEnemies = remaining;
            if (encounterIndex >= 0)
                Progress = (encounterIndex + (double)(enemies.Count - remaining) / enemies.Count) / Definition.Encounters.Count;
            if (platformHealth.IsDead)
            {
                State = LevelState.Defeat;
                return;
            }
            if (State != LevelState.EncounterActive || remaining != 0) return;
            State = Definition.Encounters[encounterIndex].IsBossEncounter ? LevelState.Victory : LevelState.BetweenEncounters;
        }
    }
}
