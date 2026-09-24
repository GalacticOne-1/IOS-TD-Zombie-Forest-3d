using System;
using System.Collections.Generic;
using Galactic1.Code.Systems.Tutorial.Authoring;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>Реестр активных camera-bounds объектов сцены — отдельный от
    /// TutorialTargetRegistry (раздел 9 ТЗ: camera bounds это отдельный тип scene
    /// resource, не highlight/arrow/camera-focus target). Тот же safe-owner Unregister
    /// паттерн, что уже существует и проверен в TutorialTargetRegistry — скопирован
    /// дословно, не переизобретён.</summary>
    public sealed class TutorialCameraBoundsRegistry : IGameService
    {
        private readonly Dictionary<TutorialTargetId, ITutorialCameraBoundsTarget> _bounds = new();

        public event Action<ITutorialCameraBoundsTarget> OnBoundsRegistered;
        public event Action<TutorialTargetId> OnBoundsUnregistered;

        public void Register(ITutorialCameraBoundsTarget target)
        {
            if (target == null || target.TargetId == null) return;

            if (_bounds.ContainsKey(target.TargetId))
                Debug.LogError($"[TutorialCameraBoundsRegistry] Duplicate targetId " +
                                $"'{target.TargetId.DebugKey}' — overwriting.");

            _bounds[target.TargetId] = target;
            OnBoundsRegistered?.Invoke(target);
        }

        /// <summary>Тот же safe-owner fix, что у TutorialTargetRegistry.Unregister — удаляет
        /// запись, только если текущий владелец id — именно этот инстанс.</summary>
        public void Unregister(ITutorialCameraBoundsTarget target)
        {
            if (target == null || target.TargetId == null) return;

            if (_bounds.TryGetValue(target.TargetId, out var current) && ReferenceEquals(current, target))
            {
                _bounds.Remove(target.TargetId);
                OnBoundsUnregistered?.Invoke(target.TargetId);
            }
        }

        public bool TryGetBounds(TutorialTargetId targetId, out ITutorialCameraBoundsTarget target)
        {
            if (targetId == null) { target = null; return false; }
            return _bounds.TryGetValue(targetId, out target);
        }
    }
}