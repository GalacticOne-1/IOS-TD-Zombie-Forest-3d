using UnityEngine;

namespace Galactic1.Code.Gameplay.Damage
{
    public sealed class UnitHitVFX : MonoBehaviour
    {
        [SerializeField] private Renderer[] renderers;

        public void PlayHit(DamageContext ctx)
        {
            // 1. вспышка материала
            //Flash();

            // 2. floating damage
            //SpawnDamageText(ctx.Damage, ctx.IsCritical, ctx.HitPoint);

            // 3. blood FX
            //SpawnBlood(ctx.HitPoint);
        }

        private void Flash()
        {
            foreach (var r in renderers)
            {
                // shader param toggle
            }
        }

        private void SpawnDamageText(float dmg, bool crit, Vector3 pos)
        {
            // PoolManager
        }

        private void SpawnBlood(Vector3 pos)
        {
            // PoolManager
        }
    }
}