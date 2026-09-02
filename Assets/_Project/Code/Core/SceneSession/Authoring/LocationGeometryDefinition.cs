using System;
using Galactic1.Gameplay.Locations.Navigation;
using UnityEngine;

namespace Galactic1.Gameplay.Locations.Authoring
{
    /// <summary>
    /// Статическое описание геометрии уровня (level authoring data).
    /// НЕ используется в рантайме — существует только как конфиг
    /// для LocationBuilderToolEditor.
    ///
    /// Никакой логики: только SerializeField + helper поиска/создания рутов.
    /// Вся генерация (пол, exit-зоны, навигация) — в кастомном Editor.
    /// </summary>
    public sealed class LocationGeometryDefinition : MonoBehaviour
    {
        // ===================================================================
        // LEVEL
        // ===================================================================
        [Header("=== LEVEL ===")]
        [Tooltip("Размер локации в метрах: X = ширина, Y = глубина (Z в мире).")]
        [SerializeField]
        private Vector2 locationSize = new Vector2(10f, 20f);

        public Vector2 LocationSize => locationSize;

        [Tooltip("Высота пола (мировая ось Y).")] [SerializeField]
        private float floorHeight = 0f;

        public float FloorHeight => floorHeight;

        // ===================================================================
        // FLOOR
        // ===================================================================
        [Header("=== FLOOR ===")] [SerializeField]
        private GameObject tilePrefab;

        public GameObject TilePrefab => tilePrefab;

        [Tooltip("Размер одной плитки в метрах: X = ширина, Y = глубина (Z в мире). " +
                 "Задаёт localScale плитки напрямую.")]
        [SerializeField]
        private Vector2 tileSize = new Vector2(2f, 2f);

        public Vector2 TileSize => tileSize;

        [Tooltip("Количество плиток по осям: X = ширина сетки, Y = глубина сетки.")] [SerializeField]
        private Vector2Int tileCount = new Vector2Int(30, 50);

        public Vector2Int TileCount => tileCount;

        [Tooltip("Глобальный сдвиг всей сетки плиток по X/Z относительно центра floorRoot.")] [SerializeField]
        private Vector2 gridOffset = Vector2.zero;

        public Vector2 GridOffset => gridOffset;

        [Tooltip("Дополнительный сдвиг (зазор) между плитками в метрах. " +
                 "0 = встык, положительное значение = зазор, отрицательное = нахлёст.")]
        [SerializeField]
        private Vector2 tileSpacing = Vector2.zero;

        public Vector2 TileSpacing => tileSpacing;

        [SerializeField] private string floorRootName = "Floor";
        public string FloorRootName => floorRootName;

        // ===================================================================
        // EXIT ZONES
        // ===================================================================
        [Header("=== EXIT ZONES ===")]
        [Tooltip("Префаб с компонентом SceneExitZone + BoxCollider (trigger).")]
        [SerializeField]
        private GameObject exitPrefab;

        public GameObject ExitPrefab => exitPrefab;

        [SerializeField] private float exitHeight = 3f;
        public float ExitHeight => exitHeight;

        [SerializeField] private float exitThickness = 1.5f;
        public float ExitThickness => exitThickness;

        [SerializeField] private string exitRootName = "ExitZones";
        public string ExitRootName => exitRootName;

        
        
        
        // ===================================================================
        // NAVIGATION (только данные — под будущий Aron Granberg A* GridGraph)
        // ===================================================================
        [Header("=== NAVIGATION ===")] [SerializeField]
        private NavigationSettings navigation = new NavigationSettings();

        public NavigationSettings Navigation => navigation;

        [Serializable]
        public sealed class NavigationSettings
        {
            [Tooltip("Сторона локации без паддинга (передняя сторона, обычно вход)")]
            public LocationFrontSide FrontSide = LocationFrontSide.None;
            
            [Tooltip("Смещение центра GridGraph относительно позиции LocationGeometryDefinition.")]
            public Vector3 GraphCenterOffset = Vector3.zero;

            [Tooltip("Размер одной ноды графа в метрах.")]
            public float NodeSize = 1f;

            [Tooltip("Диаметр коллизии для проверки проходимости ноды.")]
            public float CollisionDiameter = 1f;

            [Tooltip("Количество итераций эрозии (сужение проходимой области у стен/препятствий).")]
            public int ErosionIterations = 2;

            [Tooltip("Выполнять ли Scan сразу после конфигурации графа. " +
                     "ВНИМАНИЕ: пока не реализовано — только зарезервировано под будущую логику.")]
            public bool ScanOnBuild = false;
        }

        [SerializeField] private string navigationRootName = "Navigation";
        public string NavigationRootName => navigationRootName;

        // ===================================================================
        // HELPERS
        // ===================================================================


        public GridGraphConfigurationDTO GetDto()
            => new (navigation, locationSize);
                

        /// <summary>
        /// Ищет или создаёт вложенный root-объект под этим компонентом.
        /// Используется LocationBuilderToolEditor для Floor/ExitZones/Navigation
        /// и в будущем — для Walls/POI/SpawnRoots.
        /// </summary>
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
            var center = transform.position + new Vector3(0, floorHeight, 0);
            var size = new Vector3(locationSize.x, 0.05f, locationSize.y);
            Gizmos.matrix = Matrix4x4.TRS(center, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, size);
            Gizmos.matrix = Matrix4x4.identity;
        }
#endif
    }
}