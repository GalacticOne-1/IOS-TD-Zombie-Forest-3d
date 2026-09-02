
using UnityEngine;

namespace Galactic1.Code.UI.Inventory
{
    public abstract class InventoryContainer : MonoBehaviour, IInventoryContainer
    {
        public virtual BaseInventoryData Inventory => null;

        protected abstract void Awake();

        public void ClearSlots() => Inventory.InventoryProxy.ClearSlots();
    }
}