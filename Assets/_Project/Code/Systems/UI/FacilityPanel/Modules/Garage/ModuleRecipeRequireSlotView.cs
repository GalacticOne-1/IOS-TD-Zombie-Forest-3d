
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.UI.Tooltips;
using Galactic1.Code.Utils;
using Galactic1.Game.Meta.Items;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Galactic1.Game.UI.Production
{
    /// <summary>
    /// Отображает один требуемый ресурс в рецепте.
    /// Показывает:
    /// - Иконку ресурса
    /// - Кол-во owned / required
    /// - Задник, если ресурса хватает
    /// </summary>
    public sealed class ModuleRecipeRequireSlotView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private GameObject enoughBackground;

        private ItemConfig _item;

        /// <summary>Идентификатор ресурса</summary>
        public RuntimeId ItemId { get; private set; }

        /// <summary>Количество, которое есть в инвентаре</summary>
        private int _owned;

        /// <summary>Количество, которое требуется для рецепта</summary>
        private int _required;

        private bool _isEnough;

        /// <summary>Инициализация слота</summary>
        public void Setup(
            RuntimeId itemId,
            ItemConfig item,
            Sprite sprite,
            int ownedAmount, 
            int requiredAmount,
            bool isEnought)
        {
            ItemId = itemId;
            _item = item;
            _owned = ownedAmount;
            _required = requiredAmount;
            _isEnough = isEnought;

            icon.sprite = sprite;
            UpdateUI();
            
            // === подсказка
            var inputHandler = GetComponent<TooltipInputHandler>();
            inputHandler.RegisterOnRequest(HandleHoldStart);
            inputHandler.RegisterOnCancell(HandleHoldEnd);
        }

        /// <summary>
        /// Обновление количества (например, после изменения инвентаря)
        /// </summary>
        public void UpdateOwnedAmount(int ownedAmount)
        {
            _owned = ownedAmount;
            UpdateUI();
        }

        private void UpdateUI()
        {
            // Формат: owned / required
            amountText.text = TextUtils.FormatOwnedRequired(_owned, _required);

            // Включаем задник, если хватает
            enoughBackground.SetActive(_isEnough);
        }
        
        private void HandleHoldStart(RectTransform anchor)
            => ServiceLocator.Current.Get<TooltipController>().Show<ItemTooltipView>(
                TooltipType.Loot,
                gameObject.CMP_RectTr(),
                _item,
                _item.Physical.maxDurability);

        private void HandleHoldEnd()
            => ServiceLocator.Current.Get<TooltipController>().Hide();
    }
}