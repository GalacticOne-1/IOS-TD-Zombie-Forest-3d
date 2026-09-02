using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    /// <summary>
    /// Runtime-конфигурация поведенческого слоя PlayerCommandBrain.
    ///
    /// НЕ относится к perception.
    /// НЕ содержит сенсорных параметров.
    ///
    /// Отвечает только за:
    ///   - авто-агрегацию врагов
    ///   - реакцию на угрозу
    ///   - поведенческие тайминги FSM
    /// </summary>
    [CreateAssetMenu(menuName = "Game Configs/AI/Player Brain Config")]
    public sealed class PlayerBrainConfig : ScriptableObject
    {
        /// <summary>
        /// Дистанция авто-вступления в бой при наличии подходящего оружия.
        /// </summary>
        [Header("Auto Engage")]
        public float autoEngageRange = 15f;

        /// <summary>
        /// Дистанция поиска укрытия при отсутствии агрессии.
        /// </summary>
        [Header("Cover")]
        public float autoCoverRange = 20f;

        /// <summary>
        /// Задержка перед повторным вступлением в бой после выхода из боя.
        /// Используется как анти-flicker hysteresis.
        /// </summary>
        [Header("Re-engage")]
        public float reEngageDelay = 1.5f;
    }
}