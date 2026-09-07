using UnityEngine;

namespace Galactic1.Code.Systems.Progression
{
    /// <summary>
    /// Soft Launch placeholder for the future skill-choice-on-level-up feature.
    ///
    /// Reacts to ProgressionLevelUpEvent and currently just logs. No skill
    /// effects, no UI. The real implementation later replaces the body of
    /// OnLevelUp() with "open skill selection panel" — ProgressionService
    /// never needs to change for that to happen.
    /// </summary>
    public sealed class ProgressionSkillChoiceService
    {
        private readonly EventBinding<ProgressionLevelUpEvent> _levelUpBinding;

        public ProgressionSkillChoiceService()
        {
            _levelUpBinding = new EventBinding<ProgressionLevelUpEvent>(OnLevelUp);
            EventBus<ProgressionLevelUpEvent>.Register(_levelUpBinding);
        }

        private void OnLevelUp(ProgressionLevelUpEvent e)
        {
            Debug.Log($"[Progression] Level {e.NewLevel} reached.");
            Debug.Log("[Progression] Skill selection panel should be opened.");

            // TODO (post Soft Launch): open the real skill selection UI here,
            // persist e.NewLevel into ProgressionProxy.PendingSkillChoiceLevels
            // until the player resolves it, and clear it once they do.
        }
    }
}
