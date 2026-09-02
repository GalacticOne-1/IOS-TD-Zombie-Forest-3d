
using Galactic1.Gameplay.Locations;
using Galactic1.RaidLoot.Definition;
using Galactic1.RaidLoot.Scene;
using UnityEngine;

namespace Galactic1.Core.GameSession
{
    /// <summary>
    /// Отвечает за подготовку GameSessionContext перед запуском сессии.
    /// Содержит весь процесс загрузки: сохранений, локации, правил загрузки игрока.
    /// Аналог загрузчика "Game" и "Scene" конфигов в LDoE.
    /// </summary>
    public class SceneSessionDefinitionLoader
    {
        public SceneSessionDefinition LoadDefinition(
            SceneDefinitionData sceneData,
            LocationContext locationContext)
        {
            var context = new SceneSessionDefinition();
            
            // #1 Загружаем сохранение игрока
            //context.PlayerSave = SaveManager.LoadPlayer(); 

            // #2 Запоминаем тип локации
            locationContext.LocationType = locationContext.LocationConfig.LocationType;

            // #3 camera
            locationContext.CameraPosition = sceneData.cameraPosition;
            locationContext.CameraMinBounds = sceneData.cameraMinBounds;
            locationContext.CameraMaxBounds = sceneData.cameraMaxBounds;
            
            
            
            // #4 Выбираем профиль загрузки игрока (для леса/бункера/дома и т.п.)
            context.PlayerSpawnPreset = locationContext.LocationConfig.PlayerPreset;
            
            // === transport
            locationContext.TransportSpawnPoint = sceneData.transportSpawnPoint;
            
            // === enemies
            locationContext.AmbientSpawnPoints = sceneData.AmbientSpawnPoints;
            locationContext.WaveSpawnPoints = sceneData.WaveSpawnPoints;
            
            // === loot
            var guaranteedLootProfile = locationContext.LocationConfig.LocationIntel.guaranteedLootProfile;
            locationContext.LocationGuaranteedProfile = new LocationGuaranteedProfile(
                locationContext.LocationConfig.Id, 
                guaranteedLootProfile.Entries);
            
            var lootProfile = locationContext.LocationConfig.LocationIntel.lootProfile;
            locationContext.LocationLootProfile = new LocationLootProfile(
                locationContext.LocationConfig.Id,
                lootProfile.Multipliers);
            
            locationContext.LootSpawnPoints = sceneData.LootSpawnPoints;
            
            // #5 позиция для спавна игрока
            locationContext.PlayerSpawnPosition = sceneData.squadSpawnPoint.transform.position;

            locationContext.SquadSpawnDepth = sceneData.squadSpawnDepth;
            locationContext.SquadSpawnWidth = sceneData.squadSpawnWidth;
            locationContext.SquadSpawnMinDistance = sceneData.squadSpawnMinDistance;



            // ************************************************************************************************
            // ************************************************************************************************
            Debug.Log($"[GameSession] Scene Definition loaded. Location = {locationContext.LocationConfig.LocationType}");

            context.LocationContext = locationContext;
            return context;
        }
    }
}