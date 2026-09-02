using UnityEngine;

namespace Galactic1.Code.Systems.CampDefense.Penalty
{
    /// <summary>
    /// Все коэффициенты штрафа за поражение в Camp Defense.
    /// Баланс меняется ТОЛЬКО через этот конфиг — без изменения кода Calculator.
    /// </summary>
    [CreateAssetMenu(
        fileName = "CampDefensePenaltyConfig",
        menuName = "Game Configs/Camp Defense/Penalty Config")]
    public sealed class CampDefensePenaltyConfig : ScriptableObject
    {
        [Header("Основной процент штрафа")]
        [Tooltip("Базовый процент, который пытаемся забрать от стака. Например 0.12 = 12%.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float _penaltyPercent = 0.12f;

        [Tooltip("Абсолютный потолок: сколько максимум % от стака можно забрать, даже если PenaltyPercent выше.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float _maximumPercent = 0.2f;

        [Header("Защита игрока от полного разорения")]
        [Tooltip("Если в стаке меньше этого количества — не трогаем стак вообще.")]
        [SerializeField]
        private int _minStackToSteal = 20;

        [Tooltip("Минимальный процент стака, который должен остаться после штрафа. Например 0.3 = 30%.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float _minimumPercentLeft = 0.3f;

        [Tooltip("Минимальное абсолютное количество единиц, которое должно остаться после штрафа.")] [SerializeField]
        private int _minimumUnitsLeft = 20;

        [Header("Фильтрация предметов (задел на будущее)")]
        [Tooltip("Зарезервировано под будущее расширение — редкие предметы по Tier.")]
        [SerializeField]
        private bool _ignoreRareItems = true;

        [Tooltip("Зарезервировано — экипировка не участвует в Soft Launch штрафе " +
                 "(уже отсекается фильтром по ItemLabel.Resource).")]
        [SerializeField]
        private bool _ignoreEquipment = true;

        [Tooltip("Зарезервировано — квестовые предметы не участвуют в Soft Launch штрафе " +
                 "(уже отсекается фильтром по ItemLabel.Resource).")]
        [SerializeField]
        private bool _ignoreQuestItems = true;

        public float PenaltyPercent => _penaltyPercent;
        public float MaximumPercent => _maximumPercent;
        public int MinStackToSteal => _minStackToSteal;
        public float MinimumPercentLeft => _minimumPercentLeft;
        public int MinimumUnitsLeft => _minimumUnitsLeft;
        public bool IgnoreRareItems => _ignoreRareItems;
        public bool IgnoreEquipment => _ignoreEquipment;
        public bool IgnoreQuestItems => _ignoreQuestItems;
    }
}