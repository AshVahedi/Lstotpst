using System;
using LastOutpost.Core.Modules;
using LastOutpost.Gameplay;
using UnityEngine;

namespace LastOutpost.Presentation
{
    [RequireComponent(typeof(Camera))]
    public sealed class ShipCameraFraming : MonoBehaviour
    {
        [SerializeField] private SpaceShipSettings settings;
        public void Configure(SpaceShipSettings configuration)
        {
            settings = configuration != null ? configuration : throw new ArgumentNullException(nameof(configuration));
            settings.Build();
            Apply();
        }
        private void LateUpdate() => Apply();
        public void Apply()
        {
            if (settings == null) return;
            Camera camera = GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = (float)ShipPortraitFraming.HalfHeight(settings.widthLimit,
                settings.threatMargin, settings.minimumCameraHalfHeight, camera.aspect);
        }
    }
}
