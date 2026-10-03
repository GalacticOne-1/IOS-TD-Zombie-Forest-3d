namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public interface IPersistentTutorialObjective
    {
        int ProgressValue { get; }
        void RestoreProgress(int value);
    }
}