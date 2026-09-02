using System.Linq;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Game.Meta.Items;
using Galactic1.UI.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Galactic1.Code.UI.Inventory
{
    public class InventorySlotView : MonoBehaviour,
        IPointerClickHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IDragHandler
    {
        [SerializeField] private bool isNotActive;
        [SerializeField] private Image iconType, icon;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private TextMeshProUGUI durabilityText;
        [SerializeField] private TextMeshProUGUI ammoLoadedText;
        [SerializeField] private Image slotBar;
        [SerializeField] private Image highlight;
        [SerializeField] private CanvasGroup canvasGroup;

        public int SlotIndex { get; private set; }
        public InventoryView ParentUI { get; private set; }
        

        private float lastClickTime;
        private const float doubleClickThreshold = 0.3f; // секунды
        

        
        
        
        
        public void Init(InventoryView parent, int index)
        {
            if (!isNotActive) gameObject.SetActive(true);
            
            ParentUI = parent;
            SlotIndex = index;
            SetHighlight(false);
            if (GetComponent<CanvasGroup>() is CanvasGroup cg && cg)
            {
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }

        public void Hide()
        {
            canvasGroup.alpha = 0;
            SetHighlight(false);
            if (GetComponent<CanvasGroup>() is CanvasGroup cg && cg)
            {
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
        }

        public void Empty()
        {
            iconType?.gameObject.SetActive(true);
            icon.enabled = false;
            amountText.text = "";
            slotBar?.transform.parent.gameObject.SetActive(false);
            ammoLoadedText?.transform.parent.gameObject.SetActive(false);
        }

        public void Set(InventorySlotRuntime slot, UIStyleResolver styleResolver)
        {
            bool isEmpty = slot.IsEmpty;

            if (isEmpty)
            {
                iconType?.gameObject.SetActive(true);
                icon.enabled = false;
                amountText.text = "";
            }
            else
            {
                var item = slot.Item;
                iconType?.gameObject.SetActive(false);
                icon.enabled = true;
                icon.material = styleResolver.ResolveRarityColor(item.Classification.rarity).Material;
                icon.sprite = item.Header.icon;
                amountText.text = slot.Amount > 1 ? slot.Amount.ToString() : "";

                if (durabilityText != null)
                    durabilityText.text = "";
                
                if (slotBar)
                {
                    var maxDurability = item.Physical.maxDurability;
                    slotBar.transform.parent.gameObject.SetActive(maxDurability > 0);
                    slotBar.fillAmount = (float)slot.Durability / maxDurability;
                    
                    if(durabilityText != null)
                    {
                        durabilityText.text = Mathf.CeilToInt(((float)slot.Durability / maxDurability) * 100) + "%";
                        durabilityText.color = styleResolver.ResolveValueColor(ValueRangeType.Durability, slotBar.fillAmount);
                    }
                }

                // патроны в оружии
                if (ammoLoadedText != null && item.HasModule<WeaponModule>())
                {
                    // если оружие не использует боезапас
                    if (item.Weapon.Definition.magazineSize == 0)
                    {
                        ammoLoadedText.transform.parent.gameObject.SetActive(false);
                    }
                    else
                    {
                        ammoLoadedText.transform.parent.gameObject.SetActive(true);
                        var currAmmo = slot.AmmoInMagazine;
                        var maxAmmo = item.Weapon.Definition.magazineSize;
                        ammoLoadedText.text = $"{currAmmo}/{maxAmmo}";
                    }
                }
            }

            // 🔹 Управляем прозрачностью
            if (canvasGroup != null)
                canvasGroup.alpha = isEmpty ? 0f : 1f;

            SetHighlight(false);
        }

        public void SetHighlight(bool active) => highlight.enabled = active;

        public void SetDimmed(bool active)
        {
            if (canvasGroup != null && canvasGroup.alpha > 0f) // не трогаем полностью пустые
                canvasGroup.alpha = active ? 0.4f : 1f;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            var slot = ParentUI.GetSlot(SlotIndex);
            if (slot.IsEmpty)
            {
                ParentUI.ClearSelection();
            }
            else
            {
                ParentUI.SelectSlot(this);

                // Проверяем двойной клик
                if (Time.time - lastClickTime < doubleClickThreshold)
                {
                    ParentUI.Window.controller.HandleDoubleClick(ParentUI._source, SlotIndex);
                    SetHighlight(!ParentUI._access.GetSlots(ParentUI._source).ElementAt(SlotIndex).IsEmpty);
                }
            }

            lastClickTime = Time.time;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            ServiceLocator.Current.Get<InventoryManagementWindow>().Drag.OnPointerDown(this, eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ServiceLocator.Current.Get<InventoryManagementWindow>().Drag.OnPointerUp(this, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            ServiceLocator.Current.Get<InventoryManagementWindow>().Drag.OnDrag(eventData);
        }
    }

}