
namespace Galactic1.Code.UI.Inventory
{
    public abstract class CharacterInventoryData : BaseInventoryData
    {
        public void SetBackpack(int extraSlots)
        {
            int newCapacity = baseCapacity + extraSlots;
            InventoryProxy.SetCapacity(newCapacity);
            OnChanged?.Invoke();
        }
    }
}