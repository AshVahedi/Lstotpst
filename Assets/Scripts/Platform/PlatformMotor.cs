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
        [SerializeField, Tooltip("World-space route origin. Y/Z stay fixed.")] private Vector3 routeOrigin;
        public MovementMode Mode => session != null && session.Run != null ? session.Run.Route.Mode : MovementMode.Idle;

        public void Configure(OutpostSession owner, Transform vehicleTransform, Vector3 origin)
        {
            session = owner != null ? owner : throw new ArgumentNullException(nameof(owner));
            vehicle = vehicleTransform != null ? vehicleTransform : throw new ArgumentNullException(nameof(vehicleTransform));
            routeOrigin = origin;
            Synchronize();
        }

        private void LateUpdate() => Synchronize();

        public void Synchronize()
        {
            if (session == null || session.Run == null || vehicle == null) return;
            vehicle.position = routeOrigin + new Vector3((float)session.Run.Route.PositionX, 0, 0);
        }
    }
}
