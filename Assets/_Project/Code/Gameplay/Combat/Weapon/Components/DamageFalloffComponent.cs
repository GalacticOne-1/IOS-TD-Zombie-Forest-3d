using UnityEngine;

namespace Galactic1.Code.Gameplay.Weapons.Logic
{
    public sealed class DamageFalloffComponent : WeaponComponentBase
    {
        private WeaponEntity _entity;

        public override void OnEquip(WeaponEntity entity) => _entity = entity;

        public float GetDamageMultiplier(float distance)
        {
            var def = _entity.Definition;

            if (distance <= def.EffectiveRange)
                return 1f;

            if (def.MaxRange <= def.EffectiveRange)
                return 1f;

            float t = Mathf.InverseLerp(def.EffectiveRange, def.MaxRange, distance);

            return Mathf.Lerp(1f, def.MinDamageMultiplierAtMaxRange, t);
        }
    }
}