using Galactic1.Code.Dev;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.Inbox;
using Galactic1.Code.UI.Inventory;
using Galactic1.Configs;
using Galactic1.Core.Systems.GameLoopSession;
using UnityEngine;

namespace Galactic1.EntryPoint
{
    public class NewGameEntry
    {
        
        private const string NEW_GAME_KEY = nameof(NEW_GAME_KEY);

        
        /// <summary>
        /// Одноразовая загрузка при первом старте игры
        /// </summary>
        public void StartBattle()
        {
            if (PlayerPrefs.HasKey(NEW_GAME_KEY))
                return;

            PlayerPrefs.SetString(NEW_GAME_KEY, "y");

            // === спавн стартовых предметов на базе игрока
            
            // inventory
            var startKitData = ServiceLocator.Current.Get<ConfigProvider>().Get<StartKitData>();
            //startKitData.GetKit(EStartKit.StartGame_01).Apply();
            
            // cargo
            var cargo = ServiceLocator.Current.Get<GameSession>()
                .GameLoopContext.PlayerTransport.GetInventory;
            var resourcesPort = (IInventoryResourcesPort)cargo;
            var cargoKit = startKitData.GetKit(EStartKit.StartGameBattle_01);

            foreach (var item in cargoKit.Items)
            {
                var itemBase = GameContent.Items.Get(item.configId);
                var result = resourcesPort.TryAdd(
                    new InventorySlotRuntime(
                        itemBase,
                        item.amount,
                        item.durability != 0
                            ? item.durability
                            : (int)(itemBase.Physical.maxDurability * Random.Range(.7f, .8f)),
                        0
                    ));
                            
                DLog.Alert($"Added to cargo: {item.configId}: {result.IsFullyAdded} - {result.Added} / {result.Remaining}");
            }

            // inbox
            // var inboxKit = startKitData.GetKit(EStartKit.StartGameInbox_01);
            // foreach (var item in inboxKit.Items)
            // {
            //     ServiceLocator.Current.Get<InboxService>().AddReward(
            //         new InventorySlotRuntime(
            //             GameContent.Items.Get(item.configId),
            //             item.amount,
            //             item.durability,
            //             0));
            // }
        }


        public void StartCamp()
        {
            // inventory
            var startKitData = ServiceLocator.Current.Get<ConfigProvider>().Get<StartKitData>();
            startKitData.GetKit(EStartKit.StartGame_01).Apply();
            
            // cargo
            var cargo = ServiceLocator.Current.Get<GameSession>()
                .GameLoopContext.PlayerTransport.GetInventory;
            var resourcesPort = (IInventoryResourcesPort)cargo;
            var cargoKit = startKitData.GetKit(EStartKit.StartGameRaid);

            foreach (var item in cargoKit.Items)
            {
                var itemBase = GameContent.Items.Get(item.configId);
                var result = resourcesPort.TryAdd(
                    new InventorySlotRuntime(
                        itemBase,
                        item.amount,
                        item.durability != 0
                            ? item.durability
                            : (int)(itemBase.Physical.maxDurability * Random.Range(.7f, .8f)),
                        0
                    ));
                            
                DLog.Alert($"Added to cargo: {item.configId}: {result.IsFullyAdded} - {result.Added} / {result.Remaining}");
            }

            // inbox
            var inboxKit = startKitData.GetKit(EStartKit.StartGameInbox_01);
            foreach (var item in inboxKit.Items)
            {
                ServiceLocator.Current.Get<InboxService>().AddReward(
                    new InventorySlotRuntime(
                        GameContent.Items.Get(item.configId),
                        item.amount,
                        item.durability,
                        0));
            }
        }
    }
}