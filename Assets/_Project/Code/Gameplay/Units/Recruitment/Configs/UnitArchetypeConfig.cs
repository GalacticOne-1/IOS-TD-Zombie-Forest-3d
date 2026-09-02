
using System.Collections.Generic;
using Galactic1.Core.Enums;
using UnityEngine;

namespace Galactic1.Meta.Configs.Recruitment
{
    /// <summary>
    /// Игровой архетип юнита (геймплейная сущность).
    /// Не содержит имени и портрета.
    /// </summary>
    [CreateAssetMenu(
        fileName = "UnitArchetypeConfig",
        menuName = "Game Configs/Recruitment/Unit Archetype Config")]
    public class UnitArchetypeConfig : ScriptableObject
    {
        //public UnitClass UnitClass;
        //public UnitRole Role;

        //public int[] BaseStats;
        //public List<SkillConfig> DefaultSkills;
        [field: SerializeField] public List<WeaponType> AllowedWeaponTypes { get; private set; }
        [field: SerializeField] public List<EquipSlotType> AllowedArmorTypes { get; private set; }
    }
}