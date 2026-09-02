
using Galactic1.Code.UI.Inventory;
using Galactic1.Core.UI;
using Galactic1.Core.UI.HUD;
using UnityEngine;

namespace Galactic1.Systems.Inventory
{
    public class HUDSlotsController : MonoBehaviour
    {

        private IInventoryContainer _container;

        private UIButtonPocket[] pockets;
        private UIButtonAttack attackButton;


        /// <summary>
        /// Привязать HUD к инвентарю игрока или дракона
        /// </summary>
        public void Bind(HUDPlayer hud, IInventoryContainer container)
        {
            //pockets = new[] { hud.attackButton, hud.quickButton1, hud.quickButton2 };
            //attackButton = hud.attackButton;

            _container = container;

            _container.Inventory.OnChanged += RefreshUI;

            BindSlots();
            RefreshUI();
        }

        public void Unbind()
        {
            if (_container != null)
                _container.Inventory.OnChanged -= RefreshUI;
        }

        private void BindSlots()
        {
            for (int i = 0; i < pockets.Length; i++)
                pockets[i].Bind(_container);
        }

        /// <summary>
        /// Вызывается, когда обновляется какой-либо инвентарь
        /// </summary>
        public void RefreshUI()
        {
            foreach (var p in pockets)
                p.Refresh();
        }

    }
}