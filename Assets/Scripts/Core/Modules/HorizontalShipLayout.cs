using System;
using System.Collections.Generic;

namespace LastOutpost.Core.Modules
{
    public enum ShipSide { Left = -1, Right = 1 }

    /// <summary>Space uses XY: lateral X and forward +Y. No Unity or input dependency.</summary>
    public readonly struct ShipPoint
    {
        public double X { get; }
        public double Y { get; }
        public ShipPoint(double x, double y) { X = x; Y = y; }
        public static ShipPoint AlongRoute(double distance) => new ShipPoint(0, Finite(distance, nameof(distance)));
        internal static double Finite(double value, string name)
        { if (double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentOutOfRangeException(name); return value; }
    }

    public sealed class ShipAttachmentPoint
    {
        public string SlotId { get; }
        public ShipSide Side { get; }
        public int Step { get; }
        public ShipAttachmentPoint(string slotId, ShipSide side, int step)
        {
            SlotId = Guard.Identifier(slotId, nameof(slotId));
            if (side != ShipSide.Left && side != ShipSide.Right) throw new ArgumentOutOfRangeException(nameof(side));
            if (step < 1) throw new ArgumentOutOfRangeException(nameof(step));
            Side = side; Step = step;
        }
    }

    /// <summary>Immutable physical mapping of existing equipment slots; owns no inventory/save state.</summary>
    public sealed class HorizontalShipLayout
    {
        private readonly Dictionary<string, ShipAttachmentPoint> bySlot = new Dictionary<string, ShipAttachmentPoint>(StringComparer.Ordinal);
        public IReadOnlyList<ShipAttachmentPoint> Points { get; }
        public double CoreWidth { get; }
        public double BayWidth { get; }
        public double Gap { get; }
        public double WidthLimit { get; }
        public double MaximumHalfExtent { get; }

        public HorizontalShipLayout(EquipmentConfiguration equipment, IEnumerable<ShipAttachmentPoint> points,
            double coreWidth = 1, double bayWidth = 0.9, double gap = 0.15, int maximumSteps = 2, double widthLimit = 5.5)
        {
            if (equipment == null) throw new ArgumentNullException(nameof(equipment));
            if (ShipPoint.Finite(coreWidth, nameof(coreWidth)) <= 0 || ShipPoint.Finite(bayWidth, nameof(bayWidth)) <= 0 ||
                ShipPoint.Finite(gap, nameof(gap)) < 0 || ShipPoint.Finite(widthLimit, nameof(widthLimit)) <= 0 || maximumSteps < 1)
                throw new ArgumentOutOfRangeException(nameof(coreWidth), "Use positive finite dimensions, nonnegative gap and positive step count.");
            CoreWidth = coreWidth; BayWidth = bayWidth; Gap = gap; WidthLimit = widthLimit;
            var ordered = new List<ShipAttachmentPoint>(); var cells = new HashSet<string>(StringComparer.Ordinal);
            double half = coreWidth / 2;
            foreach (var point in points ?? throw new ArgumentNullException(nameof(points)))
            {
                if (point == null || equipment.GetSlot(point.SlotId) == null || point.Step > maximumSteps ||
                    bySlot.ContainsKey(point.SlotId) || !cells.Add(point.Side + ":" + point.Step))
                    throw new ArgumentException("Unknown/duplicate slot, occupied cell or out-of-range attachment step.");
                bySlot.Add(point.SlotId, point); ordered.Add(point);
                half = Math.Max(half, Math.Abs(Position(point).X) + bayWidth / 2);
            }
            foreach (var slot in equipment.Slots)
                if (!bySlot.ContainsKey(slot.Id)) throw new ArgumentException("Every equipment slot needs one physical mount: " + slot.Id);
            if (double.IsInfinity(half) || 2 * half > widthLimit + 1e-9)
                throw new ArgumentException("Configured ship exceeds its origin-centered width limit.");
            ordered.Sort((a, b) => Position(a).X.CompareTo(Position(b).X));
            Points = ordered.AsReadOnly(); MaximumHalfExtent = half;
        }

        private ShipPoint Position(ShipAttachmentPoint point) => new ShipPoint((int)point.Side *
            (CoreWidth / 2 + BayWidth / 2 + Gap + (point.Step - 1) * (BayWidth + Gap)), 0);
        public ShipPoint Position(string slotId) => bySlot.TryGetValue(slotId ?? "", out var point)
            ? Position(point) : throw new ArgumentException("No physical mount for slot: " + slotId);

        public ShipBounds Bounds(IEnumerable<string> occupiedSlotIds)
        {
            double left = -CoreWidth / 2, right = CoreWidth / 2;
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (string id in occupiedSlotIds ?? throw new ArgumentNullException(nameof(occupiedSlotIds)))
            {
                if (!seen.Add(id)) throw new ArgumentException("Duplicate occupied mount.");
                double x = Position(id).X;
                left = Math.Min(left, x - BayWidth / 2); right = Math.Max(right, x + BayWidth / 2);
            }
            return new ShipBounds(left, right);
        }

        public static ShipAttachmentPoint[] DefaultPoints() => new[] {
            new ShipAttachmentPoint("primary", ShipSide.Left, 1), new ShipAttachmentPoint("secondary", ShipSide.Right, 1),
            new ShipAttachmentPoint("defense", ShipSide.Left, 2), new ShipAttachmentPoint("utility", ShipSide.Right, 2) };
    }

    public readonly struct ShipBounds
    {
        public double Left { get; }
        public double Right { get; }
        public double Width => Right - Left;
        public double HalfExtent => Math.Max(Math.Abs(Left), Math.Abs(Right));
        public ShipBounds(double left, double right) { Left = left; Right = right; }
    }

    public static class ShipPortraitFraming
    {
        /// <summary>Use a fixed configuration width rather than zooming when equipment changes.</summary>
        public static double HalfHeight(double shipWidthLimit, double threatMargin, double minimumHalfHeight, double aspect)
        {
            if (ShipPoint.Finite(shipWidthLimit, nameof(shipWidthLimit)) <= 0 || ShipPoint.Finite(threatMargin, nameof(threatMargin)) < 0 ||
                ShipPoint.Finite(minimumHalfHeight, nameof(minimumHalfHeight)) <= 0 || ShipPoint.Finite(aspect, nameof(aspect)) <= 0)
                throw new ArgumentOutOfRangeException(nameof(aspect));
            double result = Math.Max(minimumHalfHeight, (shipWidthLimit + 2 * threatMargin) / (2 * aspect));
            if (double.IsInfinity(result)) throw new ArgumentOutOfRangeException(nameof(aspect));
            return result;
        }
    }
}
