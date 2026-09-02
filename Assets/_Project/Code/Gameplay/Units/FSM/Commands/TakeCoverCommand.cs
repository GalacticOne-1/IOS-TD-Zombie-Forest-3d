using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    public sealed class TakeCoverCommand : IUnitCommand
    {
        public UnitStateId TargetState => UnitStateId.TakingCover;
        public Vector3 CoverPosition { get; }

        public TakeCoverCommand(Vector3 pos)
        {
            CoverPosition = pos;
        }

        public bool CanExecute(UnitStateId s) => s is
            UnitStateId.Idle or
            UnitStateId.SquadMoving or
            UnitStateId.Engaging;
    }
}