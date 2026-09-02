using Galactic1.Meta.Configs.Recruitment;

namespace Galactic1.Code.Systems.Runtime
{
    /// <summary>
    /// Генератор стартовой экипировки рекрута.
    /// </summary>
    public interface IRecruitEquipmentGenerator : IGameService
    {
        RecruitEquipmentLoadout Generate(
            RecruitCategory category,
            UnitArchetypeConfig archetype,
            int level);
    }
}