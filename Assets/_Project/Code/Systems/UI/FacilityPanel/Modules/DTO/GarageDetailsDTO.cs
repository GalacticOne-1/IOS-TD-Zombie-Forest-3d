using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.Meta.Items;
using Galactic1.UI.CharacterPreview;
using UnityEngine;

namespace Galactic1.Game.UI.Buildings.DTO
{
    public class GarageDetailsDTO : IFacilityDetailsDTO
    {
        public FacilityType Type => FacilityType.Garage;

        
        
        /// <summary> Все доступные транспортные средства </summary>
        public IReadOnlyList<ItemConfig> AvailableVehicles { get; }

        /// <summary> ConfigId текущего транспорта игрока </summary>
        public RuntimeId EquippedVehicleId { get; }

        public UIPreviewConfig PreviewConfig { get; }
        public string PrefabPath { get; }
        
        
        

        public GarageDetailsDTO(
            IReadOnlyList<ItemConfig> availableVehicles,
            RuntimeId equippedVehicleId, 
            UIPreviewConfig previewConfig,
            string prefabPath)
        {
            AvailableVehicles = availableVehicles;
            EquippedVehicleId = equippedVehicleId;
            PreviewConfig = previewConfig;
            PrefabPath = prefabPath;
        }
    }
}