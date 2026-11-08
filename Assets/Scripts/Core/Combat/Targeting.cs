using System;
using System.Collections.Generic;

namespace LastOutpost.Core.Combat
{
    /// <summary>A per-tick target snapshot. Distance is in world units.</summary>
    public sealed class Target
    {
        public int Id { get; }
        public Health Health { get; }
        public double Distance { get; }

        public Target(int id, Health health, double distance)
        {
            Id = Guard.Positive(id, nameof(id));
            Health = health ?? throw new ArgumentNullException(nameof(health));
            Distance = Guard.NonNegativeFinite(distance, nameof(distance));
        }
    }

    public interface ITargetingStrategy
    {
        Target Select(IReadOnlyList<Target> targets, double range);
    }

    public sealed class ClosestTargetStrategy : ITargetingStrategy
    {
        public Target Select(IReadOnlyList<Target> targets, double range)
        {
            if (targets == null) throw new ArgumentNullException(nameof(targets));
            Guard.NonNegativeFinite(range, nameof(range));
            Target selected = null;
            for (int i = 0; i < targets.Count; i++)
            {
                Target candidate = targets[i];
                if (candidate == null || candidate.Health.IsDead || candidate.Distance > range) continue;
                // Stable IDs break ties independently of spawn-list iteration order.
                if (selected == null || candidate.Distance < selected.Distance ||
                    (candidate.Distance == selected.Distance && candidate.Id < selected.Id))
                    selected = candidate;
            }
            return selected;
        }
    }
}
