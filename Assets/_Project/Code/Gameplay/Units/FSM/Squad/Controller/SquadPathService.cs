using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    /// <summary>
    /// Пассивный сервис построения пути.
    ///
    /// — Путь копируется: path.vectorPath принадлежит пулу A* и может быть
    ///   переиспользован следующим запросом, пока драйвер по нему едет.
    /// — Устаревшие ответы (если успели отправить новый запрос) игнорируются.
    /// — Ошибка пути сообщается через OnPathFailed.
    /// </summary>
    public sealed class SquadPathService : MonoBehaviour
    {
        private Seeker _seeker;
        private int _requestId;

        /// <summary>Только для SquadTrailRenderer.</summary>
        public IReadOnlyList<Vector3> LastPath { get; private set; }

        public event Action<IReadOnlyList<Vector3>> OnPathReady;
        public event Action OnPathFailed;

        private void Awake() => _seeker = GetComponent<Seeker>();

        public void SetTarget(Vector3 from, Vector3 to)
        {
            int id = ++_requestId;
            _seeker.StartPath(from, to, p => OnPath(p, id));
        }

        private void OnPath(Path path, int id)
        {
            if (id != _requestId)
                return; // устаревший ответ

            if (path == null || path.error || path.vectorPath == null || path.vectorPath.Count == 0)
            {
                OnPathFailed?.Invoke();
                return;
            }

            var copy = new List<Vector3>(path.vectorPath);
            LastPath = copy;
            OnPathReady?.Invoke(copy);
        }

        public void Clear() => LastPath = null;
    }
}