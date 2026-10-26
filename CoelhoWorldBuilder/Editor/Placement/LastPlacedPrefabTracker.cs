using UnityEngine;
using UnityEngine.SceneManagement;

namespace CoelhoLabs.WorldBuilder.Editor
{
    public sealed class LastPlacedPrefabTracker
    {
        public GeneratedObject Reference { get; private set; }
        public void Register(GeneratedObject item)
        {
            if (item != null && item.modularCategory != ModularCategory.Decorative && item.gameObject.scene == SceneManager.GetActiveScene()) Reference = item;
        }
        public void Validate()
        {
            if (Reference == null || !Reference.gameObject.activeInHierarchy || Reference.gameObject.scene != SceneManager.GetActiveScene() || Reference.modularCategory == ModularCategory.Decorative) Reference = null;
        }
        public void Reconcile()
        {
            Reference = null; long order = long.MinValue;
            foreach (var item in Object.FindObjectsByType<GeneratedObject>(FindObjectsSortMode.None))
                if (item.gameObject.activeInHierarchy && item.gameObject.scene == SceneManager.GetActiveScene() && item.modularCategory != ModularCategory.Decorative && item.placementOrder > order)
                { Reference = item; order = item.placementOrder; }
        }
    }
}
