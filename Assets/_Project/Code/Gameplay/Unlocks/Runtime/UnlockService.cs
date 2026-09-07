using Galactic1.Code.Core;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Core.Systems.GameLoopSession;

namespace Galactic1.Code.Systems.Progression
{
    /// <summary>
    /// Unlock state. Pure query + mutation of "is this UnlockId unlocked",
    /// persisted through ProgressionProxy.UnlockedIds.
    ///
    /// Deliberately has zero knowledge of Level, XP, or why something got
    /// unlocked — that decision belongs entirely to ProgressionUnlockService.
    /// WorldMapService / FacilityRuntimeService / any other gameplay system
    /// only ever calls IsUnlocked() here, never computes it themselves.
    /// </summary>
    public sealed class UnlockService : IGameService
    {
        private readonly ProgressionProxy _proxy;
        private readonly ProgressionUnlockDefinition _unlockDefinition;

        public UnlockService(
            ProgressionProxy proxy, 
            ProgressionUnlockDefinition unlockDefinition)
        {
            _proxy = proxy;
            _unlockDefinition = unlockDefinition;
        }

        public bool IsUnlocked(UnlockId id)
        {
            if (id == null)
                return true; // no requirement configured == always available

            return _proxy.UnlockedIds.Contains(id.Guid);
        }
        
        public (bool unlocked, int level) GetStatus(UnlockId id)
        {
            if (id == null)
                return (true, 0); // no requirement configured == always available

            _unlockDefinition.TryGetDisplayRequiredLevel(id, out var level);
            return (_proxy.UnlockedIds.Contains(id.Guid), level);
        }

        /// <summary>
        /// Marks an UnlockId as unlocked and raises ProgressionUnlockEvent.
        /// No-op if already unlocked. Called only by ProgressionUnlockService.
        /// </summary>
        public void Unlock(UnlockId id)
        {
            if (id == null || IsUnlocked(id))
                return;

            _proxy.UnlockedIds.Add(id.Guid);

            EventBus<ProgressionUnlockEvent>.Raise(new ProgressionUnlockEvent(id));
            ServiceLocator.Current.Get<GameSession>().MarkDirty();
        }
    }
}
