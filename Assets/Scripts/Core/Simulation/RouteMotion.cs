using System;

namespace LastOutpost.Core.Simulation
{
    public enum MovementMode { Idle, Travel, Encounter, Boss, Stopped }

    public sealed class RouteConfig
    {
        public double TravelSpeed { get; }
        public double EncounterSpeed { get; }
        public double InitialTravelSeconds { get; }
        public double BetweenEncounterSeconds { get; }

        /// <summary>Speeds are world units/second; delays are seconds of ordinary travel.</summary>
        public RouteConfig(double travelSpeed = 0.9, double encounterSpeed = 0.1,
            double initialTravelSeconds = 1.5, double betweenEncounterSeconds = 2)
        {
            TravelSpeed = Guard.NonNegativeFinite(travelSpeed, nameof(travelSpeed));
            EncounterSpeed = Guard.NonNegativeFinite(encounterSpeed, nameof(encounterSpeed));
            InitialTravelSeconds = Guard.NonNegativeFinite(initialTravelSeconds, nameof(initialTravelSeconds));
            BetweenEncounterSeconds = Guard.NonNegativeFinite(betweenEncounterSeconds, nameof(betweenEncounterSeconds));
        }
    }

    /// <summary>Independent horizontal route integration; no knowledge of weapons or enemies.</summary>
    public sealed class RouteMotion
    {
        private readonly RouteConfig config;
        public double PositionX { get; private set; }
        public MovementMode Mode { get; private set; }
        public double Speed => Mode == MovementMode.Travel ? config.TravelSpeed :
            Mode == MovementMode.Encounter ? config.EncounterSpeed : 0;

        public RouteMotion(RouteConfig config)
        {
            this.config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public void SetMode(MovementMode mode)
        {
            if (!Enum.IsDefined(typeof(MovementMode), mode)) throw new ArgumentOutOfRangeException(nameof(mode));
            Mode = mode;
        }

        public void Advance(double deltaSeconds)
        {
            Guard.NonNegativeFinite(deltaSeconds, nameof(deltaSeconds));
            PositionX = Guard.NonNegativeFinite(PositionX + Speed * deltaSeconds, nameof(deltaSeconds));
        }
    }
}
