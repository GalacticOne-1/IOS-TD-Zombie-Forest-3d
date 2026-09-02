using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Zombie
{
    /// <summary>
    /// Команда погони за конкретной целью к конкретному слоту окружения.
    /// Brain пересчитывает SlotPosition каждый тик и выдаёт новую ChaseCommand.
    /// ChasingState просто обновляет destination если команда пришла повторно.
    /// </summary>
    public sealed class ChaseCommand : IUnitCommand
    {
        public UnitStateId TargetState => UnitStateId.Chasing;

        public readonly string TargetId;
        public readonly Vector3 SlotPosition; // мировая позиция слота окружения
        public readonly float Speed;

        public ChaseCommand(string targetId, Vector3 slotPosition, float speed)
        {
            TargetId = targetId;
            SlotPosition = slotPosition;
            Speed = speed;
        }

        public bool CanExecute(UnitStateId currentState)
        {
            return currentState != UnitStateId.Dying
                   && currentState != UnitStateId.Dead
                   && currentState != UnitStateId.MeleeEngaging;
        }
    }
}