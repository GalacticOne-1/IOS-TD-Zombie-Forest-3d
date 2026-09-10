namespace Galactic1.Code.Gameplay.Tasks
{
    /// <summary>Нормализованный прогресс задачи — единственное, что видит generic UI.
    /// UI никогда не знает, откуда взялись Current/Required (Tutorial objective,
    /// будущий Daily Task счётчик и т.п.) — см. IScenarioTaskService докстринг.</summary>
    public readonly struct ScenarioTaskProgress
    {
        public static readonly ScenarioTaskProgress None = new(false, 0, 0);

        public readonly bool HasProgress;
        public readonly int Current;
        public readonly int Required;

        public ScenarioTaskProgress(bool hasProgress, int current, int required)
        {
            HasProgress = hasProgress;
            Current = current;
            Required = required;
        }
    }
}