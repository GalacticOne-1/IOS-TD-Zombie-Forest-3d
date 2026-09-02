using System.Collections.Generic;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.Systems.Raid.Mission;
using Galactic1.Code.Systems.Raid.Scenarios;
using Galactic1.Code.Systems.Raid.Survivors;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Code.UI.Inventory;
using Galactic1.Configs;
using Galactic1.Meta.Configs.Recruitment;

namespace Galactic1.Code.Systems.CampDefense.Preparation
{
    /// <summary>
    /// Немедленное поражение в Camp Defense, когда у игрока нет ни одного
    /// выжившего (NoUnitsInCamp). Бой не проводится, тактическая сцена
    /// не загружается — переиспользуется существующий пайплайн результата
    /// и отчёта (CampDefenseScenario.BuildRaidResult + report flow).
    /// </summary>
    public sealed class CampDefenseImmediateDefeatService
    {
        private readonly DIContainer _container;
        private readonly GameLoopContext _context;

        public CampDefenseImmediateDefeatService(DIContainer container, GameLoopContext context)
        {
            _container = container;
            _context = context;
        }

        public void TriggerImmediateDefeat()
        {
            var configProvider = ServiceLocator.Current.Get<ConfigProvider>();
            var accessService = ServiceLocator.Current.Get<InventoryManagementWindow>()
                .controller.AccessService;

            // #1 Минимальный raid-shell — без спавна, без тактической сцены.
            //    Squad пуст по определению (это и есть причина NoUnitsInCamp).
            var raid = new RaidRuntime
            {
                Status = RaidStatus.Failed,
                MissionResult = MissionResult.Defeat,
                Squad = new SquadRuntime(
                    new List<UnitRuntime>(),
                    accessService,
                    configProvider.Get<PlayerArchetypeConfig>(),
                    configProvider.Get<SurvivorConsumptionConfig>()),
            };

            // #2 Временный сценарий — используется только чтобы переиспользовать
            //    существующий BuildRaidResult (штраф, маппинг ResourcesLost и т.п.),
            //    а не дублировать эту логику здесь.
            var scenario = new CampDefenseScenario(_container);
            raid.Scenario = scenario;

            _context.CurrentRaid = raid;

            var raidResult = scenario.BuildRaidResult(raid, raid.MissionResult);

            // #3 Записываем результат туда же, куда его пишет обычный PostRaidReportState
            _context.Proxy.LastRaidResult = raidResult;
            _context.Proxy.HasPendingRaidReport.Value = true;

            // #4 Переиспользуем существующий выход/reload сцены —
            //    именно он приводит к показу CampDefenseReport через CampReportState.
            scenario.ExitFromLocation();
        }
    }
}