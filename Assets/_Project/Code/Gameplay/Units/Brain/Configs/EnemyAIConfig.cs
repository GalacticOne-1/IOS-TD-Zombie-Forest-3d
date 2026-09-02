using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Game.Meta.Enemy
{
    [CreateAssetMenu(fileName = "ZombieAI", menuName = "Game Configs/Enemy/Zombie AI")]
    public sealed class EnemyAIConfig : ScriptableObject
    {
        [Header("Brain")] public float ThinkInterval = 0.2f;

        [Header("Roaming")] public float RoamRadius = 8f;
        public float WaypointRadius = 1f;

        [Header("Behaviour")] public bool UsePackBehaviour = true;

        [Header("Utility — Action Weights")]
        [Tooltip("Оставьте пустым — все action'ы включены с weight=1.\n" +
                 "Enabled=false полностью отключает action.\n" +
                 "Weight [0..5] множитель raw score при арбитраже.\n" +
                 "Примеры: Chase weight=2 → агрессивный архетип. " +
                 "Investigate enabled=false → слепой к звукам.")]
        public List<ActionWeightEntry> ActionWeights;

        // AllowRoaming и AllowInvestigation удалены.
        // Для отключения используй ActionWeights с Enabled=false на Roam/Investigate.
        // Единственный источник истины — ActionWeightEntry.Enabled.
    }

    [System.Serializable]
    public sealed class ActionWeightEntry
    {
        public AIActionType Action;

        [Tooltip("Множитель при выборе победителя. 1 = нейтральный. 0 = отключить через weight.")] [Range(0f, 5f)]
        public float Weight = 1f;

        [Tooltip("false = action пропускается полностью (Brain его игнорирует).")]
        public bool Enabled = true;
    }
}