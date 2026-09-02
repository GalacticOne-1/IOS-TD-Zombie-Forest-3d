using System;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.UI.Buildings.DTO;
using TMPro;
using UnityEngine;

namespace Galactic1.Code.UI.Buildings
{
    /// <summary>
    /// Отображает ресурсы склада.
    /// </summary>
    public class CombatPanelModule : FacilityPanelModule
    {
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private TMP_Text damageText;


        public override bool IsSupported(FacilityDTO dto)
            => dto.Details.Type == FacilityType.Defense;

        public override void Bind(
            FacilityDTO dto, 
            object sceneAdapter = null, 
            FacilityUpgradeSceneAdapter upgradeAdapter = null)
        {
            base.Bind(dto, sceneAdapter, upgradeAdapter);

            var details = dto.Details as CombatDetailsDTO;

            hpText.text = $"{details.HP}";

            // === описание для атакующих объектов
            damageText.gameObject.SetActive(details.Damage > 0);
            damageText.text = $"{details.Damage}";
        }
        
        public override void Rebind(FacilityDTO dto) {}

        public override void Unbind()
        {
            base.Unbind();
        }

    }
}