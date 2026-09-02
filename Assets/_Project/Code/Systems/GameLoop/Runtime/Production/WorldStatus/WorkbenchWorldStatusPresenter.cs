// Путь: Assets/Scripts/Runtime/UI/WorldStatus/WorkbenchWorldStatusPresenter.cs
// Namespace: Galactic1.Runtime.UI.WorldStatus

using System;
using Galactic1.Code.GameDatabase;
using Galactic1.Configs;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.Runtime.Production;
using Galactic1.Items;
using Galactic1.UI.WorldStatus;
using UnityEngine;

namespace Galactic1.Runtime.UI.WorldStatus
{
    public sealed class WorkbenchWorldStatusPresenter : IDisposable
    {
        private readonly BaseProductionStationRuntime runtime;
        private readonly WorkbenchWorldStatusView view;

        public WorkbenchWorldStatusPresenter(
            BaseProductionStationRuntime productionRuntime,
            WorkbenchWorldStatusView statusView)
        {
            runtime = productionRuntime;
            view = statusView;

            runtime.OnStateChanged += OnStateChanged;
        }

        public void Dispose()
        {
            runtime.OnStateChanged -= OnStateChanged;
        }

        public void ForceRefresh() => view.Render(BuildDTO());

        // =========================================================
        // PRIVATE
        // =========================================================

        private void OnStateChanged() => view.Render(BuildDTO());

        private WorkbenchStatusDTO BuildDTO()
        {
            var queue = runtime.GetQueueDTO();

            if (queue == null || queue.Count == 0)
                return WorkbenchStatusDTO.Empty;

            var active = queue[0];
            var isWorking = active.State == ProductionJobState.InProgress;
            
            var item = GameContent.Items.Get(active.RecipeId);
            
            var icon = item.Header.icon;

            var progress = 0f;
            var remainHours = -1;

            if (isWorking)
            {
                var worldNow = runtime.TotalWorldHour;
                var hoursLeft = Mathf.Max(0, active.FinishWorldHour - worldNow);
                var totalHours = Mathf.Max(1, active.TotalDurationHours);

                progress = Mathf.Clamp01(1f - (float)hoursLeft / totalHours);
                remainHours = hoursLeft;
            }

            return new WorkbenchStatusDTO(
                icon,
                active.CompletedStack,
                active.CurrentStack,
                remainHours,
                progress,
                isWorking,
                true);
        }
    }
}