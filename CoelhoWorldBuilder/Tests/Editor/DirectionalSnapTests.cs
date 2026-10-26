using NUnit.Framework;
using UnityEngine;
namespace CoelhoLabs.WorldBuilder.Editor.Tests
{
    public sealed class DirectionalSnapTests
    {
        [TestCase(SnapDirection.Right)] [TestCase(SnapDirection.Left)]
        [TestCase(SnapDirection.Forward)] [TestCase(SnapDirection.Back)]
        public void MixedDimensionsAndShiftedPivotsMeetAtEdges(SnapDirection direction)
        {
            var reference = new Bounds(new Vector3(7, .5f, -3), new Vector3(15, 1, 15));
            var next = new Bounds(new Vector3(-4, 2, 6), new Vector3(2, 1, 2));
            var position = DirectionalSnapMath.Adjacent(reference, Matrix4x4.identity, next, Quaternion.identity, Vector3.one, direction, true, Vector3.zero);
            var placed = new Bounds(next.center + position, next.size);
            float gap = direction == SnapDirection.Right ? placed.min.x - reference.max.x
                : direction == SnapDirection.Left ? reference.min.x - placed.max.x
                : direction == SnapDirection.Forward ? placed.min.z - reference.max.z : reference.min.z - placed.max.z;
            Assert.That(gap, Is.EqualTo(0).Within(.001));
            Assert.That(placed.min.y, Is.EqualTo(reference.min.y).Within(.001));
        }
    }
}
