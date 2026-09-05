using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Objectives
{
    [CreateAssetMenu(fileName = "Objective_WorldMapLocationSelected", 
        menuName = "Game Configs/Tutorial/Objectives/World Map Location Selected")]
    public sealed class WorldMapLocationSelectedObjectiveDefinition : TutorialObjectiveDefinition
    {
        public override string ObjectiveTypeId => "WorldMapLocationSelected";
        [Tooltip("Пусто = любая локация.")]
        public LocationId locationId;
    }
}
