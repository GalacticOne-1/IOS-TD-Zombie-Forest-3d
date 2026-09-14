using System;
using System.Collections.Generic;

namespace Galactic1.Code.Gameplay.Tasks
{
    /// <summary>
    /// Источник-агностичный API активных задач. НЕ знает про Tutorial/Scenario/Daily
    /// Tasks — источники задач вызывают эти методы, ScenarioTaskPanel только читает
    /// GetTasks()/подписан на OnTasksChanged.
    ///
    /// Completion vs reward: CompleteTask только помечает задачу выполненной для UI —
    /// никогда не выдаёт и не знает про награду (см. ScenarioTaskReward докстринг).
    /// </summary>
    public interface IScenarioTaskService : IGameService
    {
        event Action OnTasksChanged;
        event Action OnTaskActivity;

        /// <summary>Снэпшот в детерминированном порядке вставки — не Dictionary iteration.</summary>
        IReadOnlyList<ScenarioTaskViewData> GetTasks();

        /// <summary>Добавляет новую задачу или полностью обновляет существующую с тем же
        /// TaskId. State во входном task игнорируется — задача всегда становится Active
        /// (Completed выставляется исключительно через CompleteTask).</summary>
        void AddOrUpdateTask(ScenarioTaskViewData task);

        /// <summary>Дешёвое частичное обновление прогресса без пересборки всей задачи —
        /// для частых тиков (например, за каждое объективное событие).</summary>
        void UpdateProgress(ScenarioTaskId id, ScenarioTaskProgress progress);

        /// <summary>Помечает задачу выполненной — UI показывает completed-состояние,
        /// задача автоматически удаляется после презентационной задержки.</summary>
        void CompleteTask(ScenarioTaskId id);

        /// <summary>Немедленное удаление без completion-презентации (skip/abort/cleanup).</summary>
        void RemoveTask(ScenarioTaskId id);

        /// <summary>Немедленно очищает все задачи без completion-презентации.</summary>
        void Clear();
    }
}