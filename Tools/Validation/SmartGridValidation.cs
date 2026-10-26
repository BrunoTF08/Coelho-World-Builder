// Copy into Assets/Editor in an isolated project with this package installed.
// Unity -batchmode -projectPath <project> -executeMethod SmartGridValidation.Run -logFile <log>
using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CoelhoLabs.WorldBuilder;
using CoelhoLabs.WorldBuilder.Editor;

public static class SmartGridValidation
{
    private static int checks;
    private static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); checks++; }
    private static object Call(object target, string name, params object[] args) => target.GetType().GetMethod(name, Flags).Invoke(target, args);
    private static object Field(object target, string name) => target.GetType().GetField(name, Flags).GetValue(target);
    private static GameObject Prefab(string path, Vector3 dimensions)
    {
        var root = new GameObject("OffsetPivotStreet");
        var child = GameObject.CreatePrimitive(PrimitiveType.Cube); child.transform.SetParent(root.transform);
        child.transform.localScale = dimensions; child.transform.localPosition = dimensions * .5f;
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, AssetDatabase.GenerateUniqueAssetPath(path));
        UnityEngine.Object.DestroyImmediate(root); return prefab;
    }
    public static void Run()
    {
        IDisposable painter = null;
        try
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            if (!AssetDatabase.IsValidFolder("Assets/SmartGridChecks")) AssetDatabase.CreateFolder("Assets", "SmartGridChecks");
            var eight = Prefab("Assets/SmartGridChecks/Road8.prefab", new Vector3(8, .2f, 8));
            var fifteen = Prefab("Assets/SmartGridChecks/Road15.prefab", new Vector3(15, .2f, 15));
            Check(SmartPlacementMath.TryBounds(eight, out var bounds) && Vector3.Distance(bounds.size, new Vector3(8, .2f, 8)) < .001f, "Composite bounds dimensions");
            Check(bounds.min.sqrMagnitude < .001f, "Offset pivot detection");
            var scaled = SmartPlacementMath.TransformBounds(bounds, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 90, 0), new Vector3(2, 1, 1)));
            Check(Mathf.Abs(scaled.size.x - 8) < .001f && Mathf.Abs(scaled.size.z - 16) < .001f, "Rotation/nonuniform scale");
            var origin = new Vector3(3, 0, -5);
            Check(SmartPlacementMath.Snap(new Vector3(11.1f, 4, 3.1f), origin, Vector2.one * 8) == new Vector3(11, 4, 3), "Fixed origin snap");
            var s = BuilderSettings.instance;
            s.prefabs.Clear(); s.prefabs.Add(eight); s.prefabs.Add(fifteen);
            s.selectedPrefabGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(eight));
            s.profiles.Clear(); var p = s.Profile(s.selectedPrefabGuid);
            p.cell = Vector2.one * 8; s.gridCell = p.cell; s.gridOrigin = Vector3.zero; s.gridHeight = 0;
            s.mode = PlacementMode.Single; s.snapMode = SnapMode.Grid; s.scale = Vector2.one; s.offset = 0; s.spacing = .01f; s.avoidOverlap = true;
            var type = typeof(WorldBuilderWindow).Assembly.GetType("CoelhoLabs.WorldBuilder.Editor.ScenePainter");
            painter = (IDisposable)Activator.CreateInstance(type, true); Call(painter, "SetActive", true);
            Call(painter, "ApplyShortcut", KeyCode.R); Check(p.rotation.y == 90, "R rotation increment");
            Call(painter, "ApplyShortcut", KeyCode.G); Check(!p.snap, "G snap toggle");
            Call(painter, "ApplyShortcut", KeyCode.G); p.rotation = Vector3.zero;
            var targetArgs = new object[] { new Ray(new Vector3(2, 10, 2), Vector3.down), Vector3.zero, Vector3.zero };
            Check((bool)Call(painter, "Target", targetArgs) && (Vector3)targetArgs[1] == new Vector3(2, 0, 2), "Plane without Collider");
            s.snapMode = SnapMode.Surface;
            Check((bool)Call(painter, "Target", targetArgs), "Surface fallback without Collider");
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube); floor.transform.position = new Vector3(0, -1, 0); floor.transform.localScale = new Vector3(100, 1, 100); Physics.SyncTransforms();
            Check((bool)Call(painter, "Target", targetArgs) && Mathf.Abs(((Vector3)targetArgs[1]).y + .5f) < .001f, "Surface Collider raycast");
            UnityEngine.Object.DestroyImmediate(floor); Physics.SyncTransforms(); s.snapMode = SnapMode.Grid;
            int countBefore = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Length;
            Call(painter, "BuildPreview", Vector3.zero, Vector3.up);
            Check(UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Length == countBefore, "Preview creates no GameObjects");
            Check(((IList)Field(painter, "meshes")).Count > 0 && Field(painter, "ghost") != null, "Cached complete mesh ghost and temporary material");
            Call(painter, "Place"); Call(painter, "FinishStroke");
            Physics.SyncTransforms();
            var first = UnityEngine.Object.FindFirstObjectByType<GeneratedObject>(); Check(first != null && PrefabUtility.IsPartOfPrefabInstance(first.gameObject), "Instantiation retains prefab link");
            Check(first.GetComponentInChildren<MeshCollider>() != null && first.GetComponentInChildren<BoxCollider>() == null, "Placed instance uses mesh collider");
            Check(eight.GetComponentInChildren<BoxCollider>() != null && eight.GetComponentInChildren<MeshCollider>() == null, "Original prefab colliders unchanged");
            Vector3 firstPosition = first.transform.position;
            Call(painter, "BuildPreview", new Vector3(8, 0, 0), Vector3.up); Call(painter, "Place"); Call(painter, "FinishStroke"); Physics.SyncTransforms();
            var placed = UnityEngine.Object.FindObjectsByType<GeneratedObject>(FindObjectsSortMode.None);
            Check(placed.Length == 2, "Adjacent 8x8 placement without collision rejection");
            var b0 = placed[0].GetComponentInChildren<Renderer>().bounds; var b1 = placed[1].GetComponentInChildren<Renderer>().bounds;
            Check(Mathf.Abs(Mathf.Max(b0.min.x, b1.min.x) - Mathf.Min(b0.max.x, b1.max.x)) < .001f, "No gap at adjacent street edges");
            Undo.PerformUndo(); Check(UnityEngine.Object.FindObjectsByType<GeneratedObject>(FindObjectsSortMode.None).Length == 1, "Placement Undo");
            Undo.PerformRedo(); Physics.SyncTransforms(); Check(UnityEngine.Object.FindObjectsByType<GeneratedObject>(FindObjectsSortMode.None).Length == 2, "Placement Redo");
            foreach (var restoredItem in UnityEngine.Object.FindObjectsByType<GeneratedObject>(FindObjectsSortMode.None))
                Check(!string.IsNullOrEmpty(restoredItem.persistentId) && restoredItem.sourcePrefabGuid == s.selectedPrefabGuid, "Metadata preserved after Redo");
            Call(painter, "RebuildOccupancy");
            Call(painter, "BuildPreview", Vector3.zero, Vector3.up);
            var preview = (IList)Field(painter, "preview");
            Check(!(bool)preview[0].GetType().GetField("valid").GetValue(preview[0]), "Invalid/red preview when occupied");
            s.mode = PlacementMode.Brush; s.density = 1; s.avoidOverlap = false;
            Call(painter, "BuildPreview", new Vector3(24, 0, 0), Vector3.up); Call(painter, "Place");
            Call(painter, "BuildPreview", new Vector3(24, 0, 0), Vector3.up);
            Check(!(bool)((IList)Field(painter, "preview"))[0].GetType().GetField("valid").GetValue(((IList)Field(painter, "preview"))[0]), "Same cell blocked within paint stroke");
            Call(painter, "FinishStroke");
            Call(painter, "SetActive", false); s.selectedPrefabGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(fifteen));
            Check(SmartPlacementMath.TryBounds(fifteen, out var big) && Mathf.Abs(big.size.x - 15) < .001f, "15x15 detection");
            var originBefore = s.gridOrigin; s.gridCell = new Vector2(big.size.x, big.size.z);
            Check(s.gridOrigin == originBefore && first.transform.position == firstPosition, "Apply grid preserves origin and existing instances");
            var profile15 = s.Profile(s.selectedPrefabGuid); profile15.rotation.y = 90; profile15.offset = new Vector3(1, 2, 3); profile15.cell = s.gridCell;
            s.gridOrigin = new Vector3(3, 4, 5); s.Persist();
            Check(((IList)Field(painter, "meshes")).Count == 0 && Field(painter, "ghost") == null, "Ghost resources released on stop");
            var window = EditorWindow.GetWindow<WorldBuilderWindow>();
            window.CreateGUI();
            window.GetType().GetMethod("ShowPage", Flags).Invoke(window, new object[] { "Placement & Smart Grid" });
            window.Close();
            Debug.Log("COELHO_SMART_GRID_PASSED: " + checks + " checks; restart verification required separately");
            painter.Dispose(); painter = null; EditorApplication.Exit(0);
        }
        catch (Exception e) { Debug.LogException(e); painter?.Dispose(); EditorApplication.Exit(1); }
    }
    public static void VerifyRestart()
    {
        try
        {
            var s = BuilderSettings.instance; var p = s.Profile(s.selectedPrefabGuid);
            Check(s.SelectedPrefab != null && s.gridOrigin == new Vector3(3, 4, 5) && s.gridCell == Vector2.one * 15, "Global settings persisted across editor restart");
            Check(p.rotation.y == 90 && p.offset == new Vector3(1, 2, 3) && p.cell == Vector2.one * 15, "GUID profile persisted across editor restart");
            Debug.Log("COELHO_SMART_GRID_RESTART_PASSED: " + checks + " checks"); EditorApplication.Exit(0);
        }
        catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
    }
}
