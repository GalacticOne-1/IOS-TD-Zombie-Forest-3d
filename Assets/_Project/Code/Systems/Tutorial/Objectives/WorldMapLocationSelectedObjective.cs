using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>Требует WorldMapLocationSelectedEvent — интеграционная точка,
    /// одна строка в WorldMapController.OnNodeClicked, см. Integration/.</summary>
    public sealed class WorldMapLocationSelectedObjective : TutorialEventObjectiveBase<WorldMapLocationSelectedEvent>
    {
        private readonly LocationId _locationId; // null = любая локация

        public WorldMapLocationSelectedObjective(LocationId locationId) => _locationId = locationId;

        protected override bool EvaluateEvent(WorldMapLocationSelectedEvent e)
            => _locationId == null || e.LocationId == _locationId;
    }
}
