using Galactic1.Code.Gameplay.Units.Brain.Blackboard;
using Galactic1.Game.Meta.Enemy;

namespace Galactic1.Code.Gameplay.Units.Brain.Utility.Core
{
    /// <summary>
    /// Примитив решения Utility AI.
    ///
    /// Контракт:
    ///   Type       — идентификатор для weight lookup в EnemyAIDefinition.
    ///   Evaluate() — PURE. Возвращает raw score [0..1].
    ///                Brain применяет weight снаружи — Action не знает свой вес.
    ///   Execute()  — MUTATION. Только у победителя.
    ///   Dispose()  — очистка ресурсов.
    /// </summary>
    public interface IAIAction
    {
        /// <summary>
        /// Тип action — ключ для lookup в EnemyAIDefinition.TryGetAction().
        /// </summary>
        AIActionType Type { get; }

        /// <summary>
        /// Вычислить raw utility score [0..1].
        /// Weights применяет Brain — Action их не знает.
        /// </summary>
        ActionDecision Evaluate(
            UnitInstance    unit,
            AIContext       context,
            EnemyBlackboard blackboard);

        void Execute(
            UnitInstance    unit,
            AIContext       context,
            EnemyBlackboard blackboard,
            ActionDecision  decision);

        void Dispose() { }
    }
}