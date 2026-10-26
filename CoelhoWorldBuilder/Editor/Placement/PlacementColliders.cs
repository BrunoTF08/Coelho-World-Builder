using UnityEditor;
using UnityEngine;

namespace CoelhoLabs.WorldBuilder.Editor
{
    public static class PlacementColliders
    {
        // Operates on scene instances only. Original prefab assets remain untouched.
        public static int ReplaceBoxes(GameObject instance)
        {
            int replaced = 0;
            foreach (var box in instance.GetComponentsInChildren<BoxCollider>(true))
            {
                var filters = box.GetComponentsInChildren<MeshFilter>(true);
                bool hasMesh = false;
                foreach (var filter in filters)
                {
                    if (filter.sharedMesh == null) continue;
                    hasMesh = true;
                    var collider = filter.GetComponent<MeshCollider>();
                    if (collider != null) continue;
                    collider = Undo.AddComponent<MeshCollider>(filter.gameObject);
                    Undo.RecordObject(collider, "Coelho: configurar Mesh Collider");
                    collider.sharedMesh = filter.sharedMesh;
                    collider.sharedMaterial = box.sharedMaterial;
                    collider.enabled = box.enabled;
                    var body = collider.attachedRigidbody;
                    collider.convex = box.isTrigger || (body != null && !body.isKinematic);
                    collider.isTrigger = box.isTrigger;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(collider);
                }
                if (!hasMesh) continue;
                Undo.DestroyObjectImmediate(box); replaced++;
            }
            return replaced;
        }
    }
}
