
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.UI.Stations
{
    /// <summary>
    /// Объединяет ItemConfig + CraftingStationModule в одну запись для панели.
    /// Создаётся один раз при открытии панели.
    /// </summary>
    public sealed class StationDefinition
    {
        public RuntimeId Id { get; }
        public string DisplayName { get; }
        public int Order { get; }
        public ItemConfig ItemConfig { get; }
        public CraftingStationModule StationModule { get; }

        public StationDefinition(ItemConfig config, int currentLevel)
        {
            ItemConfig = config;
            StationModule = config.CraftStation;
            Id = config.Id;
            DisplayName = config.Header.titleLid;
            Order = config.Header.order;

        }
    }
}