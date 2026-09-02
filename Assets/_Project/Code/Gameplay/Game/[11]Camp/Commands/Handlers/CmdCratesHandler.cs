using System.Linq;
using Galactic1.Core;
using Galactic1.Code.UI.Inventory;
using UnityEngine;

namespace Galactic1
{
    public class CmdCratesHandler : ICommandHandler<CmdCrates>
    {
        private readonly GameStateProxy _gameState;

        public CmdCratesHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }

        public bool Handle(CmdCrates command)
        {
            var currentMap = _gameState.WorldsState.FirstOrDefault(map => map.Id == 0);
            if (currentMap == null)
            {
                Debug.LogError($"Couldn't create map state with Id {0}");
                return false;
            }
            
            var newCrateEntity = new CrateEntityData
            {
                //UniqueId = _gameState.CreateEntityId(),
                Unlock = command.Unlock,
                //Slot = new InventorySlot[command.SlotAmount]
            };

            var newCrateEntityProxy = new CrateEntityProxy(newCrateEntity);
            currentMap.Crates.Add(newCrateEntityProxy);

            return true;
        }
    }
}