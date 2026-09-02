using System;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.Runtime.Production;

namespace Galactic1.Runtime.Production
{
    /// <summary>
    /// Абстракция производственной станции.
    /// 
    /// Контракт между SceneAdapter и Runtime.
    /// 
    /// Гарантии:
    /// - Runtime является единственным источником истины
    /// - Очередь управляется только через Runtime
    /// - Scene не знает конкретную реализацию (Craft / Recycler / etc)
    /// </summary>
    public interface IProductionStationRuntime
    {
        /// <summary>
        /// Вызывается при изменении состояния очереди или уровня станции.
        /// </summary>
        event Action OnStateChanged;

        /// <summary>
        /// Конфиг станции (для доступа к Header, Recipes и т.д.)
        /// </summary>
        FacilityModule Config { get; }
        
        FacilityType Type { get; }

        /// <summary>
        /// Текущий уровень станции.
        /// </summary>
        int Level { get; }

        /// <summary>
        /// Текущий мировой час (источник — GameTimeService).
        /// </summary>
        int TotalWorldHour { get; }

        /// <summary>
        /// Возвращает DTO-очередь.
        /// Runtime остаётся источником истины.
        /// </summary>
        IReadOnlyList<ProductionJobRuntimeDTO> GetQueueDTO();

        /// <summary>
        /// Проверка возможности добавления нового заказа.
        /// </summary>
        bool CanAddJob(RuntimeId recipeId);

        /// <summary>
        /// Добавление нового заказа.
        /// </summary>
        bool TryAddJob(RuntimeId recipeId, int durationHours, int orders, int stackLimit, int amount);

        /// <summary>
        /// Отмена заказа.
        /// </summary>
        bool CancelJob(string jobId, int ordersToCancel = 1);

        /// <summary>
        /// Полное удаление завершённых заказов.
        /// </summary>
        void CollectCompletedOrders(string jobId, int orders);

        /// <summary>
        /// Частичное уменьшение количества в завершённом заказе.
        /// </summary>
        void ReduceCompletedAmount(string jobId, int amount);

        /// <summary>
        /// Завершить активный заказ (skip).
        /// </summary>
        void SkipActive();
    }
}