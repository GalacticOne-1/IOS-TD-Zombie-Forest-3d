using Galactic1;
using Galactic1.Code.Dev;
using Galactic1.Configs;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Game.Meta.Items;

namespace DEV
{
    public static class DevTestResolver
    {
        
        
        
        
        
        public static void ClearInventory()
        {
            var source = ServiceLocator.Current.Get<GameSession>().GameLoopContext
                .CampRuntime.GetInventory(StorageType.Regular);

            var slots = source.GetSlots();
            for (int i = 0; i < slots.Count; i++)
            {
                source.ClearSlot(i); 
            }
        }

        public static void LoadStarterKit()
        {
            ServiceLocator.Current.Get<ConfigProvider>().Get<StartKitData>().GetKit(EStartKit.StartGame_01).Apply();
        }
        
        public static void LoadAllResources()
        {
            ServiceLocator.Current.Get<ConfigProvider>().Get<StartKitData>().GetKit(EStartKit.AllResources).Apply();
        }
        
        public static void LoadConstructionKit()
        {
            ServiceLocator.Current.Get<ConfigProvider>().Get<StartKitData>().GetKit(EStartKit.ConstructionKit).Apply();
        }
    }
}