
using Galactic1.Code.UI.Tooltips;
using Galactic1.Game.Meta.Items;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.RaidReport
{
    
    /// <summary>
    /// UI-представление одного предмета лута в отчёте рейда.
    /// </summary>
    public class CampLostResourceItemView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text amountText;


        private ItemConfig item;
        private TooltipInputHandler inputHandler;
        
        
        /// <summary>
        /// Заполняет элемент данными лута.
        /// </summary>
        public void Bind(RaidLossResult result, UIStyleResolver styleResolver)
        {
            item = result.Item;
            amountText.text = result.Amount.ToString();
            
            // === rariry
            iconImage.material = styleResolver.ResolveRarityColor(item.Classification.rarity).Material;
            iconImage.sprite = item.Header.icon;
            
            
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
                item.Physical.maxDurability);

        private void HandleHoldEnd()
            => ServiceLocator.Current.Get<TooltipController>().Hide();
        
        
        
        
    }
}