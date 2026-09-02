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
    [CreateAssetMenu(fileName = "StartKitConfig", menuName = "Game Configs/Start World/Start Kit Config")]
    public class StartKitConfigBase : ScriptableObject
    {
        [Serializable]
        public struct StartItem
        {
            public ItemId configId;
            public int amount;
            public int durability;
        }

        [SerializeField] protected List<StartItem> items;
        public IReadOnlyList<StartItem> Items => items;

        [SerializeField] private bool spawnAllResources;
        


        public virtual void Apply()
        {
            ServiceLocator.Current.Get<IEconomyService>().Add(EBankResourceType.CurrencyPremium, 1000);
            
            var access = ServiceLocator.Current.Get<InventoryManagementWindow>().controller.AccessService;
            var baseRuntime = ServiceLocator.Current.Get<GameSession>().GameLoopContext.CampRuntime;
            var startKit = items;


            // добавляем все игровые ресурсы
            if (spawnAllResources)
            {
                var resources = GameContent.Items.All;
                foreach (var res in resources.Values)
                {
                    if (res.Classification.category == ItemCategory.Resource)
                    {
                        var result = access.TryAdd(
                            baseRuntime.Sources[0],
                            new InventorySlotRuntime(
                                res,
                                40,
                                0,
                                0
                            ));
                        DLog.Alert($"Added {res}: {result.IsFullyAdded} - {result.Added} / {result.Remaining}");
                    }
                }
                
                return;
            }
            
            
            foreach (var item in startKit)
            {
                var itemBase = GameContent.Items.Get(item.configId);
                var result = access.TryAdd(
                    baseRuntime.Sources[0],
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