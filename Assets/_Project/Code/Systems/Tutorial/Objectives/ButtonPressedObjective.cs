using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>Требует UITargetInteractedEvent — интеграционная точка, добавляется
    /// в обработчики конкретных кнопок (welcome_continue/claim_reward и т.п.), см. Integration/.
    /// UITargetInteractedEvent.TargetId остаётся сырым string (как e.Item.Id у
    /// ItemPickedEvent) — сравниваем через _targetId.Guid, а не сам ассет.</summary>
    public sealed class ButtonPressedObjective : TutorialEventObjectiveBase<UITargetInteractedEvent>
    {
        private readonly TutorialTargetId _targetId;
        public ButtonPressedObjective(TutorialTargetId targetId) => _targetId = targetId;

        protected override bool EvaluateEvent(UITargetInteractedEvent e)
            => _targetId != null && e.TargetId == _targetId;
    }
}
