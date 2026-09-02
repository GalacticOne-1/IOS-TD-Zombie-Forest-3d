using System.Collections.Generic;

namespace Galactic1.Code.Gameplay.Construction.States
{
    /// <summary>
    /// Фабрика состояний режима строительства
    /// Создаёт состояния один раз и переиспользует их.
    /// </summary>
    public class ConstructionStateFactory
    {
        private readonly Dictionary<ConstructionStateType, IConstructionState> _states;

        public ConstructionStateFactory(
            IdleState idle,
            SelectedObjectState selected,
            MovingObjectState moving,
            PlacingGhostState placing)
        {
            _states = new Dictionary<ConstructionStateType, IConstructionState>
            {
                { ConstructionStateType.Idle, idle },
                { ConstructionStateType.SelectedObject, selected },
                { ConstructionStateType.MovingObject, moving },
                { ConstructionStateType.PlacingGhost, placing }
            };
        }

        public IConstructionState Get(ConstructionStateType type)
        {
            return _states[type];
        }
    }
}