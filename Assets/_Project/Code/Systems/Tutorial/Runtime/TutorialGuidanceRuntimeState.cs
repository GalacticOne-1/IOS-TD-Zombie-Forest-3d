using System;
using System.Collections.Generic;
using Galactic1.Code.Systems.Tutorial.Presentation;

namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    /// <summary>
    /// Владеет lifecycle guidance-условий одного активного шага и резолвит,
    /// какой набор guidance-targets сейчас применим.
    ///
    /// Resolution остаётся first-satisfied-wins:
    /// первый guidance-entry с выполненным condition побеждает.
    ///
    /// Каждый entry может содержать несколько presentation targets.
    /// </summary>
    public sealed class TutorialGuidanceRuntimeState
    {
        private readonly IReadOnlyList<GuidanceEntry> _entries;

        private Action _onChanged;
        private bool _startInProgress;

        public IReadOnlyList<TutorialGuidanceTarget> Current { get; private set; }
            = Array.Empty<TutorialGuidanceTarget>();

        public TutorialGuidanceRuntimeState(
            IReadOnlyList<GuidanceEntry> entries)
        {
            _entries = entries ?? Array.Empty<GuidanceEntry>();
        }

        public void Start(Action onChanged)
        {
            _onChanged = onChanged;
            _startInProgress = true;

            foreach (var entry in _entries)
                entry.Condition.Start(Reevaluate);

            _startInProgress = false;

            Current = Resolve();
        }

        public void Stop()
        {
            foreach (var entry in _entries)
                entry.Condition.Stop();

            _onChanged = null;
            Current = Array.Empty<TutorialGuidanceTarget>();
        }

        private void Reevaluate()
        {
            if (_startInProgress)
                return;

            var resolved = Resolve();

            if (AreSame(Current, resolved))
                return;

            Current = resolved;
            _onChanged?.Invoke();
        }

        private IReadOnlyList<TutorialGuidanceTarget> Resolve()
        {
            foreach (var entry in _entries)
            {
                if (entry.Condition.IsSatisfied())
                    return entry.Targets;
            }

            return Array.Empty<TutorialGuidanceTarget>();
        }

        private static bool AreSame(
            IReadOnlyList<TutorialGuidanceTarget> a,
            IReadOnlyList<TutorialGuidanceTarget> b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (a == null || b == null)
                return false;

            if (a.Count != b.Count)
                return false;

            for (int i = 0; i < a.Count; i++)
            {
                if (!ReferenceEquals(a[i], b[i]))
                    return false;
            }

            return true;
        }

        public sealed class GuidanceEntry
        {
            public ITutorialGuidanceCondition Condition { get; }
            public IReadOnlyList<TutorialGuidanceTarget> Targets { get; }

            public GuidanceEntry(
                ITutorialGuidanceCondition condition,
                IReadOnlyList<TutorialGuidanceTarget> targets)
            {
                Condition = condition;
                Targets = targets ?? Array.Empty<TutorialGuidanceTarget>();
            }
        }
    }
}