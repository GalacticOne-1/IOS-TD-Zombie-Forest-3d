using Galactic1.Code.Systems.Raid;

namespace Galactic1.Code.Gameplay.Combat.Events
{
    /// <summary>
    /// Raised after suppression is flushed for a unit at the end of a burst.
    ///
    /// Aggregated — one event per unit per burst, not per bullet.
    /// Used by:
    /// - AI state machine (force into cover)
    /// - Morale systems
    /// - UI suppression indicator
    /// </summary>
    public readonly struct CombatSuppressionEvent : IEvent
    {
        public readonly IUnitSceneContext Target;

        /// <summary>Total suppression applied this burst.</summary>
        public readonly float Amount;

        public CombatSuppressionEvent(IUnitSceneContext target, float amount)
        {
            Target = target;
            Amount = amount;
        }
    }
}