
using System;
using ObservableCollections;

namespace Galactic1.Items
{
    /// <summary>
    /// Реактивный инвентарь игрока на основе ObservableList<ItemSlotSave>
    /// </summary>
    public class InventoryProxy
    {
        
        public ObservableList<InventorySlotProxy> Slots { get; private set; }
        public event Action OnChanged;
        

        public InventoryProxy(ObservableList<InventorySlotProxy> inventorySlots)
        {
            Slots = inventorySlots;
            
            EventBus<SceneServicesResetReusableEvent>.Register(
                new EventBinding<SceneServicesResetReusableEvent>(() => OnChanged = null));
        }
        
        
        
        
        public void NotifyChanged() => OnChanged?.Invoke();
        
        public void SetSlot(int index, InventorySlotProxy newSlot)
        {
            Slots[index] = newSlot;     // это вызовет ObserveReplace
            NotifyChanged();
        }


        public void ClearSlots()
        {
            foreach (var slot in Slots)
                slot.Clear();
            NotifyChanged();
        }

       

        public void SetCapacity(int newCapacity)
        {
            // Если увеличиваем количество
            while (Slots.Count < newCapacity)
            {
                var data = new InventorySlotData(null, "", 0, 0, 0);
                var proxy = new InventorySlotProxy(data);

                Slots.Add(proxy);
                //Origin.Inventory.Add(data);
            }

            // Если уменьшаем количество
            while (Slots.Count > newCapacity)
            {
                int last = Slots.Count - 1;

                //Origin.Inventory.Remove(Slots[last].Origin);  
                Slots.RemoveAt(last);
            }
            NotifyChanged();
        }
        
    }
}
