using System;
using System.Linq;
using LastOutpost.Core.Modules;
using LastOutpost.Core.Progression;
using NUnit.Framework;

namespace LastOutpost.Tests
{
    public sealed class ShipFoundationTests
    {
        private static HorizontalShipLayout Layout(ShipAttachmentPoint[] points = null) => new HorizontalShipLayout(
            EquipmentConfiguration.Prototype(), points ?? HorizontalShipLayout.DefaultPoints());

        [TestCase(0d)] [TestCase(2.5d)] [TestCase(-3d)]
        public void RouteDistanceMapsToWorldYWithoutLateralMotion(double distance)
        { var point = ShipPoint.AlongRoute(distance); Assert.That(point.X, Is.Zero); Assert.That(point.Y, Is.EqualTo(distance)); }

        [TestCase("primary", -1.1)] [TestCase("secondary", 1.1)]
        [TestCase("defense", -2.15)] [TestCase("utility", 2.15)]
        public void SlotsHaveDeterministicHorizontalCenters(string slot, double x)
        { Assert.That(Layout().Position(slot).X, Is.EqualTo(x).Within(1e-9)); Assert.That(Layout().Position(slot).Y, Is.Zero); }

        [Test]
        public void AllFootprintsAreSeparatedFromCoreAndEachOtherRegardlessOfInputOrder()
        {
            var normal = Layout(); var reverse = Layout(HorizontalShipLayout.DefaultPoints().Reverse().ToArray());
            double lastRight = double.NegativeInfinity;
            foreach (var point in normal.Points)
            {
                double x = normal.Position(point.SlotId).X;
                Assert.That(reverse.Position(point.SlotId).X, Is.EqualTo(x));
                Assert.That(Math.Abs(x) - normal.BayWidth / 2, Is.GreaterThanOrEqualTo(normal.CoreWidth / 2));
                Assert.That(x - normal.BayWidth / 2, Is.GreaterThanOrEqualTo(lastRight));
                lastRight = x + normal.BayWidth / 2;
            }
        }

        [Test]
        public void SymmetricAndAsymmetricBoundsPreserveTheHullOrigin()
        {
            var layout = Layout();
            var full = layout.Bounds(layout.Points.Select(p => p.SlotId));
            Assert.That(full.Width, Is.EqualTo(5.2).Within(1e-9)); Assert.That(full.Left, Is.EqualTo(-full.Right));
            var asymmetric = layout.Bounds(new[] { "primary", "defense" });
            Assert.That(asymmetric.Left, Is.EqualTo(-2.6).Within(1e-9)); Assert.That(asymmetric.Right, Is.EqualTo(0.5));
            Assert.That(asymmetric.HalfExtent, Is.GreaterThan(asymmetric.Width / 2), "Asymmetric camera bounds must remain hull-centered.");
            Assert.That(layout.Bounds(Array.Empty<string>()).Width, Is.EqualTo(1));
        }

        [Test]
        public void EmptyInnerOptionalBayDoesNotInvalidateOuterStructuralRailMount()
        {
            var bounds = Layout().Bounds(new[] { "primary", "utility" });
            Assert.That(bounds.Right, Is.EqualTo(2.6).Within(1e-9));
            Assert.That(Layout().Position("utility").X, Is.EqualTo(2.15).Within(1e-9));
        }

        [TestCase(0)] [TestCase(-1)]
        public void NonpositiveExpansionStepsAreRejected(int step) =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new ShipAttachmentPoint("primary", ShipSide.Left, step));
        [Test]
        public void InvalidSideAndMissingSlotIdsAreRejected()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ShipAttachmentPoint("primary", (ShipSide)0, 1));
            Assert.Throws<ArgumentException>(() => new ShipAttachmentPoint("", ShipSide.Left, 1));
            Assert.Throws<ArgumentException>(() => Layout().Position("missing"));
            Assert.Throws<ArgumentException>(() => Layout().Bounds(new[] { "primary", "primary" }));
        }

        [TestCase("duplicate-cell")] [TestCase("duplicate-slot")] [TestCase("unknown-slot")]
        [TestCase("step-limit")] [TestCase("unmapped-slot")]
        public void InvalidPhysicalConfigurationFailsBeforeAViewCanBeBuilt(string problem)
        {
            var points = HorizontalShipLayout.DefaultPoints();
            if (problem == "duplicate-cell") points[1] = new ShipAttachmentPoint("secondary", ShipSide.Left, 1);
            if (problem == "duplicate-slot") points[1] = new ShipAttachmentPoint("primary", ShipSide.Right, 1);
            if (problem == "unknown-slot") points[1] = new ShipAttachmentPoint("unknown", ShipSide.Right, 1);
            if (problem == "step-limit") points[1] = new ShipAttachmentPoint("secondary", ShipSide.Right, 3);
            if (problem == "unmapped-slot") points = points.Take(3).ToArray();
            Assert.Throws<ArgumentException>(() => Layout(points));
        }

        [TestCase(0, 0.9, 0.15)] [TestCase(1, 0, 0.15)] [TestCase(1, 0.9, -0.1)]
        [TestCase(double.NaN, 0.9, 0.15)] [TestCase(1, double.PositiveInfinity, 0.15)]
        public void InvalidDimensionsAreRejected(double core, double bay, double gap) =>
            Assert.Throws<ArgumentOutOfRangeException>(() => new HorizontalShipLayout(EquipmentConfiguration.Prototype(),
                HorizontalShipLayout.DefaultPoints(), core, bay, gap));
        [Test]
        public void WidthOverflowIsRejected() => Assert.Throws<ArgumentException>(() => new HorizontalShipLayout(
            EquipmentConfiguration.Prototype(), HorizontalShipLayout.DefaultPoints(), widthLimit: 5));

        [TestCase(0.5625)] [TestCase(0.45)] [TestCase(1d)]
        public void CameraFramingIncludesConfiguredWidthAndThreatMargins(double aspect)
        {
            double size = ShipPortraitFraming.HalfHeight(5.5, 1.5, 8, aspect);
            Assert.That(size, Is.GreaterThanOrEqualTo(8)); Assert.That(2 * size * aspect, Is.GreaterThanOrEqualTo(8.5));
            if (aspect == 0.5625) Assert.That(size, Is.EqualTo(8));
        }
        [TestCase(0)] [TestCase(-1)] [TestCase(double.NaN)] [TestCase(double.PositiveInfinity)]
        public void InvalidCameraAspectIsRejected(double aspect) => Assert.Throws<ArgumentOutOfRangeException>(() =>
            ShipPortraitFraming.HalfHeight(5.5, 1.5, 8, aspect));

        [Test]
        public void ResolvingPhysicalLayoutDoesNotChangeSchemaOwnershipCreditsOrEncodedSave()
        {
            var config = ProgressionConfiguration.Prototype(); var data = new ProgressionData { Credits = 650 };
            var codec = new JsonProgressionCodec(); string before = codec.Encode(data);
            var decoded = codec.Decode(before, config);
            var layout = Layout(); layout.Bounds(decoded.Equipment.Installations.Select(i => i.SlotId));
            Assert.That(codec.Encode(decoded), Is.EqualTo(before)); Assert.That(decoded.Credits, Is.EqualTo(650));
            Assert.That(decoded.SchemaVersion, Is.EqualTo(2)); Assert.That(decoded.Equipment.OwnedEquipmentIds.Count, Is.EqualTo(4));
            var current = new LoadoutSnapshot(decoded.Equipment);
            Assert.That(LoadoutRules.Change(config.Equipment, current, LoadoutOperation.Unequip, "secondary", null, out var changed), Is.EqualTo(LoadoutResult.Succeeded));
            Assert.That(layout.Bounds(changed.Installations.Select(i => i.SlotId)).Right, Is.EqualTo(2.6).Within(1e-9));
            Assert.That(codec.Encode(data), Is.EqualTo(before));
        }
    }
}
