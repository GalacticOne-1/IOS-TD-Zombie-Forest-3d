
using UnityEngine;
using Galactic1.Meta.Configs.Recruitment;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    [CreateAssetMenu(fileName = "Objective_RecruitCompleted",
        menuName = "Game Configs/Tutorial/Objectives/Recruit Completed")]
    public sealed class RecruitCompletedObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "RecruitCompleted";

        [Tooltip("Если указано — засчитывается только найм этой категории (Common/Experienced/Specialist). " +
                 "Не задано (значение по умолчанию enum'а) = любая категория — см. Validate, если нужно " +
                 "различать 'не задано' от 'Common' явно, понадобится nullable-обёртка.")]
        public RecruitCategory requiredCategory = RecruitCategory.Common;

        public bool anyCategory = true;   // явный флаг, раз enum не nullable без доп. обёртки
    }
}