using System;
using System.Linq;
using Galactic1;
using Galactic1.Core;

namespace Galactic1
{
    public class CmdBankResourcesAddHandler : ICommandHandler<CmdBankResourcesAdd>
    {
        private readonly GameStateProxy _gameState;


        public CmdBankResourcesAddHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }

        public bool Handle(CmdBankResourcesAdd command)
        {
            var requiredResourceType = command.ResourceType;
            var requiredResource = _gameState.BankResources.FirstOrDefault(r => r.BankResourceType == requiredResourceType);
            if (requiredResource == null)
            {
                requiredResource = CreateNewResource(requiredResourceType);
            }

            requiredResource.Amount.Value += Math.Abs(command.Amount);

            return true;
        }


        BankResourceProxy CreateNewResource(EBankResourceType resourceType)
        {
            var newResourceData = new BankResourceData
            {
                BankResourceType = resourceType
            };

            var newResource = new BankResourceProxy(newResourceData);
            _gameState.BankResources.Add(newResource);
            
            return newResource;
        }
    }
}