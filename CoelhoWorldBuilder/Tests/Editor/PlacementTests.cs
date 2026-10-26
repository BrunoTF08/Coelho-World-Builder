using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CoelhoLabs.WorldBuilder.Editor.Tests
{
    public class PlacementTests
    {
        [Test] public void DiskStaysInsideRadiusAndRepeatsForSameSeed()
        {
            var a = new System.Random(42); var b = new System.Random(42);
            for (int i = 0; i < 1000; i++)
            {
                var point = PlacementMath.Disk(a, 3);
                Assert.That(point.magnitude, Is.LessThanOrEqualTo(3.00001f));
                Assert.That(point, Is.EqualTo(PlacementMath.Disk(b, 3)));
            }
        }
        [Test] public void DiskUsesAreaRatherThanUniformRadius()
        {
            var random = new System.Random(7); float sum = 0;
            for (int i = 0; i < 10000; i++) sum += PlacementMath.Disk(random, 1).sqrMagnitude;
            Assert.That(sum / 10000, Is.InRange(.48f, .52f));
        }
        [Test] public void GeneratedInstanceSupportsUndoRedo()
        {
            Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
            var item = new GameObject("Coelho Undo Test");
            Undo.RegisterCreatedObjectUndo(item, "Test placement");
            var metadata = Undo.AddComponent<GeneratedObject>(item);
            metadata.persistentId = "test-id";
            Undo.FlushUndoRecordObjects(); Undo.CollapseUndoOperations(group);
            Undo.PerformUndo(); Assert.That(item == null, Is.True);
            Undo.PerformRedo();
            var restored = GameObject.Find("Coelho Undo Test");
            try { Assert.That(restored, Is.Not.Null); Assert.That(restored.GetComponent<GeneratedObject>(), Is.Not.Null); }
            finally { if (restored != null) Object.DestroyImmediate(restored); }
        }
    }
}
