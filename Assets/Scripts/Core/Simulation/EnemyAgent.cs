using System;
using LastOutpost.Core.Enemies;

namespace LastOutpost.Core.Simulation
{
    public sealed class EnemyAgent
    {
        // Stop one millionth of a world unit inside range to avoid subtraction rounding leaving
        // a stationary contact enemy infinitesimally outside Weapon's exact range comparison.
        private const double ContactInsetWorldUnits = 0.000001;
        public EnemyState State { get; }
        public double PositionX { get; private set; }
        internal bool DefeatReported { get; set; }

        public EnemyAgent(EnemyState state, double positionX)
        {
            State = state ?? throw new ArgumentNullException(nameof(state));
            PositionX = Guard.NonNegativeFinite(positionX, nameof(positionX));
        }

        public void Approach(double platformX, double deltaSeconds)
        {
            Guard.NonNegativeFinite(platformX, nameof(platformX));
            Guard.NonNegativeFinite(deltaSeconds, nameof(deltaSeconds));
            if (State.Health.IsDead) return;
            double distance = Math.Abs(PositionX - platformX);
            double stopDistance = State.Definition.Attack.Range - Math.Min(ContactInsetWorldUnits, State.Definition.Attack.Range / 2);
            double step = Math.Min(Math.Max(0, distance - stopDistance),
                State.Definition.MovementSpeed * deltaSeconds);
            PositionX += Math.Sign(platformX - PositionX) * step;
        }
    }
}
