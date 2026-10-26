using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoelhoLabs.WorldBuilder.Editor
{
    public static class DemoBuilder
    {
        public static void Create()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (string.IsNullOrEmpty(SceneManager.GetSceneAt(i).path))
                {
                    BuilderLog.Info("Salve as cenas sem nome antes de criar a demonstração aditiva.");
                    return;
                }
            const string folder = "Assets/CoelhoWorldBuilderDemo";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets", "CoelhoWorldBuilderDemo");
            string prefabPath = AssetDatabase.GenerateUniqueAssetPath(folder + "/DemoBlock.prefab");
            GameObject template = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                template.name = "Demo Block";
                Object.DestroyImmediate(template.GetComponent<BoxCollider>());
                template.AddComponent<MeshCollider>().sharedMesh = template.GetComponent<MeshFilter>().sharedMesh;
                template.transform.position = new Vector3(0, .5f, 0);
                var prefab = PrefabUtility.SaveAsPrefabAsset(template, prefabPath);
                BuilderSettings.instance.prefabs.Add(prefab); BuilderSettings.instance.Persist();
                var library = PrefabLibrary.instance; library.Initialize();
                library.Add(library.Active ?? library.CreateCategory("Outros"), prefab);
            }
            finally { Object.DestroyImmediate(template); }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Demo Surface (Collider)"; ground.transform.localScale = Vector3.one * 5;
            var light = new GameObject("Directional Light"); light.AddComponent<Light>().type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
            string scenePath = AssetDatabase.GenerateUniqueAssetPath(folder + "/Demo.unity");
            EditorSceneManager.SaveScene(scene, scenePath);
            if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.LookAt(Vector3.zero, Quaternion.Euler(50, 0, 0), 35);
            BuilderLog.Info("Demo criada: " + scenePath + ". Abra Prefab Painter. Offset 0,5 apoia o cubo no chão.");
        }
    }
}
