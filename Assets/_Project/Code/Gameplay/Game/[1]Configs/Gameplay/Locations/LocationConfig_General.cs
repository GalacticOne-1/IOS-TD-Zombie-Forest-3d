using System;
using System.Collections.Generic;
using Galactic1.Gameplay.Player;
using Galactic1.Tool;
using UnityEngine;

namespace Galactic1.Core.Location
{
    [CreateAssetMenu(fileName = "LocationConfig_General", menuName = "Game Configs/Locations/New Location Config General")]
    public class LocationConfig_General : ScriptableObject
    {
        public string title;
        public Sprite sprite;
        public byte tileX, tileY;

        
        
        [field: Header("=== PLAYER ===")]
        [field:Tooltip("Профиль загрузки игрока (стартовое оружие, одежда, статы). Аналог PlayerLoadProfile в LDoE.")]
        [field: SerializeField] public PlayerSpawnPreset playerPreset { get; private set; }
        [field:Tooltip("Точка спавна игрока на этой локации.")]
        [field:SerializeField] public Vector2 PlayerSpawnPoint { get; private set; }
        [field:SerializeField] public bool PlayerOnDragon { get; private set; }

        [field:Space] [field:Header("=== LOCATION ===")]
        [field: SerializeField] public string PrefabPath { get; private set; }
        public Vector2 locationCenter;
        public Vector2 locationBorder;
        
        public Vector2[] borderLands;



        [Tooltip("Zones to ignore when detecting surface tiles.")]
        public List<BoundsInt> ignoreZones = new();


        [Space] [Header("************")]
        

        [SerializeField] private CBgSetup bgSetup;
        public CBgSetup BgSetup => bgSetup;


        
        

        #region CRATES

        [Space] [Header("* CRATE")]
        [SerializeField] private CSpawnCrateRules[] _spawnCrateRules;

        public CSpawnCrateRules[] SpawnCrateRules
        {
            get => _spawnCrateRules;
            set => _spawnCrateRules = value;
        }

        [Serializable]
        public class CSpawnCrateRules : IToolVector
        {
            public bool enabled;
            public Vector2 coord;
            public Vector2 Coord => coord;
            
            
            
        }
        

        #endregion
        
        
        



        [Space]
        [Header("*REWARD")]
        [SerializeField] 
        private CMissionItems[] mainReward;
        [SerializeField] 
        private CMissionItems[] possibleReward;


        public CMissionItems[] MainReward => mainReward;

        public CMissionItems[] PossibleReward => possibleReward;
        
        
        
        [System.Serializable]
        public struct CMissionItems
        {
            public EItems item;
            [Header("true - брать предмет из снаряжения")]
            public bool useEquipment;
            public EEquipment equipment;

            [Space] 
            [Range(0,10)]
            public byte chance;
            public byte minQu;
            public byte maxQu;
        }




        public void GetLocationPoints(out Vector2 _locationCenter, out Vector2 _locationBorder)
        {
            _locationCenter = locationCenter;
            _locationBorder = locationBorder;
        }
        
    }
}