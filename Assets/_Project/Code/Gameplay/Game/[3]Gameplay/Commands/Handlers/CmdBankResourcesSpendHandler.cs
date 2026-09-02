using System;
using System.Linq;
using Galactic1;
using Galactic1.Core;
using UnityEngine;

namespace Galactic1
{
    public class CmdBankResourcesSpendHandler : ICommandHandler<CmdBankResourcesSpend>
    {
        private readonly GameStateProxy _gameState;


        public CmdBankResourcesSpendHandler(GameStateProxy gameState)
        {
            _gameState = gameState;
        }

        public bool Handle(CmdBankResourcesSpend command)
        {
            var requiredResourceType = command.ResourceType;
            var requiredResource = _gameState.BankResources.FirstOrDefault(r => r.BankResourceType == requiredResourceType);
            if (requiredResource == null)
            {
                Debug.LogError($"Trying spend not exist resource [{command.ResourceType}]");
                return false;
            }

            var sum = Math.Abs(command.Amount);
            if (requiredResource.Amount.Value < sum)
            {
                DLog.Alert($"Trying to spend resources more than have [{command.ResourceType}]\n Have : {requiredResource.Amount.Value}\n Required : [{sum}]", EDlogColor.ORANGE, AppConstants.show_log_economics);
                return false;
            }
            requiredResource.Amount.Value -= sum;

            return true;
        }


    }
}