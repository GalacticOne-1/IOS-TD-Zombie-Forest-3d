
using System.Collections;
using Galactic1.Code.UI.Common.Effects;
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
    public class RaidLootItemView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private TMP_Text amountTotalText;
        [SerializeField] private TMP_Text durabilityText;
        [SerializeField] private GameObject cSpecial;
        [SerializeField] private GameObject cUpgrades;


        private ItemConfig item;
        private int durability;
        private TooltipInputHandler inputHandler;
        private IUIFlashEffect _flash;
        private RaidLootResult _result;
        
        
        /// <summary>
        /// Заполняет элемент данными лута.
        /// </summary>
        public void Bind(RaidLootResult result, UIStyleResolver styleResolver, bool adBonusAvail)
        {
            _flash = GetComponent<IUIFlashEffect>();
            _result = result;
            
            item = result.Item;
            durability = result.Durability;
            amountText.text = result.Amount.ToString();
            
            // === rariry
            iconImage.material = styleResolver.ResolveRarityColor(item.Classification.rarity).Material;
            iconImage.sprite = item.Header.icon;
            
            // durability
            if (result.Durability > 0)
            {
                durabilityText.gameObject.SetActive(true);
                durabilityText.text = $"{(int)(result.Durability.PercentFrom(item.Physical.maxDurability) * 100)}%";
                
                float durability01 = result.Durability / item.Physical.maxDurability;
                durabilityText.color = styleResolver.ResolveValueColor(ValueRangeType.Durability, durability01);
            }
            else
            {
                durabilityText.gameObject.SetActive(false);
            }

            // оставляем улучшалки только для оружия и брони
            if (!item.HasModule<WeaponModule>() && !item.HasModule<EquipmentModule>())
            {
                cSpecial.SetActive(false);
                cUpgrades.SetActive(false);
            }
            
            
            // === ad bonus
            amountTotalText.gameObject.SetActive(adBonusAvail && result.BonusAmount > 0);
            amountTotalText.text = result.TotalAmount.ToString();
            
            
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
        
        
        
        /// <summary>
        /// Вызов после показа рекламы
        /// </summary>
        public void ApplyAdBonus()
        {
            StartCoroutine(AnimateBonus());
        }
        
        private IEnumerator AnimateBonus()
        {
            _flash.Play();

            yield return new WaitForSeconds(0.15f);

            amountText.text = _result.TotalAmount.ToString();
            amountText.color = amountTotalText.color;
            amountTotalText.gameObject.SetActive(false);
        }
        
    }
}