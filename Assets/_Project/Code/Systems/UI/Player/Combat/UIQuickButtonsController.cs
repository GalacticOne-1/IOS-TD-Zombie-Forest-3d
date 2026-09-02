
using Galactic1.Core.Enums;
using Galactic1.Core.UI;
using Galactic1.Core.UI.HUD;
using Galactic1.Items;
using Galactic1.UI;
using UnityEngine;


namespace Galactic1.Systems.UI
{
    public class UIQuickButtonsController : MonoBehaviour
    {
        private HUDPlayer _hud;
        private UIButtonPocket _buttonPocket1, _buttonPocket2;
        
        private void Start()
        {
            _hud = GetComponent<HUDPlayer>();

            //_buttonPocket1 = _hud.quickButton1;
            //_buttonPocket2 = _hud.quickButton2;
            
            _buttonPocket1.OnPocketClicked += OnPocketClicked;
            _buttonPocket2.OnPocketClicked += OnPocketClicked;
        }


        private void OnPocketClicked(UIButtonPocket pocket)
        {
            (InventorySlotProxy slot, int index) slotData = pocket.GetSlot();
            if (slotData.slot == null || slotData.slot.IsEmpty) return;


            var item = slotData.slot.Item.Value;

            // #1 swap weapon
            // if (item is WeaponItem)
            // {
            //     // Получаем индекс основного оружия в снаряжении
            //     int? slotMainWeaponIndex = pocket.Container.Inventory.FindSlotIndex(EquipmentSlotType.WeaponMain);
            //     if (slotMainWeaponIndex.HasValue)
            //     {
            //         // ServiceLocator.Current.Get<InventoryManagementWindow>().transferSystem.SwitchWeapon(
            //         //     pocket.Container,
            //         //     slotData.index,
            //         //     pocket.Container,
            //         //     slotMainWeaponIndex.Value);
            //     }
            // }
            
            // #2 use item
            // else
            // {
            //     // Активируем расходник через ItemUseSystem
            //     //var ctx = new ItemContext(pocket.Container.Inventory, slotData.slot, -1);
            //     //ItemUseSystem.UseItem(ctx);
            // }
        }
    }
}