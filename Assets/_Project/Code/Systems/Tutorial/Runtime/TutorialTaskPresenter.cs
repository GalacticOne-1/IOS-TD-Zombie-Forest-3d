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
    /// </summary>
    public sealed class TutorialTaskPresenter
    {
        private readonly IScenarioTaskService _taskService;
        private readonly TutorialRewardService _rewardService;
        private ScenarioTaskId? _activeTaskId;

        public TutorialTaskPresenter(IScenarioTaskService taskService, TutorialRewardService rewardService)
        {
            _taskService = taskService;
            _rewardService = rewardService;
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

        /// <summary>Шаг завершён генуинно — задача показывает "выполнено" и исчезает сама
        /// после презентационной задержки (см. ScenarioTaskService.CompleteTask).</summary>
        public void CompleteStep(TutorialStepDefinition stepDef)
        {
            if (_activeTaskId == null) return;
            _taskService.CompleteTask(_activeTaskId.Value);
            _activeTaskId = null;
        }

        /// <summary>Шаг пропущен (Skip) — задача убирается немедленно, без "выполнено":
        /// награда за неё не выдавалась, показывать celebration было бы неверно.</summary>
        public void RemoveStepImmediately(TutorialStepDefinition stepDef)
        {
            if (_activeTaskId == null) return;
            _taskService.RemoveTask(_activeTaskId.Value);
            _activeTaskId = null;
        }

        /// <summary>Абортивная остановка тутора (StopTutorial/ForceStep) — задача убирается
        /// немедленно, никакой completion-презентации.</summary>
        public void Clear()
        {
            if (_activeTaskId == null) return;
            _taskService.RemoveTask(_activeTaskId.Value);
            _activeTaskId = null;
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