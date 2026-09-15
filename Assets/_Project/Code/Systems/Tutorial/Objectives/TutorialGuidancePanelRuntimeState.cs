using System;
using System.Collections.Generic;
using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    /// <summary>Параллельный (не влияющий на TutorialGuidanceRuntimeState) резолвер
    /// overlay-панелей. Тот же "first satisfied wins" принцип поверх ТОГО ЖЕ порядка
    /// entries, что и у highlight-резолвера — но отдельный instance каждого condition
    /// (см. TutorialStepRuntimeState.Start про двойной Start() на разных инстансах).
    ///
    /// oneShot-панели трекаются по индексу entry в _consumedOneShot — набор живёт ровно
    /// столько же, сколько сам TutorialStepRuntimeState (то есть один шаг); при следующей
    /// активации того же stepId (новый шаг graph-навигации или Restore) состояние всегда
    /// с нуля — тот же принцип "guidance не персистится", что у TutorialGuidanceRuntimeState.
    /// </summary>
    public sealed class TutorialGuidancePanelRuntimeState
    {
        private readonly IReadOnlyList<(ITutorialGuidanceCondition Condition, TutorialGuidancePanelDefinition Panel)> _entries;
        private readonly HashSet<int> _consumedOneShot = new();
        private Action _onChanged;
        private bool _startInProgress;
        private int _currentIndex = -1;

        /// <summary>Текст текущей видимой панели. Null = ничего не показывать.</summary>
        public string CurrentText { get; private set; }

        public TutorialGuidancePanelRuntimeState(
            IReadOnlyList<(ITutorialGuidanceCondition, TutorialGuidancePanelDefinition)> entries)
        {
            _entries = entries ?? Array.Empty<(ITutorialGuidanceCondition, TutorialGuidancePanelDefinition)>();
        }

        public void Start(Action onChanged)
        {
            _onChanged = onChanged;
            _startInProgress = true;
            foreach (var e in _entries)
                e.Condition.Start(Reevaluate);
            _startInProgress = false;

            (_currentIndex, CurrentText) = Resolve();
        }

        public void Stop()
        {
            foreach (var e in _entries)
                e.Condition.Stop();
            _onChanged = null;
            CurrentText = null;
            _currentIndex = -1;
        }

        private void Reevaluate()
        {
            if (_startInProgress) return;

            var (index, text) = Resolve();
            if (text == CurrentText) return;

            _currentIndex = index;
            CurrentText = text;
            _onChanged?.Invoke();
        }

        private (int index, string text) Resolve()
        {
            for (int i = 0; i < _entries.Count; i++)
            {
                if (_entries[i].Panel.oneShot && _consumedOneShot.Contains(i))
                    continue;
                if (_entries[i].Condition.IsSatisfied())
                    return (i, _entries[i].Panel.text);
            }
            return (-1, null);
        }

        /// <summary>Игрок кликнул по панели — закрываем. Для oneShot помечаем entry
        /// израсходованным навсегда для этого шага (переживёт повторное открытие/закрытие
        /// экрана — панель больше не покажется, даже если condition снова станет true).
        /// Для repeatable — просто скрываем сейчас; появится снова на следующий Reevaluate,
        /// если condition к тому моменту всё ещё/снова true.</summary>
        public void Dismiss()
        {
            if (_currentIndex >= 0 && _currentIndex < _entries.Count && _entries[_currentIndex].Panel.oneShot)
                _consumedOneShot.Add(_currentIndex);

            CurrentText = null;
            _currentIndex = -1;
        }
    }
}