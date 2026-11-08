using System;
using LastOutpost.Core.Configuration;
using LastOutpost.Core.Modules;
using NUnit.Framework;

namespace LastOutpost.Tests
{
    public sealed class ModuleTests
    {
        [Test]
        public void ValidModuleCanBeAttachedRemovedAndReplaced()
        {
            var rack = new ModuleRack(1);
            var gun = new ModuleDefinition("gun-1", ModuleKind.BasicGun);
            Assert.That(rack.TryAttach(0, gun), Is.True);
            Assert.That(rack.GetAt(0), Is.SameAs(gun));
            Assert.That(rack.Count, Is.EqualTo(1));
            Assert.That(rack.TryDetach(0), Is.True);
            Assert.That(rack.GetAt(0), Is.Null);
            Assert.That(rack.Count, Is.Zero);
            Assert.That(rack.TryAttach(0, new ModuleDefinition("missile-1", ModuleKind.MissileLauncher)), Is.True);
        }

        [Test]
        public void FullRackRejectsReplacementAndOutOfBoundsSlots()
        {
            var rack = new ModuleRack(1);
            var gun = new ModuleDefinition("gun-1", ModuleKind.BasicGun);
            var missile = new ModuleDefinition("missile-1", ModuleKind.MissileLauncher);
            rack.TryAttach(0, gun);
            Assert.That(rack.TryAttach(0, missile), Is.False);
            Assert.That(rack.TryAttach(1, missile), Is.False);
            Assert.That(rack.TryAttach(-1, missile), Is.False);
            Assert.That(rack.GetAt(0), Is.SameAs(gun));
            Assert.That(rack.Count, Is.EqualTo(1));
        }

        [Test]
        public void DuplicateInstanceIdAndNullAttachmentAreRejected()
        {
            var rack = new ModuleRack(2);
            rack.TryAttach(0, new ModuleDefinition("gun", ModuleKind.BasicGun));
            Assert.That(rack.TryAttach(1, new ModuleDefinition("gun", ModuleKind.MissileLauncher)), Is.False);
            Assert.That(rack.TryAttach(1, null), Is.False);
            Assert.That(rack.Count, Is.EqualTo(1));
            Assert.That(rack.GetAt(1), Is.Null);
        }

        [Test]
        public void InvalidDetachAndZeroCapacityAreSafe()
        {
            var rack = new ModuleRack(0);
            Assert.That(rack.TryAttach(0, new ModuleDefinition("gun", ModuleKind.BasicGun)), Is.False);
            Assert.That(rack.TryDetach(0), Is.False);
            Assert.That(rack.TryDetach(-1), Is.False);
            Assert.That(new ModuleRack(1).TryDetach(0), Is.False);
            Assert.Throws<ArgumentOutOfRangeException>(() => rack.GetAt(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ModuleRack(-1));
        }

        [Test]
        public void InvalidDefinitionsAreRejected()
        {
            Assert.Throws<ArgumentException>(() => new ModuleDefinition(" ", ModuleKind.BasicGun));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ModuleDefinition("gun", (ModuleKind)999));
        }

        [Test]
        public void PrototypeStartsWithOnlyBasicGunAndMissileModules()
        {
            var platform = new PrototypeContent().CreatePlatform();
            Assert.That(platform.Modules.Capacity, Is.EqualTo(2));
            Assert.That(platform.Modules.GetAt(0).Kind, Is.EqualTo(ModuleKind.BasicGun));
            Assert.That(platform.Modules.GetAt(1).Kind, Is.EqualTo(ModuleKind.MissileLauncher));
        }
    }
}
