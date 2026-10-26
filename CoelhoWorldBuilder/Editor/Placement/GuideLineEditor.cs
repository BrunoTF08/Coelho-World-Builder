using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace CoelhoLabs.WorldBuilder.Editor
{
    [CustomEditor(typeof(GuideLine))]
    public sealed class GuideLineEditor : UnityEditor.Editor
    {
        private bool adding;
        private int selected = -1;
        [MenuItem("GameObject/Coelho Labs/Linha verde editável", false, 10)]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || PrefabStageUtility.GetCurrentPrefabStage() != null)
            { BuilderLog.Info("Crie linhas na cena fora do Play Mode e do Prefab Mode."); return; }
            var go = new GameObject("Coelho • Linha verde");
            Undo.RegisterCreatedObjectUndo(go, "Criar linha verde");
            Undo.AddComponent<GuideLine>(go);
            if (SceneView.lastActiveSceneView != null) go.transform.position = SceneView.lastActiveSceneView.pivot;
            Selection.activeGameObject = go;
            BuilderLog.Info("Linha criada. Edite os pontos pelo Inspector ou pela Scene View.");
        }
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.HelpBox("Arraste os pontos verdes para movê-los. Para remover, informe o índice abaixo. No modo adicionar, clique sobre Colliders; sem superfície, usa o plano horizontal da linha. Escape encerra.", MessageType.Info);
            adding = GUILayout.Toggle(adding, "Adicionar pontos na Scene View", "Button");
            selected = EditorGUILayout.IntField("Índice do ponto", selected);
            using (new EditorGUI.DisabledScope(selected < 0 || selected >= ((GuideLine)target).points.Count))
                if (GUILayout.Button("Remover ponto"))
                {
                    var line = (GuideLine)target;
                    Undo.RecordObject(line, "Remover ponto da linha");
                    line.points.RemoveAt(selected); selected = -1; Save(line);
                }
        }
        private static void Save(GuideLine line)
        {
            EditorUtility.SetDirty(line);
            PrefabUtility.RecordPrefabInstancePropertyModifications(line);
            EditorSceneManager.MarkSceneDirty(line.gameObject.scene);
            SceneView.RepaintAll();
        }
        private void OnSceneGUI()
        {
            var line = (GuideLine)target;
            if (EditorApplication.isPlayingOrWillChangePlaymode || PrefabStageUtility.GetCurrentPrefabStage() != null) return;
            var e = Event.current;
            if (adding && e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            { adding = false; e.Use(); Repaint(); return; }
            if (adding && !e.alt)
            {
                int control = GUIUtility.GetControlID(FocusType.Passive);
                if (e.type == EventType.Layout) HandleUtility.AddDefaultControl(control);
                var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                Vector3 point = default; bool found = false; float nearest = float.PositiveInfinity;
                foreach (var hit in Physics.RaycastAll(ray, 10000, BuilderSettings.instance.layers, QueryTriggerInteraction.Ignore))
                    if (hit.collider.gameObject.scene == line.gameObject.scene && hit.distance < nearest)
                    { nearest = hit.distance; point = hit.point; found = true; }
                if (!found && new Plane(Vector3.up, line.transform.position).Raycast(ray, out var distance))
                { point = ray.GetPoint(distance); found = true; }
                if (found)
                {
                    Handles.color = line.color;
                    if (line.points.Count > 0) Handles.DrawDottedLine(line.transform.TransformPoint(line.points[line.points.Count - 1]), point, 4);
                    if (e.type == EventType.MouseDown && e.button == 0)
                    {
                        Undo.RecordObject(line, "Adicionar ponto à linha");
                        line.points.Add(line.transform.InverseTransformPoint(point));
                        selected = line.points.Count - 1; Save(line); e.Use(); Repaint();
                    }
                }
                if (e.type == EventType.MouseMove) SceneView.RepaintAll();
                return;
            }
            for (int i = 0; i < line.points.Count; i++)
            {
                Vector3 world = line.transform.TransformPoint(line.points[i]);
                Handles.color = i == selected ? Color.yellow : line.color;
                EditorGUI.BeginChangeCheck();
                Vector3 moved = Handles.FreeMoveHandle(world, HandleUtility.GetHandleSize(world) * .08f, Vector3.zero, Handles.SphereHandleCap);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(line, "Mover ponto da linha");
                    line.points[i] = line.transform.InverseTransformPoint(moved); selected = i; Save(line); Repaint();
                }
                Handles.Label(world, "  " + i);
            }
        }
        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawLine(GuideLine line, GizmoType type)
        {
            if (line.points == null || line.points.Count < 2) return;
            var points = new Vector3[line.points.Count + (line.closed ? 1 : 0)];
            for (int i = 0; i < line.points.Count; i++) points[i] = line.transform.TransformPoint(line.points[i]);
            if (line.closed) points[points.Length - 1] = points[0];
            using (new Handles.DrawingScope(line.color)) Handles.DrawAAPolyLine(line.width, points);
        }
    }
}
