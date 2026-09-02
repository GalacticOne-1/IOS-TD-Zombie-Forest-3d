using System.Collections.Generic;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.GameDatabase.Registries
{
    /// <summary>
    /// Runtime registry for all facility modules.
    /// Built from ItemRegistry.
    /// </summary>
    public sealed class FacilityRegistry : RegistryBase<RuntimeId, FacilityModule>
    {
        public FacilityRegistry(IReadOnlyList<ItemConfig> items)
        {
            foreach (var item in items)
            {

                if (item == null)
                    continue;

                if (!item.IsFacility())
                    continue;

                var facility = item.GetFacilityModule();

                if (facility == null)
                {
                    DLog.Alert(
                        $"[FacilityRegistry] Item '{item.name}' marked as facility but module missing.",
                        EDlogColor.YELLOW);

                    continue;
                }

                if (map.ContainsKey(item.Id))
                {
                    DLog.Alert(
                        $"[FacilityRegistry] Duplicate facility id '{item.Id.DebugKey}'",
                        EDlogColor.RED);

                    continue;
                }

                map.Add(item.Id, facility);
            }

            BuildCraftingStationRecipes(items);
        }

        /// <summary>
        /// Auto-build crafting station recipe lists.
        /// </summary>
        private void BuildCraftingStationRecipes(IReadOnlyList<ItemConfig> items)
        {
            foreach (var stationItem in items)
            {
                if (!stationItem.HasModule<CraftingStationModule>())
                    continue;

                stationItem.CraftStation.ClearAvailableRecipes();

                foreach (var resource in items)
                {
                    var craftedItem = resource;

                    if (!craftedItem.IsCraftable)
                        continue;

                    var recipes = craftedItem.Recipes;

                    for (int i = 0; i < recipes.Count; i++)
                    {
                        var recipe = recipes[i];

                        if (recipe.RequiredStationItem == stationItem)
                        {
                            stationItem.CraftStation.AddToRecipeList(craftedItem);
                        }
                    }
                }

                stationItem.CraftStation.SortRecipes();
            }
        }

        /// <summary>
        /// Finds storage facility supporting specified tag.
        /// </summary>
        public ItemConfig FindStorageConfigForTag(ItemTag tag)
        {
            foreach (var facility in map.Values)
            {
                var item = facility.Item;

                if (!item.HasModule<StorageModule>())
                    continue;

                var storage = item.Storage;

                var allowedTags = storage.AllowedTags;
                var l = allowedTags.Count;
                
                for (int i = 0; i < l; i++)
                {
                    if (allowedTags[i] == tag)
                        return item;
                }
            }

            return null;
        }
        
        
        /*public FacilityConfigRegistry(IReadOnlyList<ItemConfig> allItems)
        {
            // allConfigs - один список для станций и предметов
            
            foreach (var item in allItems)
            {
                if(item.IsFacility())
                {
                    // добавляем объект
                    if (!_facilityItems.ContainsKey(item.Id))
                    {
                        _facilityItems.Add(item.Id, item.GetFacilityModule());
                        
                        
                        // *** находим все рецепты если это производство
                        if (item.HasModule<CraftingStationModule>())
                        {
                            item.CraftStation.ClearAvailableRecipes();
                        
                            // find recipes
                            foreach (var resourceItem in allItems)
                            {
                                foreach (var recipe in resourceItem.Recipes)
                                {
                                    if (item == recipe.RequiredStationItem)
                                    {
                                        item.CraftStation.AddToRecipeList(resourceItem);
                                    }
                                }
                            }
                            
                            // Сортировка по тиру рецепта после заполнения
                            item.CraftStation.SortRecipes();
                        }
                    }
                    else
                        Debug.LogError($"Duplicate BuildingConfig id: {item.Id.DebugKey}");
                }
            }
        }
        
        
        
        
        
        /// <summary>
        /// Ищет ItemConfig хранилища поддерживающего тег.
        /// </summary>
        public ItemConfig FindStorageConfigForTag(ItemTag tag)
        {
            foreach (var module in _facilityItems.Values)
            {
                if (!module.Item.HasModule<StorageModule>())
                    continue;

                var storage = module.Item.Storage;
                foreach (var allowed in storage.AllowedTags)
                {
                    if (allowed == tag)
                        return module.Item;
                }
            }

            return null;
        }*/
    }
}