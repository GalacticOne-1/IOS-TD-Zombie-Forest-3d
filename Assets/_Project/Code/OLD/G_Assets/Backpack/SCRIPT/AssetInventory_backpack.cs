using Galactic1;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1
{
    public class AssetInventory_backpack : InventoryConfigs, IItemUsing
    {
        [Header("Slot size")] 
        [SerializeField]
        private byte slotSize;

        public byte SlotSize => slotSize;
        
        
        [Space]
        [SerializeField] private EEquipment assetKey;
        public override int GetAssetKey() => (int)assetKey;
        
        public override string GetMainFeatures()
        {
            return "";
        }
    }
}