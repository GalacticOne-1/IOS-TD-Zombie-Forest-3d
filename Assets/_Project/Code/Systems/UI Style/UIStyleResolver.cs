using Galactic1.Code.WorldMap;
using Galactic1.Configs;
using Galactic1.Configs.WorldMap;
using Galactic1.Core.Enums;
using UnityEngine;

namespace Galactic1.UI.Core
{
    /// <summary>
    /// Централизованный резолвер UI-стилей.
    /// Переводит игровые значения в визуальные параметры UI.
    /// </summary>
    public class UIStyleResolver : IGameService
    {
        private readonly ValueRangeColorStyleConfig valueRangeConfig;
        private readonly ItemRarityStyleConfig rarityConfig;
        public readonly IntelStyleConfig locationIntelConfig;
        public readonly WorldMapStyleConfig worldMapStyleConfig;
        public readonly ManagerUIStyleConfig ManagerUIStyleConfig;
        public readonly RaidReportIconConfig RaidReportIconConfig;


        public UIStyleResolver()
        {
            var configProvider = ServiceLocator.Current.Get<ConfigProvider>();
            
            valueRangeConfig = configProvider.Get<UIStyleDatabase>()
                .Get<ValueRangeColorStyleConfig>("value_range_color_style_config");

            rarityConfig = configProvider.Get<UIStyleDatabase>()
                .Get<ItemRarityStyleConfig>("item_rarity_style_config");
            
            ManagerUIStyleConfig = configProvider.Get<UIStyleDatabase>()
                .Get<ManagerUIStyleConfig>("manager_uistyle_config");
            
            locationIntelConfig = configProvider.Get<UIStyleDatabase>()
                .Get<IntelStyleConfig>("intel_style_config");

            worldMapStyleConfig = configProvider.Get<UIStyleDatabase>()
                .Get<WorldMapStyleConfig>("world_map_style_config");
            
            RaidReportIconConfig = configProvider.Get<UIStyleDatabase>()
                .Get<RaidReportIconConfig>("raid_report_icon_config");
        }

        /// <summary>
        /// Возвращает цвет для диапазонных значений (0..1).
        /// </summary>
        public Color ResolveValueColor(ValueRangeType type, float value01)
        {
            if (valueRangeConfig == null)
                return Color.white;

            return valueRangeConfig.GetColor(type, value01);
        }
        
        /// <summary>
        /// Возвращает цвет редкости предмета.
        /// </summary>
        public RarityStyleEntry ResolveRarityColor(ItemRarity rarity)
        {
            if (rarityConfig == null)
                return new ();

            return rarityConfig.GetColor(rarity);
        }

        public Color ResolveAmountColor(int ownedAmount, int requiresAmount)
        {
            return ownedAmount >= requiresAmount ? Color.white : Color.red;
        }

        /// <summary>
        /// Возвращает стиль для указанного типа локации.
        /// </summary>
        public (Sprite, Color) GetLocationStyle(LocationType type, int difficulty)
            => worldMapStyleConfig.GetStyle(type, difficulty);
    }
}