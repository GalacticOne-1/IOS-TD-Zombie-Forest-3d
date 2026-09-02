using Galactic1.Game.Meta.Items;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Galactic1.Code.Systems.Inventory
{
    [CreateAssetMenu(
        fileName = "CampStorageInventoryConfig",
        menuName = "Game Configs/Inventory/Camp Inventory Config")]
    public class CampStorageInventoryConfig : InventoryDataBase
    {
        public override void Initialize(Object data = null)
        {
            
        }



        public string GetInventoryId(StorageType type)
        {
            return $"CampStorage_{type}";
        }
        
        public string GetConfigId(StorageType type)
        {
            return type switch
            {
                StorageType.Regular => "facility.storage.regular",
                StorageType.Weapon => "facility.storage.weapon",
                StorageType.Ammo => "facility.storage.ammo",
                
                
                _ => ""
            };
        }
    }
}