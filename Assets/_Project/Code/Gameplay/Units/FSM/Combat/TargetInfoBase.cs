using System.Collections.Generic;
using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    public abstract class TargetInfoBase : MonoBehaviour, ITargetInfo
    {
        [SerializeField] private Transform _aimPoint;

        // NEW — Siege-specific. _aimPoint переиспользуется как контейнер:
        // если у него есть дочерние Transform (например HQ с
        // AttackPoint_00.._09), они собираются здесь. Для обычных целей
        // (одиночный _aimPoint без детей) список просто пуст и ни на что
        // не влияет — AimPoint ниже не меняется.
        private readonly List<Transform> _attackPoints = new();

        public string TargetId { get; private set; }
        public IUnitSceneContext Unit { get; private set; }
        public virtual bool IsDead => Unit?.Stats.IsDead ?? true;
        public Vector3 Position => transform.position;

        public Vector3 AimPoint =>
            _aimPoint != null
                ? _aimPoint.position
                : transform.position;

        /// <summary>NEW — только для Siege HQ. Обычные цели возвращают
        /// пустой список без побочных эффектов.</summary>
        public IReadOnlyList<Transform> AttackPoints => _attackPoints;
        
        /// <summary>
        /// Возвращает ближайшую физическую точку цели
        /// относительно указанной позиции.
        ///
        /// Базовая реализация сохраняет старое поведение:
        /// для целей без специальной геометрии используется центр объекта.
        /// </summary>
        public virtual Vector3 GetClosestPoint(Vector3 fromPosition)
            => transform.position;


        
        public virtual void Initialize(IUnitSceneContext unit)
        {
            Unit = unit;

            TargetId = Unit?.Id;

            if (string.IsNullOrEmpty(TargetId))
                TargetId = System.Guid.NewGuid().ToString();

            // NEW — собираем attack points один раз при Initialize,
            // гарантированно до первого использования Siege AI.
            _attackPoints.Clear();
            if (_aimPoint != null)
            {
                foreach (Transform child in _aimPoint)
                {
                    if (child != null)
                        _attackPoints.Add(child);
                }
            }
        }
    }
}