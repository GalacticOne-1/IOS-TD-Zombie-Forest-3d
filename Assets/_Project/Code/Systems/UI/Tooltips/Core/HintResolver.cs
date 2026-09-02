using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.UI.Inventory;
using Galactic1.Code.WorldMap;
using Galactic1.Code.WorldMap.Intel;
using Galactic1.Configs;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.UI.Stats;
using Galactic1.UI;
using Galactic1.UI.Text;

namespace Galactic1.Code.UI.Tooltips
{
    public static class HintResolver
    {

        /// <summary>
        /// Создание описания для подсказки с учетом места запроса
        /// </summary>
        /// <param name="hintSource"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static TooltipData Formatting(HintSource hintSource, object data, int durability)
        {
            var result = new TooltipData();

            if (data is not ItemConfig config)
                return result;

            var dto = config.BuildTooltip();
                
            
            // ===== HEADER =====
            result.title = config.Header.titleLid;
            result.description = config.Header.descriptionLid;
            
            if(dto.descriptors != null && dto.descriptors.Count > 0)
            {
                // просто вытаскиваем название типа предмета (pistol/rifle/ammo/ ...)
                result.itemType = StatValueResolver.Resolve(dto.descriptors[0], out var statId).Item2;
            }

            
            
            // ===== WEAPON / EQUIPMENT =====
            if (config.HasModule<WeaponModule>() 
                || config.HasModule<EquipmentModule>())
            {
                switch (hintSource)
                {
                    // #1 подсказка с сравнением (для предметов юнита при активном юните)
                    case HintSource.InventoryUnit:
                    {
                        var source = ServiceLocator.Current.Get<InventoryManagementWindow>().modeController.RightSource;
                        var slots = source.GetSlots();
                        ItemConfig equipedItem = null;

                        // находим слот по типу предмета
                        var sl = slots.Count;
                        for (int i = 0; i < sl; i++)
                        {
                            var slotType = source.GetEquipmentSlotType(i);
                            
                            if (slotType == config.GetEquipSlot())
                            {
                                equipedItem = slots[i].Item;
                                break;
                            }
                        }
                        
                        
                        //
                        TooltipDataFieldStyle _style;
                        var l = dto.stats?.Count ?? 0;
                        for (int i = 0; i < l; i++)
                        {
                            // стиль поля по умолчанию
                            _style = TooltipDataFieldStyle.Orange;
                            
                            // === сравниваем если разные предметы
                            if (equipedItem != null && equipedItem.Id != config.Id)
                            {
                                var compareResult = equipedItem
                                    .StatCompare(dto.stats[i].StatId, dto.stats[i].Value);

                                _style = compareResult == CompareStat.Less
                                    ? TooltipDataFieldStyle.Red
                                    : compareResult == CompareStat.More
                                        ? TooltipDataFieldStyle.Green
                                        : TooltipDataFieldStyle.Orange;
                            }

                            var st = StatUIBuilder.Apply(
                                StatStyleResolver.Resolve(dto.stats[i].StatId),
                                dto.stats[i].Value,
                                dto.stats[i].StatId == StatId.Durability ? durability : 0);
                            
                            result.stats.Add(new ()
                            {
                                label = st.label, 
                                value = st.value,
                                Style = _style
                            });
                        }
                    }
                        break;

                    // #2 обычная подсказка для любого места
                    default:
                    {
                        var l = dto.stats?.Count ?? 0;
                        for (int i = 0; i < l; i++)
                        {
                            var st = StatUIBuilder.Apply(
                                StatStyleResolver.Resolve(dto.stats[i].StatId),
                                dto.stats[i].Value,
                                dto.stats[i].StatId == StatId.Durability ? durability : 0);
                            
                            result.stats.Add(new ()
                            {
                                label = st.label, 
                                value = st.value,
                                Style = TooltipDataFieldStyle.Orange
                            });
                        }
                    }
                        break;
                }
            }
            // отображаем статку для всех остальных предметов
            else
            {
                var l = dto.stats?.Count ?? 0;
                for (int i = 0; i < l; i++)
                {
                    var st = StatUIBuilder.Apply(
                        StatStyleResolver.Resolve(dto.stats[i].StatId),
                        dto.stats[i].Value,
                        dto.stats[i].StatId == StatId.Durability ? durability : 0);
                    
                    result.stats.Add(new ()
                    {
                        label = st.label, 
                        value = st.value,
                        Style = TooltipDataFieldStyle.Orange
                    });
                }
            }
            
            
                
            // ==== LINKED ITEMS =====
            if (config.TryGetLinkedItems(out var id, out var linkedItems))
            {
                result.linkedItemStyle = StatStyleResolver.Resolve(id);
                result.linkedItems = linkedItems;
            }

            
            
            // ===== CRAFTING =====
            var recipe = config.Recipes.FirstOrDefault();
            
            result.extra = new List<TooltipDataField>();
            if (recipe != null && recipe.RequiredStationItem != null) // показываем станцию для крафта
            {
                result.extra.Add(new ()
                {
                    label = TextBuilder.Start()
                        .Size(100)
                        .Text("Made at ")
                        .End()
                        .Bold()
                        .Text($"{recipe.RequiredStationItem.Header.titleLid} ")
                        .End()
                        .Bold()
                        .Text($"lvl. {(int)recipe.RequiredTier}")
                        .End()
                        .ToString()
                });
            }
            else // для не крафтовых предметов показываем список локаций
            {
                var locations = string.Join(", ",
                    FindLocationsByItem(config)
                        .Select(c => c.Header.TitleLid));
                result.extra.Add(new ()
                {
                    label = TextBuilder.Start()
                        .Size(100)
                        .Text("Can be found in ")
                        .End()
                        .Bold()
                        .Text($"{locations}")
                        .End()
                        .ToString()
                });
            }



            return result;
        }



        /// <summary>
        /// Собирает список локаций где представлен предмет
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static List<LocationConfig> FindLocationsByItem(ItemConfig item)
        {
            var result = new List<LocationConfig>();

            var locations = ServiceLocator.Current
                .Get<ConfigProvider>()
                .Get<LocationsConfigs>().Locations;

            var l = locations.Count;
            for (int i = 0; i < l; i++)
            {
                if (locations[i].LocationIntel.HasCategory(item.Classification.economyCategory))
                {
                    result.Add(locations[i]);
                }
            }

            return result;
        }
        
    }

    public enum HintSource
    {
        Default = 0,
        InventoryUnit = 1,
    }
}