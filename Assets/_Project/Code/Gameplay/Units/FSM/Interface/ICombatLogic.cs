namespace Galactic1.Code.Gameplay.Units
{
    /// <summary>
    /// Combat legality validation only.
    ///
    /// NOT responsible for:
    ///   — target acquisition
    ///   — target prioritization
    ///   — hostile filtering (→ TargetingUtility)
    ///
    /// Works for melee, ranged, and future attack types.
    /// Caller supplies range so CombatLogic has no weapon assumption.
    /// </summary>
    public interface ICombatLogic
    {
        /// <summary>
        /// Can this unit legally attack the given target right now?
        /// Checks LOS and range. Does NOT check team/hostility — caller
        /// is expected to have already obtained the target from TargetingUtility.
        /// </summary>
        bool CanAttack(UnitInstance unit, ITargetInfo target, float range);

        /// <summary>
        /// Is the target within range and not dead?
        /// Pure distance + alive check, no LOS.
        /// </summary>
        bool IsInRange(UnitInstance unit, ITargetInfo target, float range);

        /// <summary>
        /// Unobstructed line of sight from unit eye point to target.
        /// </summary>
        bool HasLineOfSight(UnitInstance unit, ITargetInfo target);
    }
}