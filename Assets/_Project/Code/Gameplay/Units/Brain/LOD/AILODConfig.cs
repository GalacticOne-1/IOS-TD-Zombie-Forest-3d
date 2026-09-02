using UnityEngine;

namespace Galactic1.Code.Gameplay.AI.LOD
{
    /// <summary>
    /// Authoring-конфиг AI LOD системы.
    ///
    /// Правило: все "магические числа" LOD-решений живут здесь,
    /// а не в AILODSystem/UnitInstance — их надо балансить дизайнерам
    /// без пересборки кода.
    /// </summary>
    [CreateAssetMenu(
        fileName = "AILODConfig",
        menuName = "Game Configs/AI/AI LOD Config")]
    public sealed class AILODConfig : ScriptableObject
    {
        [Header("Evaluation cadence")]
        [Tooltip("Как часто AILODSystem пересчитывает уровни всех врагов. НЕ каждый кадр.")]
        public float EvaluationInterval = 0.25f;

        [Header("Distance thresholds (от центра отряда)")]
        [Tooltip("Внутри этого радиуса — всегда Full, независимо от прочих факторов.")]
        public float FullSimulationRadius = 25f;

        [Tooltip("Между FullSimulationRadius и этим радиусом — Low. Дальше — Sleeping.")]
        public float LowSimulationRadius = 45f;

        [Header("Raid Director integration")]
        [Tooltip("Враг, заспавненный Director'ом, не может уснуть это время после спавна, " +
                 "чтобы не сломать намеренный директорский спавн-энкаунтер.")]
        public float DirectorSpawnGracePeriod = 5f;

        [Header("Low LOD (зарезервировано на будущее)")] [Tooltip("Интервал 'мышления' Brain на Low LOD.")]
        public float LowBrainThinkInterval = 1f;

        [Tooltip("Интервал обновления Perception на Low LOD.")]
        public float LowPerceptionInterval = 1f;
    }
}