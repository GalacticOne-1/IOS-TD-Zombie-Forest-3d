using System.Collections.Generic;
using Galactic1.Code.Gameplay.Targeting;
using Galactic1.Code.Gameplay.Units.Definitions;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    public sealed class PhysicsPerception : MonoBehaviour, IPerception
    {
        // ── Debug ─────────────────────────────────────────────────────────
        [Header("Debug")]
        [SerializeField] private bool _debugLOSRays;
        [SerializeField] private float _debugRayDuration = 0f; // 0 = one frame
        [SerializeField] private Color _debugClearColor = Color.green;
        [SerializeField] private Color _debugBlockedColor = Color.red;
        
        private readonly List<ITargetInfo> _visibleList = new(16);
        private readonly Dictionary<string, ITargetInfo> _visibleById = new(16);
        private readonly Collider[] _buffer = new Collider[64];

        private float _nextUpdate;
        private Transform _eyePoint;
        private PerceptionDefinition _def;

        private LayerMask _detectableLayer;
        private LayerMask _occlusionLayer;
        
        private bool _isSleeping;

        // Pre-computed cosine threshold for FOV culling.
        // Computed once in Initialize — zero per-frame trig cost.
        private float _minDot;

        public PerceptionDefinition Def => _def;

        // ── Init ──────────────────────────────────────────────────────────

        public void Initialize(
            PerceptionDefinition definition,
            Transform eyePoint,
            LayerMask detectableLayer,
            LayerMask occlusionLayer)
        {
            _def = definition;
            _eyePoint = eyePoint != null ? eyePoint : transform;
            _detectableLayer = detectableLayer;
            _occlusionLayer = occlusionLayer;

            // Convert half-angle to dot-product threshold once.
            _minDot = Mathf.Cos(_def.ViewAngle * 0.5f * Mathf.Deg2Rad);

            // Stagger first scan so units spawned in the same frame
            // do not all scan on the same tick.
            _nextUpdate = Time.time + Random.Range(0f, definition.UpdateInterval);
        }

        // ── Tick ──────────────────────────────────────────────────────────

        public void Tick()
        {
            if (_isSleeping || Time.time < _nextUpdate) 
                return;
            _nextUpdate = Time.time + _def.UpdateInterval;
            Scan();
        }
        
        /// <summary>Полностью отключает сканирование. Список видимых целей очищается,
        /// чтобы никто не держал ссылки на устаревшие цели, пока враг спит.</summary>
        public void Sleep()
        {
            _isSleeping = true;
            _visibleList.Clear();
            _visibleById.Clear();
        }

        /// <summary>Возобновляет сканирование. Следующий скан ставится со случайной
        /// задержкой — так же, как при Initialize — чтобы массовое пробуждение
        /// (например, отряд подошёл к спящему кластеру) не дало CPU-спайк за 1 кадр.</summary>
        public void Wake()
        {
            _isSleeping = false;
            _nextUpdate = Time.time + Random.Range(0f, _def.UpdateInterval);
        }

        // ── IPerception ───────────────────────────────────────────────────

        public IReadOnlyList<ITargetInfo> GetVisibleTargets() => _visibleList;
        public bool HasVisibleTarget => _visibleList.Count > 0;

        public ITargetInfo GetNearestVisibleTarget()
        {
            ITargetInfo nearest = null;
            float bestDist = float.MaxValue;

            for (int i = 0; i < _visibleList.Count; i++)
            {
                float d = (_visibleList[i].Position - transform.position).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    nearest = _visibleList[i];
                }
            }

            return nearest;
        }

        public ITargetInfo GetTargetById(string targetId)
        {
            if (string.IsNullOrEmpty(targetId)) return null;
            _visibleById.TryGetValue(targetId, out var result);
            return result;
        }

        /// <summary>Combined distance + LOS check.</summary>
        public bool CanEngage(Vector3 origin, ITargetInfo target, float range)
        {
            if (target == null || target.IsDead) return false;
            if ((target.Position - origin).sqrMagnitude > range * range) return false;
            return HasLineOfSight(origin, target.AimPoint);
        }

        public ITargetInfo FindNearestInRange(Vector3 origin, float range)
        {
            ITargetInfo best = null;
            float bestDist = float.MaxValue;

            for (int i = 0; i < _visibleList.Count; i++)
            {
                var t = _visibleList[i];
                if (!CanEngage(origin, t, range)) continue;
                float d = (t.Position - origin).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = t;
                }
            }

            return best;
        }

        /// <summary>
        /// LOS raycast using pre-computed direction and distance.
        /// Avoids recomputing magnitude when the caller already has it.
        /// </summary>
        public bool HasLineOfSight(Vector3 origin, Vector3 dirNormalized, float distance)
        {
            if (distance <= 0.01f) return true;

            // Используем RaycastHit только если нужен дебаг-рисунок,
            // чтобы не платить за него в обычном режиме.
            if (_debugLOSRays)
            {
                bool hasHit = Physics.Raycast(origin, dirNormalized, out var hit, distance, _occlusionLayer);
                DrawDebugRay(origin, dirNormalized, distance, hasHit, hasHit ? hit.point : (Vector3?)null);
                return !hasHit;
            }

            return !Physics.Raycast(origin, dirNormalized, distance, _occlusionLayer);
        }

        /// <summary>
        /// Convenience overload for callers that only have a target position.
        /// Computes direction and distance internally.
        /// </summary>
        public bool HasLineOfSight(Vector3 origin, Vector3 targetPos)
        {
            Vector3 dir = targetPos - origin;
            float dist = dir.magnitude;
            if (dist <= 0.01f) return true;
            return !Physics.Raycast(origin, dir / dist, dist, _occlusionLayer);
        }
        
        // ── Debug draw ────────────────────────────────────────────────────

        /// <summary>
        /// Draws the LOS check ray: green up to the target if clear,
        /// or green up to the obstacle + red from obstacle to target if blocked.
        /// </summary>
        private void DrawDebugRay(Vector3 origin, Vector3 dirNormalized, float distance, bool blocked, Vector3? hitPoint)
        {
            if (!blocked)
            {
                Debug.DrawRay(origin, dirNormalized * distance, _debugClearColor, _debugRayDuration);
                return;
            }

            Vector3 target = origin + dirNormalized * distance;
            Vector3 point = hitPoint ?? target;

            // Свободный участок до препятствия — зелёным.
            Debug.DrawLine(origin, point, _debugClearColor, _debugRayDuration);
            // Перекрытый участок от препятствия до цели — красным.
            Debug.DrawLine(point, target, _debugBlockedColor, _debugRayDuration);
        }

        // ── Scan ──────────────────────────────────────────────────────────

        private void Scan()
        {
            _visibleList.Clear();
            _visibleById.Clear();

            // Overlap from eye point — consistent origin with LOS raycasts.
            int count = Physics.OverlapSphereNonAlloc(
                _eyePoint.position, _def.DetectionRadius, _buffer, _detectableLayer);

            // Saturated buffer means results were silently truncated.
            if (count == _buffer.Length)
                Debug.LogWarning(
                    $"[PhysicsPerception] {name}: overlap buffer saturated " +
                    $"({_buffer.Length} colliders). Increase buffer size or " +
                    $"reduce DetectionRadius.");

            for (int i = 0; i < count; i++)
            {
                // O(1) registry lookup — no GetComponentInParent walk.
                if (!TargetInfoRegistry.TryGet(_buffer[i], out var target)) continue;
                if (target.IsDead) continue;

                // Deduplicate units that have multiple detectable colliders.
                string id = target.TargetId;
                if (!string.IsNullOrEmpty(id) && _visibleById.ContainsKey(id))
                    continue;

                // ── Single sqrt, reused for FOV and LOS ───────────────────
                Vector3 toTarget = target.AimPoint - _eyePoint.position;
                float sqrDist = toTarget.sqrMagnitude;

                bool inView;

                if (sqrDist < 0.0001f)
                {
                    // Target is essentially at the eye point — always visible.
                    inView = true;
                }
                else
                {
                    // Compute direction and distance once.
                    // dist is reused for the raycast — avoids a second sqrt.
                    float dist = Mathf.Sqrt(sqrDist);
                    Vector3 dirNorm = toTarget / dist;

                    // FOV check: one dot product, no trig, no allocation.
                    // _eyePoint.forward keeps FOV aligned with the sensor
                    // transform (head bone, turret, etc.) rather than the
                    // root transform.forward.
                    if (Vector3.Dot(_eyePoint.forward, dirNorm) < _minDot)
                    {
                        inView = false;
                    }
                    else
                    {
                        // LOS raycast — only reached after FOV passes.
                        // Passes pre-computed dirNorm and dist to avoid a
                        // third magnitude calculation inside HasLineOfSight.
                        inView = HasLineOfSight(_eyePoint.position, dirNorm, dist);
                    }
                }

                if (!inView) continue;

                _visibleList.Add(target);
                if (!string.IsNullOrEmpty(id))
                    _visibleById[id] = target;
            }
        }

        // ── Gizmos ────────────────────────────────────────────────────────

        private void OnDrawGizmosSelected()
        {
            if (_def == null) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _def.DetectionRadius);

            Gizmos.color = new Color(0f, 0.8f, 1f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, _def.HearingRadius);

            // FOV boundary rays in the eye point's local XZ plane.
            Transform fovOrigin = _eyePoint != null ? _eyePoint : transform;
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(fovOrigin.position,
                Quaternion.AngleAxis(_def.ViewAngle * 0.5f, Vector3.up) * fovOrigin.forward * _def.DetectionRadius);
            Gizmos.DrawRay(fovOrigin.position,
                Quaternion.AngleAxis(-_def.ViewAngle * 0.5f, Vector3.up) * fovOrigin.forward * _def.DetectionRadius);

            Gizmos.color = Color.red;
            for (int i = 0; i < _visibleList.Count; i++)
                Gizmos.DrawLine(fovOrigin.position, _visibleList[i].Position);
        }
    }
}