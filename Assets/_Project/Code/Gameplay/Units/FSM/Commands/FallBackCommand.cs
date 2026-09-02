using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    public sealed class FallBackCommand : IUnitCommand
    {
        public UnitStateId TargetState => UnitStateId.SquadMoving;
        public Vector3 Destination { get; }

        public FallBackCommand(Vector3 dest)
        {
            Destination = dest;
        }

        // FallBack разрешён почти из любого состояния
        public bool CanExecute(UnitStateId s) => s is not
            (UnitStateId.Dying or UnitStateId.Dead);
    }
}