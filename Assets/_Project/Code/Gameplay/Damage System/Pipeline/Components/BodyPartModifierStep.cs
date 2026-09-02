using Galactic1.Code.Gameplay.Combat.Data;

namespace Galactic1.Code.Gameplay.Damage.Pipeline
{
    /// <summary>
    /// Applies body-part damage multipliers.
    ///
    /// Multipliers:
    ///   Head      x2.0  — critical zone
    ///   LegLeft   x0.7  — extremity
    ///   LegRight  x0.7  — extremity
    ///   Other     x1.0  — no modifier
    ///
    /// Insert position in DamageService pipeline:
    ///   DeadCheck → BuffModifier → [BodyPartModifier] → ArmorReduction → ApplyDamage → ArmorDurability
    ///
    /// Used by DamageService.
    /// </summary>
    public sealed class BodyPartModifierStep : IDamageStep
    {
        public bool Process(DamageContext ctx)
        {
            switch (ctx.HitInfo.BodyPart)
            {
                case BodyPartType.Head:
                    ctx.Damage *= 2f;
                    break;

                case BodyPartType.LegLeft:
                case BodyPartType.LegRight:
                    ctx.Damage *= 0.7f;
                    break;

                // Torso, Arms — no modifier
            }

            return true;
        }
    }
}