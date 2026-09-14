
using System;
using System.Collections.Generic;
using R3;

namespace Galactic1.Code.Gameplay.Tasks
{
    /// <summary>
    /// Единственный источник истины "что сейчас показано в списке активных задач".
    ///
    /// Полностью источник-агностичный, без Unity-зависимостей.
    ///
    /// Порядок — по вставке (List, не Dictionary), детерминирован для scroll-списка.
    ///
    /// Persistence: намеренно НЕТ — это чисто презентационное состояние текущей сессии.
    /// Источники задач (Tutorial и др.) сами владеют персистентным прогрессом и заново
    /// вызывают AddOrUpdateTask при восстановлении.
    ///
    /// Lifecycle Completed-задачи:
    ///
    /// Active
    ///   ↓ CompleteTask
    /// Completed
    ///   ↓ CompletionHighlightDelay
    /// Removed
    ///   ↓
    /// pending task(s) становятся Active
    ///
    /// Важное правило:
    /// пока существует хотя бы одна Completed-задача, ожидающая удаления,
    /// новые задачи не добавляются непосредственно в _tasks.
    ///
    /// Они временно сохраняются в _pendingTasks и применяются только после
    /// фактического удаления Completed-задачи.
    /// </summary>
    public sealed class ScenarioTaskService : IScenarioTaskService, IGameService
    {
        /// <summary>
        /// Сколько Completed-задача остаётся видимой перед фактическим удалением.
        ///
        /// Presentation-слой не должен дублировать это значение.
        /// </summary>
        private static readonly TimeSpan CompletionHighlightDelay =
            TimeSpan.FromSeconds(2);

        private readonly List<ScenarioTaskId> _order = new();

        private readonly Dictionary<
            ScenarioTaskId,
            ScenarioTaskViewData> _tasks = new();

        private readonly Dictionary<
            ScenarioTaskId,
            IDisposable> _pendingRemovals = new();

        /// <summary>
        /// Задачи, пришедшие во время completion-delay.
        ///
        /// Dictionary используется намеренно:
        /// если одна и та же задача несколько раз обновилась за эту секунду,
        /// сохраняется только её последнее состояние.
        /// </summary>
        private readonly Dictionary<
            ScenarioTaskId,
            ScenarioTaskViewData> _pendingTasks = new();

        public event Action OnTasksChanged;

        /// <summary>
        /// Значимое изменение задачи.
        ///
        /// Presentation-слой использует событие для временного показа HUD-панели.
        ///
        /// Не вызывается при RemoveTask:
        /// удаление задачи само по себе не должно открывать или продлевать панель.
        /// </summary>
        public event Action OnTaskActivity;

        public IReadOnlyList<ScenarioTaskViewData> GetTasks()
        {
            var snapshot = new List<ScenarioTaskViewData>(_order.Count);

            foreach (var id in _order)
                snapshot.Add(_tasks[id]);

            return snapshot;
        }

        /// <summary>
        /// Добавляет новую Active-задачу или обновляет существующую.
        ///
        /// Если сейчас есть Completed-задача, ожидающая удаления,
        /// задача не появляется в основном списке сразу.
        /// Она сохраняется как pending и будет применена после удаления Completed.
        /// </summary>
        public void AddOrUpdateTask(ScenarioTaskViewData task)
        {
            var normalized = task.State == ScenarioTaskState.Active
                ? task
                : task.WithState(ScenarioTaskState.Active);

            /*
             * Пока хотя бы одна Completed-задача ожидает удаления,
             * физически не меняем основной список.
             */
            if (_pendingRemovals.Count > 0)
            {
                _pendingTasks[normalized.TaskId] = normalized;
                return;
            }

            ApplyTask(normalized);
        }

        /// <summary>
        /// Непосредственно применяет задачу к текущему presentation-состоянию.
        ///
        /// Только этот метод добавляет/обновляет задачу в _tasks.
        /// </summary>
        private void ApplyTask(ScenarioTaskViewData task)
        {
            if (!_tasks.ContainsKey(task.TaskId))
                _order.Add(task.TaskId);

            _tasks[task.TaskId] = task;

            OnTasksChanged?.Invoke();
            OnTaskActivity?.Invoke();
        }

        /// <summary>
        /// Обновляет прогресс существующей Active-задачи.
        ///
        /// Completed-задачу нельзя оживить через UpdateProgress.
        /// </summary>
        public void UpdateProgress(
            ScenarioTaskId id,
            ScenarioTaskProgress progress)
        {
            if (!_tasks.TryGetValue(id, out var existing))
                return;

            if (existing.State == ScenarioTaskState.Completed)
                return;

            _tasks[id] = existing.WithProgress(progress);

            OnTasksChanged?.Invoke();
            OnTaskActivity?.Invoke();
        }

        /// <summary>
        /// Переводит задачу в Completed.
        ///
        /// Completed-состояние немедленно становится доступно presentation-слою.
        /// Фактическое удаление происходит после CompletionHighlightDelay.
        /// </summary>
        public void CompleteTask(ScenarioTaskId id)
        {
            if (!_tasks.TryGetValue(id, out var existing))
                return;

            if (existing.State == ScenarioTaskState.Completed)
                return;

            _tasks[id] = existing.WithState(
                ScenarioTaskState.Completed);

            /*
             * Сначала сообщаем presentation, что задача стала Completed.
             * Это позволяет View сразу показать зелёное состояние.
             */
            OnTasksChanged?.Invoke();
            OnTaskActivity?.Invoke();

            ScheduleRemoval(id);
        }

        private void ScheduleRemoval(ScenarioTaskId id)
        {
            var subscription = Observable.Timer(CompletionHighlightDelay)
                .Subscribe(_ => RemoveTaskInternal(id));

            _pendingRemovals[id] = subscription;
        }

        /// <summary>
        /// Немедленно удаляет задачу.
        ///
        /// Публичный RemoveTask может использоваться внешним кодом,
        /// например для принудительного удаления задачи.
        /// </summary>
        public void RemoveTask(ScenarioTaskId id)
        {
            RemoveTaskInternal(id);
        }

        /// <summary>
        /// Фактическое удаление задачи.
        ///
        /// Важно:
        /// 1. сначала удаляем Completed-задачу;
        /// 2. отправляем OnTasksChanged;
        /// 3. только после этого применяем pending-задачи.
        ///
        /// Таким образом panel никогда не получает состояние,
        /// в котором старая Completed-задача и новая задача существуют
        /// одновременно из-за одного completion-flow.
        /// </summary>
        private void RemoveTaskInternal(ScenarioTaskId id)
        {
            CancelPendingRemoval(id);

            if (!_tasks.Remove(id))
                return;

            _order.Remove(id);

            /*
             * Удаление само по себе не является activity.
             *
             * Panel должна увидеть исчезновение карточки,
             * но не должна из-за этого продлевать свой activityVisibleDuration.
             */
            OnTasksChanged?.Invoke();

            /*
             * Теперь старая задача действительно исчезла.
             * Только после этого разрешаем появление следующей.
             */
            FlushPendingTasks();
        }

        /// <summary>
        /// Применяет задачи, которые пришли во время completion-delay.
        ///
        /// Если за время ожидания одна задача обновлялась несколько раз,
        /// в _pendingTasks находится только её последнее состояние.
        /// </summary>
        private void FlushPendingTasks()
        {
            /*
             * Защита на случай нескольких одновременно ожидающих removal.
             */
            if (_pendingRemovals.Count > 0)
                return;

            if (_pendingTasks.Count == 0)
                return;

            foreach (var task in _pendingTasks.Values)
                ApplyTask(task);

            _pendingTasks.Clear();
        }

        /// <summary>
        /// Полностью очищает presentation-состояние.
        ///
        /// Persistence здесь намеренно отсутствует.
        /// </summary>
        public void Clear()
        {
            if (_tasks.Count == 0 &&
                _pendingTasks.Count == 0 &&
                _pendingRemovals.Count == 0)
                return;

            foreach (var pending in _pendingRemovals.Values)
                pending.Dispose();

            _pendingRemovals.Clear();
            _pendingTasks.Clear();

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

