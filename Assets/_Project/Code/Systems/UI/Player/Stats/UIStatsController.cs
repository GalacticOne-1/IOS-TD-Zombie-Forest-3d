using System.Collections.Generic;
using Galactic1.Code.UI.Inventory;
using Galactic1.Code.UI.Units;
using R3;
using UnityEngine;

namespace Galactic1.Core.UI
{
    public class UIStatsController
    {
        private readonly CompositeDisposable _disposables = new();

        private Dictionary<StatId, List<StatSlotUI>> playerGroupSlots = new();


        /// <summary>
        /// Подключение статов UI к прокси
        /// </summary>
        public void Register(DIContainer container)
        {
            var statsPanelUis = GameObject.FindObjectsByType<UIStatsPanel>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            // #1 группируем слоты с одинаковыми статами
            foreach (var panelUi in statsPanelUis)
            {
                panelUi.Initialize();

                foreach (var group in panelUi.SlotGroups)
                {
                    foreach (var slot in group.statSlots)
                    {
                        if (!playerGroupSlots.ContainsKey(slot.StatId))
                            playerGroupSlots[slot.StatId] = new();

                        playerGroupSlots[slot.StatId].Add(slot);
                    }
                }
            }



            var gameLoopContext = container.Resolve<Systems.GameLoopSession.GameSession>().GameLoopContext;

            // #2 подписываем на прокси Player
            var displayData = gameLoopContext.GetDisplayAllUnit();
            foreach (var runtime in displayData)
                RefreshStats(runtime);

            // === подписываемся для новых юнитов
            gameLoopContext.OnUnitCreated += RefreshStats;
            EventBus<SceneClearEvent>.Register(new EventBinding<SceneClearEvent>(()
                => gameLoopContext.OnUnitCreated -= RefreshStats));

            ServiceLocator.Current.Get<InventoryManagementWindow>().modeController.OnUnitChanged += ClearStats;
            // 


            // Отписка при очистке сцены
            EventBus<SceneClearEvent>.Register(
                new EventBinding<SceneClearEvent>(_ =>
                {
                    _disposables.Dispose(); // <<<<< отписывает всё
                    _disposables.Clear(); // если хочешь использовать повторно
                })
            );
        }

        void RefreshStats(UnitDisplayData runtime)
        {
#if UNITY_EDITOR
            //DLog.Alert($"********************** RefreshStats {runtime}");
#endif

            foreach (var gs in playerGroupSlots)
            {
                // подписка статов на каждый юнит
                runtime.Stats.OnStatChanged += (e, excludeEffects) =>
                {
#if UNITY_EDITOR
                    DLog.Alert($"Changed state order: {e.Type}", EDlogColor.YELLOW);
#endif
                    if (gs.Key == e.Type)
                    {
#if UNITY_EDITOR
                        //DLog.Alert($"Changed state: {e.Type} / {e.Current}");
#endif
                        var l = gs.Value.Count;
                        for (int i = 0; i < l; i++)
                            gs.Value[i].Set(e.Current, e.Max, excludeEffects);
                    }
                };
            }
        }

        // полное обнуление статов
        void ClearStats(string unitId)
        {
            if(string.IsNullOrEmpty(unitId))
            {
                foreach (var gs in playerGroupSlots)
                {
                    var l = gs.Value.Count;
                    for (int i = 0; i < l; i++)
                        gs.Value[i].Set(0, 0, true);
                }
            }
        }

    }
}