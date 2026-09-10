using Galactic1.Code.UI.Tooltips;
using Galactic1.Game.Meta.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.Gameplay.Tasks.Presentation
{
    /// <summary>Read-only виджет одной награды. Не переиспользует InventorySlotView —
    /// та завязана на drag/click/inventory-доступ; здесь только геометрия/текст по тем
    /// же визуальным конвенциям (icon/amount/durability%/ammo).</summary>
    public sealed class ScenarioTaskRewardItem : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private TMP_Text durabilityText;
        
        
        private ItemConfig item;
        private int durability;
        private TooltipInputHandler inputHandler;

        public void Set(ScenarioTaskReward reward)
        {
            item = reward.Item;
            if (item == null) return;

            icon.sprite = item.Header.icon;
            amountText.text = reward.Amount > 1 ? $"{reward.Amount}" : "";

            var maxDurability = item.Physical.maxDurability;
            bool showDurability = item.Physical.usesDurability && maxDurability > 0;
            durabilityText.gameObject.SetActive(showDurability);
            if (showDurability)
            {
                durability = reward.Durability == -1 ? maxDurability : reward.Durability;
                durabilityText.text = Mathf.CeilToInt((float)durability / maxDurability * 100) + "%";
            }
            
            
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