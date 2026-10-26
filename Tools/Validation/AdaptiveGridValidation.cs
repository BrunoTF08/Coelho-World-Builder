// Run only in isolated test projects: creates assets/scenes and resets builder settings.
using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using CoelhoLabs.WorldBuilder;
using CoelhoLabs.WorldBuilder.Editor;

public static class AdaptiveGridValidation
{
    private static readonly BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;
    private static int checks;
    private static void Check(bool ok, string message) { if (!ok) throw new Exception(message); checks++; }
    private static object Call(object target, string name, params object[] args) => target.GetType().GetMethod(name, Flags).Invoke(target, args);
    private static object Field(object target, string name) => target.GetType().GetField(name, Flags).GetValue(target);
    private static T Value<T>(object candidate, string name) => (T)candidate.GetType().GetField(name).GetValue(candidate);
    private static object Preview(object painter, Vector3 pointer)
    { Call(painter, "BuildPreview", pointer, Vector3.up); return ((IList)Field(painter, "preview"))[0]; }
    private static GameObject Prefab(float size, Vector3 pivotOffset, string name)
    {
        var root = new GameObject(name);
        var child = GameObject.CreatePrimitive(PrimitiveType.Cube); child.transform.SetParent(root.transform);
        child.transform.localScale = new Vector3(size, .25f, size); child.transform.localPosition = pivotOffset;
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, AssetDatabase.GenerateUniqueAssetPath("Assets/AdaptiveChecks/" + name + ".prefab"));
        UnityEngine.Object.DestroyImmediate(root); return prefab;
    }
    private static string Guid(GameObject prefab) => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prefab));
    public static void Run()
    {
        IDisposable painter = null;
        try
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            if (!AssetDatabase.IsValidFolder("Assets/AdaptiveChecks")) AssetDatabase.CreateFolder("Assets", "AdaptiveChecks");
            var road8 = Prefab(8, new Vector3(12, 1, -3), "Road8OffsetPivot");
            var road15 = Prefab(15, new Vector3(-2, 2, 5), "Road15");
            var detail = Prefab(2, Vector3.zero, "Detail2");
            var s = BuilderSettings.instance;
            s.profiles.Clear(); s.prefabs.Clear(); s.selectedPrefabGuid = "";
            s.autoGridSize = true; s.ctrlDirectionalSnap = true; s.showSnapPreview = true; s.edgeAlignment = true;
            s.snapOffset = Vector3.zero; s.gridOrigin = new Vector3(3, 0, -5); s.gridHeight = 0;
            s.mode = PlacementMode.Single; s.snapMode = SnapMode.Grid; s.rotationSnap = true; s.rotationStep = 90;
            s.scale = Vector2.one; s.offset = 0; s.spacing = .01f; s.avoidOverlap = true;
            var p8 = s.Profile(Guid(road8)); p8.category = ModularCategory.Roads;
            var p15 = s.Profile(Guid(road15)); p15.category = ModularCategory.Roads;
            var type = typeof(WorldBuilderWindow).Assembly.GetType("CoelhoLabs.WorldBuilder.Editor.ScenePainter");
            painter = (IDisposable)Activator.CreateInstance(type, true);
            Call(painter, "SelectPrefab", road8);
            Check(s.gridCell == Vector2.one * 8, "Auto 8x8 on selection without Apply");
            Call(painter, "SetActive", true);
            Preview(painter, s.gridOrigin); Call(painter, "Place"); Call(painter, "FinishStroke");
            Physics.SyncTransforms();
            var tracker = (LastPlacedPrefabTracker)Field(painter, "tracker");
            var first = tracker.Reference;
            Check(first != null && first.modularCategory == ModularCategory.Roads, "Last placed modular reference");
            var initialPosition = first.transform.position;
            var rb = SmartPlacementMath.TransformBounds(first.fittingBounds, first.transform.localToWorldMatrix);
            Call(painter, "UpdateControl", true);
            var directions = new[] { Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
            foreach (var direction in directions)
            {
                var candidate = Preview(painter, rb.center + direction * 100);
                var b = Value<Bounds>(candidate, "bounds");
                float gap = direction.x > 0 ? b.min.x - rb.max.x : direction.x < 0 ? rb.min.x - b.max.x : direction.z > 0 ? b.min.z - rb.max.z : rb.min.z - b.max.z;
                Check(Mathf.Abs(gap) < .001f && Value<bool>(candidate, "valid"), "Direction contact/valid " + direction);
                Check(Quaternion.Angle(Value<Quaternion>(candidate, "rotation"), Quaternion.identity) < .001f && Value<Vector3>(candidate, "scale") == Vector3.one, "No Ctrl rotation/scale change " + direction);
            }
            Preview(painter, rb.center + Vector3.right * 100); Call(painter, "Place"); Call(painter, "FinishStroke"); Undo.FlushUndoRecordObjects(); Physics.SyncTransforms();
            Check(tracker.Reference != first, "Confirmed next module becomes reference");
            Undo.PerformUndo(); Check(tracker.Reference == first, "Undo restores reference");
            Undo.PerformRedo(); Check(tracker.Reference != null && tracker.Reference != first, "Redo restores latest reference");
            Call(painter, "RebuildOccupancy"); Physics.SyncTransforms();
            var latest = tracker.Reference;
            Call(painter, "SelectPrefab", road15);
            Check(s.gridCell == Vector2.one * 15 && (bool)type.GetProperty("Active", Flags).GetValue(painter), "Auto 15 and placement remains active");
            Check(first.transform.position == initialPosition && s.gridOrigin == new Vector3(3, 0, -5), "Selection preserves origin/instances");
            var latestBounds = SmartPlacementMath.TransformBounds(latest.fittingBounds, latest.transform.localToWorldMatrix);
            var big = Preview(painter, latestBounds.center + Vector3.right * 100);
            Check(Value<bool>(big, "valid") && Mathf.Abs(Value<Bounds>(big, "bounds").min.x - latestBounds.max.x) < .001f, "8 next to 15 with shifted pivots");
            Call(painter, "SelectPrefab", detail);
            Check(s.gridCell == Vector2.one * 2, "Auto 2x2 detail");
            Preview(painter, new Vector3(100, 0, 100)); Call(painter, "Place"); Call(painter, "FinishStroke");
            Check(tracker.Reference == latest, "Decoration does not replace modular reference");
            Call(painter, "SelectPrefab", road15); Check(s.gridCell == Vector2.one * 15, "15 -> 2 -> 15");
            Call(painter, "UpdateControl", false);
            var normal = Preview(painter, new Vector3(100, 0, 100)); Check(!Value<bool>(normal, "directional"), "Release Ctrl restores normal placement");
            s.autoGridSize = false; s.gridCell = new Vector2(3, 7); Call(painter, "SelectPrefab", road8);
            Check(s.gridCell == new Vector2(3, 7), "Global manual grid preserved");
            p8.manualDimensions = true; p8.cell = new Vector2(4, 6); s.autoGridSize = true;
            PrefabDimensionAnalyzer.ApplyAdaptiveGrid(s); Check(s.gridCell == new Vector2(4, 6), "Manual per-prefab fitting override");
            p8.rotation = new Vector3(0, 90, 0); p8.scale = new Vector3(2, 1, 1);
            PrefabDimensionAnalyzer.ApplyAdaptiveGrid(s); Check(Vector2.Distance(s.gridCell, new Vector2(6, 8)) < .001f, "Rotation and profile scale in adaptive grid");
            var refBounds = new Bounds(new Vector3(4, .125f, 4), new Vector3(8, .25f, 8));
            var refMatrix = Matrix4x4.TRS(new Vector3(5, 0, 2), Quaternion.Euler(0, 45, 0), Vector3.one);
            var next = new Bounds(new Vector3(5, 2, -4), new Vector3(2, .25f, 2));
            var nextRotation = Quaternion.Euler(0, 135, 0);
            var translated = DirectionalSnapMath.Adjacent(refBounds, refMatrix, next, nextRotation, Vector3.one, SnapDirection.Right, true, Vector3.zero);
            var axes = Matrix4x4.Rotate(Quaternion.Euler(0, -45, 0));
            var a = SmartPlacementMath.TransformBounds(refBounds, axes * refMatrix);
            var bRot = SmartPlacementMath.TransformBounds(next, axes * Matrix4x4.TRS(translated, nextRotation, Vector3.one));
            Check(Mathf.Abs(a.max.x - bRot.min.x) < .001f, "Rotated bounds directional contact without rotating next piece");
            Check(DirectionalSnapMath.OpposingPoint(SnapDirection.Right, Quaternion.identity, Quaternion.Euler(0, 90, 0)) == SnapDirection.Back, "Rotated opposing manual connection");
            var connection = new Vector3(8, 1, 4); var nextPoint = new Vector3(-2, 3, 5);
            var connectedPosition = DirectionalSnapMath.Connect(connection, nextPoint, nextRotation, Vector3.one * 2, Vector3.zero);
            Check(Vector3.Distance(connectedPosition + nextRotation * (nextPoint * 2), connection) < .001f, "Manual connection respects pivot, scale and rotation");
            // Analyzer must ignore helper geometry but retain transformed children.
            var helperRoot = new GameObject("Analyzer"); var visible = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visible.transform.SetParent(helperRoot.transform); visible.transform.localScale = new Vector3(8, 1, 8);
            var helper = GameObject.CreatePrimitive(PrimitiveType.Cube); helper.transform.SetParent(helperRoot.transform);
            helper.transform.localScale = Vector3.one * 100; helper.AddComponent<DimensionIgnore>();
            Check(PrefabDimensionAnalyzer.Analyze(helperRoot, out var analyzed) && Mathf.Abs(analyzed.size.x - 8) < .001f, "Ignore editor helper geometry");
            UnityEngine.Object.DestroyImmediate(helperRoot);
            latest.gameObject.SetActive(false); tracker.Validate(); Check(tracker.Reference == null, "Disabled reference invalidates safely");
            latest.gameObject.SetActive(true); tracker.Register(latest);
            var originalScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            EditorSceneManager.SaveScene(originalScene, AssetDatabase.GenerateUniqueAssetPath("Assets/AdaptiveChecks/ReferenceScene.unity"));
            System.IO.File.WriteAllText("Library/CoelhoAdaptiveValidationScene.txt", originalScene.path);
            var otherScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(otherScene); tracker.Validate();
            Check(tracker.Reference == null, "Reference from another active scene invalidates safely");
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(originalScene); tracker.Register(latest);
            EditorSceneManager.CloseScene(otherScene, true);
            UnityEngine.Object.DestroyImmediate(latest.gameObject); tracker.Validate(); Check(tracker.Reference == null, "Deleted reference invalidates safely");
            Call(painter, "UpdateControl", true); p8.manualDimensions = false; p8.scale = Vector3.one; p8.rotation = Vector3.zero;
            var invalid = Preview(painter, Vector3.zero); Check(!Value<bool>(invalid, "valid"), "Ctrl without reference is invalid");
            p8.snap = false;
            var free = Preview(painter, new Vector3(120, 0, 120)); Check(!Value<bool>(free, "directional"), "Disabled snap disables directional mode");
            Call(painter, "SetActive", false); Check(!(bool)type.GetProperty("Active", Flags).GetValue(painter), "Stop cancels placement");
            var window = EditorWindow.GetWindow<WorldBuilderWindow>(); window.CreateGUI();
            window.GetType().GetMethod("ShowPage", Flags).Invoke(window, new object[] { "Smart Grid & Directional Placement" }); window.Close();
            window = EditorWindow.GetWindow<WorldBuilderWindow>(); window.CreateGUI(); window.Close();
            s.Persist();
            Check(s.profiles.Exists(p => p.guid == Guid(road8) && p.category == ModularCategory.Roads), "Profile survives window close/reopen");
            painter.Dispose(); painter = null;
            Debug.Log("COELHO_ADAPTIVE_GRID_PASSED: " + checks + " integration checks"); EditorApplication.Exit(0);
        }
        catch (Exception e) { Debug.LogException(e); painter?.Dispose(); EditorApplication.Exit(1); }
    }
    public static void VerifyRestart()
    {
        try
        {
            var s = BuilderSettings.instance; var p = s.Profile(s.selectedPrefabGuid);
            Check(s.autoGridSize && s.ctrlDirectionalSnap && s.gridOrigin == new Vector3(3, 0, -5), "Adaptive settings after process restart");
            Check(p.category == ModularCategory.Roads && !p.snap && !p.manualDimensions, "Category and profile after process restart");
            EditorSceneManager.OpenScene(System.IO.File.ReadAllText("Library/CoelhoAdaptiveValidationScene.txt"));
            var tracker = new LastPlacedPrefabTracker(); tracker.Reconcile();
            Check(tracker.Reference != null && tracker.Reference.modularCategory == ModularCategory.Roads, "Recover modular reference from saved scene");
            Check(tracker.Reference.placementOrder > 0 && tracker.Reference.fittingBounds.size.x > 0, "Fitting metadata survives scene serialization");
            Debug.Log("COELHO_ADAPTIVE_RESTART_PASSED: " + checks + " checks"); EditorApplication.Exit(0);
        }
        catch (Exception e) { Debug.LogException(e); EditorApplication.Exit(1); }
    }
}
