
using System.Collections.Generic;
using System.Linq;
using Galactic1.Core.Enums;
using UnityEngine;

namespace Galactic1.Meta.Configs.Recruitment
{
    /// <summary>
    /// Ограничения доступа к предметам для категорий рекрутов.
    /// </summary>
    [CreateAssetMenu(
        fileName = "RecruitEquipmentAccessConfig",
        menuName = "Game Configs/Recruitment/Recruit Equipment Access Config")]
    public sealed class RecruitEquipmentAccessConfig : ScriptableObject
    {
        [field: SerializeField] public List<EquipmentTierRule> Rules { get; private set; }
        
    }

    [System.Serializable]
    public sealed class EquipmentTierRule
    {
        [Header("Category")]
        public RecruitCategory Category;

        [Header("Tier Limits")]
        public int MaxWeaponTier;
        public int MaxArmorTier;
        
        [Header("Armor Drop Rules")]
        [SerializeField]
        private List<ArmorSlotDropRule> armorDropRules;

        [Header("Weapon Durability Range")]
        [Min(0)] public int WeaponDurabilityMin;
        [Min(0)] public int WeaponDurabilityMax;

        [Header("Armor Durability Range")]
        [Min(0)] public int ArmorDurabilityMin;
        [Min(0)] public int ArmorDurabilityMax;
        
        
        
        public float GetDropChance(EquipSlotType slot)
        {
            var rule = armorDropRules.FirstOrDefault(r => r.Slot == slot);
            return rule != null ? rule.DropChance : 1f;
        }
    }
    
    [System.Serializable]
    public sealed class ArmorSlotDropRule
    {
        public EquipSlotType Slot;

        [Range(0f, 1f)]
        public float DropChance = 1f;
    }
}