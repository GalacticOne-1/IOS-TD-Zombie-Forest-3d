
using Galactic1.Code.Core;
using Galactic1.Code.Systems.GameLoop.States;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.Systems.UI;
using Galactic1.Gameplay;
using Galactic1.RaidLoot.Definition;
using Galactic1.RaidLoot.Events;
using Galactic1.RaidLoot.Scene.Lifecycle;
using Galactic1.RaidLoot.Services;

namespace Galactic1.Code.Systems.GameLoop.Tactical
{
    // =========================================================
    // Очистка после рейда и подготовка к PostRaidReport
    // =========================================================
    public sealed class SUB_RaidCleanupState : ITacticalState
    {
        private readonly LocationLootProfile _lootProfile;

        public SUB_RaidCleanupState(LocationLootProfile lootProfile)
        {
            _lootProfile = lootProfile;
        }


        public void Enter(DIContainer container, GameLoopContext context)
        {
            DLog.Alert("RaidCleanupState enter: очистка и подготовка к отчёту", AppConstants.show_log_core);
            
            
            var raid = context.CurrentRaid;

            
            // #1 выдача лута добытого за прохождение локации
            RaidResultProxy result = null;
            
            // - пока награду выдаем тоько при победе
            // - добавить выдачу награды даже если RaidStatus.Failed ???
            //if (raid.Status == RaidStatus.Completed)
            //{
                // ⬇ ЕДИНСТВЕННОЕ место, где считается результат рейда
                //result = raid.CalculateResult();
                result = raid.Scenario.BuildRaidResult(
                    raid,
                    raid.MissionResult);
                
                // Записываем результат в Runtime
                //raid.Result = result;
                context.Proxy.LastRaidResult = result;
            //}
            
            // === очищаем сервис лута
            raid.LootBuffer?.Clear();
            raid.EconomyState?.Clear();
            raid.CurrentRaidLootContainer?.Dispose();

            //_container.Resolve<LootContainerRepository>()?.Clear();

            //raid.LootEventBus?.Unsubscribe<ContainerOpenedEvent>(/* ... */);
            //raid.LootEventBus?.Unsubscribe<LootGeneratedEvent>(/* ... */);
            //raid.LootEventBus?.Unsubscribe<LootAutoPickedEvent>(/* ... */);

            
            // =========================================================================================================
            // =========================================================================================================
            
            // Завершение рейда: удалить временные объекты, юнитов

            
            // === очищаем сервисы
            container.Resolve<EnemyHealthBarSystem>().Dispose();
            raid.Enemies.Clear();
            raid.CurrentRaidLifecycle?.Dispose();
            raid.CurrentRaidLifecycle = null;
            raid.Combat.Dispose();
            raid.Combat.DebugDrawer?.Dispose();
            raid.AILOD?.Dispose();
            
            
            // *** выход из тактической SM в глобальную
            if (context.GameLoopStateMachine.CurrentState is RaidInProgressState inProgress)
            {
                inProgress.OnRaidFinished(result);
            }
        }

        public void Update(GameLoopContext context, float deltaTime)
        {
            
        }

        public void Exit(GameLoopContext context)
        {
            DLog.Alert("RaidCleanupState exit", EDlogColor.YELLOW, AppConstants.show_log_core);
        }
    }
}