using System.Linq;
using Galactic1.Core;
using UnityEngine;

namespace Galactic1
{
    public class CmdPlaceEntitiesHandler : ICommandHandler<CmdPlaceEntity>
    {
        private readonly GameStateProxy _gameState;

        public CmdPlaceEntitiesHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }


        public bool Handle(CmdPlaceEntity command)
        {
            var currentWorldState = _gameState.WorldsState.FirstOrDefault(map => map.Id == 0);
            if (currentWorldState == null)
            {
                Debug.LogError($"Couldn't create map state with Id {0}");
                return false;
            }

            // #1 создаем оригинальную дату
            var createdEntityData = command.EntityType switch
            {
                EntityType.Furniture => EntitiesDataFactory.CreateEntity<StructureEntityData>(
                    command.EntityType,
                    command.EntityConfigId,
                    command.PrefabPath,
                    command.Level,
                    command.Position),
                _=> throw new System.NotImplementedException()
            };
            // #2 потом ее прокси, для связки с сохранением
            var createdEntityProxy = EntitiesProxyFactory.CreateEntity(createdEntityData);
            
            // #2 add to list all entities
            currentWorldState.Entities.Add(createdEntityProxy);

            return true;
        }
    }
}