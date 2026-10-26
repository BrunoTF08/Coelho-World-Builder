using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CoelhoLabs.WorldBuilder.Editor
{
    public enum PlacementMode { Single, Brush }

    [FilePath("UserSettings/CoelhoWorldBuilder.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class BuilderSettings : ScriptableSingleton<BuilderSettings>
    {
        public List<GameObject> prefabs = new List<GameObject>();
        public PlacementMode mode;
        public float radius = 3f;
        public int density = 8;
        public float spacing = 1f;
        public Vector2 scale = Vector2.one;
        public Vector2 yaw = new Vector2(0, 360);
        public bool alignToNormal = true;
        public float offset;
        public int layers = -1;
        public int seed = 12345;
        public string selectedPrefabGuid;
        public List<PrefabPlacementProfile> profiles = new List<PrefabPlacementProfile>();
        public bool showGrid = true;
        public bool autoGridSize = true;
        public Vector2 gridCell = Vector2.one;
        public Vector3 gridOrigin;
        public float gridHeight;
        public Vector2 gridArea = new Vector2(80, 80);
        public SnapMode snapMode = SnapMode.Grid;
        public float rotationStep = 90;
        public bool rotationSnap = true;
        public bool avoidOverlap = true;
        public bool ctrlDirectionalSnap = true;
        public bool edgeAlignment = true;
        public Vector3 snapOffset;
        public bool showSnapPreview = true;
        [System.NonSerialized] private string resolvedGuid;
        [System.NonSerialized] private GameObject resolvedPrefab;
        [System.NonSerialized] private bool selectionResolved;
        public void InvalidateSelectedPrefabCache() { selectionResolved = false; }
        public GameObject SelectedPrefab
        {
            get
            {
                if (selectionResolved && resolvedGuid == selectedPrefabGuid) return resolvedPrefab;
                resolvedGuid = selectedPrefabGuid; selectionResolved = true;
                resolvedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(selectedPrefabGuid ?? ""));
                return resolvedPrefab;
            }
        }
        public PrefabPlacementProfile Profile(string guid)
        {
            var profile = profiles.Find(p => p.guid == guid);
            if (profile != null) return profile;
            profile = new PrefabPlacementProfile { guid = guid }; profiles.Add(profile); return profile;
        }
        public void Persist() { Save(true); }
        public void Sanitize()
        {
            radius = Mathf.Clamp(radius, .1f, 100f);
            density = Mathf.Clamp(density, 1, 100);
            spacing = Mathf.Max(.01f, spacing);
            scale.x = Mathf.Max(.01f, scale.x);
            scale.y = Mathf.Max(scale.x, scale.y);
            gridCell.x = Mathf.Max(.01f, gridCell.x); gridCell.y = Mathf.Max(.01f, gridCell.y);
            gridArea.x = Mathf.Clamp(gridArea.x, .1f, 10000); gridArea.y = Mathf.Clamp(gridArea.y, .1f, 10000);
            rotationStep = Mathf.Clamp(rotationStep, 1, 180);
        }
    }
}
