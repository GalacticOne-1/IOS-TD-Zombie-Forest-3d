using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Game.Meta.Items;
using Galactic1.Meta.Configs.Recruitment;

namespace Galactic1.Code.Systems.Survival
{
    /// <summary>
    /// Ежедневное потребление Food/Water выжившими.
    /// Приоритет источников ресурсов: склад лагеря → инвентарь транспорта.
    /// Приоритет получателей: Strategic Squad → остальной лагерь.
    /// Не знает про UI/иконки — только состояние, модификаторы и события.
    /// </summary>
    public sealed class SurvivorDailyConsumptionService : IGameService
    {
        private GameLoopContext _context;
        private GameTimeService _timeService;
        private SurvivorConsumptionConfig _config;
        
        

        public SurvivorDailyConsumptionService(
            GameLoopContext context,
            GameTimeService timeService,
            SurvivorConsumptionConfig config)
        {
            _context = context;
            _timeService = timeService;
            _config = config;

            _timeService.DayPassed += OnDayPassed;
        }

        private void OnDayPassed(DayPassedEvent e) => ProcessDailyConsumption();

        private void ProcessDailyConsumption()
        {
            // Склад лагеря физически доступен только когда игрок находится в лагере.
            // На карте мира / в рейде — только то, что везёт с собой транспорт.

            var campPort = _context.IsCampState
                ? _context.CampRuntime.GetInventory(StorageType.Regular) as IInventoryResourcesPort
                : null;
            var transportPort = _context.PlayerTransport.GetInventory as IInventoryResourcesPort;

            // #1 приоритет получателей: сначала Strategic Squad, затем остальной ростер
            var orderedUnits = _context.StrategicSquadUnits
                .Concat(_context.CampUnits)
                .ToList();

            int availableFood = GetAvailable(campPort, transportPort, _config.FoodItemId);
            int availableWater = GetAvailable(campPort, transportPort, _config.WaterItemId);

            // #2 распределяем по одному на юнита в порядке приоритета
            var fed = new HashSet<string>();
            var watered = new HashSet<string>();

            foreach (var unit in orderedUnits)
            {
                if (availableFood <= 0) continue;
                availableFood--;
                fed.Add(unit.Id);
            }

            foreach (var unit in orderedUnits)
            {
                if (availableWater <= 0) continue;
                availableWater--;
                watered.Add(unit.Id);
            }

            // #3 списываем реально потреблённое: сначала лагерь, остаток — из транспорта
            if (fed.Count > 0)
                Spend(campPort, transportPort, _config.FoodItemId, fed.Count);
            if (watered.Count > 0)
                Spend(campPort, transportPort, _config.WaterItemId, watered.Count);

            // #4 пересчитываем состояние каждого юнита с нуля
            foreach (var unit in orderedUnits)
                ApplyState(unit, isHungry: !fed.Contains(unit.Id), isThirsty: !watered.Contains(unit.Id));
        }

        // ── Ресурсы: склад лагеря → инвентарь транспорта ─────────────────

        private int GetAvailable(IInventoryResourcesPort campPort, IInventoryResourcesPort transportPort, RuntimeId itemId)
        {
            int total = 0;
            if (campPort != null) total += campPort.GetTotalAmount(itemId);
            if (transportPort != null) total += transportPort.GetTotalAmount(itemId);
            return total;
        }

        private void Spend(IInventoryResourcesPort campPort, IInventoryResourcesPort transportPort, RuntimeId itemId, int amount)
        {
            int remaining = amount;

            if (campPort != null && remaining > 0)
            {
                int fromCamp = System.Math.Min(remaining, campPort.GetTotalAmount(itemId));
                if (fromCamp > 0)
                {
                    campPort.TrySpend(itemId, fromCamp);
                    remaining -= fromCamp;
                }
            }

            if (transportPort != null && remaining > 0)
            {
                int fromTransport = System.Math.Min(remaining, transportPort.GetTotalAmount(itemId));
                if (fromTransport > 0)
                    transportPort.TrySpend(itemId, fromTransport);
            }
        }

        private void ApplyState(UnitRuntime unit, bool isHungry, bool isThirsty)
        {
            unit.Status.SetHungry(isHungry);
            unit.Status.SetThirsty(isThirsty);

            if (isHungry) unit.Stats.AddBuff(_config.HungerBuff);
            else unit.Stats.RemoveBuff(_config.HungerBuff.Id);

            if (isThirsty) unit.Stats.AddBuff(_config.ThirstBuff);
            else unit.Stats.RemoveBuff(_config.ThirstBuff.Id);

            EventBus<SurvivorStatusChangedEvent>.Raise(
                new SurvivorStatusChangedEvent(unit.Id, isHungry, isThirsty));
        }
    }
}