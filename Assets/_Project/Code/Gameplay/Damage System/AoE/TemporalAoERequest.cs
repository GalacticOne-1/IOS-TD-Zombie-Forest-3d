
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.AoE
{
    /// <summary>
    /// Запрос на создание временной AoE-зоны.
    /// Передаётся из GrenadeProjectile.Explode() в TemporalAoEService.
    /// Не содержит логики — только данные.
    /// </summary>
    public struct TemporalAoERequest
    {
        /// <summary>Источник урона для DamageResolver.</summary>
        public ISceneUnit Attacker;

        /// <summary>Центр зоны.</summary>
        public Vector3 Position;

        /// <summary>Радиус проверки перекрытия.</summary>
        public float Radius;

        /// <summary>Тип эффекта зоны.</summary>
        public TemporalAoEType Type;
        public VfxId VfxId;
        public bool VfxSelfDuration;

        /// <summary>Общая длительность зоны в секундах.</summary>
        public float Duration;

        /// <summary>Урон за один тик (Burn, Electric).</summary>
        public float DamagePerTick;

        /// <summary>Интервал между тиками урона в секундах.</summary>
        public float TickInterval;

        /// <summary>Множитель скорости для Electric (0..1).</summary>
        public float SpeedMultiplier;

        /// <summary>Длительность стана для Concussive.</summary>
        public float StunDuration;

        /// <summary>Маска целей (обычно LayerService.Damageable).</summary>
        public LayerMask TargetMask;
    }
}