using System;
using System.Linq;
using LastOutpost.Core.Modules;
using UnityEngine;

namespace LastOutpost.Gameplay
{
    [Serializable]
    public sealed class ShipMountSetting
    {
        public string slotId;
        public ShipSide side;
        public int step;
        public ShipMountSetting() { } // Unity serialization.
        public ShipMountSetting(string slot, ShipSide direction, int distance) { slotId = slot; side = direction; step = distance; }
    }

    [CreateAssetMenu(menuName = "Last Outpost/Space Ship Settings")]
    public sealed class SpaceShipSettings : ScriptableObject
    {
        public float coreWidth = 1;
        public float bayWidth = 0.9f;
        public float gap = 0.15f;
        public int maximumSteps = 2;
        public float widthLimit = 5.5f;
        public float threatMargin = 1.5f;
        public float minimumCameraHalfHeight = 8;
        public ShipMountSetting[] mounts = {
            new ShipMountSetting("primary", ShipSide.Left, 1), new ShipMountSetting("secondary", ShipSide.Right, 1),
            new ShipMountSetting("defense", ShipSide.Left, 2), new ShipMountSetting("utility", ShipSide.Right, 2) };

        public HorizontalShipLayout Build()
        {
            if (mounts == null || mounts.Any(m => m == null)) throw new ArgumentException("Ship mounts are missing.");
            ShipPortraitFraming.HalfHeight(widthLimit, threatMargin, minimumCameraHalfHeight, 9d / 16);
            return new HorizontalShipLayout(EquipmentConfiguration.Prototype(),
                mounts.Select(m => new ShipAttachmentPoint(m.slotId, m.side, m.step)), coreWidth, bayWidth, gap, maximumSteps, widthLimit);
        }
    }
}
