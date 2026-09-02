using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    /// <summary>
    /// Пассивный сервис построения пути.
    /// 
    /// Единственная обязанность: построить путь по запросу и уведомить.
    /// Не знает ничего о движении, сегментах, центре, прогрессе.
    /// 
    /// Заменяет SquadPathAgent. Убраны: Tick(), NodeIndex, _nodeIndex,
    /// RealignToPosition(), RequestPath(), BindCenterProvider().
    /// </summary>
    public sealed class SquadPathService : MonoBehaviour
    {
        private Seeker _seeker;

        /// <summary>
        /// Только для SquadTrailRenderer — не использовать в логике движения.
        /// </summary>
        public IReadOnlyList<Vector3> LastPath { get; private set; }

        /// <summary>
        /// Срабатывает один раз при успешном получении пути.
        /// MovementSystem подписывается и управляет дальше.
        /// </summary>
        public event Action<IReadOnlyList<Vector3>> OnPathReady;

        private void Awake() => _seeker = GetComponent<Seeker>();

        public void SetTarget(Vector3 from, Vector3 to)
        {
            _seeker.StartPath(from, to, OnPath);
        }

        private void OnPath(Path path)
        {
            if (path == null || path.error) return;
            LastPath = path.vectorPath;
            OnPathReady?.Invoke(path.vectorPath);
        }

        public void Clear() => LastPath = null;
    }
}