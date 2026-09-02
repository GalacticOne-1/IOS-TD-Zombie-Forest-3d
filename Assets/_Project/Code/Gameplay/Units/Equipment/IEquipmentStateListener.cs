
namespace Galactic1.Code.Gameplay.Equipment
{
    public interface IEquipmentStateListener
    {
        //void OnEquipmentSlotChanged(int slotIndex, InventorySlotRuntime slot);
        bool Equip(int slotIndex);
        void Unequip(int slotIndex);
    }
}