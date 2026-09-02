using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1.Code.Gameplay.AoE
{
    /// <summary>
    /// Базовый конфиг временного AoE эффекта (огонь, газ, электрика и т.д.)
    /// Используется всеми зональными гранатами.
    /// </summary>
    public abstract class AreaEffectConfig : ScriptableObject
    {
        [Header("Lifetime")]

        [Tooltip("Время существования эффекта. -1 = существует пока не будет уничтожен владельцем.")]
        public float duration = 6f;

        [Tooltip("Интервал между тиками эффекта.")]
        public float tickInterval = 0.5f;
        
        [Tooltip("true - окончание эффекта не зависит от duration зоны")]
        public bool vfxSelfDuration;

        
        //────────────────────────────────────────────
        // Shape
        //────────────────────────────────────────────
        
        [Header("Area")]
        public float radius = 3f;
        
        
        //────────────────────────────────────────────
        // Trigger
        //────────────────────────────────────────────

        [Header("Activation")]

        [Tooltip("Как активируется эффект.")]
        public AreaEffectTrigger trigger = AreaEffectTrigger.Area;

        
        //────────────────────────────────────────────
        // Combat
        //────────────────────────────────────────────
        
        [Header("Combat")]
        public float damagePerTick = 10f;

        [Tooltip("Множитель скорости (0..1), если применимо")]
        [Range(0f, 1f)]
        public float speedMultiplier = 1f;

        [Tooltip("Длительность стана (если эффект поддерживает)")]
        public float stunDuration = 0f;

    }
    
    public enum AreaEffectTrigger
    {
        /// <summary>
        /// Эффект действует на всю область.
        /// Пример:
        /// - огонь
        /// - газ
        /// - кислота
        /// </summary>
        Area,

        /// <summary>
        /// Эффект применяется только при контакте.
        /// Пример:
        /// - колья
        /// - электрический забор
        /// </summary>
        Contact
    }
}