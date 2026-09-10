using System;
using System.Collections.Generic;

namespace Galactic1.Code.Gameplay.Tasks
{
    /// <summary>
    /// Иммутабельный presentation-снэпшот одной задачи. Единственное, что видят
    /// ScenarioTaskPanel/ScenarioTaskView — никогда TutorialStepDefinition/объективы
    /// напрямую (см. ITutorialObjective/TutorialTaskPresenter).
    /// </summary>
    public sealed class ScenarioTaskViewData
    {
        public readonly ScenarioTaskId TaskId;
        public readonly string TitleKey;
        public readonly string DescriptionKey;
        public readonly ScenarioTaskInstructionType InstructionType;
        public readonly ScenarioTaskProgress Progress;
        public readonly IReadOnlyList<ScenarioTaskReward> Rewards;
        public readonly ScenarioTaskState State;

        public ScenarioTaskViewData(
            ScenarioTaskId taskId,
            string titleKey,
            string descriptionKey,
            ScenarioTaskInstructionType instructionType,
            ScenarioTaskProgress progress,
            IReadOnlyList<ScenarioTaskReward> rewards,
            ScenarioTaskState state = ScenarioTaskState.Active)
        {
            TaskId = taskId;
            TitleKey = titleKey;
            DescriptionKey = descriptionKey;
            InstructionType = instructionType;
            Progress = progress;
            Rewards = rewards ?? Array.Empty<ScenarioTaskReward>();
            State = state;
        }

        public ScenarioTaskViewData WithProgress(ScenarioTaskProgress progress)
            => new(TaskId, TitleKey, DescriptionKey, InstructionType, progress, Rewards, State);

        public ScenarioTaskViewData WithState(ScenarioTaskState state)
            => new(TaskId, TitleKey, DescriptionKey, InstructionType, Progress, Rewards, state);
    }
}