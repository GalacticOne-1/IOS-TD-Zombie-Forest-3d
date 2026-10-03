using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    [CreateAssetMenu(fileName = "Objective_ContainersLootedCount",
        menuName = "Game Configs/Tutorial/Objectives/Containers Looted (Count)")]
    public sealed class ContainersLootedCountObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "ContainersLootedCount";

        [Min(1)] public int requiredCount = 3;

        [Tooltip("Включено = считаются только контейнеры в рейде на locationId. Выключено = любая локация.")]
        public bool filterByLocation;
        public LocationId locationId;

#if UNITY_EDITOR
        public override bool Validate(out string error)
        {
            if (filterByLocation && locationId == null)
            {
                error = "ContainersLootedCountObjectiveDefinition: filterByLocation включён, но locationId пуст.";
                return false;
            }
            error = null;
            return true;
        }
#endif
    }
}