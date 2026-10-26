using UnityEngine;

namespace CoelhoLabs.WorldBuilder.Editor
{
    public static class SmartPlacementMath
    {
        public static Vector3 Snap(Vector3 point, Vector3 origin, Vector2 cell)
        {
            point.x = origin.x + Mathf.Round((point.x - origin.x) / Mathf.Max(.01f, cell.x)) * Mathf.Max(.01f, cell.x);
            point.z = origin.z + Mathf.Round((point.z - origin.z) / Mathf.Max(.01f, cell.y)) * Mathf.Max(.01f, cell.y);
            return point;
        }
        public static Bounds TransformBounds(Bounds source, Matrix4x4 matrix)
        {
            var result = new Bounds(matrix.MultiplyPoint3x4(source.center), Vector3.zero);
            for (int i = 0; i < 8; i++)
            {
                var sign = new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1);
                result.Encapsulate(matrix.MultiplyPoint3x4(source.center + Vector3.Scale(source.extents, sign)));
            }
            return result;
        }
        public static Vector3 Anchor(Bounds bounds, PrefabPlacementProfile profile)
        {
            switch (profile.anchorMode)
            {
                case AnchorMode.BottomCenter: return new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
                case AnchorMode.BottomCorner: return bounds.min;
                case AnchorMode.Custom: return profile.customAnchor;
                default: return Vector3.zero;
            }
        }
        public static bool TryBounds(GameObject prefab, out Bounds bounds)
        {
            bounds = default; bool found = false;
            foreach (var renderer in prefab.GetComponentsInChildren<Renderer>(true))
            {
                if (!renderer.enabled || Ignore(renderer.transform)) continue;
                Bounds local;
                var filter = renderer.GetComponent<MeshFilter>();
                if (renderer is SkinnedMeshRenderer skin) local = skin.localBounds;
                else if (filter != null && filter.sharedMesh != null) local = filter.sharedMesh.bounds;
                else
                {
                    var fallback = TransformBounds(renderer.bounds, prefab.transform.worldToLocalMatrix);
                    if (!found) bounds = fallback; else bounds.Encapsulate(fallback);
                    found = true; continue;
                }
                var transformed = TransformBounds(local, prefab.transform.worldToLocalMatrix * renderer.transform.localToWorldMatrix);
                if (!found) bounds = transformed; else bounds.Encapsulate(transformed);
                found = true;
            }
            if (found) return true;
            foreach (var collider in prefab.GetComponentsInChildren<Collider>(true))
            {
                if (!collider.enabled || collider.isTrigger || Ignore(collider.transform)) continue;
                Bounds local;
                if (collider is BoxCollider box) local = new Bounds(box.center, box.size);
                else if (collider is SphereCollider sphere) local = new Bounds(sphere.center, Vector3.one * sphere.radius * 2);
                else if (collider is MeshCollider mesh && mesh.sharedMesh != null) local = mesh.sharedMesh.bounds;
                else if (collider is CapsuleCollider capsule)
                {
                    var size = Vector3.one * capsule.radius * 2;
                    size[capsule.direction] = Mathf.Max(capsule.height, capsule.radius * 2); local = new Bounds(capsule.center, size);
                }
                else continue;
                var transformed = TransformBounds(local, prefab.transform.worldToLocalMatrix * collider.transform.localToWorldMatrix);
                if (!found) bounds = transformed; else bounds.Encapsulate(transformed);
                found = true;
            }
            return found;
        }
        private static bool Ignore(Transform transform)
        {
            for (var node = transform; node != null; node = node.parent)
                if (!node.gameObject.activeSelf || node.GetComponent<DimensionIgnore>() != null
                    || (node.gameObject.hideFlags & (HideFlags.DontSaveInEditor | HideFlags.HideInHierarchy)) != 0
                    || node.CompareTag("EditorOnly")) return true;
            return false;
        }
    }
}
