using System;
using LastOutpost.Core.Simulation;
using UnityEngine;

namespace LastOutpost.Gameplay
{
    /// <summary>Applies the independent route model to a Unity transform; never decides combat state.</summary>
    public sealed class PlatformMotor : MonoBehaviour
    {
        [SerializeField] private OutpostSession session;
        [SerializeField] private Transform vehicle;
        [SerializeField, Tooltip("World-space origin; the selected route axis advances automatically.")] private Vector3 routeOrigin;
        [SerializeField] private RouteOrientation orientation;
        public MovementMode Mode => session != null && session.Run != null ? session.Run.Route.Mode : MovementMode.Idle;

        public void Configure(OutpostSession owner, Transform vehicleTransform, Vector3 origin,
            RouteOrientation routeOrientation = RouteOrientation.LegacyHorizontal)
        {
            session = owner != null ? owner : throw new ArgumentNullException(nameof(owner));
            vehicle = vehicleTransform != null ? vehicleTransform : throw new ArgumentNullException(nameof(vehicleTransform));
            routeOrigin = origin;
            orientation = routeOrientation;
            Synchronize();
        }

        private void LateUpdate() => Synchronize();

        public void Synchronize()
        {
            if (session == null || session.Run == null || vehicle == null) return;
            vehicle.position = RouteProjection.Position(routeOrigin, session.Run.Route.PositionX, orientation);
        }
    }
}
