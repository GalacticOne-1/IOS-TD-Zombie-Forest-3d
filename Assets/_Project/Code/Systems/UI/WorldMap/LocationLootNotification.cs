using System.Collections.Generic;
using Galactic1.RaidLoot.Authoring;

namespace Galactic1.Code.WorldMap
{
    public static class LocationLootNotification
    {
        private static Dictionary<LootEconomyCategory, string> _map;



        static void Mapping()
        {
            _map = new();

            _map[LootEconomyCategory.FoodSupply] = "Food & Water";
            _map[LootEconomyCategory.WaterSupply] = "Food & Water";
            
            _map[LootEconomyCategory.Fuel] = "Fuel";
            _map[LootEconomyCategory.GasCylinder] = "Gas Cylinder";
            _map[LootEconomyCategory.EmptyCylinder] = "Empty Gas Cylinder";
            
            
            _map[LootEconomyCategory.Chemicals] = "Chemicals";
            _map[LootEconomyCategory.Plastics] = "Plastic Scrap";
            _map[LootEconomyCategory.Wood] = "Wood";
            _map[LootEconomyCategory.Stone] = "Stone";
            _map[LootEconomyCategory.Ore] = "Ore";
            _map[LootEconomyCategory.RareMinerals] = "Rare Minerals";
            _map[LootEconomyCategory.Scrap] = "Scrap";
            _map[LootEconomyCategory.Cloth] = "Cloth & Leazer";
            _map[LootEconomyCategory.Mechanical] = "Mechanical Parts";
            _map[LootEconomyCategory.IronMetrials] = "Iron Materials";
            _map[LootEconomyCategory.Electronics] = "Electronics Parts";
            
            
            _map[LootEconomyCategory.Tool] = "Tools";
            _map[LootEconomyCategory.ConstructionKit] = "Construction Kit";
            
            _map[LootEconomyCategory.Ammo] = "Ammo";
            _map[LootEconomyCategory.Weapon] = "Weapons";
            _map[LootEconomyCategory.Armor] = "Armors";
            _map[LootEconomyCategory.Grenade] = "Grenades";
            
            _map[LootEconomyCategory.Medical] = "Medical Consumables";
        }


        public static string GetMessage(LootEconomyCategory category)
        {
            if (_map == null)
            {
                Mapping();
            }

            return _map.ContainsKey(category) ? _map[category] : "";
        }
    }
}