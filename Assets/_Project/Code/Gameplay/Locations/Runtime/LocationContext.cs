using System.Collections.Generic;
using Galactic1.Code.Gameplay.Enemies.Authoring;
using Galactic1.Code.WorldMap;
using Galactic1.Core.GameSession;
using Galactic1.Core.Location;
using Galactic1.RaidLoot.Definition;
using Galactic1.RaidLoot.Scene;
using UnityEngine;

namespace Galactic1.Gameplay.Locations
{
    /// <summary>
    /// ЛОКАЛЬНЫЙ КОНТЕКСТ ЛОКАЦИИ
    /// Он должен содержать только то, что нужно:
    /// для загрузки окружения
    /// расчёта позиций
    /// временных данных
    /// </summary>
    public class LocationContext
    {
        public int LcoationId;
        public GameObject LocationInstance;
        public LocationType LocationType;
        
        public LocationConfig LocationConfig;

        public Vector3 CameraPosition;
        public Vector3 CameraMinBounds;
        public Vector3 CameraMaxBounds;



        public Transform TransportSpawnPoint;
        public Vector3 PlayerSpawnPosition;
        public Vector3[] CampUnitSpawnPosition = new Vector3[0];
        public float SquadSpawnWidth = 10f;
        public float SquadSpawnDepth = 10f;
        public float SquadSpawnMinDistance = 2f;

        /// <summary>
        /// Позиции уже заспавненных юнитов отряда на этой локации.
        /// Используется PlayerFactory, чтобы не спавнить юнитов друг на друге.
        /// Живёт ровно один рейд — создаётся заново с новым LocationContext.
        /// </summary>
        public List<Vector3> OccupiedSpawnPositions = new();
        public sbyte LastIdCampSpawnPoint = -1;
        
        public EnemySpawnPoint[] AmbientSpawnPoints;
        public WaveSpawnPoint[] WaveSpawnPoints;

        public LocationGuaranteedProfile LocationGuaranteedProfile;
        public LocationLootProfile LocationLootProfile;
        public LootSpawnPoint[] LootSpawnPoints;

    }
}