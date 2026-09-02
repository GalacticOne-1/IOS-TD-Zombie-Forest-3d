
using System;
using UnityEngine;

namespace Galactic1.Gameplay.Death
{
    /// <summary>
    /// События, которые поднимает система смерти.
    /// Используются другими системами (UI, аналитика, квесты).
    /// </summary>
    public static class DeathEvents
    {
        /// <summary>
        /// Вызывается когда игрок умирает. Параметр — позиция смерти и ссылка на игрок (если нужно).
        /// </summary>
        public static event Action<Vector3> OnPlayerDied;

        /// <summary>
        /// Вызывается когда игрок был успешно респанут.
        /// </summary>
        public static event Action<Vector3> OnPlayerRespawned;

        public static void RaisePlayerDied(Vector3 pos) => OnPlayerDied?.Invoke(pos);
        public static void RaisePlayerRespawned(Vector3 pos) => OnPlayerRespawned?.Invoke(pos);
    }
}