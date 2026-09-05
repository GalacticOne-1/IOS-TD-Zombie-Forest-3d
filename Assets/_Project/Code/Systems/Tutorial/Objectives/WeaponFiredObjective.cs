namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// ЗАБЛОКИРОВАН: требует WeaponFiredEvent. Точка интеграции не определена —
    /// не видел определение WeaponFireService (только использование в RaidInProgressState).
    /// </summary>
    public sealed class WeaponFiredObjective : TutorialEventObjectiveBase<WeaponFiredEvent>
    {
        protected override bool EvaluateEvent(WeaponFiredEvent e) => true;
    }
}
