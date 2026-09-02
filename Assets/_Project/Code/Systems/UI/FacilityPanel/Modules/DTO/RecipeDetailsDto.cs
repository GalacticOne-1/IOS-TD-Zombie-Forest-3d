using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Core.Enums;
using Galactic1.Game.UI.Stats.DTO;
using UnityEngine;

namespace Galactic1.Game.UI.Production.DTO
{
    /// <summary>
    /// DTO для отображения подробной информации о рецепте в UI.
    /// Не содержит ссылок на Runtime или ScriptableObject.
    /// </summary>
    public readonly struct RecipeDetailsDto
    {
        /// <summary>Уникальный идентификатор рецепта</summary>
        public readonly RuntimeId RecipeId;

        /// <summary>Название результата</summary>
        public readonly string Title;

        /// <summary>Иконка результата</summary>
        public readonly Sprite Icon;

        public readonly ItemCategory ItemCategory;
        public readonly ItemRarity Rarity;

        /// <summary>Количество на выходе</summary>
        public readonly int OutputCount;
            
        /// <summary>Мульти результат на выходе (Recycler) </summary> 
        public IReadOnlyList<RecyclerOutputDTO> OutputResources { get; }

        /// <summary>Время крафта (в секундах или игровых часах — в зависимости от доменной модели)</summary>
        public readonly int CraftTime;

        /// <summary>Можно ли сейчас создать заказ</summary>
        public readonly bool CanAddOrder;
        public readonly bool OrderButtonActive;
        public readonly StationUpgradeCtx StationRequiresCtx;
        
        
        /// <summary>Список требуемых ресурсов</summary>
        public readonly IReadOnlyList<RecipeRequirementDto> Requirements;
        
        
        
        public readonly IReadOnlyList<StatDtoBase> DescriptorDto;
        public readonly IReadOnlyList<StatGroupViewDto> StatGroups;
        

        public RecipeDetailsDto(
            RuntimeId recipeId,
            string title,
            Sprite icon,
            ItemCategory itemCategory,
            ItemRarity rarity,
            int outputCount,
            IReadOnlyList<RecyclerOutputDTO> outputResources,
            int craftTime,
            IReadOnlyList<RecipeRequirementDto> requirements, 
            IReadOnlyList<StatDtoBase> descriptorDto,
            IReadOnlyList<StatGroupViewDto> statGroups,
            bool canAddOrder,
            bool orderButtonActive, 
            StationUpgradeCtx stationRequiresCtx)
        {
            RecipeId = recipeId;
            Title = title;
            Icon = icon;
            ItemCategory = itemCategory;
            Rarity = rarity;
            OutputCount = outputCount;
            OutputResources = outputResources;
            CraftTime = craftTime;
            Requirements = requirements;
            DescriptorDto = descriptorDto;
            StatGroups = statGroups;
            CanAddOrder = canAddOrder;
            OrderButtonActive = orderButtonActive;
            StationRequiresCtx = stationRequiresCtx;
        }

        
        public struct StationUpgradeCtx
        {
            public bool requiresBlueprint;
            
            public bool requiresStationUpgrade;
            public string stationAlertMessage;
        }

        
    }
}