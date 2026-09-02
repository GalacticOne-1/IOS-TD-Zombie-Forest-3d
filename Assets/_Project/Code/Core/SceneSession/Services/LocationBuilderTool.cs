using UnityEngine;

namespace Galactic1.Gameplay.Locations.Authoring
{
    /// <summary>
    /// Конфиг для editor-инструмента сборки локации (LocationBuilderToolEditor).
    /// Сам компонент не содержит логики — только данные и helper-методы
    /// поиска/создания вложенных рутов. Вся генерация — в кастомном Editor.
    /// </summary>
    public sealed class LocationBuilderTool : MonoBehaviour
    {
        [Header("=== LOCATION BOUNDS ===")]
        [Tooltip("Размер локации в метрах: X = ширина, Y = глубина (Z в мире).")]
        [SerializeField]
        private Vector2 locationSize = new Vector2(10f, 20f);

        public Vector2 LocationSize => locationSize;

        [Header("=== FLOOR TILES ===")]
        [SerializeField] private GameObject tilePrefab;
        public GameObject TilePrefab => tilePrefab;

        [Tooltip("Размер одной плитки в метрах: X = ширина, Y = глубина (Z в мире).")]
        [SerializeField] private Vector2 tileSize = new Vector2(2f, 2f);
        public Vector2 TileSize => tileSize;

        [Tooltip("Количество плиток по осям: X = ширина сетки, Y = глубина сетки.")]
        [SerializeField] private Vector2Int tileCount = new Vector2Int(30, 50);
        public Vector2Int TileCount => tileCount;
        
        [Tooltip("Глобальный сдвиг всей сетки плиток по X/Z относительно центра floorRoot.")]
        [SerializeField] private Vector2 gridOffset = Vector2.zero;
        public Vector2 GridOffset => gridOffset;

        [Tooltip("Дополнительный сдвиг (зазор) между плитками в метрах. " +
                 "0 = встык, положительное значение = зазор, отрицательное = нахлёст.")]
        [SerializeField] private Vector2 tileSpacing = Vector2.zero;
        public Vector2 TileSpacing => tileSpacing;

        [SerializeField] private float floorY = 0f;
        public float FloorY => floorY;

        [SerializeField] private string floorRootName = "Floor";
        public string FloorRootName => floorRootName;

        [Header("=== EXIT ZONES ===")]
        [Tooltip("Префаб с компонентом SceneExitZone + BoxCollider (trigger).")]
        [SerializeField]
        private GameObject exitZonePrefab;

        public GameObject ExitZonePrefab => exitZonePrefab;

        [SerializeField] private float exitZoneHeight = 3f;
        public float ExitZoneHeight => exitZoneHeight;
        
        [SerializeField] private float exitZoneThickness = 1.5f;
        public float ExitZoneThickness => exitZoneThickness;

        [SerializeField] private string exitRootName = "ExitZones";
        public string ExitRootName => exitRootName;

        // -------------------------------------------------------------
        // Helpers — поиск/создание вложенных рутов. Безопасны и в рантайме,
        // но используются только editor-инструментом.
        // -------------------------------------------------------------
        public Transform GetOrCreateRoot(string rootName)
        {
            var existing = transform.Find(rootName);
            if (existing != null)
                return existing;

            var go = new GameObject(rootName);
            go.transform.SetParent(transform, false);
            return go.transform;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.5f);
            var center = transform.position + new Vector3(0, floorY, 0);
            var size = new Vector3(locationSize.x, 0.05f, locationSize.y);
            Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, size);
        }
#endif
    }
}