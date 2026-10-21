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

            var scenario = new CampDefenseScenario(_container);
            raid.Scenario = scenario;
            _context.CurrentRaid = raid;

            var raidResult = scenario.BuildRaidResult(raid, raid.MissionResult);

            _context.Proxy.LastRaidResult = raidResult;
            _context.Proxy.HasPendingRaidReport.Value = true;

            // Замыкаем цикл угрозы: в обычном потоке это делает Scenario.Cleanup()
            // из RaidInProgressState.Exit(), которого здесь нет.
            _container.Resolve<CampDefensePreparationService>().CompleteDefense();

            scenario.ExitFromLocation();
        }
    }
}