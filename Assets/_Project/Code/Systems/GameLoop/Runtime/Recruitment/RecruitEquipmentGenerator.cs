using System.Collections.Generic;
using System.Linq;
using Galactic1.Core.Enums;
using Galactic1.Items;
using Galactic1.Meta.Configs.Recruitment;
using UnityEngine;

namespace Galactic1.Code.Systems.Runtime
{

    /// <summary>
    /// Генерирует стартовую экипировку рекрута
    /// с учётом категории и tier-ограничений.
    /// </summary>
    public sealed class RecruitEquipmentGenerator : IRecruitEquipmentGenerator
    {
        private readonly ItemDatabase _itemDatabase;
        private readonly RecruitmentDatabase _database;
        private readonly RecruitEquipmentAccessConfig _accessConfig;
        private readonly IWeightedRandomService _rng;
        
        
        private static readonly EquipSlotType[] ArmorSlotOrder =
        {
            EquipSlotType.Head,
            EquipSlotType.Torso,
            EquipSlotType.Pants,
            EquipSlotType.Boots
        };
        

        public RecruitEquipmentGenerator(
            ItemDatabase itemDatabase,
            RecruitmentDatabase database,
            RecruitEquipmentAccessConfig accessConfig,
            IWeightedRandomService rng)
        {
            _itemDatabase = itemDatabase;
            _database = database;
            _accessConfig = accessConfig;
            _rng = rng;
        }

        // генератор для одного юнита
        public RecruitEquipmentLoadout Generate(
            RecruitCategory category,
            UnitArchetypeConfig archetype,
            int level)
        {
            var rule = _accessConfig.Rules
                .First(r => r.Category == category);

            // === получаем список доступного оружия для категории выжившего
            var allowedWeapons = _itemDatabase.GetAllWeapons()
                .Where(w =>
                    w.RecruitAccess.tier <= rule.MaxWeaponTier &&
                    w.RecruitAccess.allowedCategories.Contains(category) &&
                    archetype.AllowedWeaponTypes.Contains(w.Weapon.Info.weaponType))
                .ToList();

            // === получаем список доступной защиты для категории выжившего
            var allowedArmor = _itemDatabase.GetAllArmors()
                .Where(a =>
                    a.RecruitAccess.tier <= rule.MaxArmorTier &&
                    a.RecruitAccess.allowedCategories.Contains(category) &&
                    archetype.AllowedArmorTypes.Contains(a.GetEquipSlot()))
                .ToList();

            var weapon = allowedWeapons.Count > 0
                ? _rng.PickWeighted(allowedWeapons, w => w.RecruitAccess.weight)
                : null;


            var weaponDurability = weapon.Physical.maxDurability;
            var range = Random.Range(rule.WeaponDurabilityMin, rule.WeaponDurabilityMax + 1) / 100f;
            weaponDurability = (int)(weaponDurability * range);
            var weaponLoadout = new RecruitEquipmentLoadout.RecruitEquipmentLoadoutBox
            {
                Id = weapon?.Id.Guid,
                Durability = weaponDurability
            };

            var armorLoadoutList = new List<RecruitEquipmentLoadout.RecruitEquipmentLoadoutBox>();

            foreach (var slot in ArmorSlotOrder)
            {
                armorLoadoutList.Add(new RecruitEquipmentLoadout.RecruitEquipmentLoadoutBox
                {
                    Id = null
                });
                
                // Проверка шанса выпадения
                if (_rng.Value01() > rule.GetDropChance(slot))
                    continue;
                
                var slotItems = allowedArmor
                    .Where(a => a.GetEquipSlot() == slot)
                    .ToList();

                if (slotItems.Count == 0)
                    continue;

                var picked = _rng.PickWeighted(slotItems, x => x.RecruitAccess.weight);

                var armorDurability = picked.Physical.maxDurability;
                range = Random.Range(rule.ArmorDurabilityMin, rule.ArmorDurabilityMax + 1) / 100f;
                armorDurability = (int)(armorDurability * range);
                armorLoadoutList[armorLoadoutList.Count - 1] = new RecruitEquipmentLoadout.RecruitEquipmentLoadoutBox
                {
                    Id = picked.Id.Guid,
                    Durability = armorDurability
                };
            }

            return new RecruitEquipmentLoadout(
                weaponLoadout,
                armorLoadoutList
            );
        }

    }
}