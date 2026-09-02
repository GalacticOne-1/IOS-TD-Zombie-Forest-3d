using System.Collections.Generic;
using Galactic1.Core;
using Galactic1.Core.Location;
using UnityEngine;

namespace Galactic1
{

    

    public struct CSortingData
    {
        public IAssetSorting aseet;
        public Iid item;                    // UI card (класс должен быть на карточке)
    }
    
    
    public class SortingItems_RARITY
    {
        /// <summary>
        /// Сортировка предметов по редкости
        /// </summary>
        /// <param name="list"></param>
        public SortingItems_RARITY(CSortingData[] list, bool use_priority)
        {
            var l = list.Length;

            // #1 сортируем
            CSortingData t;
            for (int j = 0; j <= l-2; j++)
            {
                for (int i = 0; i <= l-2; i++)
                {
                    if (list[i].aseet.Rare < list[i+1].aseet.Rare)
                    {
                        t = list[i + 1];
                        list[i + 1] = list[i];
                        list[i] = t;
                        list[i].item.obj.transform.SetSiblingIndex(i);
                    }
                }
            }
            
            // #1 сортируем по приоритету
            if(use_priority)
            {
                for (int j = 0; j <= l - 2; j++)
                {
                    for (int i = 0; i <= l - 2; i++)
                    {
                        if (list[i].aseet.Sorting < list[i + 1].aseet.Sorting)
                        {
                            t = list[i + 1];
                            list[i + 1] = list[i];
                            list[i] = t;
                            list[i].item.obj.transform.SetSiblingIndex(i);
                        }
                    }
                }
            }
        }
    }
    
    
    

    
    
    public class GetMissionItems
    {
        /// <summary>
        /// Формирование предметов доступных в миссии
        /// <br/>для выдачи игроку
        /// </summary>
        public GetMissionItems(
            LocationConfig_General.CMissionItems[] main_reward, 
            LocationConfig_General.CMissionItems[] possible_reward,
            out List<CPlayerInventory> list)
        {
            // ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.WorldMap.Value.map_auto_gathering++;
            // ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.WorldMap.Value =
            //     ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.WorldMap.Value;
            
            list = new List<CPlayerInventory>();
            
            // #1 main reward
            var l = main_reward.Length;
            for (int i = 0; i < l; i++)
            {
                new LIB_GetAsset_key(
                    !main_reward[i].useEquipment ? 0 : 1,
                    0,
                    !main_reward[i].useEquipment ? (int)main_reward[i].item : (int)main_reward[i].equipment,
                    out AssetItems assetItems,
                    out InventoryConfigs equipment);
                list.Add(new CPlayerInventory()
                {
                    type = (byte)(!main_reward[i].useEquipment ? 0 : 1),
                    category = 0,
                    //id = (byte)(assetItems ? assetItems.ID : equipment.ID),
                    volume = (short)Random.Range(main_reward[i].minQu, main_reward[i].maxQu)
                });
            }
            
            
            // #2 possible reward
            byte avail_possible_reward = 4;
            byte n = 0;
            
            l = possible_reward.Length;
            bool[] closed = new bool[l];
            
            while (avail_possible_reward > 0)
            {
                for (int i = 0; i < l; i++)
                {
                    // *** в одной лоттереи предмет выпадает только один раз
                    if (closed[i]) continue;
                    
                    new LIB_GetAsset_key(
                        !possible_reward[i].useEquipment ? 0 : 1,
                        0,
                        !possible_reward[i].useEquipment ? (int)possible_reward[i].item : (int)possible_reward[i].equipment,
                        out AssetItems assetItems,
                        out InventoryConfigs equipment);
                    
                    // оружие разыгрываем только по выполнении условия
                    // if((equipment as AssetInventory_weapon || equipment as AssetInventory_equipment) &&
                    //    ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.WorldMap.Value.map_auto_gathering < 5) continue;
                
                    
                    // * шанс выпадания предмета
                    new RarityChance((assetItems ? assetItems : equipment).rare, out bool is_win);
                    if (Random.Range(0,3) == 0 && is_win)
                    {
                        avail_possible_reward--;
                        closed[i] = true;
                    
                        list.Add(new CPlayerInventory()
                        {
                            type = (byte)(!possible_reward[i].useEquipment ? 0 : 1),
                            category = 0,
                            //id = (byte)(assetItems ? assetItems.ID : equipment.ID),
                            volume = (short)Random.Range(possible_reward[i].minQu, possible_reward[i].maxQu),
                            strength = (short)(equipment as AssetEquipmement != null
                                ? (equipment as AssetEquipmement).Durability
                                : 0)
                        });

                        // когда выпадает оружие сбрасываем условие
                        if ((equipment as AssetInventory_weapon || equipment as AssetInventory_equipment))
                        {
                            // ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.WorldMap.Value.map_auto_gathering = 0;
                            // ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.WorldMap.Value =
                            //     ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.WorldMap.Value;
                        }
                    }
                    
                    if(avail_possible_reward <= 0) return;
                }
                
                DLog.Alert($">>> {avail_possible_reward}", EDlogColor.YELLOW);
                
                // *** max try count
                n++;
                if(n > 5) break;
            }
            
        }
    }


    
    public class GetChestItems
    {
        /// <summary>
        /// Формирование предметов для ящика в локации
        /// </summary>
        /// <param name="possible_reward"></param>
        public GetChestItems(LocationConfig_General.CMissionItems[] possible_reward, ERarities rariry, out List<CPlayerInventory> list)
        {
            list = new List<CPlayerInventory>();

            byte avail_possible_reward = (byte)Random.Range(3, 5);
            byte n = 0;
            
            var l = possible_reward.Length;
            bool[] closed = new bool[l];
            bool have_equipment = false;
           
            while (avail_possible_reward > 0)
            {
                for (int i = 0; i < l; i++)
                {
                    // *** в одной лоттереи предмет выпадает только один раз
                    if (closed[i]) continue;
                    
                    new LIB_GetAsset_key(
                        !possible_reward[i].useEquipment ? 0 : 1,
                        0,
                        !possible_reward[i].useEquipment ? (int)possible_reward[i].item : (int)possible_reward[i].equipment,
                        out AssetItems assetItems,
                        out InventoryConfigs equipment);

                    if ((rariry == ERarities.Standard || rariry == ERarities.Superior || have_equipment) &&
                        ((equipment as AssetInventory_weapon) || (equipment as AssetInventory_equipment))) continue;
                
                    // * шанс выпадания предмета
                    new RarityChance((assetItems ? assetItems : equipment).rare, out bool is_win);
                    if (Random.Range(0,3) == 0 && is_win)
                    {
                        avail_possible_reward--;
                        closed[i] = true;

                        if ((equipment as AssetInventory_weapon) || (equipment as AssetInventory_equipment))
                            have_equipment = true;
                    
                        list.Add(new CPlayerInventory()
                        {
                            type = (byte)(!possible_reward[i].useEquipment ? 0 : 1),
                            category = 0,
                            //id = (byte)(assetItems ? assetItems.ID : equipment.ID),
                            volume = (short)Random.Range(possible_reward[i].minQu, possible_reward[i].maxQu),
                            strength = (short)(equipment as AssetEquipmement != null
                                ? (equipment as AssetEquipmement).Durability
                                : 0)
                        });
                    }
                    
                    if(avail_possible_reward <= 0) return;
                }
                
                DLog.Alert($">>> {avail_possible_reward}", EDlogColor.YELLOW);
                
                // *** max try count
                n++;
                if(n > 5) break;
            }
        }
    }
}