using Galactic1;
using UnityEngine;

namespace Galactic1
{
    public class AssetEquipment_assist : InventoryConfigs, IItemUsing
    {
        [Space] 
        [SerializeField] private EEquipment assetKey;
        
        
        [SerializeField] private AssetItems.CUsing[] useResult;
        public AssetItems.CUsing[] UseResult => useResult;
        
        
        public override int GetAssetKey() => (int)assetKey;


        public override string GetMainFeatures() => "";
    }
}