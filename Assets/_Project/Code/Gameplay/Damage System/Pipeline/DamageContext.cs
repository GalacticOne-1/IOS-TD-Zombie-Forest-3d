using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Damage
{
    /// <summary>
    /// Контекст обработки урона (живёт 1 хит)
    /// </summary>
    public sealed class DamageContext
    {
        public IUnitSceneContext Attacker { get; }
        public IUnitSceneContext Target { get; }

        public DamageType Type { get; }
        public HitInfo HitInfo { get; }

        /// <summary> Входной урон </summary>
        public float BaseDamage { get; }

        /// <summary> Текущий урон в пайплайне </summary>
        public float Damage { get; set; }
        
        /// <summary> Финальный урон после всех модификаторов </summary>
        public float FinalDamage { get; set; }

        /// <summary> Пробитие брони (0..1) </summary>
        public float ArmorPenetration { get; set; }

        /// <summary> Был ли урон отменён </summary>
        public bool IsCancelled { get; private set; }
        

        public DamageContext(
            IUnitSceneContext attacker,
            IUnitSceneContext target,
            float damage,
            DamageType type,
            HitInfo hitInfo)
        {
            Attacker = attacker;
            Target = target;
            BaseDamage = damage;
            Damage = damage;
            Type = type;
            HitInfo = hitInfo;
            
            // 🔹 дефолт
            ArmorPenetration = 0f;
        }

        public void Cancel() => IsCancelled = true;
    }
}
