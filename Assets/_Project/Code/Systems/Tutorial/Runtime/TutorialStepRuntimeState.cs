using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    /// <summary>
    /// Транзиентное состояние активного шага. Владеет lifecycle объективов (Start/Stop)
    /// И, независимо, lifecycle guidance-условий (см. TutorialGuidanceRuntimeState).
    /// Objectives решают "завершён ли шаг", Guidance решает "что сейчас показать" — эти
    /// два механизма читают одни и те же query/event источники, но никак не влияют друг
    /// на друга (см. TutorialGuidanceDefinition докстринг про их разделение).
    ///
    /// Start() НИКОГДА не даёт синхронному завершению объектива рекурсивно всплыть
    /// в TutorialService изнутри самого себя — возвращает bool "уже завершён после
    /// Start()", а не поднимает событие в этом случае. Событие OnStepCompleted
    /// используется ТОЛЬКО для асинхронного завершения (реальное игровое событие,
    /// пришедшее позже, с собственного call stack через EventBus). Guidance использует
    /// тот же приём для своего начального резолва — см. CurrentGuidanceTarget.
    /// </summary>
    public sealed class TutorialStepRuntimeState
    {
        public readonly TutorialStepDefinition Definition;
        public readonly IReadOnlyList<TutorialObjectiveRuntimeState> Objectives;
        private readonly ObjectiveCompositionMode _mode;
        private readonly TutorialGuidanceRuntimeState _guidance;

        public event Action OnStepCompleted;
        public event Action OnProgressChanged;

        /// <summary>Резолвнутый guidance target поменялся — вызывающий код (TutorialService)
        /// обязан перерисовать presentation. Никак не связано с завершением шага; может
        /// сработать сколько угодно раз за жизнь шага, в т.ч. ни разу (шаг без guidance,
        /// или guidance, для которого ни одно condition ни разу не поменяло резолв).</summary>
        public event Action OnGuidanceChanged;

        private bool _startInProgress;
        private bool _completedFired;

        public TutorialStepRuntimeState(
            TutorialStepDefinition definition,
            IReadOnlyList<TutorialObjectiveRuntimeState> objectives,
            IReadOnlyList<(ITutorialGuidanceCondition Condition, TutorialGuidanceTarget Target)> guidanceEntries)
        {
            Definition = definition;
            Objectives = objectives;
            _mode = definition.objectives.mode;
            _guidance = new TutorialGuidanceRuntimeState(guidanceEntries);
        }

        public bool IsCompleted => _mode == ObjectiveCompositionMode.All
            ? Objectives.All(o => o.IsCompleted)
            : Objectives.Any(o => o.IsCompleted);

        /// <summary>Текущий резолвнутый guidance target — null означает "нет подходящего
        /// guidance" (валидное состояние; presentation просто не показывает highlight/
        /// arrow/camera от guidance, см. TutorialGuidanceRuntimeState докстринг). Читается
        /// TutorialService сразу после Start() для начального presentation-снэпшота,
        /// дальше — из OnGuidanceChanged.</summary>
        public TutorialGuidanceTarget CurrentGuidanceTarget => _guidance.Current;

        /// <summary>Возвращает true, если шаг уже полностью завершён сразу после
        /// того, как все объективы были запущены — вызывающий код (TutorialService)
        /// сам решает, что делать дальше, а не через рекурсивный колбэк. Guidance
        /// стартует ПОСЛЕ объективов (порядок: Activate → Start Objectives → Resolve
        /// Guidance), но выполняется независимо от результата IsCompleted — даже если
        /// шаг завершится мгновенно и guidance никогда не будет показан вызывающей
        /// стороной, её Start()/Stop() всё равно парны (см. ветку alreadyComplete в
        /// TutorialService.ActivateStep, которая вызывает Stop() сразу же после).</summary>
        public bool Start()
        {
            _startInProgress = true;
            foreach (var o in Objectives)
                o.Objective.Start(OnObjectiveProgressChanged);
            _guidance.Start(() => OnGuidanceChanged?.Invoke());
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
            _guidance.Stop();
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
