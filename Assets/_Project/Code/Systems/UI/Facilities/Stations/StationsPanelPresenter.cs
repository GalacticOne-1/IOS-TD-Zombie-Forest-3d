
using System;
using System.Collections.Generic;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Configs;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.Runtime.Production;
using Galactic1.Items;
using Galactic1.UI.Core;

namespace Galactic1.Code.UI.Stations
{
    public sealed class StationsPanelPresenter : IDisposable
    {
        private readonly GameLoopContext gameLoopContext;
        private readonly IReadOnlyList<ItemConfig> catalog;
        private readonly StorageRegistry storageRegistry;
        private readonly StationsPanelView view;

        private readonly List<BaseProductionStationRuntime> subscribed = new();
        private readonly UIStyleResolver styleResolver;


        public StationsPanelPresenter(
            GameLoopContext context,
            IReadOnlyList<ItemConfig> stationCatalog,
            StorageRegistry storageReg,
            StationsPanelView panelView)
        {
            gameLoopContext = context;
            catalog = stationCatalog;
            storageRegistry = storageReg;
            view = panelView;
            styleResolver = ServiceLocator.Current.Get<UIStyleResolver>();
        }

        public void Open()
        {
            SubscribeAll();
            storageRegistry.OnStorageChanged += Refresh;
            Refresh();
        }

        public void Dispose()
        {
            UnsubscribeAll();
            storageRegistry.OnStorageChanged -= Refresh;
        }

        // =========================================================
        // PRIVATE
        // =========================================================

        private void SubscribeAll()
        {
            UnsubscribeAll();

            foreach (var itemConfig in catalog)
            {
                var runtime = FindRuntime(itemConfig.Id);
                if (runtime == null) continue;

                runtime.OnStateChanged += Refresh;
                subscribed.Add(runtime);
            }
        }

        private void UnsubscribeAll()
        {
            foreach (var r in subscribed)
                r.OnStateChanged -= Refresh;
            subscribed.Clear();
        }

        private void Refresh()
        {
            var result = new List<StationCardDTO>();

            foreach (var itemConfig in catalog)
            {
                if (!itemConfig.HasModule<CraftingStationModule>())
                    continue;

                var runtime = FindRuntime(itemConfig.Id);
                bool isBuilt = runtime != null;
                int level = runtime?.Level ?? 0;
                var def = new StationDefinition(itemConfig, level);
                var slots = BuildSlots(runtime);
                var alert = BuildAlert(itemConfig);

                int remainingActive = 0;
                var totalRemaining = 0;
                if (runtime != null)
                {
                    remainingActive = ProductionTimeCalculator.CalculateRemaining(runtime);
                    totalRemaining = ProductionTimeCalculator.CalculateTotalRemaining(runtime, remainingActive);
                }


                result.Add(new StationCardDTO(
                    def.Id,
                    def.DisplayName,
                    itemConfig.Header.icon,
                    level,
                    isBuilt,
                    totalRemaining,
                    slots,
                    alert));
            }

            view.Render(styleResolver, result);
        }

        private BaseProductionStationRuntime FindRuntime(RuntimeId configId)
            => gameLoopContext.GetFacilityByConfigId(configId) as BaseProductionStationRuntime;

        private SlotStatusDTO[] BuildSlots(BaseProductionStationRuntime runtime)
        {
            SlotStatusDTO[] result;
            if (runtime == null)
            {
                result = new SlotStatusDTO[5];
                for (int i = 0; i < 5; i++)
                    result[i] = SlotStatusDTO.Empty(i);
                return result;
            }


            var queue = runtime.GetQueueDTO();
            int currentHour = runtime.TotalWorldHour;

            var l = queue.Count;
            result = new SlotStatusDTO[l];

            for (int i = 0; i < l; i++)
            {
                if (i >= queue.Count)
                {
                    result[i] = SlotStatusDTO.Empty(i);
                    continue;
                }

                var job = queue[i];
                bool isActive = job.State == ProductionJobState.InProgress;
                bool isDone = job.State == ProductionJobState.Completed;

                int remaining = isActive
                    ? ProductionTimeCalculator.CalculateRemaining(runtime)
                    : 0;

                float progress = ProductionTimeCalculator.CalcProgress(
                    remaining,
                    job.TotalDurationHours);

                // Иконка + rarity через ItemDatabase — идентично FacilityDetailsFactory
                var outputItem = GameContent.Items.Get(job.RecipeId);
                var icon = outputItem?.Header.icon;
                var rarity = outputItem?.Classification.rarity ?? ItemRarity.Common;

                result[i] = new SlotStatusDTO(
                    i,
                    isActive,
                    isDone,
                    progress,
                    remaining,
                    job.TotalDurationHours,
                    icon,
                    rarity,
                    job.CompletedStack,
                    job.CurrentStack);
            }

            return result;
        }

        private StorageAlertDTO BuildAlert(ItemConfig itemConfig)
        {
            if (!itemConfig.HasModule<CraftingStationModule>())
                return new StorageAlertDTO(false, false, null);

            var station = itemConfig.CraftStation;

            // получаем рецепты из станции
            var availableRecipes = station.AvailableRecipes;

            foreach (var recipe in availableRecipes)
            {
                foreach (var tag in recipe.Tags)
                {
                    if (!storageRegistry.HasStorageForTag(tag))
                    {
                        // Ищем ItemConfig хранилища которое должно поддерживать этот тег
                        var storageConfig = GameContent.Facilities.FindStorageConfigForTag(tag);
                        var storageName = storageConfig?.Header.titleLid ?? tag.ToString();

                        return new StorageAlertDTO(
                            true,
                            false,
                            storageName);
                    }
                }
            }

            return new StorageAlertDTO(false, true, null);
        }
    }
}