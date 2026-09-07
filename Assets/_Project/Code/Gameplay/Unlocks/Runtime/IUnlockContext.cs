namespace Galactic1.Code.Systems.Progression
{
    /// <summary>
    /// Everything an UnlockRequirement is allowed to read when deciding whether
    /// it's satisfied. Kept deliberately narrow (just the current level for
    /// now) so future requirement types (currency spent, day reached, quest
    /// completed, etc.) extend this interface instead of requirements reaching
    /// into arbitrary services directly.
    /// </summary>
    public interface IUnlockContext
    {
        int ProgressionLevel { get; }
    }
}
