using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Brain.Utility.Core
{
    /// <summary>
    /// Результат Evaluate() — pure, без side-effects.
    ///
    /// Про проблему #3 (generic payload):
    ///   Текущий вариант с MovePosition и Flag — это осознанный компромисс
    ///   пока actions однородны (chase, roam, attack).
    ///
    ///   Когда появятся abilities/combo — переходи к типизированным решениям:
    ///
    ///     interface IAIDecision { float Score; }
    ///     struct ChaseDecision  : IAIDecision { Vector3 SlotPosition; float Speed; }
    ///     struct AttackDecision : IAIDecision { string TargetId; bool IsFinisher; }
    ///
    ///   Пока этого нет — MovePosition и Flag достаточно и не создают проблем.
    ///   Главное правило: не добавлять сюда поля "для конкретного action".
    ///
    /// readonly struct — zero alloc в hot path.
    /// </summary>
    public readonly struct ActionDecision
    {
        public readonly float Score;
        public readonly Vector3 MovePosition;
        public readonly bool Flag;

        public static readonly ActionDecision Zero = new ActionDecision(0f);

        public ActionDecision(float score)
        {
            Score = score;
            MovePosition = Vector3.zero;
            Flag = false;
        }

        public ActionDecision(float score, Vector3 movePosition)
        {
            Score = score;
            MovePosition = movePosition;
            Flag = false;
        }

        public ActionDecision(float score, Vector3 movePosition, bool flag)
        {
            Score = score;
            MovePosition = movePosition;
            Flag = flag;
        }
    }
}