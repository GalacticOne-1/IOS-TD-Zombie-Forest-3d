using System.Linq;
using Galactic1.Code.Gameplay.BaseBuilding;
using Galactic1.Code.Gameplay.Enemies.Authoring;
using Galactic1.Configs.Galactic1.Code.GameDatabase;
using Galactic1.Gameplay.Locations.Authoring;
using Galactic1.RaidLoot.Scene;
using UnityEngine;

namespace Galactic1.Gameplay.Locations
{
    /// <summary>
    /// SceneDefinition — главный носитель данных для каждой локации (как в LDoE).
    /// Этот объект ДОЛЖЕН быть на сцене и сообщает GameSessionManagerу:
    /// 1) как загрузить игрока
    /// 2) кого спавнить (враги, ресурсы, лут)
    /// 3) какие триггеры активны
    /// 4) какие ловушки и интерактивы присутствуют
    /// 5) параметры камеры, музыки, навигации
    /// 6) как сохраняется локация
    /// 
    /// LocationContext НЕ создаёт объекты самостоятельно —
    /// он только хранит ссылки и конфигурации.
    /// 
    /// Это точка входа для любой сцены.
    /// </summary>
    public class SceneContext : MonoBehaviour
    {
        // --------------------------------------------------------------------
        // PLAYER SETTINGS
        // --------------------------------------------------------------------
        [Header("=== SPAWN POINTS ===")]
        [Tooltip("Точка спавна транспорта. Для лагеря — около гаража, для рейда — точка входа.")]
        [SerializeField] private Transform transportSpawnPoint;
        [SerializeField] private Transform squadSpawnPoint;
        
        [Tooltip("Ширина прямоугольной области, в которой разбрасываются юниты отряда вокруг squadSpawnPoint (по оси X).")]
        [SerializeField] private float squadSpawnWidth = 10f;

        [Tooltip("Глубина прямоугольной области, в которой разбрасываются юниты отряда вокруг squadSpawnPoint (по оси Z).")]
        [SerializeField] private float squadSpawnDepth = 10f;

        [Tooltip("Минимальная дистанция между заспавненными юнитами отряда, чтобы не пересекались.")]
        [SerializeField] private float squadSpawnMinDistance = 2f;

        // --------------------------------------------------------------------
        // LOCATION SETTINGS
        // --------------------------------------------------------------------

        //[Tooltip("Погодный профиль — влияет на ambient sound, визуальные эффекты.")]
        //public WeatherProfile weatherProfile;



        // --------------------------------------------------------------------
        // ENEMY SPAWNING
        // --------------------------------------------------------------------

        // [Header("=== ENEMY SPAWNING ===")]
        // [Tooltip("Точки спавна ambient-групп врагов. Не использовать FindObjectsOfType.")]
        // [SerializeField] private Transform enemySpawnRoot;

        //[Tooltip("Спавнер случайных событий (как волки, громилы, грабители).")]
        //public RandomEncounterSpawner randomEncounterSpawner;

        //[Tooltip("Точка спавна босса, если есть.")]
        //public BossSpawnPoint bossSpawnPoint;



        // --------------------------------------------------------------------
        // LOOT & RESOURCES
        // --------------------------------------------------------------------

        // [Header("=== LOOT & RESOURCES ===")]
        // [Tooltip("Генератор лута — ящики, сундуки, мешки.")]
        // [SerializeField] public Transform lootSpawnRoot;



        // --------------------------------------------------------------------
        // INTERACTABLES & TRAPS
        // --------------------------------------------------------------------

        // [Header("=== INTERACTABLE OBJECTS ===")]
        // [Tooltip("Интерактивные объекты: двери, верстаки, ящики, терминалы.")]
        // public InteractableObject[] interactables;
        //
        // [Tooltip("Спавнер ловушек: мины, газовые ловушки, огнемёты.")]
        // public TrapSpawner trapSpawner;



        // --------------------------------------------------------------------
        // TRIGGERS & EVENTS
        // --------------------------------------------------------------------

        // [Header("=== TRIGGERS ===")]
        // [Tooltip("Обычные триггеры: вход, выход, зоны миссий, активации волн.")]
        // public TriggerZone[] triggerZones;
        //
        // [Tooltip("Скриптовые зоны (катсцены, scripted events).")]
        // public ScriptedEventZone[] scriptedZones;
        //
        // [Tooltip("Зона выхода с уровня (как зелёный круг в LDoE).")]
        // public ExitZone exitZone;



        // --------------------------------------------------------------------
        // NAVIGATION & AI
        // --------------------------------------------------------------------

        // [Header("=== NAVIGATION ===")]
        // [Tooltip("NavMesh локации (отдельный для каждой зоны).")]
        // public NavMeshSurfaceProvider navMesh;
        //
        // [Tooltip("Дополнительные NavMesh ссылки (лестницы, прыжки).")]
        // public NavLinkGroup[] navLinks;



        // --------------------------------------------------------------------
        // AUDIO & MUSIC
        // --------------------------------------------------------------------

        // [Header("=== AUDIO ===")]
        // [Tooltip("Настройки окружения: ветер, лес, эхо, помещение.")]
        // public AudioEnvironmentProfile audioProfile;
        //
        // [Tooltip("Музыкальная плейлиста локации.")]
        // public MusicPlaylist musicPlaylist;



        // --------------------------------------------------------------------
        // CAMERA
        // --------------------------------------------------------------------

        [Header("=== CAMERA ===")] 
        [SerializeField] private Vector3 cameraPosition;
        [SerializeField] private Vector3 cameraMinBounds;
        [SerializeField] private Vector3 cameraMaxBounds;
        [SerializeField] private bool useAutoBounds;
        
        // [Tooltip("Какой риг камеры используется на этой локации.")]
        // public CameraRig cameraRigPrefab;
        //
        // [Tooltip("Если нужно привязать камеру к другим целям, например босс-арена.")]
        // public Transform cameraTargetOverride;



        // --------------------------------------------------------------------
        // PERSISTENCE
        // --------------------------------------------------------------------

        // [Header("=== PERSISTENCE ===")]
        // [Tooltip("Если true — локация сохраняет состояние (как Бункер).")]
        // public bool isPersistentLocation = false;
        //
        // [Tooltip("Название слота сохранения.")]
        // public string saveSlotId = "";
        //
        // [Tooltip("Контроллер времени жизни локации (пока игрок находится здесь).")]
        // public LocationLifetimeController lifetimeController;



        // --------------------------------------------------------------------
        // OBJECT POOL
        // --------------------------------------------------------------------

        // [Header("=== OBJECT POOL ===")]
        // [Tooltip("Локальный пул объектов — оптимизация (особенно в бункере).")]
        // public LocalObjectPool objectPool;



        // --------------------------------------------------------------------
        // NOISE SYSTEM
        // --------------------------------------------------------------------

        // [Header("=== NOISE SYSTEM ===")]
        // [Tooltip("Трекер шума — влияет на агро врагов, как в LDoE.")]
        // public NoiseTracker noiseTracker;



        // --------------------------------------------------------------------
        // MAIN ENTRY POINT FOR GAMESESSIONMANAGER
        // --------------------------------------------------------------------

        /// <summary>
        /// Этот метод вызывается GameSessionManager сразу после загрузки сцены.
        /// Он не создаёт ничего — только возвращает набор данных.
        /// </summary>
        public SceneDefinitionData GetDefinitionData()
        {
            
            // === границы для камеры
            var locationSize = GetComponent<LocationGeometryDefinition>().LocationSize;
            var locationBoundsX = locationSize.x / 2 - 5;
            var locationBoundsY = locationSize.y / 2;

            Vector3 _cameraMinBounds = !useAutoBounds
                ? cameraMinBounds
                : new Vector3(-locationBoundsX, 0, -(locationBoundsY-1));
            Vector3 _cameraMaxBounds = !useAutoBounds
                ? cameraMaxBounds
                : new Vector3(locationBoundsX, 0, locationBoundsY);
            
            
            // === определение позиции для транспорта
            Transform transportPosition = null;
            var garage = ServiceLocator.Current.Get<BaseFacilityRepository>().TryGetWithConfig(GameIdProvider.Garage);

            if (garage.done)
                transportPosition = (garage.instance as GarageInstance).TransportSpawnPoint;

            else if (transportSpawnPoint)
                transportPosition = transportSpawnPoint;

            else
            {
                Debug.LogError("Transport spawn variant not exist");
            }
            //


            // --- Собираем все спавны на сцене
            
            // #1 loot
            var lootSpawn = FindObjectsByType<LootSpawnPoint>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            LootSpawnPoint[] lootSpawnPoints = lootSpawn.ToArray();
            
            
            // #2 enemies
            var enemySpawn = FindObjectsByType<EnemySpawnPoint>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            EnemySpawnPoint[] enemySpawnPoints = enemySpawn.ToArray();
            
            // спавн точки для волн в режиме осады лагеря
            var waveSpawn = FindObjectsByType<WaveSpawnPoint>(
                FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            WaveSpawnPoint[] waveSpawnPoints = waveSpawn.ToArray();

            
            return new SceneDefinitionData
            {
                context = this,
                
                squadSpawnPoint = squadSpawnPoint,
                squadSpawnWidth = squadSpawnWidth,
                squadSpawnDepth = squadSpawnDepth,
                squadSpawnMinDistance = squadSpawnMinDistance,
                transportSpawnPoint = transportPosition,
                
                cameraPosition = cameraPosition,
                cameraMinBounds = _cameraMinBounds,
                cameraMaxBounds = _cameraMaxBounds,
                
                AmbientSpawnPoints = enemySpawnPoints,
                WaveSpawnPoints = waveSpawnPoints,
                
                LootSpawnPoints = lootSpawnPoints,
                
                // enemyGroups = enemyGroups,
                // randomEvents = randomEncounterSpawner,
                // bossSpawnPoint = bossSpawnPoint,
                // lootSpawner = lootSpawner,
                // lootContainers = lootContainers,
                // resourceSpawner = resourceSpawner,
                // interactables = interactables,
                // trapSpawner = trapSpawner,
                // triggers = triggerZones,
                // scriptedZones = scriptedZones,
                // exitZone = exitZone,
                // navMesh = navMesh,
                // navLinks = navLinks,
                // audioProfile = audioProfile,
                // musicPlaylist = musicPlaylist,
                // cameraRig = cameraRigPrefab,
                // cameraOverrideTarget = cameraTargetOverride,
                // isPersistent = isPersistentLocation,
                // saveSlotId = saveSlotId,
                // lifetimeController = lifetimeController,
                // objectPool = objectPool,
                // noiseTracker = noiseTracker
            };
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (squadSpawnPoint == null)
                return;

            // Зона спавна отряда - прямоугольник с независимыми шириной (X) и глубиной (Z)
            var boxSize = new Vector3(squadSpawnWidth, 2f, squadSpawnDepth);

            Gizmos.color = new Color(0.2f, 1f, 0.4f, 0.8f);
            Gizmos.DrawCube(squadSpawnPoint.position, boxSize);
            Gizmos.color = new Color(0.2f, 1f, 0.4f);
            Gizmos.DrawWireCube(squadSpawnPoint.position, boxSize);

            Gizmos.color = new Color(1f, 0.85f, 0.1f, 0.9f);
            Gizmos.DrawWireSphere(squadSpawnPoint.position, squadSpawnMinDistance);
        }
#endif
    }

    /// <summary>
    /// DTO-структура, которую GameSessionManager получает от LocationContext.
    /// Это облегчает перенос данных.
    /// </summary>
    public class SceneDefinitionData
    {
        public SceneContext context;

        public Transform squadSpawnPoint;
        public float squadSpawnWidth;
        public float squadSpawnDepth;
        public float squadSpawnMinDistance;
        public Transform transportSpawnPoint;
        
        public EnemySpawnPoint[] AmbientSpawnPoints;
        public WaveSpawnPoint[] WaveSpawnPoints;

        public LootSpawnPoint[] LootSpawnPoints;
        

        // public EnemySpawnGroup[] enemyGroups;
        // public RandomEncounterSpawner randomEvents;
        // public BossSpawnPoint bossSpawnPoint;
        //
        // public LootSpawner lootSpawner;
        // public LootContainer[] lootContainers;
        // public ResourceNodeSpawner resourceSpawner;
        //
        // public InteractableObject[] interactables;
        // public TrapSpawner trapSpawner;
        //
        // public TriggerZone[] triggers;
        // public ScriptedEventZone[] scriptedZones;
        // public ExitZone exitZone;
        //
        // public NavMeshSurfaceProvider navMesh;
        // public NavLinkGroup[] navLinks;
        //
        // public AudioEnvironmentProfile audioProfile;
        // public MusicPlaylist musicPlaylist;
        //
        // public CameraRig cameraRig;
        // public Transform cameraOverrideTarget;
        public Vector3 cameraPosition;
        public Vector3 cameraMinBounds;
        public Vector3 cameraMaxBounds;
        
        // public bool isPersistent;
        // public string saveSlotId;
        // public LocationLifetimeController lifetimeController;
        //
        // public LocalObjectPool objectPool;
        // public NoiseTracker noiseTracker;
    }

}