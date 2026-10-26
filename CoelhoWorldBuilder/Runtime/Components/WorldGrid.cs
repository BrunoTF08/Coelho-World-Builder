using UnityEngine;

namespace CoelhoLabs.WorldBuilder
{
    [DisallowMultipleComponent, AddComponentMenu("Coelho Labs/World Grid")]
    public sealed class WorldGrid : MonoBehaviour
    {
        [Min(.1f), Tooltip("Distância entre linhas, em unidades locais.")]
        public float cellSize = 1f;
        [Range(1, 200)] public int columns = 30;
        [Range(1, 200)] public int rows = 30;
        public Color color = new Color(.45f, 1f, .65f, .55f);
        [Range(1, 10)] public int majorEvery = 5;
        public Color majorColor = new Color(.65f, 1f, .75f, .9f);
        [Tooltip("Mostrar a grade mesmo quando há objetos à frente.")]
        public bool alwaysVisible = true;
        public bool visible = true;

        private void OnValidate()
        {
            cellSize = float.IsNaN(cellSize) || float.IsInfinity(cellSize) ? 1f : Mathf.Max(.1f, cellSize);
            columns = Mathf.Clamp(columns, 1, 200);
            rows = Mathf.Clamp(rows, 1, 200);
            majorEvery = Mathf.Clamp(majorEvery, 1, 10);
        }
    }
}
