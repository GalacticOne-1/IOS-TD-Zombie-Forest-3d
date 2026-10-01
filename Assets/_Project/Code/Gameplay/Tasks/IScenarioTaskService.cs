
using System;
using System.Collections.Generic;

namespace Galactic1.Code.Gameplay.Tasks
{
    public interface IScenarioTaskService : IGameService
    {
        event Action OnTasksChanged;

        /// <summary>
        /// Значимое изменение задачи (создание/progress/completion/появление
        /// задачи после удаления Completed). RemoveTask это событие НЕ вызывает.
        /// </summary>
        event Action OnTaskActivity;

        /// <summary>
        /// Момент истечения completion presentation delay для конкретной задачи.
        /// Эмитится ДО фактического удаления задачи. Generic-сигнал, ничего не
        /// знает про подписчиков (Tutorial или иных).
        /// </summary>
        event Action<ScenarioTaskId> OnTaskCompletionDelayElapsed;

        IReadOnlyList<ScenarioTaskViewData> GetTasks();

        void AddOrUpdateTask(ScenarioTaskViewData task);

        void UpdateProgress(ScenarioTaskId id, ScenarioTaskProgress progress);

        /// <summary>
        /// Переводит задачу в Completed и запускает completion presentation delay.
        /// Возвращает false, если задача не найдена или уже была Completed — в
        /// этом случае состояние не изменилось и delay-таймер не запущен;
        /// вызывающая сторона обязана явно обработать этот случай, а не ждать
        /// событие, которого не будет.
        /// </summary>
        bool CompleteTask(ScenarioTaskId id);

        /// <summary>
        /// Немедленно удаляет задачу. Если задача ожидала completion delay,
        /// отменяет соответствующий таймер — OnTaskCompletionDelayElapsed для
        /// неё больше не будет эмитирован.
        /// </summary>
        void RemoveTask(ScenarioTaskId id);

        void Clear();
    }
}