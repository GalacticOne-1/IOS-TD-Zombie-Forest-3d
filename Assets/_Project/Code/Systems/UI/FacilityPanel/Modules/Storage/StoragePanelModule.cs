
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.UI.Buildings.DTO;
using TMPro;
using UnityEngine;

namespace Galactic1.Code.UI.Buildings
{
    /// <summary>
    /// Отображает ресурсы склада.
    /// </summary>
    public class StoragePanelModule : FacilityPanelModule
    {
        [SerializeField] private TMP_Text capacityText;
        [SerializeField] private TMP_Text specialStorageText;
        
        
        public override bool IsSupported(FacilityDTO dto)
            => dto.Details.Type == FacilityType.Storage;

        public override void Bind(
            FacilityDTO dto, 
            object sceneAdapter = null, 
            FacilityUpgradeSceneAdapter upgradeAdapter = null)
        {
            base.Bind(dto, sceneAdapter, upgradeAdapter);

            var details = dto.Details as StorageFacilityDetailsDTO;

            capacityText.text = $"+{details.Capacity}";

            // === описание авто сбора
            specialStorageText.transform.parent.gameObject.SetActive(details.StorageType != StorageType.Regular);
            specialStorageText.text = details.SpecialDescription;
        }
        
        public override void Rebind(FacilityDTO dto) {}

        public override void Unbind()
        {
            base.Unbind();
        }

    }
}