using System;
using Galactic1.Core.Enums;
using Galactic1.Items;
using Galactic1.Code.UI.Inventory;
using Galactic1.Game.Meta.Items;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Core.UI
{
    public class UIButtonPocket : BaseUIButton
    {
        [field:SerializeField] public Image icon {get; private set;}
        [field:SerializeField] public TMP_Text valueText {get; private set;}
        [field:SerializeField] public Image bar {get; private set;}
        [field:SerializeField] public EquipmentSlotType SlotType {get; private set;}
        
        public IInventoryContainer Container {get; private set;}
        public event Action<UIButtonPocket> OnPocketClicked;
        
        
        

        public virtual void Bind(IInventoryContainer container)
        {
            Container = container;
            gameObject.RegisterButtonClick(() => OnPocketClicked?.Invoke(this));
        }
        
        public (InventorySlotProxy, int) GetSlot()
        {
            var index = Container.Inventory.FindSlotIndex(SlotType);
            if (!index.HasValue) return (null, -1);
            return (Container.Inventory.InventoryProxy.Slots[index.Value], index.Value);
        }

        public void Refresh()
        {
            if (Container == null) return;
            
            // получаем слот снаряжения по типу
            var slotIndex = Container.Inventory.FindSlotIndex(SlotType);
            if(!slotIndex.HasValue) return;
            
            var slot = Container.Inventory.InventoryProxy.Slots[slotIndex.Value];

            if (slot.IsEmpty)
            {
                icon.enabled = false;
                valueText.text = "";
                bar.fillAmount = 0;
                bar.transform.parent.gameObject.SetActive(false);
                return;
            }

            var item = slot.Item.Value;

            icon.enabled = true;
            icon.sprite = item.Header.icon;
            valueText.text = item.IsStackable && slot.Amount.Value > 1 ? $"{slot.Amount.Value}" : "";

            // Прочность для оружия или предметов со стабильностью
            if (item.HasModule<WeaponModule>())
            {
                bar.fillAmount = (float)slot.Durability.Value / item.Physical.maxDurability;
                bar.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                bar.fillAmount = 0f;
                bar.transform.parent.gameObject.SetActive(false);
            }
        }
    }
}