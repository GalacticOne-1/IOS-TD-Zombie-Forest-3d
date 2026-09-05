namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// ЗАБЛОКИРОВАН: требует TargetSelectedEvent. Точка интеграции не определена —
    /// не видел определение TargetInfoBase/TargetInfoProxy (только использование).
    /// </summary>
    public sealed class TargetSelectedObjective : TutorialEventObjectiveBase<TargetSelectedEvent>
    {
        protected override bool EvaluateEvent(TargetSelectedEvent e) => true;
    }
}
