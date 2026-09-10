using System;
using System.Collections.Generic;
using R3;

namespace Galactic1.Code.Gameplay.Tasks
{
    /// <summary>
    /// Единственный источник истины "что сейчас показано в списке активных задач".
    /// Полностью источник-агностичен.
    ///
    /// Порядок — по вставке (List, не Dictionary) — детерминирован для scroll-списка.
    ///
    /// Persistence: намеренно НЕТ — это чисто презентационное состояние текущей сессии.
    /// Источники задач (Tutorial и др.) сами владеют персистентным прогрессом и заново
    /// вызывают AddOrUpdateTask при восстановлении (см. TutorialService.Restore →
    /// TutorialTaskPresenter.ShowStep, вызывается изнутри уже существующего ActivateStep).
    /// </summary>
    public sealed class ScenarioTaskService : IScenarioTaskService, IGameService
    {
        private static readonly TimeSpan CompletionPresentationDelay = TimeSpan.FromSeconds(1.2);

        private readonly List<ScenarioTaskId> _order = new();
        private readonly Dictionary<ScenarioTaskId, ScenarioTaskViewData> _tasks = new();
        private readonly Dictionary<ScenarioTaskId, IDisposable> _pendingRemovals = new();

        public event Action OnTasksChanged;

        public IReadOnlyList<ScenarioTaskViewData> GetTasks()
        {
            var snapshot = new List<ScenarioTaskViewData>(_order.Count);
            foreach (var id in _order)
                snapshot.Add(_tasks[id]);
            return snapshot;
        }

        public void AddOrUpdateTask(ScenarioTaskViewData task)
        {
            var normalized = task.State == ScenarioTaskState.Active
                ? task
                : task.WithState(ScenarioTaskState.Active);

            // Защита от гонки: если для этого TaskId ещё тикает отложенное удаление
            // после предыдущего CompleteTask (например ForceStep пересоздал тот же шаг),
            // свежая задача не должна быть выдернута устаревшим таймером.
            CancelPendingRemoval(normalized.TaskId);

            if (!_tasks.ContainsKey(normalized.TaskId))
                _order.Add(normalized.TaskId);

            _tasks[normalized.TaskId] = normalized;
            OnTasksChanged?.Invoke();
        }

        public void UpdateProgress(ScenarioTaskId id, ScenarioTaskProgress progress)
        {
            if (!_tasks.TryGetValue(id, out var existing)) return;
            _tasks[id] = existing.WithProgress(progress);
            OnTasksChanged?.Invoke();
        }

        public void CompleteTask(ScenarioTaskId id)
        {
            if (!_tasks.TryGetValue(id, out var existing)) return;
            if (existing.State == ScenarioTaskState.Completed) return; // уже в процессе завершения

            _tasks[id] = existing.WithState(ScenarioTaskState.Completed);
            OnTasksChanged?.Invoke();

            var subscription = Observable.Timer(CompletionPresentationDelay)
                .Subscribe(_ => RemoveTask(id));
            _pendingRemovals[id] = subscription;
        }

        public void RemoveTask(ScenarioTaskId id)
        {
            CancelPendingRemoval(id);
            if (!_tasks.Remove(id)) return;
            _order.Remove(id);
            OnTasksChanged?.Invoke();
        }

        public void Clear()
        {
            if (_tasks.Count == 0) return;
            foreach (var pending in _pendingRemovals.Values) pending.Dispose();
            _pendingRemovals.Clear();
            _tasks.Clear();
            _order.Clear();
            OnTasksChanged?.Invoke();
        }

        private void CancelPendingRemoval(ScenarioTaskId id)
        {
            if (_pendingRemovals.TryGetValue(id, out var pending))
            {
                pending.Dispose();
                _pendingRemovals.Remove(id);
            }
        }
    }
}