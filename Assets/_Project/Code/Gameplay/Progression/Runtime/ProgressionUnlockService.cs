namespace Galactic1.Code.Systems.Progression
{
    /// <summary>
    /// Level → Unlocks.
    ///
    /// Subscribes to ProgressionLevelUpEvent, re-evaluates every not-yet-unlocked
    /// entry in ProgressionUnlockDefinition against the current IUnlockContext,
    /// and calls UnlockService.Unlock() for every entry now satisfied.
    ///
    /// Runs the full evaluation (not just entries gated at the new level)
    /// so that any future non-level requirement type resolving out of order
    /// still gets picked up correctly.
    /// </summary>
    public sealed class ProgressionUnlockService
    {
        private readonly UnlockService _unlockService;
        private readonly ProgressionUnlockDefinition _unlockDefinition;
        private readonly IUnlockContext _context;

        private readonly EventBinding<ProgressionLevelUpEvent> _levelUpBinding;

        public ProgressionUnlockService(
            UnlockService unlockService,
            ProgressionUnlockDefinition unlockDefinition,
            IUnlockContext context)
        {
            _unlockService = unlockService;
            _unlockDefinition = unlockDefinition;
            _context = context;

            _levelUpBinding = new EventBinding<ProgressionLevelUpEvent>(OnLevelUp);
            EventBus<ProgressionLevelUpEvent>.Register(_levelUpBinding);
        }

        /// <summary>
        /// Evaluates every entry once. Called on level-up, and safe to call
        /// once more right after loading a save (covers definitions that
        /// changed between sessions / were added mid-development).
        /// </summary>
        public void EvaluateAll()
        {
            foreach (var entry in _unlockDefinition.Entries)
            {
                if (_unlockService.IsUnlocked(entry.Id))
                    continue;

                if (_unlockDefinition.IsSatisfied(entry, _context))
                    _unlockService.Unlock(entry.Id);
            }
        }

        private void OnLevelUp(ProgressionLevelUpEvent e) => EvaluateAll();
    }
}
