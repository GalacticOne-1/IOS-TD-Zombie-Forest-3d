namespace Galactic1.Code.Gameplay.Combat.Data
{
    /// <summary>
    /// Body zones used by combat hit resolution.
    /// Used by:
    /// - BodyPartResolver
    /// - BodyPartModifierStep
    /// - Wound systems
    /// </summary>
    public enum BodyPartType
    {
        Head,
        Torso,
        ArmLeft,
        ArmRight,
        LegLeft,
        LegRight
    }
}