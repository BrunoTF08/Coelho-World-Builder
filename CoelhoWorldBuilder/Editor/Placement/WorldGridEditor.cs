using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace CoelhoLabs.WorldBuilder.Editor
{
    [CustomEditor(typeof(WorldGrid))]
    public sealed class WorldGridEditor : UnityEditor.Editor
    {
        [MenuItem("GameObject/Coelho Labs/Grade verde", false, 10)]
        public static void Create()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || PrefabStageUtility.GetCurrentPrefabStage() != null)
            { BuilderLog.Info("Crie a grade na cena fora do Play Mode e do Prefab Mode."); return; }
            var go = new GameObject("Coelho • Grade verde");
            Undo.RegisterCreatedObjectUndo(go, "Criar grade verde");
            Undo.AddComponent<WorldGrid>(go);
            if (SceneView.lastActiveSceneView != null) go.transform.position = SceneView.lastActiveSceneView.pivot;
            Selection.activeGameObject = go;
            BuilderLog.Info("Grade criada. Ajuste Cell Size, Columns e Rows no Inspector; mova/gire com os controles do Unity.");
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var grid = (WorldGrid)target;
            EditorGUILayout.HelpBox($"Grade: {grid.columns * grid.cellSize:0.##} × {grid.rows * grid.cellSize:0.##} unidades locais. Use Move/Rotate para ajustar posição, altura e orientação. Ative Gizmos na Scene View. A grade é uma guia plana do editor.", MessageType.Info);
            if (GUI.changed) SceneView.RepaintAll();
        }

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
        private static void DrawGrid(WorldGrid grid, GizmoType type)
        {
            if (!grid.visible || grid.cellSize <= 0 || float.IsNaN(grid.cellSize) || float.IsInfinity(grid.cellSize)) return;
            // Editor guide only: do not draw in Game View or other camera gizmo passes.
            if (Camera.current == null || Camera.current.cameraType != CameraType.SceneView) return;
            int columns = Mathf.Clamp(grid.columns, 1, 200), rows = Mathf.Clamp(grid.rows, 1, 200);
            int major = Mathf.Clamp(grid.majorEvery, 1, 10);
            float width = columns * grid.cellSize, depth = rows * grid.cellSize;
            var oldDepth = Handles.zTest;
            try
            {
                Handles.zTest = grid.alwaysVisible ? CompareFunction.Always : CompareFunction.LessEqual;
                using (new Handles.DrawingScope(grid.transform.localToWorldMatrix))
                {
                    for (int x = 0; x <= columns; x++)
                    {
                        Handles.color = x % major == 0 ? grid.majorColor : grid.color;
                        float px = x * grid.cellSize - width * .5f;
                        Handles.DrawLine(new Vector3(px, 0, -depth * .5f), new Vector3(px, 0, depth * .5f));
                    }
                    for (int z = 0; z <= rows; z++)
                    {
                        Handles.color = z % major == 0 ? grid.majorColor : grid.color;
                        float pz = z * grid.cellSize - depth * .5f;
                        Handles.DrawLine(new Vector3(-width * .5f, 0, pz), new Vector3(width * .5f, 0, pz));
                    }
                }
            }
            finally { Handles.zTest = oldDepth; }
        }
    }
}
