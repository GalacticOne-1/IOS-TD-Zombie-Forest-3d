using System;
using Galactic1.Code.Core;
using Galactic1.Core.Systems.GameLoopSession;
using UnityEngine;

namespace Galactic1.Code.Systems.Progression
{
    /// <summary>
    /// XP → Level. Nothing else.
    ///
    /// Strictly does NOT know about zombies, loot, locations, facilities or
    /// unlockable content of any kind — see ProgressionXPService (events → XP)
    /// and ProgressionUnlockService (Level → Unlocks) for everything downstream.
    ///
    /// Also the single source of truth for HUD-facing progression math
    /// (CurrentLevelXP / NextLevelXP / ExperienceProgress) so the HUD never
    /// re-derives the XP curve itself.
    /// </summary>
    public sealed class ProgressionService : IGameService, IUnlockContext
    {
        private readonly ProgressionProxy _proxy;
        private readonly ProgressionDefinition _definition;

        public int CurrentLevel => _proxy.Level.Value;
        public int CurrentXP => _proxy.CurrentXP.Value;
        public int ProgressionLevel => CurrentLevel; // IUnlockContext

        /// <summary>Total XP threshold at the start of the current level.</summary>
        public int CurrentLevelXP => _definition.GetLevelStartXP(CurrentLevel);

        /// <summary>
        /// Total XP threshold required for the next level.
        /// At MaxLevel there is no next threshold — returns CurrentLevelXP so
        /// callers relying on ExperienceProgress still get a stable value (see below).
        /// </summary>
        public int NextLevelXP => _definition.TryGetNextLevelXP(CurrentLevel, out var next)
            ? next
            : CurrentLevelXP;

        /// <summary>
        /// Normalized [0..1] progress within the current level.
        /// XP is cumulative, so this is NOT CurrentXP / NextLevelXP — it's the
        /// position between this level's start and the next level's start.
        /// Never NaN/Infinity: a zero-width span (MaxLevel, or misconfigured
        /// curve) clamps to 1.
        /// </summary>
        public float ExperienceProgress
        {
            get
            {
                int levelStart = CurrentLevelXP;
                int span = NextLevelXP - levelStart;

                if (span <= 0)
                    return 1f;

                float progress = (float)(CurrentXP - levelStart) / span;
                return Mathf.Clamp01(progress);
            }
        }

        /// <summary>Fired whenever the level actually changes (not on every XP gain).</summary>
        public event Action<int, int> OnLevelUp;

        public ProgressionService(ProgressionProxy proxy, ProgressionDefinition definition)
        {
            _proxy = proxy;
            _definition = definition;
        }

        /// <summary>
        /// Adds raw XP and resolves any level-ups that follow.
        /// Called exclusively by ProgressionXPService — gameplay systems never
        /// call this directly, they publish gameplay events instead.
        ///
        /// Order (matches the HUD/event-flow spec exactly):
        ///   1. Ignore non-positive XP.
        ///   2. Add XP to the proxy.
        ///   3. Resolve the resulting level from cumulative XP (handles
        ///      crossing more than one level threshold in a single grant).
        ///   4. Update the stored level.
        ///   5. Raise ProgressionExperienceChangedEvent with the FINAL state.
        ///   6. If the level changed, raise ProgressionLevelUpEvent.
        /// </summary>
        public void AddExperience(int amount)
        {
            if (amount <= 0)
                return;

            int previousLevel = CurrentLevel;

            _proxy.CurrentXP.Value += amount;

            int newLevel = _definition.GetLevelForXP(_proxy.CurrentXP.Value);
            if (newLevel != previousLevel)
                _proxy.Level.Value = newLevel;

            // State is fully updated at this point — events describe the new state.
            EventBus<ProgressionExperienceChangedEvent>.Raise(new ProgressionExperienceChangedEvent(
                CurrentLevel,
                CurrentXP,
                CurrentLevelXP,
                NextLevelXP,
                ExperienceProgress));

            if (newLevel > previousLevel)
            {
                OnLevelUp?.Invoke(previousLevel, newLevel);
                EventBus<ProgressionLevelUpEvent>.Raise(new ProgressionLevelUpEvent(previousLevel, newLevel));
            }

            ServiceLocator.Current.Get<GameSession>().MarkDirty();
        }
    }
}