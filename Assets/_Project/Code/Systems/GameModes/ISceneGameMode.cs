namespace Galactic1.Code.Systems
{
    /// <summary>
    /// Scene game mode interface.
    /// Each mode controls input, camera and interaction rules.
    /// </summary>
    public interface ISceneGameMode
    {
        GameModeType ModeType { get; }

        void Enter();
        void Exit();
    }
}