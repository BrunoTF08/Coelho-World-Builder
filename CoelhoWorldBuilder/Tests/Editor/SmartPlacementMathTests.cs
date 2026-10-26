using NUnit.Framework;
using UnityEngine;

namespace CoelhoLabs.WorldBuilder.Editor.Tests
{
    public sealed class SmartPlacementMathTests
    {
        [Test] public void SnapRespectsTranslatedOriginAndPreservesHeight()
        {
            Assert.That(SmartPlacementMath.Snap(new Vector3(11.1f, 4, 3.1f), new Vector3(3, 0, -5), Vector2.one * 8), Is.EqualTo(new Vector3(11, 4, 3)));
        }
        [Test] public void RotatedScaledBoundsIncludeAllCorners()
        {
            var b = new Bounds(new Vector3(4, .1f, 4), new Vector3(8, .2f, 8));
            var transformed = SmartPlacementMath.TransformBounds(b, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 90, 0), new Vector3(2, 1, 1)));
            Assert.That(transformed.size.x, Is.EqualTo(8).Within(.001));
            Assert.That(transformed.size.z, Is.EqualTo(16).Within(.001));
        }
        [Test] public void CornerAnchorHandlesOffCenterPivot()
        {
            var b = new Bounds(new Vector3(10, 2, 6), new Vector3(8, 4, 8));
            Assert.That(SmartPlacementMath.Anchor(b, new PrefabPlacementProfile { anchorMode = AnchorMode.BottomCorner }), Is.EqualTo(new Vector3(6, 0, 2)));
        }
    }
}
