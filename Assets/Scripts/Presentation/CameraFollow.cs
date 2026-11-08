using System;
using UnityEngine;

namespace LastOutpost.Presentation
{
    [DefaultExecutionOrder(100)]
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform platform;
        [SerializeField, Tooltip("World units ahead of the platform, along X.")] private float leadDistance = 2.1f;

        public void Configure(Transform target, float leadWorldUnits = 2.1f)
        {
            platform = target != null ? target : throw new ArgumentNullException(nameof(target));
            leadDistance = leadWorldUnits;
        }

        private void LateUpdate()
        {
            if (platform == null) return;
            Vector3 position = transform.position;
            position.x = platform.position.x + leadDistance;
            transform.position = position;
        }
    }
}
