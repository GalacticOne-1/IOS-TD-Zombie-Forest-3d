using System;
using Galactic1.Code.UI.Tooltips;
using Galactic1.Code.Utility;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.UI.Buildings.DTO;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Game.UI.Inbox
{
    /// <summary>
    /// UI карточка предмета во входящих.
    /// </summary>
    public class InboxCardView : MonoBehaviour
    {
        [SerializeField] private Image itemImg;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text countText;

        [SerializeField] private TMP_Text durabilityText;

        [SerializeField] private TMP_Text timeLeftText;
        [SerializeField] private GameObject takeButton;

        private string _slotId;
        private ItemConfig item;
        private int durability;
        private TooltipInputHandler inputHandler;

        public event Action<string> OnTakeClicked;

        
        
        
        public void Bind(InboxItemDTO dto, UIStyleResolver styleResolver)
        {
            item = dto.Item;
            durability = dto.DurabilityCurrent;
            _slotId = dto.SlotId;

            nameText.text = dto.Item.Header.titleLid;
            countText.text = dto.Count.ToString();
            
            // === rariry
            itemImg.material = styleResolver.ResolveRarityColor(item.Classification.rarity).Material;
            itemImg.sprite = item.Header.icon;
            
            // durability
            if (dto.DurabilityCurrent > 0)
            {
                durabilityText.gameObject.SetActive(true);
                durabilityText.text = $"{dto.DurabilityCurrent}%";
                durabilityText.color = styleResolver.ResolveValueColor(ValueRangeType.Durability, dto.Durability01);
            }
            else
            {
                durabilityText.gameObject.SetActive(false);
            }

            // time
            timeLeftText.text = TimeUtils.FormatTime(dto.RemainingHours);

            takeButton.RegisterButtonClick(() => OnTakeClicked?.Invoke(_slotId));
            
            // === подсказка
            inputHandler = itemImg.GetComponent<TooltipInputHandler>();
            inputHandler.RegisterOnRequest(HandleHoldStart);
            inputHandler.RegisterOnCancell(HandleHoldEnd);
        }

        private void HandleHoldStart(RectTransform anchor)
            => ServiceLocator.Current.Get<TooltipController>().Show<ItemTooltipView>(
                TooltipType.Loot,
                itemImg.gameObject.CMP_RectTr(),
                item,
                durability);

        private void HandleHoldEnd()
            => ServiceLocator.Current.Get<TooltipController>().Hide();
    }
}