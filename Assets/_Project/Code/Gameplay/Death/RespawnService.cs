
using Galactic1.Configs;
using Galactic1.Core;
using Galactic1.Core.GameSession;
using Galactic1.Code.Gameplay.Units.Stats;
using Galactic1.Utility;

namespace Galactic1.Gameplay.Death
{
    /// <summary>
    /// Сервис, отвечающий за респаун игрока после смерти.
    /// Он:
    /// - Телепортирует игрока в home spawn
    /// - Сбрасывает инвентарь/экипировку
    /// - Восстанавливает статы (HP и т.д.)
    /// - Поднимает событие OnPlayerRespawned
    /// 
    /// Этот сервис должен быть вызван DeathSystem'ом.
    /// </summary>
    public class RespawnService
    {
        private readonly DIContainer _container;

        public RespawnService(DIContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// Асинхронный респаун — можно показать экран "You Died" и ждать нажатия.
        /// В простейшем варианте мы выполняем немедленный респаун.
        /// </summary>
        public void RespawnPlayerImmediate(SceneSessionDefinition session)
        {
            // var player = session.Player;
            // if (player == null)
            // {
            //     Debug.LogWarning("[RespawnService] Session.Player == null");
            //     return;
            // }
            //
            // // #1 Сброс статов
            // ClearPlayerStats();
            //
            // // #2 Респавн всегда на домашней локации
            // _container.Resolve<LocationTransitionService>().GoToLocation(0);   


            // 5) Событие
            //DeathEvents.RaisePlayerRespawned(homePos);
        }

        /// <summary>
        /// Для удаления всего лута у игрока
        /// </summary>
        /// <param name="session"></param>
        public void ClearPlayerInventory()
        {
            var stateProvider = _container.Resolve<IGameStateProvider>();
            
            // player
            stateProvider.GameStateProxy.PlayerUnits[0].InventoryProxy.ClearSlots();
            stateProvider.GameStateProxy.PlayerUnits[0].EquipmentProxy.ClearSlots();
            
            // dragon
            stateProvider.GameStateProxy.PlayerUnits[1].InventoryProxy.ClearSlots();
            stateProvider.GameStateProxy.PlayerUnits[1].EquipmentProxy.ClearSlots();
        }


        public void ClearPlayerStats()
        {
            var stateProvider = _container.Resolve<IGameStateProvider>();
            var configProvider = _container.Resolve<IConfigProvider>(); 
            var playerStatsBase = configProvider.Get<PlayerStatsBase>();
            var dragonStatsBase = configProvider.Get<PlayerDragonStatsBase>();
            
            _container.Resolve<IGameStateProvider>().GameStateProxy.PlayerUnits[0].IsDead.Value = false;
            
            
            var playerData = DictionaryUtility.ToList(playerStatsBase.GetBaseStats());
            foreach (var kvp in playerData)
                stateProvider.GameStateProxy.PlayerUnits[0].Stats[kvp.Key].Value = kvp.Value;
            
            var dragonData = DictionaryUtility.ToList(dragonStatsBase.GetBaseStats());
            foreach (var kvp in dragonData)
                stateProvider.GameStateProxy.PlayerUnits[1].Stats[kvp.Key].Value = kvp.Value;
        }
    }
}
