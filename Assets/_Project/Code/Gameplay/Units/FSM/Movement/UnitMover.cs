using System;
using Galactic1.Code.Gameplay.Interaction;
using Galactic1.Code.Gameplay.Units.Movement;
using Pathfinding;
using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    /// <summary>
    /// LOW LEVEL NAVIGATION DRIVER. Остаётся generic navigation component —
    /// никакой Siege/HQ/wall/AI-логики здесь нет.
    ///
    /// ИЗМЕНЕНИЯ ДЛЯ SIEGE (минимальные, аддитивные):
    ///   — OnPathComputed: событие, стреляющее когда асинхронный расчёт пути
    ///     РЕАЛЬНО завершился (в отличие от callback-параметра MoveTo,
    ///     который сигнализирует о ПРИБЫТИИ). Несёт per-request данные
    ///     (destination, requestId, endpoint) — ничего не читается из
    ///     мутабельных полей типа _pendingDestination в момент callback,
    ///     всё захватывается локально в момент создания запроса или
    ///     вычисляется локально при его завершении.
    ///   — LastPathEndpoint УДАЛЁН: единственный прежний потенциальный
    ///     consumer (SiegePathService) использует per-request endpoint
    ///     из события, поэтому дублирующее мутабельное свойство не нужно.
    ///   — Существующий _activeRequestId guard (stale callback filter)
    ///     не менялся и остаётся единственным механизмом защиты от
    ///     устаревших асинхронных результатов.
    ///
    /// MoveTo(), PathStatus, navigation state machine, NavigationMoveResult
    /// callback — семантика не изменена.
    /// </summary>
    [RequireComponent(typeof(AIPath))]
    [RequireComponent(typeof(Seeker))]
    public sealed class UnitMover : MonoBehaviour, IUpdate
    {
        // =========================================================
        // Config
        // =========================================================

        [SerializeField] private float repathInterval = 0.15f;
        [SerializeField] private float arrivedDistance = 0.15f;
        [SerializeField] private float destinationThreshold = 0.25f;

        [SerializeField] private float walkSpeed = 2f;
        [SerializeField] private float runSpeed = 5f;

        // =========================================================
        // Runtime
        // =========================================================

        private AIPath _ai;
        private Seeker _seeker;

        private uint _activeRequestId;

        private Vector3 _lastDestination = Vector3.positiveInfinity;
        private Vector3 _pendingDestination;

        private float _nextRepathTime;

        private bool _isDead;
        private bool _arrivalProcessed;

        private WorldInputDispatcher.MoveMode _currentMoveMode =
            WorldInputDispatcher.MoveMode.Walk;

        private Action<NavigationMoveResult> _pendingCallback;

        private NavigationPathStatus _pathStatus =
            NavigationPathStatus.None;

        private bool _isSleeping;
        private bool _wasMovingBeforeSleep;

        // =========================================================
        // Public
        // =========================================================

        public NavigationState State { get; private set; }
            = NavigationState.Idle;

        public NavigationPathStatus PathStatus => _pathStatus;

        /// <summary>
        /// NEW — стреляет когда асинхронный расчёт пути РЕАЛЬНО завершился,
        /// для КОНКРЕТНОГО request-а. Параметры: (status, destination запроса,
        /// requestId, endpoint последнего валидного corner-а этого результата
        /// или Vector3.positiveInfinity если corner-ов нет). Не читайте
        /// какие-либо мутабельные поля UnitMover из обработчика этого события —
        /// используйте только переданные параметры, они per-request immutable.
        /// </summary>
        public event Action<NavigationPathStatus, Vector3, uint, Vector3> OnPathComputed;

        public bool IsMoving =>
            State == NavigationState.Moving ||
            State == NavigationState.CalculatingPath;

        public bool HasArrived =>
            State == NavigationState.Arrived;

        public bool HasPath =>
            _ai != null && _ai.hasPath;

        public Vector3 Velocity =>
            _ai != null ? _ai.velocity : Vector3.zero;

        public Vector3 Destination =>
            _pendingDestination;

        public float WalkSpeed => walkSpeed;
        public float RunSpeed => runSpeed;

        public WorldInputDispatcher.MoveMode CurrentMoveMode =>
            _currentMoveMode;

        // =========================================================
        // Slow Override
        // =========================================================

        private float _slowWalkOverride = -1f;
        private float _slowRunOverride = -1f;

        // =========================================================
        // Unity
        // =========================================================

        private void Awake()
        {
            _ai = GetComponent<AIPath>();
            _seeker = GetComponent<Seeker>();

            _ai.canSearch = false;
            _ai.endReachedDistance = arrivedDistance;

            ServiceLocator.Current
                .Get<MonoBehaviourMaster>()
                .update
                .Add(this);
        }

        private void OnDestroy()
        {
            IUpdateClear();
        }

        public void Setup(float newWalkSpeed, float newRunSpeed)
        {
            walkSpeed = newWalkSpeed;
            runSpeed = newRunSpeed;
        }

        // =========================================================
        // Tick
        // =========================================================

        public void UpdateM()
        {
            if (_isDead || _isSleeping)
                return;

            if (State != NavigationState.Moving)
                return;

            if (_ai.pathPending)
                return;

            if (_ai.reachedEndOfPath && !_arrivalProcessed)
            {
                _arrivalProcessed = true;

                if (_pathStatus == NavigationPathStatus.Partial)
                {
                    State = NavigationState.Failed;
                    FireCallback(NavigationMoveResult.PartialPath);
                }
                else
                {
                    State = NavigationState.Arrived;
                    FireCallback(NavigationMoveResult.Success);
                }
            }
        }

        public void IUpdateClear()
        {
            ServiceLocator.Current
                .Get<MonoBehaviourMaster>()
                .update
                .Remove(this);
        }

        // =========================================================
        // Movement
        // =========================================================

        public void MoveTo(
            Vector3 destination,
            WorldInputDispatcher.MoveMode mode,
            bool forceRepath,
            Action<NavigationMoveResult> callback = null)
        {
            if (_isDead || _isSleeping)
                return;

            bool destinationChanged =
                Vector3.SqrMagnitude(destination - _lastDestination)
                >= destinationThreshold * destinationThreshold;

            bool repathCooldownReady = Time.time >= _nextRepathTime;

            bool needImmediateRepath =
                destinationChanged ||
                _pathStatus == NavigationPathStatus.Failed ||
                State == NavigationState.Failed ||
                State == NavigationState.Arrived;

            bool allowForcedRepath =
                forceRepath &&
                repathCooldownReady;

            bool needRepath =
                needImmediateRepath ||
                allowForcedRepath;

            if (!needRepath)
            {
                if (State == NavigationState.Moving ||
                    State == NavigationState.CalculatingPath)
                {
                    callback?.Invoke(NavigationMoveResult.Success);
                }

                return;
            }

            _currentMoveMode = mode;

            _lastDestination = destination;
            _pendingDestination = destination;

            _pendingCallback = callback;

            ApplyMoveSpeed(mode);

            _arrivalProcessed = false;

            _nextRepathTime =
                UnityEngine.Time.time + repathInterval;

            uint requestId = ++_activeRequestId;
            Vector3 requestDestination = destination; // per-request immutable capture

            if (State != NavigationState.Moving)
            {
                State = NavigationState.CalculatingPath;
            }

            _pathStatus = NavigationPathStatus.None;

            _seeker.StartPath(
                transform.position,
                destination,
                p =>
                {
                    // Существующий stale guard — единственный механизм фильтрации
                    // устаревших асинхронных результатов. Стоит ДО вызова
                    // OnPathComplete: устаревший callback никогда не долетает
                    // ни до OnPathComplete, ни до OnPathComputed.
                    if (requestId != _activeRequestId)
                        return;

                    OnPathComplete(p, requestId, requestDestination);
                });
        }

        public void MoveTo(MoveRequest request)
        {
            MoveTo(
                request.Destination,
                request.Mode,
                false,
                request.Callback);
        }

        public void Stop()
        {
            _pendingCallback = null;

            State = NavigationState.Idle;

            _ai.isStopped = true;
            _ai.SetPath(null);

            _lastDestination = Vector3.positiveInfinity;
            _pendingDestination = Vector3.positiveInfinity;
        }

        public void Die()
        {
            _isDead = true;

            _pendingCallback = null;

            State = NavigationState.Idle;

            _ai.isStopped = true;
            _ai.SetPath(null);

            _ai.enabled = false;
        }

        public void Restore()
        {
            _isDead = false;
            _ai.enabled = true;
        }

        public void Sleep()
        {
            if (_isSleeping || _isDead) return;
            _isSleeping = true;

            _wasMovingBeforeSleep = IsMoving;

            _ai.isStopped = true;
            _ai.enabled = false;
        }

        public void Wake()
        {
            if (!_isSleeping) return;
            _isSleeping = false;

            _ai.enabled = true;

            if (_wasMovingBeforeSleep && _pendingDestination != Vector3.positiveInfinity)
            {
                _nextRepathTime = 0f;
                MoveTo(_pendingDestination, _currentMoveMode, true, _pendingCallback);
            }
        }

        // =========================================================
        // Movement Settings
        // =========================================================

        public void SetRotationControl(bool enabled)
        {
            if (_ai != null)
                _ai.updateRotation = enabled;
        }

        public void SetSpeed(float speed)
        {
            _ai.maxSpeed = speed;
        }

        // =========================================================
        // Slow Override
        // =========================================================

        public void SetSlowOverride(
            float slowWalk,
            float slowRun)
        {
            _slowWalkOverride = slowWalk;
            _slowRunOverride = slowRun;

            if (IsMoving)
            {
                _ai.maxSpeed =
                    Mathf.Min(_ai.maxSpeed, slowRun);
            }
        }

        public void ClearSlowOverride()
        {
            _slowWalkOverride = -1f;
            _slowRunOverride = -1f;

            if (IsMoving)
            {
                ApplyMoveSpeed(_currentMoveMode);
            }
        }

        // =========================================================
        // Path Callback
        // =========================================================

        /// <summary>
        /// requestId и requestDestination — per-request immutable, захвачены
        /// локально в MoveTo() в момент создания ЭТОГО конкретного запроса.
        /// endpoint вычисляется здесь же, локально, для ЭТОГО конкретного
        /// path-результата — никогда не читается из состояния предыдущего
        /// запроса.
        /// </summary>
        private void OnPathComplete(Path path, uint requestId, Vector3 requestDestination)
        {
            if (_isDead)
                return;

            if (State == NavigationState.Idle)
                return;

            if (path.error)
            {
                _pathStatus = NavigationPathStatus.Failed;
                State = NavigationState.Failed;

                FireCallback(NavigationMoveResult.PathFailed);
                // error-путь не имеет corner-ов — explicit sentinel, не "старый" endpoint
                OnPathComputed?.Invoke(_pathStatus, requestDestination, requestId, Vector3.positiveInfinity);
                return;
            }

            _ai.isStopped = false;
            _ai.destination = _pendingDestination;

            Vector3 endpoint = Vector3.positiveInfinity; // локально, для ЭТОГО path

            if (path is ABPath abPath)
            {
                if (abPath.vectorPath != null && abPath.vectorPath.Count > 0)
                    endpoint = abPath.vectorPath[abPath.vectorPath.Count - 1];

                if (abPath.CompleteState == PathCompleteState.Partial)
                {
                    _pathStatus = NavigationPathStatus.Partial;
                    State = NavigationState.Moving;

                    OnPathComputed?.Invoke(_pathStatus, requestDestination, requestId, endpoint);
                    return;
                }
            }

            _pathStatus = NavigationPathStatus.Complete;
            State = NavigationState.Moving;

            OnPathComputed?.Invoke(_pathStatus, requestDestination, requestId, endpoint);
        }

        // =========================================================
        // Private
        // =========================================================

        private void FireCallback(NavigationMoveResult result)
        {
            Action<NavigationMoveResult> cb =
                _pendingCallback;

            _pendingCallback = null;

            cb?.Invoke(result);
        }

        private void ApplyMoveSpeed(
            WorldInputDispatcher.MoveMode mode)
        {
            float walk =
                _slowWalkOverride >= 0f
                    ? _slowWalkOverride
                    : walkSpeed;

            float run =
                _slowRunOverride >= 0f
                    ? _slowRunOverride
                    : runSpeed;

            _ai.maxSpeed =
                mode == WorldInputDispatcher.MoveMode.Walk
                    ? walk
                    : run;
        }
    }
}
