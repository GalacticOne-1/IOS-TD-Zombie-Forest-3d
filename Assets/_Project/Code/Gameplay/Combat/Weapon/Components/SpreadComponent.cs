using Galactic1.Core.Enums;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Weapons.Logic
{
    public sealed class SpreadComponent : WeaponComponentBase
    {
        public float CurrentSpreadDeg { get; private set; }

        private IOwnerStatsProvider _owner;
        private WeaponEntity _entity;

        public SpreadComponent(IOwnerStatsProvider owner)
        {
            _owner = owner;
        }

        public override void OnEquip(WeaponEntity entity) => _entity = entity;

        public override void Tick(float dt)
        {
            if (_entity == null) return;
            var def = _entity.Definition;

            float accuracy = _owner?.GetAccuracyModifier() ?? 1f;
            float stress = _owner?.GetStressLevel() ?? 0f;
            bool isMoving = _owner?.IsMoving() ?? false;

            CurrentSpreadDeg = def.BaseSpreadDeg
                               * (1f + (1f - accuracy))
                               * (isMoving ? def.MovingSpreadMul : 1f)
                               * (1f + stress / 100f * def.StressSpreadMul);
        }

        /// <summary>
        /// Возвращает итоговый разброс для выстрела на заданной дистанции.
        /// Читает EffectiveRange / MaxRange / MaxRangeSpreadPenalty из Definition напрямую.
        /// FireComponent не передаёт WeaponDefinitionData — SpreadComponent знает свою entity.
        /// </summary>
        public float GetSpreadForDistance(float distance)
        {
            var def = _entity.Definition;

            // * дробовик и так на дистанции увеличивает разброс
            // штраф за дальность сделан через понижение урона либо вообще нет!
            if (def.WeaponType == WeaponType.Shotgun)
                return CurrentSpreadDeg;

            float rangePenalty = ComputeRangePenalty(
                distance,
                def.EffectiveRange,
                def.MaxRange,
                def.MaxRangeSpreadPenalty);

            return CurrentSpreadDeg * rangePenalty;
        }

        /// <summary>
        /// Чистая функция — выделена для читаемости и тестируемости.
        ///
        /// distance &lt;= effectiveRange          → 1f        (штрафов нет)
        /// effectiveRange &lt; distance &lt; maxRange → плавный рост
        /// distance &gt;= maxRange               → maxPenalty (потолок штрафа)
        ///
        /// Защита от кривого конфига: если maxRange &lt;= effectiveRange → всегда 1f.
        /// </summary>
        private static float ComputeRangePenalty(
            float distance,
            float effectiveRange,
            float maxRange,
            float maxPenalty)
            => ComputeRangePenaltyStatic(distance, effectiveRange, maxRange, maxPenalty);

        /// <summary>
        /// Публичная статическая версия — используется симулятором и тестами.
        /// Единственная реализация формулы: приватный метод делегирует сюда.
        /// </summary>
        public static float ComputeRangePenaltyStatic(
            float distance,
            float effectiveRange,
            float maxRange,
            float maxPenalty)
        {
            if (maxRange <= effectiveRange) return 1f; // защита от кривого конфига
            if (distance <= effectiveRange) return 1f; // в зоне комфорта — без штрафа

            float t = Mathf.InverseLerp(effectiveRange, maxRange, distance);
            return Mathf.Lerp(1f, maxPenalty, t);
        }
    }
}