using System;
using System.Collections.Generic;
using Galactic1.Code.Systems.Tutorial.Authoring;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>Реестр активных ITutorialTarget. Живёт в root-контейнере, но
    /// содержимое полностью пересобирается сценой (регистрация в OnEnable
    /// каждого нового UI-объекта).</summary>
    public sealed class TutorialTargetRegistry : IGameService
    {
        private readonly Dictionary<TutorialTargetId, ITutorialTarget> _targets = new();

        public event Action<ITutorialTarget> OnTargetRegistered;
        public event Action<TutorialTargetId> OnTargetUnregistered;

        public void Register(ITutorialTarget target)
        {
            if (target == null || target.TargetId == null) return;

            if (_targets.ContainsKey(target.TargetId))
                Debug.LogError($"[TutorialTargetRegistry] Duplicate targetId '{target.TargetId.DebugKey}' — overwriting. " +
                                "Два UI-элемента регистрируются под одним и тем же tutorial target id.");

            _targets[target.TargetId] = target;
            OnTargetRegistered?.Invoke(target);
        }

        /// <summary>Fix: раньше Unregister принимал string targetId и удалял запись по ключу
        /// безусловно — при дублирующихся targetId (A регистрирует "Button", затем B тоже
        /// регистрирует "Button" поверх, затем A.OnDisable вызывает Unregister("Button"))
        /// это стирало регистрацию B. Теперь удаляется, только если текущий владелец id —
        /// именно этот инстанс.</summary>
        public void Unregister(ITutorialTarget target)
        {
            if (target == null || target.TargetId == null) return;

            if (_targets.TryGetValue(target.TargetId, out var current) && ReferenceEquals(current, target))
            {
                _targets.Remove(target.TargetId);
                OnTargetUnregistered?.Invoke(target.TargetId);
            }
            // если владелец id — другой инстанс, ничего не делаем: чужая регистрация
            // не должна затрагиваться.
        }

        public bool TryGetTarget(TutorialTargetId targetId, out ITutorialTarget target)
        {
            if (targetId == null) { target = null; return false; }
            return _targets.TryGetValue(targetId, out target);
        }
    }
}
