using System.Collections.Generic;
using System.Linq;
using Galactic1.Configs;
using Galactic1.Core;
using Galactic1.Game.Buildings.Proxy;
using UnityEngine;

namespace Galactic1
{
    public class CmdCreateWorldsStateHandler : ICommandHandler<CmdCreateWorldState>
    {
        private readonly GameStateProxy _gameState;
        private readonly GameConfig _gameConfig;
        private readonly IConfigProvider _configProvider;


        public CmdCreateWorldsStateHandler(GameStateProxy gameState, GameConfig gameConfig, IConfigProvider configProvider)
        {
            _gameState = gameState;
            _gameConfig = gameConfig;
            _configProvider = configProvider;
        }


        
        public bool Handle(CmdCreateWorldState command)
        {
            var worldStateExist = _gameState.WorldsState.Any(l => l.Id == command.WorldStateId);

            if (worldStateExist)
            {
                Debug.LogError($"World State with Id {command.WorldStateId} exists!");
                return false;
            }

            var newWorldStateSettings = _configProvider.LocationsState._configs
                .First(l => l.Key == command.WorldStateId);
            var newWorldInitialStateSettings = newWorldStateSettings.Value.initialStateConfigs;
            
            
            // ******************************************************************************************
            // здесь настраивается состояние по умолчанию, при первом запуске
            
            
            // создаем объекты по умолчанию которые находятся на карте
            // #1
            // var structureConfigs = _configProvider.Structures._configs;                 // FIX : remove
            // var initialEntities = new List<EntityData>();
            // foreach (var entityConfigs in newWorldInitialStateSettings.Entities)
            // {
            //     // FIX : этот массив убрать !!! 
            //     // должны получать нужный конфиг из ConfigManager по entityConfigs.ConfigId
            //     foreach (var config in structureConfigs)
            //     {
            //         if (config.Key == entityConfigs.ConfigId)
            //         {
            //             var initialEntityData = EntitiesDataFactory.CreateEntity(entityConfigs, config.Value);
            //             initialEntityData.UniqueId = _gameState.CreateEntityId(); 
            //             initialEntities.Add(initialEntityData);
            //         }
            //     }
            // }
            
            
            // #2
            var initialBuildings = new List<FacilityData>();
            // todo ...
            
            
            // #3
            // ...
            
            
            
            // т.е кроме объектов, можно устанавливать какие предметы должны быть у игрока
            //      - например он спавнится во второй локации где может строить, но там используется новые ресурсы
            //          их можно загрузить игроку в инвентарь здесь
            
            // #1
            // ...
            
            
            // #2 
            // ...
            
            
            // ******************************************************************************************
            // создаем карту со всеми созданными объектами до этого ^^
            var newWorldState = new WorldData
            {
                Id = command.WorldStateId,
                //Facilities = initialBuildings
                //Entities = initialEntities,
                //Crates = 
            };

            var newWorldStateProxy = new WorldDataProxy(newWorldState);
            _gameState.WorldsState.Add(newWorldStateProxy);

            return true;
        }
    }
}