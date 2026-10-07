using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    /// <summary>
    /// Единственный владелец: Path, CurrentSegment, Center,
    /// MovementHeading (Forward), FormationHeading.
    ///
    /// Центр движется строго по ломаной: остаток дистанции кадра переносится
    /// на следующий сегмент, вершины не «срезаются» и не телепортируются.
    ///
    /// FormationHeading поворачивается по yaw с ограниченной скоростью
    /// (всё в плоскости XZ, без наклона формации и без неоднозначной оси
    /// при противоположных векторах).
    ///
    /// Новый путь заменяет старый на лету (SetPath), а Retarget() меняет
    /// только скорость — поэтому повторная команда не останавливает центр.
    /// </summary>
    public sealed class FormationCenterDriver : System.IDisposable
    {
        // ── Config ──────────────────────────────────────────────────────────

        /// <summary>Скорость поворота FormationHeading, рад/с.</summary>
        private const float TurnSpeed = 1.2f;

        private const float ThrottleDeadZone = 2.0f;
        private const float ThrottleMaxError = 10.0f;
        private const float ThrottleMinMultiplier = 0.5f;

        // ── References ──────────────────────────────────────────────────────

        private readonly SquadFormationRuntime _runtime;
        private readonly SquadPathService _pathService;

        // ── Path state ──────────────────────────────────────────────────────

        private IReadOnlyList<Vector3> _path;
        private TrailGeometry _geometry = TrailGeometry.Invalid;
        private int _segment;

        // ── Motion state ────────────────────────────────────────────────────

        private float _baseSpeed;
        private bool _active;

        public bool Finished => !_active;

        /// <summary>Увеличивается при каждом принятом пути (включая путь из одной точки).</summary>
        public int PathSerial { get; private set; }

        public Vector3 Center => _runtime.Center;
        public Vector3 MovementHeading => _runtime.Forward;
        public Vector3 FormationHeading => _runtime.FormationHeading;

        /// <summary>
        /// Снимок для SquadTrailRenderer: геометрия + прогресс по пути.
        /// </summary>
        public TrailRenderSnapshot RenderSnapshot =>
            _geometry.IsValid
                ? new TrailRenderSnapshot(
                    _geometry,
                    _segment,
                    _runtime.NavigationCenter,
                    _runtime.VisualCenter,
                    _runtime.FormationHeading)
                : TrailRenderSnapshot.Invalid;

        // ── Lifecycle ───────────────────────────────────────────────────────

        public FormationCenterDriver(
            SquadFormationRuntime runtime,
            SquadPathService pathService)
        {
            _runtime = runtime;
            _pathService = pathService;
            _pathService.OnPathReady += SetPath;
        }

        public void Dispose()
        {
            _pathService.OnPathReady -= SetPath;
        }

        // ── Control API ─────────────────────────────────────────────────────

        /// <summary>Старт из покоя: сбрасывает путь и ждёт новый.</summary>
        public void Begin(float speed)
        {
            _baseSpeed = speed;
            _path = null;
            _segment = 0;
            _active = false;
        }

        /// <summary>Повторная команда во время движения: путь не трогаем.</summary>
        public void Retarget(float speed)
        {
            _baseSpeed = speed;
        }

        public void Stop() => _active = false;

        public void SetPath(IReadOnlyList<Vector3> path)
        {
            if (path == null || path.Count == 0) return;

            _path = path;
            _segment = 0;
            PathSerial++;

            if (path.Count == 1)
            {
                // Цель совпадает с текущей позицией — двигаться некуда.
                _geometry = TrailGeometry.Invalid;
                _active = false;
                return;
            }

            _geometry = TrailGeometryBuilder.Build(_path);
            _active = true;
        }

        // ── Tick ────────────────────────────────────────────────────────────

        public void Tick(SquadSlot[] slots, float deltaTime)
        {
            if (!_active || _path == null) return;

            RotateFormationHeading(deltaTime);

            float remaining = ComputeSpeed(slots) * deltaTime;
            int last = _path.Count - 1;

            while (_segment < last && remaining > 0f)
            {
                Vector3 to = _path[_segment + 1] - _runtime.Center;
                float dist = to.magnitude;

                if (dist > 1e-4f)
                    _runtime.Forward = to / dist;

                if (dist <= remaining)
                {
                    _runtime.Center = _path[_segment + 1];
                    remaining -= dist;
                    _segment++;
                }
                else
                {
                    _runtime.Center += _runtime.Forward * remaining;
                    remaining = 0f;
                }
            }

            if (_segment >= last)
                _active = false;
        }

        // ── FormationHeading rotation ───────────────────────────────────────

        private void RotateFormationHeading(float deltaTime)
        {
            Vector3 target = _runtime.Forward;
            target.y = 0f;
            if (target.sqrMagnitude < 1e-6f) return;

            Vector3 current = _runtime.FormationHeading;
            current.y = 0f;
            if (current.sqrMagnitude < 1e-6f)
            {
                _runtime.FormationHeading = target.normalized;
                return;
            }

            float curYaw = Mathf.Atan2(current.x, current.z) * Mathf.Rad2Deg;
            float tgtYaw = Mathf.Atan2(target.x, target.z) * Mathf.Rad2Deg;
            float yaw = Mathf.MoveTowardsAngle(
                curYaw, tgtYaw, TurnSpeed * Mathf.Rad2Deg * deltaTime) * Mathf.Deg2Rad;

            _runtime.FormationHeading = new Vector3(Mathf.Sin(yaw), 0f, Mathf.Cos(yaw));
        }

        // ── Catch-up throttle ───────────────────────────────────────────────

        private float ComputeSpeed(SquadSlot[] slots)
        {
            float maxError = 0f;
            foreach (var slot in slots)
            {
                if (slot.Occupant == null) continue;
                float e = Vector3.Distance(
                    slot.Occupant.transform.position,
                    slot.FinalWorldPosition);
                if (e > maxError) maxError = e;
            }

            if (maxError <= ThrottleDeadZone)
                return _baseSpeed;

            float t = Mathf.Clamp01(
                (maxError - ThrottleDeadZone) /
                (ThrottleMaxError - ThrottleDeadZone));

            float smooth = t * t * (3f - 2f * t);
            return _baseSpeed * Mathf.Lerp(1f, ThrottleMinMultiplier, smooth);
        }

        public void ClearTrail()
        {
            _geometry = TrailGeometry.Invalid;
        }
    }
}
