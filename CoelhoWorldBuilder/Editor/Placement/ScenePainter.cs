using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoelhoLabs.WorldBuilder.Editor
{
    internal sealed class ScenePainter : IDisposable
    {
        private struct MeshPart { public Mesh mesh; public Matrix4x4 relative; public bool owned; }
        private struct Candidate
        {
            public Vector3 position, scale, target;
            public Quaternion rotation;
            public Bounds bounds;
            public Bounds fitting;
            public bool directional;
            public bool valid;
            public string reason;
        }
        private struct Occupant { public Vector3 position; public Bounds bounds; public GameObject owner; }
        private readonly List<MeshPart> meshes = new List<MeshPart>();
        private readonly List<Candidate> preview = new List<Candidate>(100);
        private readonly Dictionary<Vector2Int, List<Occupant>> occupied = new Dictionary<Vector2Int, List<Occupant>>();
        private readonly List<Occupant> largeObjects = new List<Occupant>();
        private readonly HashSet<Vector2Int> strokeCells = new HashSet<Vector2Int>();
        private readonly Collider[] overlaps = new Collider[256];
        private readonly RaycastHit[] rayHits = new RaycastHit[256];
        private Material ghost;
        private GameObject prefab;
        private Bounds localBounds;
        private bool active, occupancyDirty = true, geometryDirty = true, pendingStroke;
        private int stamp, undoGroup = -1, ownedControl;
        private Vector3 lastStamp;
        private Vector2 lastMouse;
        private Tool previousTool;
        private bool ownsTool;
        private bool ctrlHeld;
        private bool ctrlPoseCaptured;
        private Quaternion ctrlRotation;
        private Vector3 ctrlScale;
        private Vector3 capturedProfileRotation, capturedProfileScale;
        private readonly LastPlacedPrefabTracker tracker = new LastPlacedPrefabTracker();
        internal string LastReference => tracker.Reference == null ? "Nenhuma" : tracker.Reference.name;
        internal string CurrentDirection => ctrlHeld && DirectionalAvailable ? currentDirection.ToString() : "Normal";
        private SnapDirection currentDirection;
        internal void SelectPrefab(GameObject selected)
        {
            FinishStroke(); preview.Clear(); ctrlPoseCaptured = false;
            S.selectedPrefabGuid = selected == null ? "" : AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(selected));
            S.InvalidateSelectedPrefabCache();
            PrefabDimensionAnalyzer.ApplyAdaptiveGrid(S); S.Persist(); geometryDirty = true;
            if (selected == null) SetActive(false);
            else if (active) RefreshGeometry();
            SelectionChanged?.Invoke(); SettingsChanged?.Invoke();
            SceneView.RepaintAll();
        }
        private bool DirectionalAvailable => S.ctrlDirectionalSnap && P.snap && P.category != ModularCategory.Decorative
            && tracker.Reference != null && tracker.Reference.modularCategory == P.category;
        private void UpdateControl(bool pressed)
        {
            if (ctrlHeld == pressed) return;
            ctrlHeld = pressed; ctrlPoseCaptured = false;
            if (pressed && preview.Count > 0)
            {
                ctrlRotation = preview[0].rotation; ctrlScale = preview[0].scale; ctrlPoseCaptured = true;
                capturedProfileRotation = P.rotation; capturedProfileScale = P.scale;
            }
        }
        internal bool Active => active;
        internal Action SettingsChanged;
        internal event Action SelectionChanged;
        private BuilderSettings S => BuilderSettings.instance;
        private PrefabPlacementProfile P => S.Profile(S.selectedPrefabGuid);
        internal ScenePainter()
        {
            S.InvalidateSelectedPrefabCache();
            SceneView.duringSceneGui += OnSceneGUI;
            Undo.undoRedoPerformed += UndoChanged;
            EditorApplication.hierarchyChanged += MarkOccupancy;
            EditorApplication.projectChanged += MarkGeometry;
            ObjectChangeEvents.changesPublished += ObjectChanges;
            EditorSceneManager.activeSceneChangedInEditMode += ActiveSceneChanged;
        }
        private void MarkOccupancy() { occupancyDirty = true; SceneView.RepaintAll(); }
        private void ActiveSceneChanged(Scene before, Scene after)
        { FinishStroke(); tracker.Validate(); MarkOccupancy(); SettingsChanged?.Invoke(); }
        private void UndoChanged() { tracker.Reconcile(); MarkOccupancy(); SettingsChanged?.Invoke(); }
        private void MarkGeometry() { geometryDirty = true; S.InvalidateSelectedPrefabCache(); }
        private void ObjectChanges(ref ObjectChangeEventStream stream)
        {
            for (int i = 0; i < stream.length; i++)
                if (stream.GetEventType(i) == ObjectChangeKind.ChangeGameObjectOrComponentProperties)
                { MarkOccupancy(); return; }
        }
        internal void SetActive(bool value)
        {
            FinishStroke(); active = value; stamp = 0; preview.Clear();
            if (value)
            {
                if (S.SelectedPrefab == null)
                {
                    foreach (var item in S.prefabs)
                        if (item != null && PrefabUtility.IsPartOfPrefabAsset(item))
                        { S.selectedPrefabGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(item)); break; }
                }
                if (S.SelectedPrefab == null) { active = false; BuilderLog.Info("Selecione um prefab do Project."); return; }
                if (!ownsTool) { previousTool = Tools.current; ownsTool = true; }
                Tools.current = Tool.None;
                PrefabDimensionAnalyzer.ApplyAdaptiveGrid(S);
                S.Persist(); RefreshGeometry(); RebuildOccupancy(); tracker.Reconcile();
            }
            else { ctrlHeld = false; ctrlPoseCaptured = false; ReleaseGeometry(); RestoreTool(); }
            BuilderLog.Info(active ? "Construção ativa. Clique: colocar; R: girar; G: snap; Escape: sair." : "Construção desativada.");
            SceneView.RepaintAll();
        }
        public void Dispose()
        {
            SettingsChanged = null;
            SelectionChanged = null;
            FinishStroke(); ReleaseGeometry(); RestoreTool();
            SceneView.duringSceneGui -= OnSceneGUI; Undo.undoRedoPerformed -= UndoChanged;
            EditorApplication.hierarchyChanged -= MarkOccupancy; EditorApplication.projectChanged -= MarkGeometry;
            ObjectChangeEvents.changesPublished -= ObjectChanges;
            EditorSceneManager.activeSceneChangedInEditMode -= ActiveSceneChanged;
        }
        private void FinishStroke()
        {
            if (undoGroup >= 0) Undo.CollapseUndoOperations(undoGroup);
            undoGroup = -1; pendingStroke = false; strokeCells.Clear();
            if (ownedControl != 0 && GUIUtility.hotControl == ownedControl) GUIUtility.hotControl = 0;
            ownedControl = 0;
        }
        private void RestoreTool()
        {
            if (ownsTool && Tools.current == Tool.None) Tools.current = previousTool;
            ownsTool = false;
        }
        private void ReleaseGeometry()
        {
            foreach (var part in meshes) if (part.owned && part.mesh != null) UnityEngine.Object.DestroyImmediate(part.mesh);
            meshes.Clear(); prefab = null;
            if (ghost != null) UnityEngine.Object.DestroyImmediate(ghost);
            ghost = null;
        }
        private void RefreshGeometry()
        {
            if (!geometryDirty && prefab == S.SelectedPrefab) return;
            ReleaseGeometry(); prefab = S.SelectedPrefab; geometryDirty = false;
            if (prefab == null) return;
            PrefabDimensionAnalyzer.Analyze(prefab, out localBounds);
            if (PrefabDimensionAnalyzer.ApplyAdaptiveGrid(S)) { S.Persist(); SettingsChanged?.Invoke(); }
            foreach (var filter in prefab.GetComponentsInChildren<MeshFilter>(true))
            {
                var renderer = filter.GetComponent<MeshRenderer>();
                if (filter.sharedMesh == null || renderer == null || !renderer.enabled || !EnabledInPrefab(filter.transform)) continue;
                meshes.Add(new MeshPart { mesh = filter.sharedMesh, relative = prefab.transform.worldToLocalMatrix * filter.transform.localToWorldMatrix });
            }
            foreach (var skin in prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (skin.sharedMesh == null || !skin.enabled || !EnabledInPrefab(skin.transform)) continue;
                var mesh = new Mesh { hideFlags = HideFlags.HideAndDontSave }; skin.BakeMesh(mesh);
                meshes.Add(new MeshPart { mesh = mesh, relative = prefab.transform.worldToLocalMatrix * skin.transform.localToWorldMatrix, owned = true });
            }
            var shader = Shader.Find("Hidden/CoelhoLabs/PlacementGhost");
            if (shader != null) ghost = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        }
        private static bool EnabledInPrefab(Transform transform)
        {
            for (var current = transform; current != null; current = current.parent)
                if (!current.gameObject.activeSelf) return false;
            return true;
        }
        private static Vector2Int Bucket(Vector3 p) => new Vector2Int(Mathf.FloorToInt(p.x / 16), Mathf.FloorToInt(p.z / 16));
        private void Index(Occupant item)
        {
            var min = Bucket(item.bounds.min); var max = Bucket(item.bounds.max);
            if ((long)(max.x - min.x + 1) * (max.y - min.y + 1) > 4096) { largeObjects.Add(item); return; }
            for (int x = min.x; x <= max.x; x++) for (int z = min.y; z <= max.y; z++)
            {
                var key = new Vector2Int(x, z);
                if (!occupied.TryGetValue(key, out var list)) occupied.Add(key, list = new List<Occupant>());
                list.Add(item);
            }
        }
        private void RebuildOccupancy()
        {
            occupied.Clear(); largeObjects.Clear(); occupancyDirty = false;
            foreach (var item in UnityEngine.Object.FindObjectsByType<GeneratedObject>(FindObjectsSortMode.None))
            {
                if (item.gameObject.scene != SceneManager.GetActiveScene()) continue;
                var renderers = item.GetComponentsInChildren<Renderer>();
                var bounds = new Bounds(item.transform.position, Vector3.zero);
                bool found = false;
                foreach (var renderer in renderers) { if (!found) bounds = renderer.bounds; else bounds.Encapsulate(renderer.bounds); found = true; }
                Index(new Occupant { position = item.transform.position, bounds = bounds, owner = item.gameObject });
            }
        }
        private bool Surface(Ray ray, out Vector3 point, out Vector3 normal)
        {
            point = default; normal = Vector3.up; float nearest = float.PositiveInfinity;
            int count = Physics.RaycastNonAlloc(ray, rayHits, 10000, S.layers, QueryTriggerInteraction.Ignore);
            if (count == rayHits.Length) return false; // Do not choose an arbitrary hit from a truncated buffer.
            for (int i = 0; i < count; i++)
            {
                var hit = rayHits[i];
                if (hit.collider.gameObject.scene != SceneManager.GetActiveScene() || hit.collider.GetComponentInParent<GeneratedObject>() != null || hit.distance >= nearest) continue;
                point = hit.point; normal = hit.normal; nearest = hit.distance;
            }
            return !float.IsPositiveInfinity(nearest);
        }
        private bool Target(Ray ray, out Vector3 point, out Vector3 normal)
        {
            normal = Vector3.up; point = default;
            if (S.snapMode != SnapMode.Grid && Surface(ray, out point, out normal)) return true;
            var plane = new Plane(Vector3.up, new Vector3(0, S.gridOrigin.y + S.gridHeight, 0));
            if (!plane.Raycast(ray, out float distance)) return false;
            point = ray.GetPoint(distance); return true;
        }
        private Vector2Int Cell(Vector3 p) => new Vector2Int(Mathf.RoundToInt((p.x - S.gridOrigin.x) / S.gridCell.x), Mathf.RoundToInt((p.z - S.gridOrigin.z) / S.gridCell.y));
        private bool Intersects(Occupant item, Candidate candidate)
        {
            if (candidate.directional && tracker.Reference != null && item.owner == tracker.Reference.gameObject && !PenetratesReference(candidate)) return false;
            var bounds = candidate.bounds; bounds.Expand(-.02f);
            return (S.avoidOverlap && bounds.Intersects(item.bounds)) || (item.position - candidate.position).sqrMagnitude < S.spacing * S.spacing;
        }
        private bool Blocked(Candidate candidate)
        {
            var query = candidate.bounds; query.Encapsulate(candidate.position - Vector3.one * S.spacing); query.Encapsulate(candidate.position + Vector3.one * S.spacing);
            var min = Bucket(query.min); var max = Bucket(query.max);
            if ((long)(max.x - min.x + 1) * (max.y - min.y + 1) > 4096) return true;
            for (int x = min.x; x <= max.x; x++) for (int z = min.y; z <= max.y; z++)
                if (occupied.TryGetValue(new Vector2Int(x, z), out var list))
                    foreach (var item in list) if (Intersects(item, candidate)) return true;
            foreach (var item in largeObjects) if (Intersects(item, candidate)) return true;
            foreach (var other in preview)
                if (other.valid && ((!candidate.directional && P.snap && S.snapMode != SnapMode.Free && Cell(other.target) == Cell(candidate.target))
                    || Intersects(new Occupant { position = other.position, bounds = other.bounds }, candidate))) return true;
            if (!S.avoidOverlap) return false;
            var extents = Vector3.Max(Vector3.Scale(localBounds.extents, new Vector3(Mathf.Abs(candidate.scale.x), Mathf.Abs(candidate.scale.y), Mathf.Abs(candidate.scale.z))) - Vector3.one * .015f, Vector3.one * .001f);
            var center = candidate.position + candidate.rotation * Vector3.Scale(localBounds.center, candidate.scale);
            int count = Physics.OverlapBoxNonAlloc(center, extents, overlaps, candidate.rotation, ~0, QueryTriggerInteraction.Ignore);
            if (count == overlaps.Length) return true;
            for (int i = 0; i < count; i++)
                if (overlaps[i].gameObject.scene == SceneManager.GetActiveScene())
                {
                    var generated = overlaps[i].GetComponentInParent<GeneratedObject>();
                    if (candidate.directional && generated == tracker.Reference && !PenetratesReference(candidate)) continue;
                    return true;
                }
            return false;
        }
        private bool PenetratesReference(Candidate candidate)
        {
            var reference = tracker.Reference;
            var axes = Matrix4x4.Rotate(Quaternion.Euler(0, -reference.transform.eulerAngles.y, 0));
            var a = SmartPlacementMath.TransformBounds(reference.fittingBounds, axes * reference.transform.localToWorldMatrix);
            var b = SmartPlacementMath.TransformBounds(candidate.fitting, axes * Matrix4x4.TRS(candidate.position, candidate.rotation, candidate.scale));
            const float tolerance = .001f;
            return Mathf.Min(a.max.x, b.max.x) - Mathf.Max(a.min.x, b.min.x) > tolerance
                && Mathf.Min(a.max.y, b.max.y) - Mathf.Max(a.min.y, b.min.y) > tolerance
                && Mathf.Min(a.max.z, b.max.z) - Mathf.Max(a.min.z, b.min.z) > tolerance;
        }
        private Candidate MakeCandidate(Vector3 target, Vector3 normal, System.Random random)
        {
            var profile = P;
            tracker.Validate();
            bool directional = ctrlHeld && DirectionalAvailable;
            bool snapped = profile.snap && S.snapMode != SnapMode.Free;
            var pointer = target;
            if (snapped && !directional)
            {
                target = SmartPlacementMath.Snap(target, S.gridOrigin, S.gridCell);
                if (S.snapMode == SnapMode.Surface)
                {
                    if (Surface(new Ray(target + Vector3.up * 1000, Vector3.down), out var surface, out var surfaceNormal))
                    { target = surface; normal = surfaceNormal; }
                    else target.y = S.gridOrigin.y + S.gridHeight;
                }
            }
            Vector3 rotation = profile.rotation;
            if (!snapped && !directional) rotation.y += PlacementMath.Range(random, S.yaw);
            if (S.rotationSnap) rotation.y = Mathf.Round(rotation.y / S.rotationStep) * S.rotationStep;
            var orientation = (profile.alignToSurface ? Quaternion.FromToRotation(Vector3.up, normal) : Quaternion.identity) * Quaternion.Euler(rotation) * prefab.transform.rotation;
            Vector3 scale = Vector3.Scale(prefab.transform.localScale, profile.scale) * PlacementMath.Range(random, S.scale);
            if (directional)
            {
                if (!ctrlPoseCaptured || capturedProfileRotation != profile.rotation || capturedProfileScale != profile.scale)
                {
                    ctrlRotation = orientation; ctrlScale = scale; ctrlPoseCaptured = true;
                    capturedProfileRotation = profile.rotation; capturedProfileScale = profile.scale;
                }
                orientation = ctrlRotation; scale = ctrlScale;
            }
            // Corner anchoring uses rotated bounds so R preserves horizontal edge alignment.
            var fitting = PrefabDimensionAnalyzer.FittingBounds(localBounds, profile, prefab);
            Vector3 anchor = SmartPlacementMath.Anchor(fitting, profile);
            var matrix = Matrix4x4.TRS(Vector3.zero, orientation, scale);
            Vector3 transformedAnchor = matrix.MultiplyPoint3x4(anchor);
            if (profile.anchorMode == AnchorMode.BottomCorner) transformedAnchor = SmartPlacementMath.TransformBounds(fitting, matrix).min;
            Vector3 position = target + profile.offset + Vector3.up * S.offset - transformedAnchor;
            if (directional)
            {
                var reference = tracker.Reference;
                var referenceWorld = SmartPlacementMath.TransformBounds(reference.fittingBounds, reference.transform.localToWorldMatrix);
                currentDirection = DirectionalSnapMath.Direction(pointer, referenceWorld, reference.transform.rotation);
                position = DirectionalSnapMath.Adjacent(reference.fittingBounds, reference.transform.localToWorldMatrix, fitting,
                    orientation, scale, currentDirection, S.edgeAlignment, S.snapOffset + profile.offset + Vector3.up * S.offset);
                if (profile.useConnectionPoints && reference.connectionPoints != null && reference.connectionPoints.Length == 4)
                {
                    var nextPoint = DirectionalSnapMath.OpposingPoint(currentDirection, reference.transform.rotation, orientation);
                    var point = nextPoint == SnapDirection.Right ? profile.rightConnection : nextPoint == SnapDirection.Left ? profile.leftConnection
                        : nextPoint == SnapDirection.Forward ? profile.forwardConnection : profile.backConnection;
                    position = DirectionalSnapMath.Connect(reference.transform.TransformPoint(reference.connectionPoints[(int)currentDirection]), point,
                        orientation, scale, S.snapOffset + profile.offset + Vector3.up * S.offset);
                }
            }
            var candidate = new Candidate { position = position, target = target, rotation = orientation, scale = scale,
                bounds = SmartPlacementMath.TransformBounds(localBounds, Matrix4x4.TRS(position, orientation, scale)), fitting = fitting, directional = directional, valid = true };
            if (ctrlHeld && S.ctrlDirectionalSnap && profile.snap && profile.category != ModularCategory.Decorative && !DirectionalAvailable)
            { candidate.valid = false; candidate.reason = "Ctrl: coloque primeiro uma referência da mesma categoria"; }
            else if (directional && PenetratesReference(candidate)) { candidate.valid = false; candidate.reason = "Snap Offset causa sobreposição com a referência"; }
            else if (!directional && snapped && strokeCells.Contains(Cell(target))) { candidate.valid = false; candidate.reason = "Célula já usada neste gesto"; }
            else if (Blocked(candidate)) { candidate.valid = false; candidate.reason = "Sobreposição ou espaçamento mínimo"; }
            return candidate;
        }
        private void BuildPreview(Vector3 point, Vector3 normal)
        {
            preview.Clear();
            var random = new System.Random(unchecked(S.seed + stamp * 7919));
            int count = (ctrlHeld && S.ctrlDirectionalSnap && P.category != ModularCategory.Decorative) || S.mode == PlacementMode.Single ? 1 : S.density;
            for (int i = 0; i < count; i++)
            {
                Vector3 target = point;
                if (i > 0)
                {
                    var disk = PlacementMath.Disk(random, S.radius); target += new Vector3(disk.x, 0, disk.y);
                    if (S.snapMode != SnapMode.Grid && Surface(new Ray(target + Vector3.up * 1000, Vector3.down), out var surface, out var n))
                    { target = surface; normal = n; }
                }
                preview.Add(MakeCandidate(target, normal, random));
            }
        }
        private void Place()
        {
            if (!active || prefab == null || !PrefabUtility.IsPartOfPrefabAsset(prefab)) return;
            int placed = 0;
            foreach (var candidate in preview)
            {
                if (!candidate.valid) continue;
                if (undoGroup < 0)
                { Undo.IncrementCurrentGroup(); undoGroup = Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Coelho: construir prefabs"); }
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, SceneManager.GetActiveScene());
                Undo.RegisterCreatedObjectUndo(instance, "Coelho: posicionar prefab");
                Undo.RecordObject(instance.transform, "Coelho: transformação");
                instance.transform.SetPositionAndRotation(candidate.position, candidate.rotation); instance.transform.localScale = candidate.scale;
                PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
                PlacementColliders.ReplaceBoxes(instance);
                var metadata = Undo.AddComponent<GeneratedObject>(instance);
                Undo.RecordObject(metadata, "Coelho: metadados");
                metadata.persistentId = Guid.NewGuid().ToString("N"); metadata.sourcePrefabGuid = S.selectedPrefabGuid; metadata.seed = S.seed;
                metadata.modularCategory = P.category; metadata.fittingBounds = candidate.fitting; metadata.placementOrder = DateTime.UtcNow.Ticks;
                metadata.connectionPoints = P.useConnectionPoints ? P.ConnectionPoints() : null;
                tracker.Register(metadata);
                if (!candidate.directional && P.snap && S.snapMode != SnapMode.Free) strokeCells.Add(Cell(candidate.target));
                Index(new Occupant { position = candidate.position, bounds = candidate.bounds, owner = instance }); placed++;
            }
            if (placed > 0) { stamp++; BuilderLog.Info($"{placed} prefab(s) colocado(s). Ctrl+Z desfaz o gesto."); }
            else if (preview.Count > 0) BuilderLog.Info(preview[0].reason ?? "Posição inválida.");
        }
        private void DrawPreview()
        {
            foreach (var candidate in preview)
            {
                if (candidate.directional && !S.showSnapPreview) continue;
                var color = candidate.valid ? new Color(.2f, 1, .45f, .35f) : new Color(1, .15f, .15f, .4f);
                if (ghost != null)
                {
                    ghost.SetColor("_Color", color);
                    if (ghost.SetPass(0))
                    {
                        var root = Matrix4x4.TRS(candidate.position, candidate.rotation, candidate.scale);
                        foreach (var part in meshes) for (int i = 0; i < part.mesh.subMeshCount; i++) Graphics.DrawMeshNow(part.mesh, root * part.relative, i);
                    }
                }
                Handles.color = new Color(color.r, color.g, color.b, 1);
                Handles.DrawWireCube(candidate.bounds.center, candidate.bounds.size);
            }
            if (preview.Count > 0 && (!preview[0].directional || S.showSnapPreview))
                Handles.Label(preview[0].bounds.max, preview[0].valid ? "Clique para colocar • R girar • G snap" : preview[0].reason);
        }
        private void DrawGrid()
        {
            if (!S.showGrid) return;
            float y = S.gridOrigin.y + S.gridHeight;
            int nx = Mathf.Min(100, Mathf.FloorToInt(S.gridArea.x * .5f / S.gridCell.x));
            int nz = Mathf.Min(100, Mathf.FloorToInt(S.gridArea.y * .5f / S.gridCell.y));
            Handles.color = new Color(.35f, 1, .6f, .55f);
            float xmin = S.gridOrigin.x - nx * S.gridCell.x, xmax = S.gridOrigin.x + nx * S.gridCell.x;
            float zmin = S.gridOrigin.z - nz * S.gridCell.y, zmax = S.gridOrigin.z + nz * S.gridCell.y;
            for (int i = -nx; i <= nx; i++) { float x = S.gridOrigin.x + i * S.gridCell.x; Handles.DrawLine(new Vector3(x, y, zmin), new Vector3(x, y, zmax)); }
            for (int i = -nz; i <= nz; i++) { float z = S.gridOrigin.z + i * S.gridCell.y; Handles.DrawLine(new Vector3(xmin, y, z), new Vector3(xmax, y, z)); }
        }
        private void ApplyShortcut(KeyCode key)
        {
            if (key == KeyCode.R) P.rotation.y = Mathf.Repeat(P.rotation.y + S.rotationStep, 360);
            else if (key == KeyCode.G) P.snap = !P.snap;
            S.Persist(); SettingsChanged?.Invoke();
        }
        private bool TryCyclePrefab(Event e, bool sceneFocused)
        {
            if (!active || !sceneFocused || EditorGUIUtility.editingTextField || e.type != EventType.KeyDown || !e.control
                || e.alt || e.shift || e.command || (e.keyCode != KeyCode.Q && e.keyCode != KeyCode.E)) return false;
            var library = PrefabLibrary.instance; library.Initialize();
            if (library.Next(S.selectedPrefabGuid, e.keyCode == KeyCode.Q ? -1 : 1, out var next)) SelectPrefab(next);
            else BuilderLog.Info("A categoria ativa não possui prefabs válidos. Adicione prefabs ou ative outra categoria.");
            e.Use(); SceneView.RepaintAll(); return true;
        }
        private void OnSceneGUI(SceneView view)
        {
            var e = Event.current;
            if (active && ctrlHeld != e.control) { UpdateControl(e.control); view.Repaint(); }
            if (e.type == EventType.Repaint) DrawGrid();
            if (!active) return;
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape) { SetActive(false); e.Use(); return; }
            if (e.rawType == EventType.MouseUp && ownedControl != 0)
            { FinishStroke(); e.Use(); view.Repaint(); return; }
            if (e.type == EventType.Ignore || e.type == EventType.MouseLeaveWindow) FinishStroke();
            if (EditorApplication.isPlayingOrWillChangePlaymode || PrefabStageUtility.GetCurrentPrefabStage() != null) return;
            if (e.alt || Tools.current == Tool.View || EditorGUIUtility.editingTextField || e.button > 0
                || view.rootVisualElement.panel?.focusController?.focusedElement is UnityEngine.UIElements.TextElement) return;
            int control = GUIUtility.GetControlID(FocusType.Passive);
            if (GUIUtility.hotControl != 0 && GUIUtility.hotControl != ownedControl) return;
            if (TryCyclePrefab(e, EditorWindow.focusedWindow == view)) return;
            if (e.type == EventType.Layout) HandleUtility.AddDefaultControl(control);
            if (e.type == EventType.KeyDown || e.type == EventType.KeyUp) view.Repaint();
            if (e.type == EventType.KeyDown && !e.control && !e.command && !e.shift && (e.keyCode == KeyCode.R || e.keyCode == KeyCode.G))
            {
                ApplyShortcut(e.keyCode); e.Use(); view.Repaint(); return;
            }
            RefreshGeometry(); if (prefab == null) return;
            if (occupancyDirty && !pendingStroke) RebuildOccupancy();
            if (e.type == EventType.MouseMove || e.type == EventType.MouseDown || e.type == EventType.MouseDrag) lastMouse = e.mousePosition;
            if (!Target(HandleUtility.GUIPointToWorldRay(lastMouse), out var point, out var normal)) { preview.Clear(); return; }
            BuildPreview(point, normal);
            if (e.type == EventType.Repaint) DrawPreview();
            if (e.type == EventType.MouseDown && e.button == 0 && HandleUtility.nearestControl == control)
            {
                FinishStroke(); ownedControl = control; GUIUtility.hotControl = control; pendingStroke = true;
                try { Place(); lastStamp = point; } catch (Exception exception) { Debug.LogException(exception); SetActive(false); }
                e.Use(); view.Repaint();
            }
            else if (!ctrlHeld && e.type == EventType.MouseDrag && e.button == 0 && pendingStroke && S.mode == PlacementMode.Brush)
            {
                if (Vector3.Distance(lastStamp, point) >= Mathf.Max(.01f, S.spacing))
                {
                    try
                    {
                        var from = lastStamp;
                        int steps = Mathf.Clamp(Mathf.CeilToInt(Vector3.Distance(from, point) / S.spacing), 1, Mathf.Max(1, Mathf.Min(32, 100 / S.density)));
                        for (int i = 1; i <= steps; i++) { BuildPreview(Vector3.Lerp(from, point, i / (float)steps), normal); Place(); }
                        lastStamp = point;
                    }
                    catch (Exception exception) { Debug.LogException(exception); SetActive(false); }
                }
                e.Use(); view.Repaint();
            }
            if (e.type == EventType.MouseMove) view.Repaint();
        }
    }
}
