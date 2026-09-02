using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Inventory;
using Galactic1.Code.Systems.Raid.Mission;
using Galactic1.Code.Systems.Raid.Scenarios;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Systems.CampDefense.Penalty
{
    /// <summary>
    /// Новая точка входа пайплайна штрафа за поражение в Camp Defense.
    /// Отвечает только за выполнение пайплайна:
    /// MissionCompleted → (этот сервис) → Calculator → PenaltyResult → Applier → Inventory.
    ///
    /// Никаких вычислений внутри — только проверки и делегирование.
    /// </summary>
    public sealed class CampDefenseFailureService
    {
        private readonly ICampDefensePenaltyCalculator _calculator;
        private readonly CampDefensePenaltyApplier _applier;

        public CampDefenseFailureService(
            ICampDefensePenaltyCalculator calculator,
            CampDefensePenaltyApplier applier)
        {
            _calculator = calculator;
            _applier = applier;
        }

        /// <summary>
        /// Выполняет пайплайн и возвращает результат штрафа.
        /// Результат нужно сохранить у вызывающей стороны (например в RaidRuntime.PenaltyResult),
        /// чтобы позже отобразить его в отчёте — по аналогии с RaidLootBuffer.
        /// Возвращает CampDefensePenaltyResult.Empty, если штраф не применялся.
        /// </summary>
        public CampDefensePenaltyResult Evaluate(GameLoopContext context)
        {
            var raid = context.CurrentRaid;
            if (raid == null)
                return CampDefensePenaltyResult.Empty;

            // если миссия успешна (или ещё не завершилась) — штраф не применяется
            if (raid.MissionResult.Status != MissionStatus.Defeat)
                return CampDefensePenaltyResult.Empty;

            // если сценарий не CampDefense — штраф не применяется
            if (raid.Scenario is not CampDefenseScenario)
                return CampDefensePenaltyResult.Empty;

            var storage = context.CampRuntime.GetInventory(StorageType.Regular);
            if (storage is not IInventoryResourcesPort inventoryPort)
                return CampDefensePenaltyResult.Empty;

            var result = _calculator.Calculate(context);
            _applier.Apply(inventoryPort, result);

            return result;
        }
    }
}