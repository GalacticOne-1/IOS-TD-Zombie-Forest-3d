using System;
using System.Collections.Generic;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.Economy;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Inbox;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.Runtime.Production;
using Galactic1.Game.Runtime.Recruitment;
using Galactic1.Items;
using Galactic1.Runtime.Production;
using UnityEngine;

namespace Galactic1.Code.Systems.Runtime
{
    public sealed class SceneAdapterFactory : ISceneAdapterFactory
    {
        private readonly GameLoopContext gameLoopContext;
        private readonly Dictionary<Type, Func<object, IFacilitySceneAdapter>> _map = new();

        
        

        public SceneAdapterFactory(
            IEconomyService economy,
            ItemDatabase itemDatabase,
            GameLoopContext glc,
            StorageRegistry storageRegistry)
        {
            gameLoopContext = glc;

            Register<IInboxFacilityRuntime>(runtime =>
                new InboxSceneAdapter(
                    runtime,
                    glc.CampRuntime.Sources[0] as IInventoryResourcesPort,
                    glc.PlayerTransport.GetInventory as IInventoryResourcesPort,
                    glc.CurrentRaid?.PlayerTransport?.Sources.Cargo as IInventoryResourcesPort
                ));
            
            Register<IProductionStationRuntime>(runtime =>
                new UniversalProductionSceneAdapter(
                    runtime,
                    glc.CampRuntime.Sources[0] as IInventoryResourcesPort,
                    economy
                ));
            
            Register<IStorageFacilityRuntime>(runtime =>
                new StorageSceneAdapter(runtime));

            Register<IRecruitmentTavernRuntime>(runtime =>
                new RecruitmentTavernSceneAdapter(runtime));
            
            Register<ILivingModuleFacilityRuntime>(runtime =>
                new LivingModuleSceneAdapter(runtime));

            Register<IGarageFacilityRuntime>(runtime =>
                new GarageSceneAdapter(
                    glc,
                    runtime,
                    glc.CampRuntime.Sources[0] as IInventoryResourcesPort,
                    economy,
                    itemDatabase
                ));

            Register<IRaidFacilityRuntime>(runtime =>
                new DamageableFacilitySceneAdapter(runtime));
        }

        public void Register<TRuntime>(Func<TRuntime, IFacilitySceneAdapter> factory)
        {
            _map[typeof(TRuntime)] = runtime => factory((TRuntime)runtime);
        }

        public (IFacilitySceneAdapter, FacilityUpgradeSceneAdapter) Create(BaseCampFacilityRuntime runtime)
        {
            var runtimeType = runtime.GetType();

            foreach (var kvp in _map)
            {
                if (kvp.Key.IsAssignableFrom(runtimeType))
                {
                    var sceneAdapter = kvp.Value(runtime);
                    var upgradeAdapter = new FacilityUpgradeSceneAdapter(
                        runtime, 
                        gameLoopContext.CampRuntime.Sources[0] as IInventoryResourcesPort);

                    return (sceneAdapter, upgradeAdapter);
                }
            }

            Debug.LogError($"No SceneAdapter registered for {runtimeType}");
            return (null, null);
        }
    }
}