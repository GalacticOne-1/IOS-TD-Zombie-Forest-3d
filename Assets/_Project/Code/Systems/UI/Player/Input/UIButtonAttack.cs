
using System;
using Galactic1.Gameplay.Interaction;
using Galactic1.Code.UI.Inventory;
using UnityEngine;

namespace Galactic1.Core.UI
{
    public class UIButtonAttack : UIButtonPocket
    {
        [SerializeField] private GameObject highlight;
        
        private InventorySlotProxy _activeWeaponSlot;


        
        
        
        public override void Bind(IInventoryContainer container)
        {
            base.Bind(container);
            gameObject.RegisterButtonClick(OnClicked);
        }


        public void Hide()
        {
            highlight.SetActive(false);
        }

        public void Show()
        {
            highlight.SetActive(true);
        }
        
        private void OnClicked()
        {
            ServiceLocator.Current.Get<InteractionSystem>().AttackCurrent(null); // можно передать transform игрока
        }
    }
}