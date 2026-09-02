using Galactic1.AbstractFactory;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Gameplay.Player;
using UnityEngine;
using Galactic1.Core.GameSession;

namespace Galactic1.Core.Systems.Factories
{
    
    public sealed class TransportFactory : IGameService
    {

        public TransportFactory() {}

        public TransportInstance Create(
            int index, 
            SceneSessionDefinition context,
            ITransportRuntime runtimeSource,
            string prefabId)
        {
            //var playerData = context.PlayerSpawnPreset.GetData();
            //playerData.RuntimeUnitViewSource = runtimeSource;
            
            // Создаем
            var instance = $"{AppConstants.PATH_ENTITIES}{prefabId}"
                .CreateGO(ServiceLocator.Current.Get<Environment>().playerUnits)
                .GetComponent<TransportInstance>();
            
            var uniqueId = runtimeSource.Id;
            instance.UniqueId = uniqueId;
            instance.Id = index;
            //ServiceLocator.Current.Get<UnitRepository>().Register(uniqueId, instance);

            instance.Tr.position = context.LocationContext.TransportSpawnPoint.position;
            instance.Tr.rotation = context.LocationContext.TransportSpawnPoint.rotation;
            
            // Передаём контекст
            context.Transport = instance;
            
            // создаём адаптер для сцены
            ISceneUnit sceneUnit = new SceneTransportAdapter(runtimeSource);
            instance.Entity_Initialize(sceneUnit);


            // 2 — статы
            //ApplyStats(playerData, instance, context.PlayerSpawnPreset);
            

            // 3 — экипировка
            //ApplyEquipment(playerData, instance, context.PlayerSpawnPreset);
            
            // 4 — инвентарь
            //ApplyInventory(playerData, instance, context.PlayerSpawnPreset);


            // Пока игрок не активирован (не принимает управление)
            //player.DisableInput();
            
            
            // * регистрация для очистки
            //EventBus<SceneClearEvent>.Register(new EventBinding<SceneClearEvent>(() => Clear(uniqueId)));
            
            return instance;
        }

        public void Clear(string uniqueId)
        {
            // var rep = ServiceLocator.Current.Get<UnitRepository>().TryGet(uniqueId);
            // if (rep.done)
            // {
            //     ServiceLocator.Current.Get<UnitRepository>().Unregister(uniqueId, rep.instance);
            //     rep.instance.Entity_Destroy();
            //     //ServiceLocator.Current.Get<PlayerInteractionController>().Clear();
            //     //new ParallaxClear();
            // }
        }


        private void ApplyStats(PlayerLoadData playerData, TransportInstance instance, PlayerSpawnPreset preset)
        {
            instance.Entity_Setup(playerData);
            //survGO.GetComponent<SurvivalController>().Initialize();
        }

        private void ApplyEquipment(PlayerLoadData playerData, TransportInstance instance, PlayerSpawnPreset preset)
        {
            //PlayerEquipmentApplier.Apply(playerData, instance);
        }

        private void ApplyInventory(PlayerLoadData playerData, TransportInstance instance, PlayerSpawnPreset preset)
        {
            
        }
    }
}