using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Gameplay.Weapons.Infrastructure;
using Galactic1.Code.Items;
using Galactic1.Core.Enums;
using Galactic1.Game.UI.Stats.DTO;
using UnityEngine;
using Galactic1.Code.Gameplay.Weapons.Logic;
using Galactic1.Game.Meta.Stats;
using Galactic1.UI;

namespace Galactic1.Game.Meta.Items
{
    /// <summary>
    /// Оружейная логика.
    /// Использует AmmoDefinition для калибра.
    /// </summary>
    [System.Serializable]
    public class WeaponModule : EquipmentModuleBase, ILinkedItemsProvider
    {
        
        [Space(20)] 
        [Header("📊 Weapon Data")] 
        [SerializeField] private WeaponInfo info;
        
        [SerializeField] private WeaponDefinition definition; // "как предмет работает в бою"
        
        public WeaponInfo Info => info;
        public AmmoDefinition SupportedAmmo => definition.supportedAmmo;

        public WeaponDefinition Definition => definition;


        // 🔹 Дополнительно — удобно для редактора и тултипов
        public float DPS
        {
            get
            {
                if (definition == null) return 0f;

                var shotsPerSecond = definition.roundsPerMinute / 60f;
        
                // один выстрел = N пуль
                var damagePerShot = definition.damage * definition.projectilesPerShot;

                // Burst — пауза между очередями снижает реальный DPS
                if (definition.fireMode == FireMode.Burst)
                {
                    var burstDuration = definition.burstCount / shotsPerSecond;
                    var cycleTime = burstDuration + definition.burstPauseSec;
                    var burstDamage = damagePerShot * definition.burstCount;
                    return burstDamage / cycleTime;
                }

                return damagePerShot * shotsPerSecond;
            }
        }


        public override IReadOnlyList<ItemStatEntry> BaseStats()
        {
            var ar = new List<ItemStatEntry>();
            ar.Add(new ItemStatEntry()
            {
                StatId = StatId.Damage,
                Operation = ModifierOperation.Flat,
                Value = definition.damage,
                applyToUnit = false,
                showInTooltip = true,
            });
            ar.Add(new ItemStatEntry()
            {
                StatId = StatId.Accuracy,
                Operation = ModifierOperation.Flat,
                Value = definition.GetAccuracyScore(),
                applyToUnit = false,
                showInTooltip = true,
            });
            ar.Add(new ItemStatEntry()
            {
                StatId = StatId.DamagePerSec,
                Operation = ModifierOperation.Flat,
                Value = (int)DPS,
                applyToUnit = false,
                showInTooltip = true,
            });
            ar.Add(new ItemStatEntry()
            {
                StatId = StatId.AttackRange,
                Operation = ModifierOperation.Flat,
                Value = definition.range,
                applyToUnit = false,
                showInTooltip = true,
            });
            ar.Add(new ItemStatEntry()
            {
                StatId = StatId.MagazineCapacity,
                Operation = ModifierOperation.Flat,
                Value = definition.magazineSize,
                applyToUnit = false,
                showInTooltip = true,
            });
            ar.Add(new ItemStatEntry()
            {
                StatId = StatId.Durability,
                Operation = ModifierOperation.Flat,
                Value = item.Physical.maxDurability,
                applyToUnit = false,
                showInTooltip = true,
            });

            return ar;
        }

        
        public override void CollectDescriptors(List<DescriptorDisplayEntry> list)
        {
            list.Add(new(DescriptorId.WeaponType, 
                info.weaponType, ValueType.Enum));
            list.Add(new(DescriptorId.ExtraValue, 
                new ExtraStatValueEntry(StatId.Damage, definition.damage), ValueType.Custom));
        }


        public override void BuildTooltip(ref TooltipItemDto data)
        {
            data.stats = BaseStats();

            data.descriptors = new[]
            {
                new DescriptorDisplayEntry()
                {
                    DescriptorId = DescriptorId.WeaponType,
                    RawValue = info.weaponType,
                    ValueType = ValueType.Enum
                }
            };
        }


        public override CompareStat StatCompare(StatId toCompare, float value)
        {
            return toCompare switch
            {
                StatId.Damage => value > definition.damage ? CompareStat.More : CompareStat.Less,
                StatId.Accuracy => value > definition.GetAccuracyScore() ? CompareStat.More : CompareStat.Less,
                StatId.DamagePerSec => value > DPS ? CompareStat.More : CompareStat.Less,
                StatId.AttackRange => value > definition.range ? CompareStat.More : CompareStat.Less,
                StatId.MagazineCapacity => value > definition.magazineSize ? CompareStat.More : CompareStat.Less,
                StatId.Durability => value > item.Physical.maxDurability ? CompareStat.More : CompareStat.Less,
                
                _=> CompareStat.Fail
            };
        }


        public (StatId, List<RuntimeId>) LinkedItems()
        {
            // используемые боприпасы
            var ammoIds = GameContent.Ammo
                .GetByCaliber(definition.supportedAmmo.Id)
                .Select(ammo => ammo.Id)
                .ToList();

            return (StatId.LinkedAmmo, ammoIds);
        }

    }
    
    
    [System.Serializable]
    public class WeaponInfo
    {
        [Tooltip("Тип оружия: пистолет, винтовка, лук и т.д.")]
        public WeaponType weaponType;
        public WeaponSystem weaponSystem;

        [Tooltip("Тип боеприпасов, если применимо.")]
        public AmmoType ammoType;

        [Tooltip("Требует ли две руки для использования.")]
        public bool isTwoHanded;

        [Tooltip("Можно ли ставить модификации (модули, прицелы, приклады).")]
        public bool canBeModified;

    }
}