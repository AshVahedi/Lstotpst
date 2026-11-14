using System;
using System.Collections.Generic;
using LastOutpost.Core.Modules;

namespace LastOutpost.Core.Combat
{
    public enum WeaponFacing { Forward, Left, Right, Rear, Outboard }
    public enum ArrivalSide { Front, Left, Right, Rear }

    /// <summary>Double precision XY geometry. Forward is ship-local +Y; no rendering/physics dependency.</summary>
    public static class SpaceGeometry
    {
        public static ShipPoint Add(ShipPoint a, ShipPoint b) => new ShipPoint(a.X + b.X, a.Y + b.Y);
        public static ShipPoint Subtract(ShipPoint a, ShipPoint b) => new ShipPoint(a.X - b.X, a.Y - b.Y);
        public static ShipPoint Scale(ShipPoint p, double scale) => new ShipPoint(p.X * scale, p.Y * scale);
        public static double LengthSquared(ShipPoint p) => p.X * p.X + p.Y * p.Y;
        public static ShipPoint Unit(ShipPoint p)
        {
            Validate(p);
            double length = Math.Sqrt(LengthSquared(p));
            if (length == 0 || double.IsInfinity(length)) throw new ArgumentException("Direction must have finite nonzero length.");
            return Scale(p, 1 / length);
        }
        public static void Validate(ShipPoint p) { ShipPoint.Finite(p.X, nameof(p)); ShipPoint.Finite(p.Y, nameof(p)); }
        public static ShipPoint Transform(ShipPoint local, ShipPoint forward)
        {
            ShipPoint f = Unit(forward);
            return new ShipPoint(f.Y * local.X + f.X * local.Y, -f.X * local.X + f.Y * local.Y);
        }
        public static ShipPoint Facing(WeaponFacing facing, double mountX)
        {
            switch (facing)
            {
                case WeaponFacing.Forward: return new ShipPoint(0, 1);
                case WeaponFacing.Left: return new ShipPoint(-1, 0);
                case WeaponFacing.Right: return new ShipPoint(1, 0);
                case WeaponFacing.Rear: return new ShipPoint(0, -1);
                case WeaponFacing.Outboard:
                    if (mountX == 0) throw new ArgumentException("Outboard weapons require a side mount.");
                    return new ShipPoint(Math.Sign(mountX), 0);
                default: throw new ArgumentOutOfRangeException(nameof(facing));
            }
        }

        /// <summary>Inclusive range/arc boundaries. A coincident target has no angle and is eligible.</summary>
        public static bool Eligible(ShipPoint origin, ShipPoint direction, ShipPoint target, double range, double arcDegrees)
        {
            Validate(origin); Validate(target);
            Guard.PositiveFinite(range, nameof(range));
            if (ShipPoint.Finite(arcDegrees, nameof(arcDegrees)) <= 0 || arcDegrees > 360) throw new ArgumentOutOfRangeException(nameof(arcDegrees));
            ShipPoint unit = Unit(direction), delta = Subtract(target, origin);
            double squared = LengthSquared(delta);
            if (squared > range * range + 1e-10) return false;
            if (squared == 0 || arcDegrees == 360) return true;
            double cosine = (unit.X * delta.X + unit.Y * delta.Y) / Math.Sqrt(squared);
            return cosine + 1e-12 >= Math.Cos(arcDegrees * Math.PI / 360);
        }

        /// <summary>First segment/circle impact fraction, including initial overlap and tangency.</summary>
        public static double CircleHit(ShipPoint from, ShipPoint to, ShipPoint center, double radius)
        {
            ShipPoint d = Subtract(to, from), p = Subtract(from, center);
            double c = LengthSquared(p) - radius * radius;
            if (c <= 0) return 0;
            double a = LengthSquared(d);
            if (a == 0) return double.PositiveInfinity;
            double b = p.X * d.X + p.Y * d.Y, disc = b * b - a * c;
            if (disc < 0) return double.PositiveInfinity;
            double t = (-b - Math.Sqrt(disc)) / a;
            return t >= 0 && t <= 1 ? t : double.PositiveInfinity;
        }
    }

    public readonly struct ShipHitBox
    {
        public ShipPoint Center { get; }
        public double HalfWidth { get; }
        public double HalfHeight { get; }
        public ShipHitBox(ShipPoint center, double halfWidth, double halfHeight)
        {
            SpaceGeometry.Validate(center); Center = center;
            HalfWidth = Guard.PositiveFinite(halfWidth, nameof(halfWidth));
            HalfHeight = Guard.PositiveFinite(halfHeight, nameof(halfHeight));
        }
        public double Hit(ShipPoint from, ShipPoint to, double radius)
        {
            double near = 0, far = 1;
            return Axis(from.X, to.X - from.X, Center.X - HalfWidth - radius, Center.X + HalfWidth + radius, ref near, ref far) &&
                Axis(from.Y, to.Y - from.Y, Center.Y - HalfHeight - radius, Center.Y + HalfHeight + radius, ref near, ref far)
                ? near : double.PositiveInfinity;
        }
        private static bool Axis(double p, double d, double min, double max, ref double near, ref double far)
        {
            if (Math.Abs(d) < 1e-14) return p >= min && p <= max;
            double a = (min - p) / d, b = (max - p) / d;
            near = Math.Max(near, Math.Min(a, b)); far = Math.Min(far, Math.Max(a, b)); return near <= far;
        }
        public static IReadOnlyList<ShipHitBox> Build(HorizontalShipLayout layout, IEnumerable<string> occupied)
        {
            var slots = new List<string>(occupied);
            ShipBounds bounds = layout.Bounds(slots); // Validates unknown/duplicate occupancy.
            var boxes = new List<ShipHitBox> { new ShipHitBox(new ShipPoint(0, -0.1), layout.CoreWidth / 2, 1) };
            foreach (string slot in slots) boxes.Add(new ShipHitBox(layout.Position(slot), layout.BayWidth / 2, 0.45));
            boxes.Add(new ShipHitBox(new ShipPoint((bounds.Left + bounds.Right) / 2, 0), bounds.Width / 2, 0.08));
            return boxes.AsReadOnly();
        }
    }
}
