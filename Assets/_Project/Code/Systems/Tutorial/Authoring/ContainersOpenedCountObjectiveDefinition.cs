using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    [CreateAssetMenu(fileName = "Objective_ContainersOpenedCount",
        menuName = "Game Configs/Tutorial/Objectives/Containers Opened (Count)")]
    public sealed class ContainersOpenedCountObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "ContainersOpenedCount";

        [Min(1)] public int requiredCount = 3;

        [Tooltip("Включено = считаются только контейнеры, открытые в рейде на locationId.")]
        public bool filterByLocation;
        public LocationId locationId;

#if UNITY_EDITOR
        public override bool Validate(out string error)
        {
            if (filterByLocation && locationId == null)
            {
                error = "ContainersOpenedCountObjectiveDefinition: filterByLocation включён, но locationId пуст.";
                return false;
            }
            error = null;
            return true;
        }
#endif
    }
}