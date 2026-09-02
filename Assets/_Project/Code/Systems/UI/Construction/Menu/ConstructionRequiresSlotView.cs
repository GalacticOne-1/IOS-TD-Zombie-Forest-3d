
using Galactic1.Code.Systems.Economy;
using Galactic1.Code.UI.Tooltips;
using Galactic1.Game.Meta.Items;
using Galactic1.UI.Core;
using Galactic1.UI.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Construction
{
    /// <summary>
    /// UI слот ингредиента рецепта строительства.
    /// Отображает:
    /// - иконку ресурса
    /// - требуемое количество
    /// - зеленый overlay если ресурсов достаточно
    /// </summary>
    public class ConstructionRequiresSlotView : MonoBehaviour
    {
        [SerializeField] private Image itemImg;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private GameObject overlay, locked;
        
        
        private ItemConfig item;
        private TooltipInputHandler inputHandler;

        
        /// <summary>
        /// Заполнить слот ингредиента
        /// </summary>
        public void Bind(
            UIStyleResolver styleResolver, 
            int ownedAmount,
            RequirementData requirement,
            bool hasEnough)
        {
            gameObject.SetActive(true);

            item = requirement.Item;

            itemImg.material = styleResolver.ResolveRarityColor(requirement.Item.Classification.rarity).Material;
            itemImg.sprite = requirement.Item.Header.icon;
            
            countText.text = TextBuilder.Start()
                .Color(styleResolver.ResolveAmountColor(ownedAmount, requirement.Amount))
                .Size(115)
                .Text(ownedAmount)
                .End()      // size
                .End()      // color
                .Text("/")
                .Size(90)
                .Text(requirement.Amount)
                .End()
                .ToString();

            overlay.SetActive(hasEnough);
            
            
            // === подсказка
            inputHandler = GetComponent<TooltipInputHandler>();
            inputHandler.RegisterOnRequest(HandleHoldStart);
            inputHandler.RegisterOnCancell(HandleHoldEnd);
        }

        /// <summary>
        /// Скрыть слот если ингредиента нет
        /// </summary>
        public void Hide()
        {
            locked.SetActive(true);
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