
using Galactic1.Code.UI.Tooltips;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.UI.Buildings.DTO;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Buildings
{
    public class TavernOfferEquipmentSlotView : MonoBehaviour
    {
        public GameObject cRoot;
        public Image itemImg;
        public TMP_Text durabilityText;
        public Image durabilityBar;

        private ItemConfig item;
        private int durability;
        private TooltipInputHandler inputHandler;
        
        
        public void Bind(GearSlotDTO dto, UIStyleResolver styleResolver)
        {
            item = dto.Item;
            durability = dto.Durability;
            
            // item image
            itemImg.material = styleResolver.ResolveRarityColor(dto.Rarity).Material;
            itemImg.sprite = dto.Icon;
            
            
            durabilityText.text = $"{dto.DurabilityPrcnt}%";
            durabilityText.color = styleResolver.ResolveValueColor(ValueRangeType.Durability, dto.Durability01);
            durabilityBar.fillAmount = dto.Durability01;
            
            // === подсказка
            inputHandler = GetComponent<TooltipInputHandler>();
            inputHandler.RegisterOnRequest(HandleHoldStart);
            inputHandler.RegisterOnCancell(HandleHoldEnd);
        }
        
        
        private void HandleHoldStart(RectTransform anchor)
            => ServiceLocator.Current.Get<TooltipController>().Show<ItemTooltipView>(
                TooltipType.Loot,
                gameObject.CMP_RectTr(),
                item,
                durability);

        private void HandleHoldEnd()
            => ServiceLocator.Current.Get<TooltipController>().Hide();
    }
}