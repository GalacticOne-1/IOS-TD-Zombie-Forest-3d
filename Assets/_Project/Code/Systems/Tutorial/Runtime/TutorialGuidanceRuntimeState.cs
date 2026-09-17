using System;
using System.Collections.Generic;
using Galactic1.Code.Systems.Tutorial.Presentation;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    /// <summary>
    /// Владеет lifecycle guidance-условий одного активного шага и резолвит, какой
    /// guidance-target сейчас применим. НЕ решает, завершён ли шаг — это исключительно
    /// Objectives (см. TutorialStepRuntimeState докстринг); guidance и completion — два
    /// независимых, никак не связанных друг с другом механизма над одним и тем же набором
    /// game-state запросов.
    ///
    /// Резолв — чистая функция текущего состояния (Resolve() перебирает entries в authoring-
    /// порядке, первый IsSatisfied()==true побеждает), НЕ конечный автомат: нет понятия
    /// "текущая стадия N" ни в памяти сверх Current, ни тем более в персисте — после
    /// Restore/Restart Start() резолвит Current заново с нуля из live game state (см.
    /// TutorialGuidanceDefinition докстринг).
    ///
    /// _startInProgress подавляет OnChanged во время Start() по тому же паттерну, что
    /// TutorialStepRuntimeState использует для объективов: инициализация не должна
    /// синхронно всплывать как "изменение" колбэку, который ещё не успел подписаться —
    /// вызывающий код читает Current напрямую сразу после Start() вместо этого.
    /// </summary>
    public sealed class TutorialGuidanceRuntimeState
    {
        private readonly IReadOnlyList<(ITutorialGuidanceCondition Condition, TutorialGuidanceTarget Target)> _entries;
        private Action _onChanged;
        private bool _startInProgress;

        /// <summary>Текущий резолвнутый target. Null — валидное состояние "ни одно
        /// condition не подошло" (fallback это "ничего не показывать", не "показать
        /// что-то наугад").</summary>
        public TutorialGuidanceTarget Current { get; private set; }

        public TutorialGuidanceRuntimeState(
            IReadOnlyList<(ITutorialGuidanceCondition Condition, TutorialGuidanceTarget Target)> entries)
        {
            _entries = entries ?? Array.Empty<(ITutorialGuidanceCondition, TutorialGuidanceTarget)>();
        }

        public void Start(Action onChanged)
        {
            _onChanged = onChanged;
            _startInProgress = true;
            foreach (var e in _entries)
                e.Condition.Start(Reevaluate);
            _startInProgress = false;

            // Финальный резолв ВСЕГДА выполняется явно здесь, а не только в ответ на
            // Reevaluate — условия, которые уже сейчас удовлетворены (например, шаг
            // активировался, когда Inventory уже открыт), не обязаны сами "стрельнуть"
            // событием при Start(), чтобы попасть в начальный Current.
            Current = Resolve();
        }

        public void Stop()
        {
            foreach (var e in _entries)
                e.Condition.Stop();
            _onChanged = null;
            Current = null;
        }

        private void Reevaluate()
        {
            if (_startInProgress) return; // финальный Resolve() в конце Start() и так покроет это

            var resolved = Resolve();
            if (ReferenceEquals(resolved, Current)) return; // без изменений — не дёргаем presentation зря

            Current = resolved;
            _onChanged?.Invoke();
        }

        private TutorialGuidanceTarget Resolve()
        {
            foreach (var e in _entries)
            {
                if (e.Condition.IsSatisfied())
                    return e.Target;
            }
            return null;
        }
    }
}
