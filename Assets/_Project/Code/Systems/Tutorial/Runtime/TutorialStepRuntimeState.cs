using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    /// <summary>
    /// Транзиентное состояние активного шага. Владеет lifecycle объективов (Start/Stop).
    ///
    /// Start() НИКОГДА не даёт синхронному завершению объектива рекурсивно всплыть
    /// в TutorialService изнутри самого себя — возвращает bool "уже завершён после
    /// Start()", а не поднимает событие в этом случае. Событие OnStepCompleted
    /// используется ТОЛЬКО для асинхронного завершения (реальное игровое событие,
    /// пришедшее позже, с собственного call stack через EventBus).
    /// </summary>
    public sealed class TutorialStepRuntimeState
    {
        public readonly TutorialStepDefinition Definition;
        public readonly IReadOnlyList<TutorialObjectiveRuntimeState> Objectives;
        private readonly ObjectiveCompositionMode _mode;

        public event Action OnStepCompleted;
        public event Action OnProgressChanged;

        private bool _startInProgress;
        private bool _completedFired;

        public TutorialStepRuntimeState(
            TutorialStepDefinition definition,
            IReadOnlyList<TutorialObjectiveRuntimeState> objectives)
        {
            Definition = definition;
            Objectives = objectives;
            _mode = definition.objectives.mode;
        }

        public bool IsCompleted => _mode == ObjectiveCompositionMode.All
            ? Objectives.All(o => o.IsCompleted)
            : Objectives.Any(o => o.IsCompleted);

        /// <summary>Возвращает true, если шаг уже полностью завершён сразу после
        /// того, как все объективы были запущены — вызывающий код (TutorialService)
        /// сам решает, что делать дальше, а не через рекурсивный колбэк.</summary>
        public bool Start()
        {
            _startInProgress = true;
            foreach (var o in Objectives)
                o.Objective.Start(OnObjectiveProgressChanged);
            _startInProgress = false;

            if (IsCompleted && !_completedFired)
            {
                _completedFired = true;
                return true;
            }

            return false;
        }

        public void Stop()
        {
            foreach (var o in Objectives)
                o.Objective.Stop();
        }

        private void OnObjectiveProgressChanged()
        {
            if (_completedFired) return;
            if (!IsCompleted) return;
            if (_startInProgress) return; // Start() сам синхронно обработает финальное состояние

            OnProgressChanged?.Invoke();

            if (!IsCompleted) return;

            _completedFired = true;
            OnStepCompleted?.Invoke();
        }

        /// <summary>Прогресс шага для generic Scenario Task layer — см. ITutorialObjective.
        /// TryGetProgress. Экспонируется ТОЛЬКО когда в группе ровно один объектив: для
        /// нескольких разнородных объективов (ALL/ANY) нет единого корректного
        /// current/required без знания семантики конкретных типов, а генерик-слой обязан
        /// оставаться objective-type-agnostic. Ограничение намеренное и задокументированное,
        /// не забытый недосмотр — см. финальный отчёт, "Multi-objective progress".</summary>
        public bool TryGetProgress(out int current, out int required)
        {
            current = 0;
            required = 0;
            return Objectives.Count == 1 && Objectives[0].Objective.TryGetProgress(out current, out required);
        }
    }
}
