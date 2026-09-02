using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.UI.Garage;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.UI.Stats.DTO;
using Galactic1.UI.CharacterPreview;
using UnityEngine;

namespace Galactic1.Game.UI.Garage.DTO
{
    /// <summary>
    /// DTO для одного модуля транспорта
    /// </summary>
    public readonly struct GarageModuleDetailsDTO
    {
        public readonly RuntimeId ID;
        public readonly string Title;
        public readonly Sprite Icon;
        public readonly VehicleSlotType SlotType;
        public readonly ItemRarity Rarity;
        
        
        public readonly string PrefabPath;
        public readonly UIPreviewConfig PreviewConfig;
        
        /// <summary>Список требований для покупки</summary>
        public readonly IReadOnlyList<ModuleRequirementDto> Requirements;
        public readonly bool IsPurchased;
        public readonly bool IsEquipped;
        
        
        public readonly IReadOnlyList<StatDtoBase> DescriptorDto;
        public readonly IReadOnlyList<StatGroupViewDto> StatGroups;
        
        

        public GarageModuleDetailsDTO(
            RuntimeId id,
            string title,
            Sprite icon,
            ItemRarity rarity,
            VehicleSlotType slotType,
            IReadOnlyList<StatDtoBase> descriptorDto,
            IReadOnlyList<StatGroupViewDto> statGroups,
            string prefabPath,
            UIPreviewConfig previewConfig,
            IReadOnlyList<ModuleRequirementDto> requirements,
            bool isPurchased,
            bool isEquipped)
        {
            ID = id;
            Title = title;
            Icon = icon;
            Rarity = rarity;
            SlotType = slotType;
            DescriptorDto = descriptorDto;
            StatGroups = statGroups;
            
            PrefabPath = prefabPath;
            PreviewConfig = previewConfig;
            Requirements = requirements;
            IsPurchased = isPurchased;
            IsEquipped = isEquipped;
        }
    }

    public readonly struct ModuleRequirementDto
    {
        public readonly RuntimeId Id;
        public readonly ItemConfig Item;
        public readonly Sprite Icon;
        public readonly int RequiredAmount;
        public readonly int OwnedAmount;
        public readonly bool IsEnough;

        public ModuleRequirementDto(
            RuntimeId id, 
            ItemConfig item,
            Sprite icon, 
            int requiredAmount,
            int ownedAmount)
        {
            Id = id;
            Item = item;
            Icon = icon;
            RequiredAmount = requiredAmount;
            OwnedAmount = ownedAmount;
            IsEnough = ownedAmount >= requiredAmount;
        }
    }
}