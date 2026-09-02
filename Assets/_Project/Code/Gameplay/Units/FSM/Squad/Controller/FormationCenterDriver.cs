using System.Collections.Generic;
using Galactic1.Code.Gameplay.Construction;
using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    /// <summary>
    /// Единственный владелец: Path, CurrentSegment, Center,
    /// MovementHeading, FormationHeading.
    ///
    /// MovementHeading — направление движения центра.
    ///   Обновляется сразу при смене сегмента пути.
    ///   Используется только для движения центра.
    ///
    /// FormationHeading — направление ориентации формации.
    ///   Плавно догоняет MovementHeading со скоростью TurnSpeed рад/с.
    ///   Используется только FormationFollower.
    ///   Формация не разворачивается мгновенно → нет массового перестроения
    ///   → ComputeSpeed() не видит большой ошибки → центр не тормозит.
    ///
    /// Catch-up throttle:
    ///   Заменён на плавную нелинейную функцию вместо бинарного порога.
    ///   Небольшое рассогласование не влияет на скорость.
    ///   Критическое рассогласование снижает скорость постепенно.
    ///   Центр никогда «не ждёт» полного перестроения формации.
    /// </summary>
    public sealed class FormationCenterDriver : System.IDisposable
    {
        // ── Config ──────────────────────────────────────────────────────────

        private const float NodeReachDistance = 0.7f;

        /// <summary>
        /// Скорость поворота FormationHeading, рад/с.
        /// 1.2 рад/с ≈ 70°/с → при беге (5 м/с) поворот на 90° за ~1.3 с.
        /// Достаточно быстро чтобы формация не отставала,
        /// достаточно медленно чтобы не разворачиваться мгновенно.
        /// </summary>
        private const float TurnSpeed = 1.2f;

        /// <summary>
        /// Ошибка (м), ниже которой catch-up throttle не действует вообще.
        /// </summary>
        private const float ThrottleDeadZone = 2.0f;

        /// <summary>
        /// Ошибка (м), при которой скорость достигает минимума.
        /// </summary>
        private const float ThrottleMaxError = 10.0f;

        /// <summary>
        /// Минимальный множитель скорости при максимальной ошибке.
        /// 0.5 вместо старых 0.35 — центр никогда не останавливается.
        /// </summary>
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
        public Vector3 Center => _runtime.Center;

        /// <summary>Направление движения центра. Доступно FormationFollower через runtime.</summary>
        public Vector3 MovementHeading => _runtime.Forward;

        /// <summary>Направление ориентации формации. Плавно догоняет MovementHeading.</summary>
        public Vector3 FormationHeading => _runtime.FormationHeading;
        
        /// Снимок состояния для визуализации: геометрия + текущий прогресс
        /// по пути. Единственная точка доступа для SquadTrailRenderer.
        /// Не раскрывает _path/_segment напрямую — только готовые данные.
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

        public void Begin(float speed)
        {
            _baseSpeed = speed;
            _path = null;
            _segment = 0;
            _active = false;
        }

        public void Stop() => _active = false;

        public void SetPath(IReadOnlyList<Vector3> path)
        {
            if (path == null || path.Count == 0) return;
            _path = path;

            if (_path.Count == 1)
            {
                _runtime.Center = _path[0];
                _active = false;
                return;
            }

            // DebugScene.CreateSphere(
            //     _path[_path.Count-1], 
            //     Color.red,
            //     .2f,
            //     true,
            //     2f);
            _segment = FindClosestSegment(_runtime.Center);
            _geometry = TrailGeometryBuilder.Build(_path);
            _active = true;
        }

        // ── Tick ────────────────────────────────────────────────────────────

        public void Tick(SquadSlot[] slots, float deltaTime)
        {
            if (!_active || _path == null) return;

            // 1. Плавно поворачиваем FormationHeading к MovementHeading
            RotateFormationHeading(deltaTime);

            // 2. Двигаем центр по пути
            while (_segment < _path.Count - 1)
            {
                Vector3 target = _path[_segment + 1];
                Vector3 toTarget = target - _runtime.Center;
                float dist = toTarget.magnitude;

                if (dist < NodeReachDistance)
                {
                    _runtime.Center = target;
                    _segment++;
                    continue;
                }

                // MovementHeading — сразу по текущему сегменту
                if (toTarget.sqrMagnitude > 0.001f)
                    _runtime.Forward = toTarget.normalized;

                float speed = ComputeSpeed(slots);
                _runtime.Center = Vector3.MoveTowards(
                    _runtime.Center, target, speed * deltaTime);

                return;
            }

            _runtime.Center = _path[^1];
            _active = false;
        }

        // ── FormationHeading rotation ────────────────────────────────────────

        /// <summary>
        /// Вращает FormationHeading к MovementHeading с ограниченной скоростью.
        /// RotateTowards работает в радианах/сек через maxRadiansDelta.
        /// </summary>
        private void RotateFormationHeading(float deltaTime)
        {
            _runtime.FormationHeading = Vector3.RotateTowards(
                _runtime.FormationHeading,
                _runtime.Forward, // MovementHeading
                TurnSpeed * deltaTime, // рад/с
                0f); // без изменения длины
        }

        // ── Catch-up throttle ────────────────────────────────────────────────

        /// <summary>
        /// Нелинейное замедление по максимальной ошибке агентов.
        ///
        /// [0, DeadZone]           → множитель 1.0 (нет замедления)
        /// [DeadZone, MaxError]    → плавный SmoothStep от 1.0 до MinMultiplier
        /// [MaxError, ∞)           → множитель MinMultiplier (потолок замедления)
        ///
        /// SmoothStep даёт плавный вход и выход без резких переключений.
        /// Центр никогда не останавливается — только плавно тормозит.
        /// </summary>
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

            // Нормализуем ошибку в [0, 1] на диапазоне [DeadZone, MaxError]
            float t = Mathf.Clamp01(
                (maxError - ThrottleDeadZone) /
                (ThrottleMaxError - ThrottleDeadZone));

            // SmoothStep: плавный S-образный переход
            float smooth = t * t * (3f - 2f * t);

            float multiplier = Mathf.Lerp(1f, ThrottleMinMultiplier, smooth);
            return _baseSpeed * multiplier;
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private int FindClosestSegment(Vector3 worldPos)
        {
            int best = 0;
            float bestDist = float.MaxValue;
            for (int i = 0; i < _path.Count - 1; i++)
            {
                Vector3 mid = (_path[i] + _path[i + 1]) * 0.5f;
                float dist = Vector3.Distance(worldPos, mid);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = i;
                }
            }
            return best;
        }
        
        public void ClearTrail()
        {
            _geometry = TrailGeometry.Invalid;
        }
    }
}