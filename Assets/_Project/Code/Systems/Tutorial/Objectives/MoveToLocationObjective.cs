using System;
using Galactic1.Code.Systems.Squad;
using Galactic1.Code.Systems.Tutorial.Runtime;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// State-семантика: "отряд сейчас стоит (SquadState.Idle) в пределах radius от
    /// targetPosition" — раздел 8/37 ТЗ Chapter 2. НЕ управляет движением — только
    /// наблюдает существующий SquadMovementSystem.OnMovementFinished (новое минимальное
    /// событие, см. её докстринг) как триггер перепроверки, тот же принцип, что
    /// TutorialStateRecheckObjectiveBase использует для EventBus-событий, но здесь прямая
    /// C#-подписка на runtime-объект (тот же паттерн, что DomainTransitionObjective
    /// использует для IGameLoopStateQuery.OnDomainTransition), поэтому не наследуется от
    /// TutorialStateRecheckObjectiveBase<TEvent> (та жёстко завязана на EventBus<T>).
    ///
    /// SquadController/SquadMovementSystem — scene-scoped (создаются на сцене рейда, не
    /// существуют в момент конструирования TutorialObjectiveFactory при старте приложения)
    /// — поэтому НЕ принимаются через конструктор от фабрики, а резолвятся lazy через
    /// ServiceLocator в Start(), тот же приём, что TutorialUnitSlotTargetProvider/
    /// ConstructionFacilitySlotTargetProvider используют для scene-сервисов.
    ///
    /// Использует UnityEngine.Vector3 — оправданное исключение из общего правила "Runtime
    /// не ссылается на Unity": пространственная проверка позиции неотделима от Vector3,
    /// та же ситуация, что у SquadSceneRuntime/SquadMovementSystem (Systems-слой), не
    /// нарушает границу Authoring/Runtime/Scene, о которой говорит это правило.
    /// </summary>
    public sealed class MoveToLocationObjective : ITutorialObjective
    {
        private readonly Vector3 _targetPosition;
        private readonly float _radius;

        private SquadMovementSystem _movement;
        private SquadSceneRuntime _squad;
        private Action _onProgressChanged;

        public bool IsCompleted { get; private set; }

        public MoveToLocationObjective(Vector3 targetPosition, float radius)
        {
            _targetPosition = targetPosition;
            _radius = radius;
        }

        public void Start(Action onProgressChanged)
        {
            _onProgressChanged = onProgressChanged;

            var squadController = ServiceLocator.Current.Get<SquadController>();
            _movement = squadController?.MovementSystem;
            _squad = squadController?.Squad;

            if (_movement == null || _squad == null)
            {
                Debug.LogError("[MoveToLocationObjective] SquadController.MovementSystem/Squad " +
                                "недоступны при активации шага — объектив не сможет завершиться.");
                return;
            }

            if (EvaluateCurrentState())
            {
                IsCompleted = true;
                _onProgressChanged?.Invoke();
                return;
            }

            _movement.OnMovementFinished += OnMovementFinished;
        }

        public void Stop()
        {
            if (_movement != null)
                _movement.OnMovementFinished -= OnMovementFinished;
            _onProgressChanged = null;
        }

        private void OnMovementFinished()
        {
            if (IsCompleted) return;
            if (EvaluateCurrentState())
            {
                IsCompleted = true;
                _onProgressChanged?.Invoke();
            }
        }

        public bool EvaluateCurrentState()
            => _squad != null
               && _squad.State == SquadState.Idle
               && Vector3.Distance(_squad.ComputeMassCenter(), _targetPosition) <= _radius;

        public bool EvaluateEvent(object gameplayEvent) => false;

        public bool TryGetProgress(out int current, out int required)
        {
            current = 0;
            required = 0;
            return false;
        }
    }
}
