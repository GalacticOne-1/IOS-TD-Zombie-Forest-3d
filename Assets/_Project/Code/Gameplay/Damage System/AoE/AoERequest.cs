
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.AoE
{
    /// <summary>
    /// Универсальный запрос AoE.
    /// </summary>
    public struct AoERequest
    {
        public ISceneUnit Attacker;

        public Vector3 Position;
        public float SmallRadius;
        public float BigRadius;
        public VfxId VfxId;

        // === Damage ===
        public float MaxDamage;
        public float BigRadiusDamagePercent;
        public AnimationCurve DamageFalloff; // distance → multiplier

        // === Effects ===
        public bool ApplyEffects;
        //public List<EffectData> Effects;

        // === Lifetime ===
        public float Duration; // 0 = instant

        // === Rules
        public LayerMask TargetMask;
        public bool RequireLOS;
    }
}