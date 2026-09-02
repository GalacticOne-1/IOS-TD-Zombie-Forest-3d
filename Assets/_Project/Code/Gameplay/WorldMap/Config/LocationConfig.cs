
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.WorldMap.Intel;
using Galactic1.Code.WorldMap.Visuals;
using Galactic1.Gameplay.Player;
using UnityEngine;

namespace Galactic1.Code.WorldMap
{
    /// <summary>
    /// Конфигурация локации на карте мира.
    /// Используется для:
    /// - отображения информации в UI карты
    /// - расчёта времени визита
    /// - предварительной валидации рейда
    ///
    /// НЕ содержит боевой логики или конкретного контента рейда.
    /// </summary>
    [CreateAssetMenu(fileName = "LocationConfig", menuName = "Game Configs/World Map/Location Config")]
    public sealed class LocationConfig : ScriptableObject
    {

        // =========================
        // Presentation
        // =========================

        [Header("Presentation")] [SerializeField]
        private LocationId id;

        public LocationId Id => id;


        // element in []
        public int Index { get; private set; } // нужно для загрузчика сцен, он работает только с int id

        public int SetIndex
        {
            set => Index = value;
        }




        [field: Space(10)]

        #region HEADER

        [field: SerializeField]
        public CHeader Header { get; private set; }

        [System.Serializable]
        public struct CHeader
        {
            public string TitleLid;
            [TextArea] public string DescriptionLid;

            public int Order;

            [Space] public Sprite Icon;
            public float SizeUI;
            public Vector2 IconOffset;
        }

        #endregion


        [field: SerializeField] public LocationType LocationType { get; private set; }
        [field: SerializeField] public string PrefabPath { get; private set; }
        [field: SerializeField] public Vector2 LocationBorder { get; private set; }


        // =========================
        // Difficulty
        // =========================

        [field: Header("Difficulty")]
        [field: SerializeField]
        public int RequiresLevel { get; private set; }

        [field: SerializeField, Range(1, 3)] public int Difficulty { get; private set; }


        // =========================
        // Time
        // =========================

        [field: Header("Time")]
        [field: Tooltip("Base time cost (in days) to perform a raid in this location")]
        [field: SerializeField]
        public LocationVisitCostConfig VisitCostConfig { get; private set; }



        [field: Header("=== ENEMY VISUAL RULES ===")]
        [field: SerializeField]
        public LocationEnemyVisualRules EnemyVisualRules { get; private set; }


        // =========================
        // Loot (preview only)
        // =========================

        [field: Header("Loot Preview")]
        [field: SerializeField]
        public LocationIntel LocationIntel { get; private set; }

        // =========================
        // Squad Requirements
        // =========================

        //[field: Header("Squad Requirements")]
        //[field: SerializeField]
        //public SquadRequirementProfile SquadRequirements { get; private set; }



        // =========================
        // Player
        // =========================
        [field: Header("=== PLAYER ===")]
        [field: Tooltip("Профиль загрузки игрока (стартовое оружие, одежда, статы). Аналог PlayerLoadProfile в LDoE.")]
        [field: SerializeField]
        public PlayerSpawnPreset PlayerPreset { get; private set; }

        [field: Tooltip("Точка спавна игрока на этой локации.")]
        [field: SerializeField]
        public Vector2 PlayerSpawnPoint { get; private set; }


    }
}