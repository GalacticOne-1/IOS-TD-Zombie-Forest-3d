/// <summary>
/// Raised by ProgressionService every time XP is successfully added — on
/// every gain, not just on level-ups. This is what the HUD's XP bar
/// subscribes to so it updates immediately without waiting for a level-up.
///
/// Carries the fully-resolved state (already computed by ProgressionService,
/// using ProgressionDefinition) so the HUD never re-derives progression math
/// itself.
/// </summary>
public sealed class ProgressionExperienceChangedEvent : IEvent
{
    public readonly int CurrentLevel;
    public readonly int CurrentXP;

    /// <summary>Total XP threshold at the start of CurrentLevel.</summary>
    public readonly int CurrentLevelXP;

    /// <summary>Total XP threshold required for the next level (== CurrentLevelXP at max level).</summary>
    public readonly int NextLevelXP;

    /// <summary>Normalized [0..1] progress within the current level.</summary>
    public readonly float ExperienceProgress;

    public ProgressionExperienceChangedEvent(
        int currentLevel,
        int currentXP,
        int currentLevelXP,
        int nextLevelXP,
        float experienceProgress)
    {
        CurrentLevel = currentLevel;
        CurrentXP = currentXP;
        CurrentLevelXP = currentLevelXP;
        NextLevelXP = nextLevelXP;
        ExperienceProgress = experienceProgress;
    }

}