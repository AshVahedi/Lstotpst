using System;
using UnityEngine;
using LastOutpost.Gameplay;

namespace LastOutpost.Presentation
{
    [DefaultExecutionOrder(100)]
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform platform;
        [SerializeField, Tooltip("World units ahead of the platform, along X.")] private float leadDistance = 2.1f;
        [SerializeField] private RouteOrientation orientation;

        public void Configure(Transform target, float leadWorldUnits = 2.1f,
            RouteOrientation routeOrientation = RouteOrientation.LegacyHorizontal)
        {
            platform = target != null ? target : throw new ArgumentNullException(nameof(target));
            leadDistance = leadWorldUnits;
            orientation = routeOrientation;
        }

        private void LateUpdate()
        {
            if (platform == null) return;
            Vector3 position = transform.position;
            if (orientation == RouteOrientation.SpaceForwardY)
            { position.x = platform.position.x; position.y = platform.position.y + leadDistance; }
            else position.x = platform.position.x + leadDistance;
            transform.position = position;
        }
    }
}
