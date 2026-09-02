using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Core.Enums;
using Galactic1.Game.UI.Stats.DTO;
using UnityEngine;

namespace Galactic1.Game.UI.Stats
{
    public static class StatOrderResolve
    {

        /// <summary>
        /// Сортирует уже созданные stat view в соответствии с правилами.
        /// </summary>
        public static void ReorderSpawned(
            List<(StatLayoutType, IPooledStatView<StatDtoBase>)> _spawned,
            ItemCategory itemCategory)
        {
            var ordered = _spawned
                .OrderBy(tuple =>
                {
                    var (layoutType, view) = tuple;
                    return GetStatPriority(
                        itemCategory,
                        layoutType,
                        view.Dto);
                })
                .ToList();

            for (int i = 0; i < ordered.Count; i++)
            {
                ordered[i].Item2.RectTransform.SetSiblingIndex(i);
            }

            _spawned.Clear();
            _spawned.AddRange(ordered);
        }

        /// <summary>
        /// Возвращает приоритет отображения стата в зависимости от типа предмета.
        /// Меньше число — выше в списке.
        /// </summary>
        static int GetStatPriority(
            ItemCategory itemCategory,
            StatLayoutType layoutType,
            StatDtoBase rawDto)
        {

            if (layoutType == StatLayoutType.LabelText ||
                layoutType == StatLayoutType.DescriptionText ||
                layoutType == StatLayoutType.PriceText)
            {
                var statId = (rawDto as IStatTextDto)?.StatStyleEntry.statId;

                switch (itemCategory)
                {
                    case ItemCategory.Weapon:
                        return statId switch
                        {
                            // 0
                            //StatId.AmmoType => 1,
                            StatId.MagazineCapacity => 2,
                            StatId.AttackRange => 3,
                            StatId.DamagePerSec => 4,
                            StatId.ReloadSpeed => 5,
                            // --- 6
                            StatId.Accuracy => 10,
                            StatId.CritChance => 11,
                            StatId.CritDamage => 12,
                            StatId.Penetration => 13,
                            // --- 14
                            StatId.Durability => 14,
                            StatId.LinkedAmmo 
                                or StatId.LinkedWeapons
                                or StatId.LinkedArmors
                                or StatId.LinkedModules => 15,

                            _ => 100
                        };
                    
                    case ItemCategory.Ammo:
                        return statId switch
                        {
                            StatId.LinkedWeapons => 0,
                            _ => 100
                        };

                    case ItemCategory.Armor:
                        return statId switch
                        {
                            StatId.Armor => 0,
                            StatId.Health => 1,
                            StatId.Resistance => 2,
                            StatId.Durability => 3,
                            _ => 100
                        };


                    default:
                        return 100;
                }

            }

            return 100;
        }


        public static void InsertStructure(
            StatViewFactory statFactory,
            Transform root,
            ItemCategory itemCategory,
            Action<(StatLayoutType, IPooledStatView<StatDtoBase>)> onSpawned)
        {
            onSpawned(SpawnStructure(statFactory, root, StatLayoutType.Spacer, 0));
            
            switch (itemCategory)
            {
                case ItemCategory.Weapon:
                    onSpawned(SpawnStructure(statFactory, root, StatLayoutType.Divider, 5));
                    
                    // отступ в конец
                    onSpawned(SpawnStructure(statFactory, root, StatLayoutType.Spacer, 100));
                    break;
                
                case ItemCategory.Ammo:
                    
                    break;
                
                case ItemCategory.Armor:
                    
                    break;
                
                case ItemCategory.Consumable:
                    
                    break;
            }
        }


        static (StatLayoutType, IPooledStatView<StatDtoBase>) SpawnStructure(
            StatViewFactory statFactory,
            Transform root,
            StatLayoutType layoutType,
            byte order)
        {
            var view = statFactory.Get(layoutType, root);
            view.RectTransform.SetSiblingIndex(order);

            if (layoutType == StatLayoutType.Spacer)
                view.Bind(new StatSpacerDto());
            else if (layoutType == StatLayoutType.Divider)
                view.Bind(new StatDividerDto());

            return (layoutType, view);
        }
    }
}