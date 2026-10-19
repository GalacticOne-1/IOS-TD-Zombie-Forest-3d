using System;
using Galactic1.Code.Systems.Economy;
using Galactic1.Runtime.Production;

namespace Galactic1.Game.Runtime.Production
{
    /// <summary>
    /// Общие расчёты времени и цены производства.
    /// Используется в FacilityDetailsFactory, StationsPanelPresenter и адаптере станции.
    /// Единственный источник истины для логики времени и цены скипа.
    /// </summary>
    public static class ProductionTimeCalculator
    {
        /// <summary>
        /// Оставшееся время активного заказа в часах.
        /// </summary>
        public static int CalculateRemaining(IProductionStationRuntime runtime)
        {
            var queue = runtime.GetQueueDTO();

            if (queue.Count == 0)
                return 0;

            var active = queue[0];

            if (active.State != ProductionJobState.InProgress)
                return 0;

            int remaining =
                active.StartWorldHour + active.TotalDurationHours
                - runtime.TotalWorldHour;

            return Math.Max(0, remaining);
        }

        /// <summary>
        /// Суммарное оставшееся время всей очереди в часах.
        /// </summary>
        public static int CalculateTotalRemaining(IProductionStationRuntime runtime, int activeRemaining)
        {
            int total = 0;

            var slots = runtime.GetQueueDTO();

            var l = slots.Count;
            for (int i = 0; i < l; i++)
            {
                var job = slots[i];
                int remainingStack = job.CurrentStack - job.CompletedStack;

                if (remainingStack <= 0)
                    continue;

                // текущий заказ слота 0 считается по оставшемуся времени,
                // а не по полной длительности
                if (i == 0 && job.State == ProductionJobState.InProgress)
                {
                    remainingStack--;
                    total += activeRemaining;
                }

                total += remainingStack * job.TotalDurationHours;
            }

            return total;
        }

        /// <summary>
        /// Цена скипа активного слота (слот 0).
        /// Считаются ВСЕ оставшиеся часы слота:
        /// - текущий заказ: оставшиеся часы
        /// - каждый следующий заказ стека: полная длительность
        /// Формула цены применяется ОДИН раз к сумме часов, поэтому минимум в 1 монету
        /// и округление вниз не накапливаются по каждой единице стека.
        /// Возвращает -1, если скипать нечего.
        /// </summary>
        public static int CalculateSkipCost(IProductionStationRuntime runtime, IEconomyService economy)
        {
            int totalHours = CalculateSkipHours(runtime);
            if (totalHours <= 0)
                return -1;

            return economy.CalculateProductionSkipCost(totalHours, runtime.Level);
        }

        /// <summary>
        /// Сколько часов производства убирает скип: остаток текущего заказа
        /// плюс полная длительность каждого следующего заказа стека слота 0.
        /// </summary>
        public static int CalculateSkipHours(IProductionStationRuntime runtime)
        {
            var queue = runtime.GetQueueDTO();
            if (queue.Count == 0)
                return 0;

            var active = queue[0];
            if (active.State != ProductionJobState.InProgress)
                return 0;

            int remainingOrders = active.CurrentStack - active.CompletedStack;
            if (remainingOrders <= 0)
                return 0;

            int currentLeft = Math.Max(
                0,
                active.StartWorldHour + active.TotalDurationHours - runtime.TotalWorldHour);

            return currentLeft + (remainingOrders - 1) * active.TotalDurationHours;
        }

        /// <summary>
        /// Прогресс текущего заказа [0..1].
        /// </summary>
        public static float CalcProgress(int remainingHours, int totalHours)
        {
            if (totalHours <= 0 || remainingHours <= 0)
                return 0f;

            return 1f - (float)remainingHours / totalHours;
        }
    }
}