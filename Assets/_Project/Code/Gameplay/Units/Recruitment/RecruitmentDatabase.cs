using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Meta.Configs.Recruitment
{
    /// <summary>
    /// База доступных юнитов для найма.
    /// </summary>
    [CreateAssetMenu(
        fileName = "RecruitmentDatabase",
        menuName = "Game Configs/Recruitment/Recruitment Database")]
    public sealed class RecruitmentDatabase : ScriptableObject
    {
        [field: SerializeField] public List<UnitArchetypeConfig> CommonArchetypes { get; private set; }
        [field: SerializeField] public List<UnitArchetypeConfig> ExperiencedArchetypes { get; private set; }
        [field: SerializeField] public List<SpecialistArchetypeConfig> SpecialistArchetypes { get; private set; }

    }
}