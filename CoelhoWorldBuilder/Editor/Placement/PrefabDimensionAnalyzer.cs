using UnityEngine;

namespace CoelhoLabs.WorldBuilder.Editor
{
    public static class PrefabDimensionAnalyzer
    {
        public static bool Analyze(GameObject prefab, out Bounds localBounds) => SmartPlacementMath.TryBounds(prefab, out localBounds);
        public static Bounds FittingBounds(Bounds visual, PrefabPlacementProfile profile, GameObject prefab)
        {
            if (!profile.manualDimensions) return visual;
            // Manual X/Z are root-local dimensions after the prefab's authored root scale,
            // before the profile scale and placement rotation. Preserve the visual center/pivot.
            var size = visual.size;
            size.x = Mathf.Max(.01f, profile.cell.x) / Mathf.Max(.0001f, Mathf.Abs(prefab.transform.localScale.x));
            size.z = Mathf.Max(.01f, profile.cell.y) / Mathf.Max(.0001f, Mathf.Abs(prefab.transform.localScale.z));
            return new Bounds(visual.center, size);
        }
        public static Vector2 CellSize(GameObject prefab, Bounds bounds, PrefabPlacementProfile profile, BuilderSettings settings = null)
        {
            var fitting = FittingBounds(bounds, profile, prefab);
            var rotation = profile.rotation;
            float fixedScale = 1;
            if (settings != null)
            {
                if (settings.rotationSnap) rotation.y = Mathf.Round(rotation.y / settings.rotationStep) * settings.rotationStep;
                if (Mathf.Approximately(settings.scale.x, settings.scale.y)) fixedScale = settings.scale.x;
            }
            var transformed = SmartPlacementMath.TransformBounds(fitting, Matrix4x4.TRS(Vector3.zero,
                Quaternion.Euler(rotation) * prefab.transform.rotation, Vector3.Scale(prefab.transform.localScale, profile.scale) * fixedScale));
            return new Vector2(Mathf.Max(.01f, transformed.size.x), Mathf.Max(.01f, transformed.size.z));
        }
        public static bool ApplyAdaptiveGrid(BuilderSettings settings)
        {
            var prefab = settings.SelectedPrefab;
            if (!settings.autoGridSize || prefab == null) return false;
            var profile = settings.Profile(settings.selectedPrefabGuid);
            if (!Analyze(prefab, out var bounds))
            {
                if (!profile.manualDimensions) return false;
                bounds = new Bounds(Vector3.zero, Vector3.one);
            }
            var size = CellSize(prefab, bounds, profile, settings);
            if ((settings.gridCell - size).sqrMagnitude < 1e-10f) return false;
            settings.gridCell = size;
            SceneRepaint(); return true;
        }
        private static void SceneRepaint() { UnityEditor.SceneView.RepaintAll(); }
    }
}
