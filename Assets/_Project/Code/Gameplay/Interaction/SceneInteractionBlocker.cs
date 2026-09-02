
namespace Galactic1.Code.Systems.Interaction
{
    /// <summary>
    /// Blocks world interaction while mode is active.
    /// Prevents units, buildings and world objects from receiving clicks.
    /// 
    /// Used by ConstructionGameMode.
    /// </summary>
    public class SceneInteractionBlocker : IGameService
    {
        private bool _blocked;

        /// <summary>
        /// Returns true if world interaction is blocked.
        /// </summary>
        public bool IsBlocked => _blocked;

        /// <summary>
        /// Enable interaction blocking.
        /// </summary>
        public void Enable()
        {
            if (_blocked)
                return;

            _blocked = true;

#if UNITY_EDITOR
            DLog.Alert("[Interaction] Scene interaction BLOCKED", EDlogColor.YELLOW);
#endif
        }

        /// <summary>
        /// Disable interaction blocking.
        /// </summary>
        public void Disable()
        {
            if (!_blocked)
                return;

            _blocked = false;

#if UNITY_EDITOR
            DLog.Alert("[Interaction] Scene interaction RESTORED", EDlogColor.YELLOW);
#endif
        }
    }
}