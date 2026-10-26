// Isolated project only: resets builder/library settings and creates disposable assets.
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using CoelhoLabs.WorldBuilder;
using CoelhoLabs.WorldBuilder.Editor;

public static class CategoryValidation
{
    private static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
    private static int checks;
    private static void Check(bool ok, string message) { if (!ok) throw new Exception(message); checks++; }
    private static object Call(object target, string name, params object[] args) => target.GetType().GetMethod(name, Flags).Invoke(target, args);
    private static string Guid(GameObject go) => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(go));
    private static GameObject Prefab(string name, float size)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.transform.localScale = new Vector3(size, .2f, size);
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, AssetDatabase.GenerateUniqueAssetPath("Assets/CategoryChecks/" + name + ".prefab"));
        UnityEngine.Object.DestroyImmediate(go); return prefab;
    }
    private static bool Key(object painter, KeyCode key, bool focused = true)
    {
        var e = new Event { type = EventType.KeyDown, keyCode = key, modifiers = EventModifiers.Control };
        bool used = (bool)Call(painter, "TryCyclePrefab", e, focused);
        Check(used == (e.type == EventType.Used), "Event consumption matches navigation handling"); return used;
    }
    public static void Run()
    {
        IDisposable painter = null;
        try
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            if (!AssetDatabase.IsValidFolder("Assets/CategoryChecks")) AssetDatabase.CreateFolder("Assets", "CategoryChecks");
            var roads = new[] { Prefab("Road8", 8), Prefab("Road15", 15), Prefab("Junction2", 2), Prefab("Curve4", 4) };
            var tree = Prefab("Tree", 1);
            var s = BuilderSettings.instance; s.prefabs.Clear(); s.profiles.Clear(); s.autoGridSize = true;
            s.scale = Vector2.one; s.gridOrigin = new Vector3(3, 0, -5); s.offset = 0; s.spacing = .01f; s.rotationStep = 90;
            s.mode = PlacementMode.Single; s.snapMode = SnapMode.Grid; s.avoidOverlap = true; s.ctrlDirectionalSnap = true;
            s.prefabs.Add(roads[0]);
            var library = PrefabLibrary.instance; library.Initialize(); library.categories.Clear();
            var roadCategory = library.CreateCategory("RUAS");
            foreach (var road in roads) Check(library.Add(roadCategory, road), "Register road");
            Check(!library.Add(roadCategory, roads[0]), "No duplicated reference");
            var trees = library.CreateCategory("ÁRVORES"); library.Add(trees, tree); library.Activate(roadCategory);
            var type = typeof(WorldBuilderWindow).Assembly.GetType("CoelhoLabs.WorldBuilder.Editor.ScenePainter");
            painter = (IDisposable)Activator.CreateInstance(type, true);
            Call(painter, "SelectPrefab", roads[1]); Call(painter, "SetActive", true);
            int before = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Length;
            Check(Key(painter, KeyCode.E) && s.SelectedPrefab == roads[2] && s.gridCell == Vector2.one * 2, "Next/automatic grid");
            Check(Key(painter, KeyCode.Q) && s.SelectedPrefab == roads[1] && s.gridCell == Vector2.one * 15, "Previous/automatic grid");
            Call(painter, "SelectPrefab", roads[3]); Key(painter, KeyCode.E); Check(s.SelectedPrefab == roads[0], "Forward wrap");
            Key(painter, KeyCode.Q); Check(s.SelectedPrefab == roads[3], "Backward wrap");
            Check((bool)type.GetProperty("Active", Flags).GetValue(painter), "Placement stays active");
            Check(UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Length == before, "Cycling instantiates no objects");
            Check(s.gridOrigin == new Vector3(3, 0, -5), "Grid origin preserved");
            library.Activate(trees); Key(painter, KeyCode.E); Check(s.SelectedPrefab == tree, "Navigation limited to active category");
            Key(painter, KeyCode.Q); Check(s.SelectedPrefab == tree && library.Active == trees, "Single item circular category");
            library.Activate(roadCategory);
            s.Profile(Guid(roads[0])).category = ModularCategory.Roads; s.Profile(Guid(roads[1])).category = ModularCategory.Roads;
            Call(painter, "SelectPrefab", roads[0]);
            Call(painter, "BuildPreview", s.gridOrigin, Vector3.up); Call(painter, "Place"); Call(painter, "FinishStroke"); Physics.SyncTransforms();
            var tracker = (LastPlacedPrefabTracker)type.GetField("tracker", Flags).GetValue(painter);
            var reference = tracker.Reference; Check(reference != null, "Directional reference exists before shortcut");
            Call(painter, "UpdateControl", true); int instances = UnityEngine.Object.FindObjectsByType<GeneratedObject>(FindObjectsSortMode.None).Length;
            Key(painter, KeyCode.E);
            Check(tracker.Reference == reference && UnityEngine.Object.FindObjectsByType<GeneratedObject>(FindObjectsSortMode.None).Length == instances, "Ctrl+E preserves directional reference without placement");
            var referenceBounds = SmartPlacementMath.TransformBounds(reference.fittingBounds, reference.transform.localToWorldMatrix);
            Call(painter, "BuildPreview", referenceBounds.center + Vector3.right * 100, Vector3.up);
            var preview = (System.Collections.IList)type.GetField("preview", Flags).GetValue(painter); var candidate = preview[0];
            var bounds = (Bounds)candidate.GetType().GetField("bounds").GetValue(candidate);
            Check((bool)candidate.GetType().GetField("directional").GetValue(candidate) && Mathf.Abs(bounds.min.x - referenceBounds.max.x) < .001f, "Ctrl directional still works after shortcut");
            Call(painter, "UpdateControl", false);
            var previous = s.SelectedPrefab;
            Check(!Key(painter, KeyCode.E, false) && s.SelectedPrefab == previous, "Unfocused Scene View ignored");
            EditorGUIUtility.editingTextField = true;
            Check(!Key(painter, KeyCode.E) && s.SelectedPrefab == previous, "Text editing ignored"); EditorGUIUtility.editingTextField = false;
            Call(painter, "SetActive", false); Check(!Key(painter, KeyCode.E), "Inactive placement ignored"); Call(painter, "SetActive", true);
            library.Activate(roadCategory); Call(painter, "SelectPrefab", roads[0]);
            s.autoGridSize = false; s.gridCell = new Vector2(3, 7); Key(painter, KeyCode.E);
            Check(s.gridCell == new Vector2(3, 7), "Manual grid preserved"); s.autoGridSize = true;
            var profile = s.Profile(Guid(roads[2])); profile.manualDimensions = true; profile.cell = new Vector2(6, 9);
            Key(painter, KeyCode.E); Check(s.gridCell == new Vector2(6, 9), "Per-prefab dimensions respected");
            library.ReorderPrefab(roadCategory, Guid(roads[0]), 1); Check(roadCategory.prefabs[1] == Guid(roads[0]), "Reorder references");
            library.Move(roadCategory, trees, Guid(roads[3])); Check(!roadCategory.prefabs.Contains(Guid(roads[3])) && trees.prefabs.Contains(Guid(roads[3])), "Move virtual reference");
            library.Rename(trees, "Vegetação personalizada"); Check(trees.name == "Vegetação personalizada", "Rename category");
            library.ReorderCategory(trees, -1); Check(library.categories[0] == trees, "Reorder categories");
            var missing = Guid(roads[1]); AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(roads[1])); library.InvalidateCache();
            Call(painter, "SelectPrefab", roads[0]); library.Activate(roadCategory); Key(painter, KeyCode.E);
            Check(s.SelectedPrefab != null && s.selectedPrefabGuid != missing, "Missing prefab skipped");
            Check(roadCategory.prefabs.Contains(missing), "Missing GUID retained for inspection");
            var empty = library.CreateCategory("Vazia"); Key(painter, KeyCode.E); Check(library.Active == empty, "Empty category does not jump elsewhere");
            library.Delete(empty); Check(!library.categories.Contains(empty), "Delete category without assets");
            Check(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GetAssetPath(tree)) != null, "Virtual operations preserve assets");
            library.Activate(roadCategory);
            var window = EditorWindow.GetWindow<WorldBuilderWindow>(); window.CreateGUI();
            window.GetType().GetMethod("ShowPage", Flags).Invoke(window, new object[] { "Smart Grid & Directional Placement" });
            var content = (VisualElement)window.GetType().GetField("content", Flags).GetValue(window);
            var folders = content.ElementAt(1); // Heading then persistent library view.
            var windowPainter = window.GetType().GetField("painter", Flags).GetValue(window);
            Call(windowPainter, "SelectPrefab", roads[0]);
            Check(ReferenceEquals(folders, content.ElementAt(1)), "Selection does not rebuild folder tree");
            window.Close();
            window = EditorWindow.GetWindow<WorldBuilderWindow>(); window.CreateGUI();
            window.GetType().GetMethod("ShowPage", Flags).Invoke(window, new object[] { "Asset Library" }); window.Close();
            library.Commit(); s.Persist();
            painter.Dispose(); painter = null;
            Debug.Log("COELHO_CATEGORIES_PASSED: " + checks + " checks"); EditorApplication.Exit(0);
        }
        catch (Exception e) { EditorGUIUtility.editingTextField = false; Debug.LogException(e); painter?.Dispose(); EditorApplication.Exit(1); }
    }
    public static void VerifyRestart()
    {
        try
        {
            var library = PrefabLibrary.instance; library.Initialize();
            Check(library.categories.Count == 2, "Category count persisted");
            Check(library.categories[0].name == "Vegetação personalizada", "Category name and order persisted");
            Check(library.Active != null && library.Active.name == "RUAS" && library.Active.prefabs.Count == 3, "Active category/reference order persisted");
            Check(library.Next(BuilderSettings.instance.selectedPrefabGuid, 1, out var prefab) && prefab != null, "Navigation works after restart with missing reference");
            Debug.Log("COELHO_CATEGORIES_RESTART_PASSED: " + checks + " checks"); EditorApplication.Exit(0);
        }
        catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
    }
}
