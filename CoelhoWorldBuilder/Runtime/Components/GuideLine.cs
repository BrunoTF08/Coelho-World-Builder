using System.Collections.Generic;
using UnityEngine;
namespace CoelhoLabs.WorldBuilder
{
    [DisallowMultipleComponent, AddComponentMenu("Coelho Labs/Guide Line")]
    public sealed class GuideLine : MonoBehaviour
    {
        [Tooltip("Coordenadas locais dos pontos.")]
        public List<Vector3> points = new List<Vector3> { Vector3.zero, Vector3.right * 5 };
        public bool closed;
        public Color color = new Color(.2f, 1f, .35f);
        [Range(1, 10)] public float width = 3;
    }
}
