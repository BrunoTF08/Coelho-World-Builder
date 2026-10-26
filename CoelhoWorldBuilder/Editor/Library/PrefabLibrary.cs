using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CoelhoLabs.WorldBuilder.Editor
{
    [Serializable]
    public sealed class PrefabCategory
    {
        public string id;
        public string name;
        public bool expanded = true;
        public List<string> prefabs = new List<string>();
    }

    [FilePath("ProjectSettings/CoelhoWorldBuilderLibrary.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class PrefabLibrary : ScriptableSingleton<PrefabLibrary>
    {
        public List<PrefabCategory> categories = new List<PrefabCategory>();
        public string activeCategoryId;
        [SerializeField] private bool initialized;
        [NonSerialized] private Dictionary<string, GameObject> assets = new Dictionary<string, GameObject>();
        public event Action Changed;
        private void OnEnable() { EditorApplication.projectChanged -= InvalidateCache; EditorApplication.projectChanged += InvalidateCache; }
        private void OnDisable() { EditorApplication.projectChanged -= InvalidateCache; }
        public PrefabCategory Active => categories.Find(c => c.id == activeCategoryId);
        public void Initialize()
        {
            if (initialized) return;
            initialized = true;
            foreach (var name in new[] { "Ruas", "Calçadas", "Grama", "Árvores", "Prédios", "Construções destruídas", "Decorações", "Vegetação", "Outros" })
                categories.Add(new PrefabCategory { id = Guid.NewGuid().ToString("N"), name = name, expanded = false });
            var others = categories[categories.Count - 1];
            foreach (var prefab in BuilderSettings.instance.prefabs)
            {
                if (prefab == null) continue;
                var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prefab));
                if (!string.IsNullOrEmpty(guid) && !others.prefabs.Contains(guid)) others.prefabs.Add(guid);
            }
            activeCategoryId = others.id; others.expanded = true; Commit();
        }
        public void Commit() { Save(true); Changed?.Invoke(); }
        public void SaveExpansion() { Save(true); }
        public void InvalidateCache() { assets?.Clear(); Changed?.Invoke(); }
        public GameObject Resolve(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            if (assets == null) assets = new Dictionary<string, GameObject>();
            if (assets.TryGetValue(guid, out var cached)) return cached;
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset != null && (!PrefabUtility.IsPartOfPrefabAsset(asset) || asset.transform.parent != null)) asset = null;
            assets[guid] = asset; return asset;
        }
        public PrefabCategory CreateCategory(string name)
        {
            var category = new PrefabCategory { id = Guid.NewGuid().ToString("N"), name = string.IsNullOrWhiteSpace(name) ? "Nova categoria" : name.Trim() };
            categories.Add(category); activeCategoryId = category.id; Commit(); return category;
        }
        public void Rename(PrefabCategory category, string name)
        { if (category == null || string.IsNullOrWhiteSpace(name)) return; category.name = name.Trim(); Commit(); }
        public void Activate(PrefabCategory category)
        { if (category == null || !categories.Contains(category) || activeCategoryId == category.id) return; activeCategoryId = category.id; Commit(); }
        public void Delete(PrefabCategory category)
        {
            if (!categories.Remove(category)) return;
            if (activeCategoryId == category.id) activeCategoryId = categories.Count == 0 ? null : categories[0].id;
            Commit();
        }
        public void ReorderCategory(PrefabCategory category, int delta)
        {
            int index = categories.IndexOf(category), next = Mathf.Clamp(index + delta, 0, categories.Count - 1);
            if (index < 0 || index == next) return;
            categories.RemoveAt(index); categories.Insert(next, category); Commit();
        }
        public bool Add(PrefabCategory category, GameObject prefab)
        {
            if (category == null || prefab == null || !PrefabUtility.IsPartOfPrefabAsset(prefab) || prefab.transform.parent != null) return false;
            var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prefab));
            if (category.prefabs.Contains(guid)) return false;
            category.prefabs.Add(guid); if (assets == null) assets = new Dictionary<string, GameObject>(); assets[guid] = prefab; Commit(); return true;
        }
        public void Remove(PrefabCategory category, string guid)
        { if (category != null && category.prefabs.Remove(guid)) Commit(); }
        public int AddRange(PrefabCategory category, UnityEngine.Object[] entries)
        {
            if (category == null) return 0;
            int count = 0;
            foreach (var entry in entries)
            {
                var prefab = entry as GameObject;
                if (prefab == null || !PrefabUtility.IsPartOfPrefabAsset(prefab) || prefab.transform.parent != null) continue;
                var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prefab));
                if (category.prefabs.Contains(guid)) continue;
                category.prefabs.Add(guid); count++;
            }
            if (count > 0) Commit(); return count;
        }
        public void ReorderPrefab(PrefabCategory category, string guid, int delta)
        {
            int index = category.prefabs.IndexOf(guid), next = Mathf.Clamp(index + delta, 0, category.prefabs.Count - 1);
            if (index < 0 || next == index) return;
            category.prefabs.RemoveAt(index); category.prefabs.Insert(next, guid); Commit();
        }
        public void Move(PrefabCategory source, PrefabCategory target, string guid)
        {
            if (source == null || target == null || source == target || !source.prefabs.Remove(guid)) return;
            if (!target.prefabs.Contains(guid)) target.prefabs.Add(guid);
            Commit();
        }
        public bool Next(string selected, int delta, out GameObject prefab)
        {
            prefab = null; var category = Active;
            if (category == null || category.prefabs.Count == 0) return false;
            int start = category.prefabs.IndexOf(selected);
            if (start < 0) start = delta > 0 ? -1 : 0;
            for (int i = 1; i <= category.prefabs.Count; i++)
            {
                int index = ((start + (delta > 0 ? i : -i)) % category.prefabs.Count + category.prefabs.Count) % category.prefabs.Count;
                prefab = Resolve(category.prefabs[index]); if (prefab != null) return true;
            }
            return false;
        }
    }
}
