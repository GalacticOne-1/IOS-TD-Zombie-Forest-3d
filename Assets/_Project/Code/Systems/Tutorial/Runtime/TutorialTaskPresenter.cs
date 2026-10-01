using System;
using Galactic1.Code.Gameplay.Tasks;
using Galactic1.Code.Systems.Tutorial.Authoring;
using Galactic1.Code.Systems.Tutorial.Rewards;

namespace Galactic1.Code.Systems.Tutorial.Runtime
{
    /// <summary>
    /// Единственная точка перевода активного Tutorial-шага в generic Scenario Task API.
    /// Владеет ТОЛЬКО knowledge "какой TaskId сейчас показан от Tutorial" — ничего не
    /// знает про граф/переходы/чекпоинты (TutorialService/TutorialGraphNavigator/
    /// TutorialCheckpointService). Единственный вызывающий — TutorialService.
    ///
    /// TaskId = stepId.Guid — тот же guid, что уже используется на границе
    /// persist/restore (CGameStateTutorial.currentStepId), стабилен и уникален в
    /// рамках одной кампании. Инструкция/прогресс/награда идут через этот класс;
    /// highlight/arrow/camera focus по-прежнему через TutorialPresentationService
    /// (см. class docstring там же — Tutorial-специфичное presentation НЕ переносится).
    ///
    /// Дополнительно служит adapter-boundary для completion presentation delay:
    /// ScenarioTaskService эмитит generic OnTaskCompletionDelayElapsed(id), presenter
    /// фильтрует "своё" по _completingTaskId и транслирует в Tutorial-специфичный
    /// OnCompletionDelayElapsed (без данных — TutorialService сам хранит, какой
    /// stepDef ожидает продолжения). ScenarioTaskService при этом ничего не знает
    /// про Tutorial — зависимость идёт только в одну сторону: Tutorial → generic.
    /// </summary>
    public sealed class TutorialTaskPresenter
    {
        private readonly IScenarioTaskService _taskService;
        private readonly TutorialRewardService _rewardService;
        private ScenarioTaskId? _activeTaskId;

        /// <summary>
        /// Id задачи, уже переведённой в Completed и ожидающей истечения completion
        /// presentation delay внутри ScenarioTaskService. Пока не null — TutorialService
        /// ещё не должен резолвить graph transition для соответствующего шага.
        /// Устанавливается только после подтверждённого успеха CompleteTask — см. CompleteStep.
        /// </summary>
        private ScenarioTaskId? _completingTaskId;

        /// <summary>
        /// Completion presentation delay для шага, который завершался последним
        /// успешным CompleteStep-вызовом, истёк. TutorialService слушает это событие,
        /// чтобы продолжить graph transition строго после того, как панель отрисовала
        /// Completed-состояние нужное время (см. TutorialService.HandleCompletionDelayElapsed).
        /// </summary>
        public event Action OnCompletionDelayElapsed;

        public TutorialTaskPresenter(IScenarioTaskService taskService, TutorialRewardService rewardService)
        {
            _taskService = taskService;
            _rewardService = rewardService;
            _taskService.OnTaskCompletionDelayElapsed += HandleTaskCompletionDelayElapsed;
        }

        public void ShowStep(TutorialStepRuntimeState stepState)
        {
            var stepDef = stepState.Definition;
            var taskId = new ScenarioTaskId(stepDef.stepId.Guid);
            _activeTaskId = taskId;

            _taskService.AddOrUpdateTask(BuildViewData(taskId, stepDef, stepState));
        }

        public void UpdateProgress(TutorialStepRuntimeState stepState)
        {
            if (_activeTaskId == null) return;

            var progress = stepState.TryGetProgress(out var current, out var required)
                ? new ScenarioTaskProgress(true, current, required)
                : ScenarioTaskProgress.None;

            _taskService.UpdateProgress(_activeTaskId.Value, progress);
        }

        /// <summary>Шаг завершён генуинно — задача переводится в Completed и сама
        /// исчезнет после completion presentation delay (см. ScenarioTaskService.CompleteTask).
        /// TutorialService продолжит graph transition не сразу, а по приходу
        /// OnCompletionDelayElapsed — см. его докстринг.
        ///
        /// Возвращает false, если ScenarioTaskService не смог перевести задачу в
        /// Completed (нет активной задачи или она уже была Completed — в штатном
        /// потоке недостижимо, т.к. _activeTaskId всегда соответствует реально
        /// активному шагу, но проверяется явно как lifecycle-инвариант на границе
        /// между generic-слоем и Tutorial). В этом случае completion delay не
        /// наступит: _completingTaskId НЕ выставляется, и вызывающая сторона
        /// (TutorialService) обязана сама решить, как продолжать — presenter не
        /// имеет права молча оставить TutorialService ждать сигнала, которого не
        /// будет.</summary>
        public bool CompleteStep(TutorialStepDefinition stepDef)
        {
            if (_activeTaskId == null) return false;

            var taskId = _activeTaskId.Value;
            _activeTaskId = null;

            if (!_taskService.CompleteTask(taskId))
                return false;

            // _completingTaskId выставляется ТОЛЬКО после подтверждённого успеха
            // CompleteTask — иначе HandleTaskCompletionDelayElapsed будет ждать
            // событие для задачи, для которой таймер так и не был запущен.
            _completingTaskId = taskId;
            return true;
        }

        /// <summary>Шаг пропущен (Skip) — задача убирается немедленно, без "выполнено":
        /// награда за неё не выдавалась, показывать celebration было бы неверно.
        /// Никакого completion delay для Skip не бывает — RemoveTask не планирует таймер.</summary>
        public void RemoveStepImmediately(TutorialStepDefinition stepDef)
        {
            if (_activeTaskId == null) return;
            _taskService.RemoveTask(_activeTaskId.Value);
            _activeTaskId = null;
        }

        /// <summary>Абортивная остановка тутора (StopTutorial/ForceStep/RestartTutorial/
        /// ClearProgress) — убирает немедленно ЛЮБУЮ задачу, которой сейчас владеет
        /// presenter: как ещё активную (_activeTaskId), так и уже Completed, но
        /// ожидающую completion delay (_completingTaskId). Для последней RemoveTask
        /// отменяет pending removal внутри ScenarioTaskService (Dispose таймера),
        /// поэтому OnTaskCompletionDelayElapsed для неё гарантированно больше не придёт —
        /// TutorialService не получит "протухший" сигнал продолжить transition.</summary>
        public void Clear()
        {
            if (_completingTaskId != null)
            {
                _taskService.RemoveTask(_completingTaskId.Value);
                _completingTaskId = null;
            }

            if (_activeTaskId != null)
            {
                _taskService.RemoveTask(_activeTaskId.Value);
                _activeTaskId = null;
            }
        }

        private void HandleTaskCompletionDelayElapsed(ScenarioTaskId id)
        {
            // ScenarioTaskService — generic-сервис, событие может относиться к задаче
            // другого источника, не Tutorial. Реагируем только на "свою" задачу.
            if (_completingTaskId == null || !_completingTaskId.Value.Equals(id))
                return;

            _completingTaskId = null;
            OnCompletionDelayElapsed?.Invoke();
        }

        private ScenarioTaskViewData BuildViewData(
            ScenarioTaskId taskId, TutorialStepDefinition stepDef, TutorialStepRuntimeState stepState)
        {
            var progress = stepState.TryGetProgress(out var current, out var required)
                ? new ScenarioTaskProgress(true, current, required)
                : ScenarioTaskProgress.None;

            var instructionType = progress.HasProgress
                ? ScenarioTaskInstructionType.Progress
                : ScenarioTaskInstructionType.Text;

            return new ScenarioTaskViewData(
                taskId,
                stepDef.presentation.instructionTitleKey,
                stepDef.presentation.instructionDesKey,
                instructionType,
                progress,
                _rewardService.GetRewardSnapshot(stepDef));
        }
    }
}
