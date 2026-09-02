using System;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.Economy;
using Galactic1.Code.UI.Inventory;
using Galactic1.Core.Enums;
using Galactic1.Core.Systems.GameLoopSession;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Galactic1.Code.Dev
{
    [CreateAssetMenu(fileName = "StartKitNewGame", menuName = "Game Configs/Start World/Start Kit New Game")]
    public class StartKitNewGame : StartKitConfigBase
    {

        public override void Apply()
        {
            ServiceLocator.Current.Get<IEconomyService>().Add(EBankResourceType.CurrencyPremium, 20);
            
            var baseRuntime = ServiceLocator.Current.Get<GameSession>().GameLoopContext.CampRuntime;
            var startKit = items;

            var resourcesPort = (IInventoryResourcesPort)baseRuntime.Sources[0];

            foreach (var item in startKit)
            {
                var itemBase = GameContent.Items.Get(item.configId);
                var result = resourcesPort.TryAdd(
                    new InventorySlotRuntime(
                        itemBase,
                        item.amount,
                        item.durability != 0
                            ? item.durability
                            : (int)(itemBase.Physical.maxDurability * Random.Range(.1f, .8f)),
                        0
                    ));
                            
                DLog.Alert($"Added {item.configId}: {result.IsFullyAdded} - {result.Added} / {result.Remaining}");
            }
        }
    }
}