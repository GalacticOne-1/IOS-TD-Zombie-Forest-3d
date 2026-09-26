using Galactic1.Code.Dev;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Configs;
using Galactic1.Core.Systems.GameLoopSession;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    
    [CreateAssetMenu(fileName = "TutorialGrenadeActionDefinition",
        menuName = "Game Configs/Tutorial/Actions/Grenade Inventory")]
    public  class TutorialGrenadeActionDefinition : TutorialActionDefinition
    {
        public override void Evaluate()
        {
            var startKitData = ServiceLocator.Current.Get<ConfigProvider>().Get<StartKitData>();
            
            var cargo = ServiceLocator.Current.Get<GameSession>()
                .GameLoopContext.CurrentRaid.PlayerTransport.Sources.Cargo;
            var resourcesPort = (IInventoryResourcesPort)cargo;
            var cargoKit = startKitData.GetKit(EStartKit.StartGameBattle_02);

            foreach (var item in cargoKit.Items)
            {
                var itemBase = GameContent.Items.Get(item.configId);
                resourcesPort.TryAdd(
                    new InventorySlotRuntime(
                        itemBase,
                        item.amount,
                        0,
                        0
                    ));
            }
        }
    }
}