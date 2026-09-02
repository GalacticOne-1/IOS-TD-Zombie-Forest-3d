
using System;
using System.Collections.Generic;
using Galactic1.Runtime.Production;

namespace Galactic1.Game.Runtime.Production
{
    /// <summary>
    /// Общие расчёты времени производства.
    /// Используется в FacilityDetailsFactory и StationsPanelPresenter.
    /// Единственный источник истины для логики времени.
    /// </summary>
    public static class ProductionTimeCalculator
    {
        /// <summary>
        /// Оставшееся время активного заказа в часах.
        /// Идентично FacilityDetailsFactory.CalculateRemaining.
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
        /// Идентично FacilityDetailsFactory.CalculateTotalRemaining.
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

                if (i == 0)
                {
                    remainingStack--;
                    total = activeRemaining;
                    
                    if (remainingStack > 0)
                        total += remainingStack * job.TotalDurationHours;
                }
                else
                {
                    total += remainingStack * job.TotalDurationHours;
                }
            }

            return total;
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