using System.Collections.Generic;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Configs;
using Galactic1.Game.Buildings.Proxy;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.World.StartLocation;

namespace Galactic1.Code.Systems.Runtime.Building
{
    public sealed class GarageFacilityRuntime : 
        BaseCampFacilityRuntime,
        IGarageFacilityRuntime
    {
        private readonly GameLoopContext _gameLoopContext;

        public override FacilityType Type => FacilityType.Garage;
        //public override bool CanUpgrade => false;
        
        
        
        public GarageFacilityRuntime(
            FacilityProxy proxy,
            GarageModule config,
            GameTimeService timeService,
            GameLoopContext gameLoopContext)
            : base(proxy, config, timeService)
        {
            _gameLoopContext = gameLoopContext;

            // === открываем первую машину
            var startingTransport= ServiceLocator.Current.Get<ConfigProvider>().Get<WorldStartConfig>().Transport;
            UnlockModule(startingTransport.Id);
        }

        public override void Dispose(){}




        public RuntimeId GetCurrentVehicleId()
            => _gameLoopContext.PlayerTransport.Item?.Id;

        public IReadOnlyCollection<RuntimeId> GetUnlockedModules()
        {
            List<RuntimeId> result = new();

            var l = Proxy.UnlockedModules.Count;
            for (int i = 0; i < l; i++)
            {
                var guid = Proxy.UnlockedModules[i];

                if (GameContent.ItemIds.TryGet(guid, out var itemId))
                    result.Add(itemId);
            }

            return result;
        }




        public bool IsModuleUnlocked(RuntimeId moduleId)
            => Proxy.UnlockedModules.Contains(moduleId.Guid);

        public void UnlockModule(RuntimeId moduleId)
        {
            if (Proxy.UnlockedModules.Contains(moduleId.Guid))
                return;

            Proxy.UnlockedModules.Add(moduleId.Guid);

            MarkStateChanged();
        }

        /// <summary>
        /// Для смены транспорта
        /// </summary>
        /// <param name="configId"></param>
        public void ReplaceCurrentVehicle(RuntimeId configId)
        {
            _gameLoopContext.ReplaceCurrentVehicle(configId);
            MarkStateChanged();
        }
    }
}