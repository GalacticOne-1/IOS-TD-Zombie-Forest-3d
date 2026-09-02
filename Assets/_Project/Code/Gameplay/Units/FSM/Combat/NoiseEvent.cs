using Galactic1.Code.Gameplay.Units;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Noise
{
    public enum NoiseType
    {
        Gunshot,
        Explosion,
        Running,
        Melee,
        Footstep,
    }

    /// <summary>
    /// Immutable noise event. Передаётся через NoiseSystem.Emit().
    /// </summary>
    public readonly struct NoiseEvent
    {
        /// <summary>Мировая позиция источника звука.</summary>
        public readonly Vector3 Position;

        /// <summary>Радиус распространения (метры).</summary>
        public readonly float Radius;

        public readonly NoiseType Type;

        /// <summary>Интенсивность [0..1]. Влияет на aggro score.</summary>
        public readonly float Intensity;

        /// <summary>
        /// Источник шума (опционально). Позволяет AI определить attacker.
        /// null для взрывов и окружения.
        /// </summary>
        public readonly ITargetInfo Source;

        public NoiseEvent(
            Vector3 position,
            float radius,
            NoiseType type,
            float intensity = 1f,
            ITargetInfo source = null)
        {
            Position = position;
            Radius = radius;
            Type = type;
            Intensity = intensity;
            Source = source;
        }
    }

    /// <summary>
    /// Реализуется слушателями шума (ZombieInstance).
    /// NoiseSystem не знает о gameplay деталях слушателей.
    /// </summary>
    public interface INoiseListener
    {
        /// <summary>Мировая позиция слушателя для spatial query.</summary>
        Vector3 Position { get; }

        /// <summary>Вызывается когда шум попал в радиус слушателя.</summary>
        void OnNoiseHeard(NoiseEvent evt);
    }
}