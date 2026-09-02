using Galactic1.Code.Gameplay.Interaction;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    public sealed class MoveCommand : IUnitCommand
    {
        public Vector3 Destination { get; }
        public WorldInputDispatcher.MoveMode MoveMode { get; }
        public UnitStateId TargetState => UnitStateId.SquadMoving;

        public MoveCommand(Vector3 destination, WorldInputDispatcher.MoveMode mode)
        {
            Destination = destination;
            MoveMode = mode;
            GConsole.ClearLog();
        }

        public bool CanExecute(UnitStateId s) =>
            s != UnitStateId.Suppressed &&
            s != UnitStateId.Dying &&
            s != UnitStateId.Dead;
    }
}