using UnityEngine;

namespace Galactic1.Code.Gameplay.RaidDirector
{
    [CreateAssetMenu(fileName = "DirectorConfig", menuName = "Game Configs/Raid/DirectorConfig")]
    public sealed class DirectorConfig : ScriptableObject
    {
        [Header("Threat — Decay")] [Tooltip("Угроза снижается на X единиц в секунду при бездействии")]
        public float ThreatDecayPerSecond = 2f;

        public float ThreatMin = 0f;
        public float ThreatMax = 100f;

        [Header("Threat — Noise Weights")] public float FootstepWeight = 0.1f;
        public float RunningWeight = 0.3f;
        public float MeleeWeight = 0.5f;
        public float GunshotWeight = 1.0f;
        public float ExplosionWeight = 8.0f;

        [Header("Threat — Event Weights")] public float KillThreatWeight = 0.5f;
        public float PlayerDamagedThreatReduction = 1.5f;

        [Tooltip("На сколько снижается Threat после успешной отправки группы")]
        public float ThreatReductionAfterSpawn = 5f;

        [Header("Director States — пороги Threat")]
        public float ThresholdSearching = 20f;

        public float ThresholdPressure = 40f;
        public float ThresholdHunting = 70f;

        [Header("Spawn — Budget")] [Tooltip("Максимум живых врагов заспавненных Director одновременно")]
        public int MaxAliveFromDirector = 8;

        [Tooltip("Глобальный лимит живых врагов на карте (включая Static и Wave). " +
                 "Director не спавнит если превышен.")]
        public int GlobalAliveEnemyLimit = 20;

        [Tooltip("Минимальный Threat для начала спавна")]
        public float MinThreatToSpawn = 20f;

        [Header("Spawn — Cooldown")] public float SpawnCooldown = 20f;

        [Header("Spawn — Group Size")] public int GroupSizeSearching = 2;
        public int GroupSizePressure = 3;
        public int GroupSizeHunting = 5;

        [Tooltip("Абсолютный максимум врагов в одной отправке")]
        public int MaxGroupSize = 6;

        [Header("Spawn — Position")] public float MinSpawnDistance = 30f;
        public float MaxSpawnDistance = 80f;
        public int SpawnPositionAttempts = 10;
    }
}