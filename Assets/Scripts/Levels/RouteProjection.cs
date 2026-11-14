using System;
using LastOutpost.Core.Modules;
using UnityEngine;

namespace LastOutpost.Gameplay
{
    // Zero is deliberately legacy X so existing serialized scenes retain their behavior.
    public enum RouteOrientation { LegacyHorizontal = 0, SpaceForwardY = 1 }

    public static class RouteProjection
    {
        public static Vector3 Position(Vector3 origin, double distance, RouteOrientation orientation)
        {
            if (orientation == RouteOrientation.LegacyHorizontal) return origin + new Vector3((float)distance, 0, 0);
            if (orientation != RouteOrientation.SpaceForwardY) throw new ArgumentOutOfRangeException(nameof(orientation));
            ShipPoint point = ShipPoint.AlongRoute(distance);
            return origin + new Vector3((float)point.X, (float)point.Y, 0);
        }
    }
}
