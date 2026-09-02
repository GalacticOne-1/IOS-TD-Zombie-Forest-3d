using UnityEngine;

namespace Galactic1.Meta.Configs.Recruitment
{
    /// <summary>
    /// Архетип специалиста.
    /// Определяет базовую боевую роль и набор скиллов.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SpecialistArchetypeConfig",
        menuName = "Game Configs/Recruitment/Specialis Archetype Config")]
    public class SpecialistArchetypeConfig : ScriptableObject
    {
        public string id;

        public UnitArchetypeConfig baseArchetype;

        public int minLevel;
        public int maxLevel;

        //public List<SkillConfig> guaranteedSkills;
        //public List<SkillConfig> bonusSkillPool;

        public int hardCurrencyCost;

        public float weight; // для weighted RNG
    }
}