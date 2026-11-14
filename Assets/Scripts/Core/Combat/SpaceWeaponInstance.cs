using System;
using System.Collections.Generic;
using LastOutpost.Core.Modules;

namespace LastOutpost.Core.Combat
{
    /// <summary>One mounted emitter, one clock. New instances start ready; disabling never resets the clock.</summary>
    public sealed class SpaceWeaponInstance
    {
        public string InstanceId { get; }
        public string SlotId { get; }
        public SpaceWeaponDefinition Definition { get; }
        public ShipPoint LocalPosition { get; }
        public ShipPoint LocalFacing { get; }
        public bool Enabled { get; set; } = true;
        public double NextFireTime { get; private set; }
        public int ShotsFired { get; private set; }
        public SpaceWeaponInstance(string instanceId, string slotId, SpaceWeaponDefinition definition, ShipPoint position)
        {
            InstanceId = Guard.Identifier(instanceId, nameof(instanceId)); SlotId = Guard.Identifier(slotId, nameof(slotId));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            SpaceGeometry.Validate(position); LocalPosition = position;
            LocalFacing = SpaceGeometry.Facing(definition.Facing, position.X);
        }
        public double Remaining(double now) { Guard.NonNegativeFinite(now, nameof(now)); return Math.Max(0, NextFireTime - now); }
        public ShipPoint Origin(ShipPoint ship, ShipPoint forward) => SpaceGeometry.Add(ship, SpaceGeometry.Transform(LocalPosition, forward));
        public SpaceEnemy SelectTarget(IReadOnlyList<SpaceEnemy> enemies, ShipPoint ship, ShipPoint forward)
        {
            if (enemies == null) throw new ArgumentNullException(nameof(enemies));
            if (!Enabled) return null;
            ShipPoint origin = Origin(ship, forward), direction = SpaceGeometry.Transform(LocalFacing, forward);
            SpaceEnemy best = null; double closest = double.PositiveInfinity;
            foreach (var enemy in enemies)
            {
                if (enemy == null || !enemy.Active || !enemy.Targetable || enemy.Agent.State.Health.IsDead ||
                    !SpaceGeometry.Eligible(origin, direction, enemy.Position, Definition.Range, Definition.ArcDegrees)) continue;
                double distance = SpaceGeometry.LengthSquared(SpaceGeometry.Subtract(enemy.Position, origin));
                if (distance < closest || (distance == closest && (best == null || enemy.Agent.State.Id < best.Agent.State.Id)))
                { best = enemy; closest = distance; }
            }
            return best;
        }
        public bool TryFire(double now, ShipPoint ship, ShipPoint forward, IReadOnlyList<SpaceEnemy> enemies,
            Func<SpaceWeaponInstance, SpaceEnemy, bool> emit)
        {
            Guard.NonNegativeFinite(now, nameof(now));
            if (emit == null) throw new ArgumentNullException(nameof(emit));
            if (!Enabled || now + 1e-12 < NextFireTime) return false;
            SpaceEnemy target = SelectTarget(enemies, ship, forward);
            if (target == null) return false;
            double next = Guard.NonNegativeFinite(now + Definition.Cooldown, nameof(now));
            if (!emit(this, target)) return false; // Pool saturation must not spend a shot/cooldown.
            NextFireTime = next; ShotsFired++; return true;
        }
    }
}
