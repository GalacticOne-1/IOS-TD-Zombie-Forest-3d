
using Galactic1.Code.WorldMap;

namespace Galactic1.Code.Systems.Raid.Scenarios
{
    /// <summary>
    /// Единственное место, решающее, какой сценарий строить.
    /// Использует уже существующий LocationType — отдельный enum
    /// "тип сценария" не заводится, чтобы не плодить параллельную классификацию.
    /// </summary>
    public static class RaidScenarioFactory
    {
        public static IRaidScenario Create(LocationType locationType, DIContainer container)
        {
            return locationType switch
            {
                LocationType.Home => new CampDefenseScenario(container),
                _ => new ExplorationRaidScenario(container) // безопасный default для обычных локаций
            };
        }
    }
}