using UnityEngine;

namespace Galactic1.Code.WorldMap
{
    /// <summary>
    /// Отвечает за визуализацию маршрута между локациями на карте.
    /// Использует LineRenderer и не содержит логики выбора пути.
    /// </summary>
    public class MapRouteRenderer : MonoBehaviour
    {
        [Header("Line Renderer")]
        [SerializeField] private LineRenderer lineRenderer;

        [Header("Visual")]
        [SerializeField] private float yOffset = 0.2f;

        private void Awake()
        {
            lineRenderer.positionCount = 0;
            lineRenderer.enabled = false;
        }

        /// <summary>
        /// Отрисовывает маршрут между двумя нодами.
        /// </summary>
        public void ShowRoute(MapNode from, MapNode to)
        {
            if (from == null || to == null)
                return;

            lineRenderer.positionCount = 2;

            lineRenderer.SetPosition(0, from.transform.position + Vector3.up * yOffset);
            lineRenderer.SetPosition(1, to.transform.position + Vector3.up * yOffset);

            lineRenderer.enabled = true;
        }

        /// <summary>
        /// Скрывает маршрут.
        /// </summary>
        public void Hide()
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 0;
        }
    }
}