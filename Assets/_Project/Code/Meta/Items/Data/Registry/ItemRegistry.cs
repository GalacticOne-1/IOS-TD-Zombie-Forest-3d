using System.Collections.Generic;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.GameDatabase.Registries
{
    /// <summary>
    /// Runtime lookup registry for items.
    /// </summary>
    public sealed class ItemRegistry : RegistryBase<RuntimeId, ItemConfig>
    {
        public ItemRegistry(IReadOnlyList<ItemConfig> configs)
        {
            for (int i = 0; i < configs.Count; i++)
            {
                var config = configs[i];

                if (config == null)
                {
                    DLog.Alert($"[ItemRegistry] Null config at index {i}", EDlogColor.YELLOW);
                    continue;
                }

                if (config.Id == null)
                {
                    DLog.Alert($"[ItemRegistry] Item '{config.name}' has NULL ItemId.", EDlogColor.YELLOW);
                    continue;
                }

                if (map.ContainsKey(config.Id))
                {
                    DLog.Alert($"[ItemRegistry] Duplicate ItemId detected: {config.Id.name}", EDlogColor.YELLOW);
                    continue;
                }

                map.Add(config.Id, config);
            }
        }
        
        
        
        
        public IReadOnlyCollection<ItemConfig> GetAllWeapons()
        {
            List<ItemConfig> result = new();
            
            foreach (var pair in All)
            {
                if (pair.Value.HasModule<WeaponModule>())
                    result.Add(pair.Value);
            }

            return result;
        }
        
        public IReadOnlyCollection<ItemConfig> GetAllArmors()
        {
            List<ItemConfig> result = new();
            
            foreach (var pair in All)
            {
                if (pair.Value.HasModule<EquipmentModule>())
                    result.Add(pair.Value);
            }

            return result;
        }
        
        public IReadOnlyCollection<ItemConfig> GetAllCraftStation()
        {
            List<ItemConfig> result = new();
            
            foreach (var pair in All)
            {
                if (pair.Value.HasModule<CraftingStationModule>())
                    result.Add(pair.Value);
            }

            return result;
        }
        
        public IReadOnlyCollection<ItemConfig> GetAllTransport()
        {
            List<ItemConfig> result = new();
            
            foreach (var pair in All)
            {
                if (pair.Value.HasModule<VehicleModule>())
                    result.Add(pair.Value);
            }

            return result;
        }

        
        /*
         *  ! Проверять имена !
         */
        public ItemConfig GetItemTool(ItemEquipType itemEquipType)
        {
            return itemEquipType switch
            {
                // ItemEquipType.ToolStoneAxe => GetItemByName("Stone Axe"),
                // ItemEquipType.ToolIronAxe => GetItemByName("Iron Axe"),
                // ItemEquipType.ToolStonePickaxe => GetItemByName("Stone Pickaxe"),
                // ItemEquipType.ToolIronPickaxe => GetItemByName("Iron Pickaxe"),
                _ => null
            };
        }
    }
}