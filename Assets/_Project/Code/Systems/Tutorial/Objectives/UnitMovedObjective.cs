namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>
    /// ЗАБЛОКИРОВАН: требует UnitMovedEvent, которого нет в проекте. Точка интеграции
    /// не определена — не видел squad movement command систему (упоминается
    /// SquadController/FormationCenterDriver в AILODSystem, определение не присылалось).
    /// Компилируется и логически корректен, но событие нужно добавить и поднять вручную.
    /// </summary>
    public sealed class UnitMovedObjective : TutorialEventObjectiveBase<UnitMovedEvent>
    {
        protected override bool EvaluateEvent(UnitMovedEvent e) => true;
    }
}
