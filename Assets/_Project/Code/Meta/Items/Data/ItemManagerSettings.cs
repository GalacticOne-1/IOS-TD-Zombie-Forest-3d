using UnityEngine.Serialization;

namespace Galactic1.Items
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "ItemManagerSettings", menuName = "Game Configs/Inventory/New ItemManager BasicSettings")]
    public class ItemManagerSettings : ScriptableObject
    {
        [Header("Default Save Paths")] 
        public string basePath = "";
        public string resourcePath = "/Items/Resources";
        
        public string ammoPath = "/Items/Ammo";
        public string weaponPath = "/Items/Weapons";
        public string armorPath = "/Items/Armor";
        public string upgradePath = "/Items/Upgrade";
        public string consumablePath = "/Items/Consumables";
        
        public string vehiclePath = "/Items/Vehicle";
        
        public string blueprintPath = "/Items/Blueprint";
        
        public string stationPath = "/Items/Stations";
        public string storagePath = "/Items/Storages";
        public string defensePath = "/Items/Defense";
        public string campPath = "/Items/Camp";
    }

}