using System.Collections.Generic;
using System.Linq;
using Galactic1.Game.Buildings.Proxy;
using Galactic1.Meta.Configs.Recruitment;

namespace Galactic1.Code.Systems.Runtime.Recruitment
{
    /// <summary>
    /// Генерация специалиста.
    /// Имеет собственную логику баланса и редкости.
    /// </summary>
    public sealed class SpecialistGenerationPipeline : IRecruitGenerationPipeline
    {
        private readonly List<SpecialistArchetypeConfig> _archetypeList;
        private readonly IIdentityGenerator _identity;
        private readonly IWeightedRandomService _rng;
        private readonly IRecruitEquipmentGenerator _equipmentGenerator;
        private readonly RecruitmentSettingsConfig _settings;

        public SpecialistGenerationPipeline(
            List<SpecialistArchetypeConfig> archetypeList,
            IIdentityGenerator identity,
            IWeightedRandomService rng,
            IRecruitEquipmentGenerator equipmentGenerator,
            RecruitmentSettingsConfig settings)
        {
            _archetypeList = archetypeList;
            _identity = identity;
            _rng = rng;
            _equipmentGenerator = equipmentGenerator;
            _settings = settings;
            
        }

        public RecruitOfferData Generate()
        {
            // var archetype = _rng.PickWeighted(
            //     _archetypeList,
            //     s => s.weight);
            //
            // int level = _rng.Range(
            //     archetype.minLevel,
            //     archetype.maxLevel + 1);
            //
            // var identity = _identity.Generate();
            //
            // var skills = new List<SkillConfig>(archetype.guaranteedSkills);
            //
            // if (archetype.bonusSkillPool != null &&
            //     archetype.bonusSkillPool.Count > 0)
            // {
            //     var bonus = archetype.bonusSkillPool[
            //         _rng.Range(0, archetype.bonusSkillPool.Count)];
            //
            //     skills.Add(bonus);
            // }
            //
            // return RecruitOfferRuntime.CreateSpecialist(
            //     archetype.baseArchetype,
            //     identity,
            //     level,
            //     skills,
            //     archetype.hardCurrencyCost);
            return null;
        }
    }
}