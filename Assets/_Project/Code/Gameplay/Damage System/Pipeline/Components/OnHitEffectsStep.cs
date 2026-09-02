using UnityEngine;

namespace Galactic1.Code.Gameplay.Damage
{
    public sealed class OnHitEffectsStep : IDamageStep
    {
        public bool Process(DamageContext ctx)
        {
            if (ctx.Target is MonoBehaviour mb)
            {
                //var view = mb.GetComponentInChildren<UnitHitVFX>();
                //view?.PlayHit(ctx);
            }

            return true;
        }
    }
}