namespace Galactic1.Code.Gameplay.Tasks
{
    /// <summary>Визуальный вариант отображения задачи. Расширяемо — новое значение не
    /// требует изменений в ScenarioTaskService/IScenarioTaskService, только в
    /// презентационных виджетах (см. ScenarioTaskProgressView).</summary>
    public enum ScenarioTaskInstructionType
    {
        Text,
        Progress,
        Timer
    }
}