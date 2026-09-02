using System;
using Galactic1.Code.Items;
using Galactic1.Core.Enums;
using Galactic1.Game.UI.Stats.DTO;
using ValueType = Galactic1.Code.Items.ValueType;

namespace Galactic1.Game.UI.Stats
{
    public class StatValueResolver
    {


        public static (string, string) Resolve(DescriptorDisplayEntry entry, out StatId statId)
        {
            statId = 0;
            if (entry.RawValue == null)
                return ("", "");

            return entry.ValueType switch
            {
                ValueType.String => ("", (string)entry.RawValue),
                ValueType.Int => ("", ((int)entry.RawValue).ToString()),
                ValueType.Float => ("", ((float)entry.RawValue).ToString("0.##")),
                ValueType.Bool => ("", ((bool)entry.RawValue) ? "Yes" : "No"),
                //ValueType.Enum => ResolveEnum(entry.RawValue),
                ValueType.Enum => ("", ResolveEnum(entry.DescriptorId, entry.RawValue)),
                ValueType.Custom => ResolveExtra(entry, out statId),

                _ => ("", "")
            };
        }
        
        
        

        /// <summary>
        /// Возвращает enum как строку + сохраняет реальный тип.
        /// </summary>
        // private static string ResolveEnum(object rawValue)
        // {
        //     var enumType = rawValue.GetType();
        //
        //     if (!enumType.IsEnum)
        //         return string.Empty;
        //
        //     return Enum.GetName(enumType, rawValue);
        // }

        /// <summary>
        /// Позволяет безопасно получить enum конкретного типа.
        /// </summary>
        public static bool TryGetEnum<TEnum>(
            DescriptorDisplayEntry entry,
            out TEnum value)
            where TEnum : struct, Enum
        {
            if (entry.ValueType == ValueType.Enum &&
                entry.RawValue is TEnum typed)
            {
                value = typed;
                return true;
            }

            value = default;
            return false;
        }

        
        static (string, string) ResolveExtra(DescriptorDisplayEntry entry, out StatId statId)
        {
            if (entry.RawValue is ExtraStatValueEntry valueEntry)
            {
                statId = valueEntry.StatId;
                var style = StatStyleResolver.Resolve(valueEntry.StatId);
                var label = style?.localizationKey ?? valueEntry.StatId.ToString();

                var value = valueEntry.StatId == StatId.Tier || valueEntry.StatId == StatId.Level
                    ? valueEntry.Value + 1
                    : valueEntry.Value;

                return (label, $"{value}");
            }
            
            if (entry.RawValue is ExtraStatEnumEntry enumEntry)
            {
                statId = enumEntry.StatId;
                var style = StatStyleResolver.Resolve(enumEntry.StatId);
                var label = style?.localizationKey ?? enumEntry.StatId.ToString();

                return (label, $"{ResolveEnum(entry.DescriptorId, enumEntry.RawEnum)}");
            }

            statId = 0;
            return ("", "");
        }

        static string ResolveEnum(DescriptorId descriptorId, object rawValue)
        {
            return descriptorId switch
            {
                DescriptorId.Rarity => Rarity(rawValue),
                DescriptorId.WeaponType => Weapon(rawValue),
                DescriptorId.AmmoType => Ammo(rawValue),
                DescriptorId.ArmorType => Armor(rawValue),

                _ => ""
            };
        }


        static string Rarity(object rawValue)
        {
            if(rawValue is ItemRarity rarity)
            {
                return rarity switch
                {
                    ItemRarity.Common => "Common",
                    ItemRarity.Uncommon => "Uncommon",
                    ItemRarity.Rare => "Rare",
                    ItemRarity.Epic => "Epic",
                    ItemRarity.Legendary => "Legendary",
                    ItemRarity.Artifact => "Artifact",
                    _ => ""
                };
            }

            return "";
        }
        
        static string Weapon(object rawValue)
        {
            if (rawValue is WeaponType weaponType)
            {
                return weaponType switch
                {
                    WeaponType.Pistol => "Pistol",
                    WeaponType.Rifle => "Rifle",
                    WeaponType.AR => "Assault Rifle",
                    WeaponType.DMR => "Designated Marksman Rifle",
                    WeaponType.SniperRifle => "Sniper Rifle",
                    WeaponType.Shotgun => "Shotgun",
                    WeaponType.SMG => "Submachine Gun",
                    WeaponType.LMG => "Heavy Machine Gun",
                    WeaponType.ExplosiveRocketLauncher => "Explosive Rocket Launcher",
                    WeaponType.Grenade => "Grenade",
                    _ => ""
                };
            }

            return "";
        }
        
        static string Armor(object rawValue)
        {
            if (rawValue is EquipSlotType type)
            {
                return type switch
                {
                    EquipSlotType.Head => "Head Armor",
                    EquipSlotType.Torso => "Torso Armor",
                    _ => ""
                };
            }

            return "";
        }
        
        static string Ammo(object rawValue)
        {
            if(rawValue is AmmoType ammoType)
            {
                return ammoType switch
                {
                    AmmoType.Bullets => "Bullets",
                    AmmoType.Shells => "Shells",
                    AmmoType.Energy => "Energy",
                    _ => ""
                };
            }

            return "";
        }
    }
}