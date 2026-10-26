using UnityEngine;

namespace CoelhoLabs.WorldBuilder
{
    public enum ModularCategory { Decorative, Roads, Floors }
    [DisallowMultipleComponent, AddComponentMenu("")]
    public sealed class GeneratedObject : MonoBehaviour
    {
        [HideInInspector] public string persistentId;
        [HideInInspector] public string sourcePrefabGuid;
        [HideInInspector] public int seed;
        [HideInInspector] public ModularCategory modularCategory;
        [HideInInspector] public Bounds fittingBounds;
        [HideInInspector] public long placementOrder;
        [HideInInspector] public Vector3[] connectionPoints;
    }
}
