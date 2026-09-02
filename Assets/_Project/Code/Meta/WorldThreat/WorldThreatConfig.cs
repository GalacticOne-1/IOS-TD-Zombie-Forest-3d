using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1.Code.Gameplay.WorldThreatConfig
{
    [CreateAssetMenu(
        fileName = "WorldThreatConfig",
        menuName = "Game Configs/World Map/Threat Config")]
    public sealed class WorldThreatConfig : ScriptableObject
    {
        [SerializeField] private bool testThreat;
        
        [Header("Initial Threat")]
        
        [SerializeField] [Min(0)] private int initialQuietDaysMin = 2;
        [SerializeField] [Min(0)] private int initialQuietDaysMax = 3;

        [SerializeField] [Min(0)] private int initialPreparationDaysMin = 2;
        [SerializeField] [Min(0)] private int initialPreparationDaysMax = 2;

        [Header("Recurring Threats")]

        [Tooltip("Период полного затишья после предыдущей атаки.")]
        [SerializeField] [Min(0)] private int quietDaysMin = 2;
        [SerializeField] [Min(0)] private int quietDaysMax = 5;

        [Header("Preparation After Quiet")]

        [Tooltip("Если период тишины >= этого значения, используется FastAttack.")]
        [SerializeField] private int longQuietThreshold = 4;

        [FormerlySerializedAs("fastPreparationMin")]
        [Header("Fast Attack")]

        [Tooltip("Долгая тишина → короткая подготовка.")]
        [SerializeField] [Min(0)] private int fastAttackDaysMin = 2;
        [FormerlySerializedAs("fastPreparationMax")] [SerializeField] [Min(0)] private int fastAttackDaysMax = 4;

        [FormerlySerializedAs("slowPreparationMin")]
        [Header("Slow Attack")]

        [Tooltip("Короткая тишина → длинная подготовка.")]
        [SerializeField] [Min(0)] private int slowAttackDaysMin = 4;
        [FormerlySerializedAs("slowPreparationMax")] [SerializeField] [Min(0)] private int slowAttackDaysMax = 7;


        public bool TestThreat => testThreat;

        public int InitialQuietDaysMin => initialQuietDaysMin;
        public int InitialQuietDaysMax => initialQuietDaysMax;

        public int InitialPreparationDaysMin => initialPreparationDaysMin;
        public int InitialPreparationDaysMax => initialPreparationDaysMax;

        public int QuietDaysMin => quietDaysMin;
        public int QuietDaysMax => quietDaysMax;

        public int LongQuietThreshold => longQuietThreshold;

        public int FastAttackDaysMin => fastAttackDaysMin;
        public int FastAttackDaysMax => fastAttackDaysMax;

        public int SlowAttackDaysMin => slowAttackDaysMin;
        public int SlowAttackDaysMax => slowAttackDaysMax;
    }
}