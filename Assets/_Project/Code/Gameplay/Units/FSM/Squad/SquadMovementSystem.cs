using System;
using Galactic1.Code.Gameplay.Interaction;
using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    /// <summary>
    /// Единственная машина состояний движения отряда.
    ///
    /// Состояния:
    ///   Idle             — отряд стоит
    ///   WaitingForPath   — первая команда из покоя, путь ещё не пришёл
    ///   MovingCenter     — центр формации движется по пути
    ///   WaitingFollowers — центр дошёл, ждём агентов
    ///
    /// Новая команда во время движения НИЧЕГО не сбрасывает:
    ///   — путь строится от текущего NavigationCenter;
    ///   — старый путь продолжает выполняться, пока не придёт новый;
    ///   — FormationCenter / FormationHeading / кэш диспетчера не трогаются.
    /// Центры телепортируются в массу отряда только при старте из покоя.
    ///
    /// Tick pipeline (строгий порядок):
    ///   1. CenterDriver.Tick()
    ///   2. Smoother.Tick()
    ///   3. FormationFollower.Tick()
    ///   4. SlotProjector.Project()
    ///   5. SlotSeparator.Separate()
    ///   6. SlotMovementDispatcher.Dispatch()
    ///   7. Проверка перехода состояния
    /// </summary>
    public sealed class SquadMovementSystem : IDisposable
    {
        private enum MoveState
        {
            Idle,
            WaitingForPath,
            MovingCenter,
            WaitingFollowers
        }

        // ── References ──────────────────────────────────────────────────────
        private readonly SquadSceneRuntime _squad;
        private readonly SquadPathService _pathService;
        private readonly SquadTrailRenderer _trailRenderer;
        private readonly SquadFormationRuntime _runtime;

        // ── Lazy-created pipeline ───────────────────────────────────────────
        private SquadFormationSlots _formationSlots;
        private FormationCenterDriver _centerDriver;
        private FormationCenterSmoother _smoother;
        private FormationFollower _follower;
        private SlotMovementDispatcher _dispatcher;
        private bool _pipelineReady;

        public FormationCenterDriver CenterDriver => _centerDriver;
        public Action<FormationCenterDriver> OnInitialized;

        // ── State ───────────────────────────────────────────────────────────
        private WorldInputDispatcher.MoveMode _currentMode;
        private MoveState _state = MoveState.Idle;

        // Ждём ответ от PathService на последний запрос.
        private bool _awaitingPath;
        private int _serialAtRequest;

        public event Action OnMovementFinished;

        public Vector3 Center => _runtime.Center;
        public Vector3 Forward => _runtime.Forward;

        public TrailRenderSnapshot RenderSnapshot =>
            _centerDriver?.RenderSnapshot ?? TrailRenderSnapshot.Invalid;

        // ── Constructor ─────────────────────────────────────────────────────
        public SquadMovementSystem(
            SquadSceneRuntime squad,
            SquadTrailRenderer trailRenderer,
            SquadPathService pathService)
        {
            _squad = squad;
            _trailRenderer = trailRenderer;
            _pathService = pathService;
            _runtime = new SquadFormationRuntime();

            _squad.CompositionChanged += RebuildFormation;
            _pathService.OnPathFailed += OnPathFailed;
        }

        public void Dispose()
        {
            _centerDriver?.Dispose();
            _squad.CompositionChanged -= RebuildFormation;
            _pathService.OnPathFailed -= OnPathFailed;
        }

        // ── Init pipeline ───────────────────────────────────────────────────
        private bool EnsurePipelineReady()
        {
            if (_pipelineReady)
                return true;

            if (_squad.Agents.Count == 0)
                return false;

            BuildFormation();

            _centerDriver = new FormationCenterDriver(_runtime, _pathService);
            _trailRenderer.Bind(_centerDriver);
            _smoother = new FormationCenterSmoother(_runtime);
            OnInitialized?.Invoke(_centerDriver);

            _pipelineReady = true;
            return true;
        }

        private void BuildFormation()
        {
            _formationSlots = new SquadFormationSlots(
                _squad,
                FormationSystem.FormationType.Grid,
                FormationSystem.GridParams.Default);

            _follower = new FormationFollower(_formationSlots);
            _dispatcher = new SlotMovementDispatcher(_formationSlots.Slots.Length);
        }

        private void RebuildFormation()
        {
            if (!_pipelineReady)
                return;

            BuildFormation();

            if (_squad.Agents.Count == 0)
                return;

            // Отряд стоит — центры должны совпадать с реальной массой,
            // иначе новая формация соберётся вокруг устаревшей точки.
            if (_state == MoveState.Idle)
            {
                Vector3 mass = _squad.ComputeMassCenter();
                _runtime.NavigationCenter = mass;
                _runtime.FormationCenter = mass;
            }

            _follower.Tick(_runtime.FormationCenter, _runtime.FormationHeading);
            SlotProjector.Project(_formationSlots.Slots);
            SlotSeparator.Separate(_formationSlots.Slots);
            _dispatcher.Dispatch(_formationSlots.Slots, _currentMode);
        }

        // ── Public API ──────────────────────────────────────────────────────
        public void IssueMove(Vector3 targetCenter, WorldInputDispatcher.MoveMode mode)
        {
            if (!EnsurePipelineReady()) return;

            _currentMode = mode;

            float speed = mode == WorldInputDispatcher.MoveMode.Walk
                ? _squad.Agents[0].Mover.WalkSpeed
                : _squad.Agents[0].Mover.RunSpeed;

            bool alreadyMoving = _state != MoveState.Idle;
            Vector3 from;

            if (!alreadyMoving)
            {
                // ── Старт из покоя: центры = реальная масса отряда ──────────
                _dispatcher.Reset();

                from = _squad.ComputeMassCenter();
                _runtime.NavigationCenter = from;
                _runtime.FormationCenter = from;
                _runtime.VisualCenter = from;

                Vector3 dir = targetCenter - from;
                dir.y = 0f;
                if (dir.sqrMagnitude > 0.001f)
                    _runtime.Forward = dir.normalized;
                else if (!_runtime.IsInitialized)
                    _runtime.Forward = Vector3.forward;

                // Первая команда: формация сразу смотрит в сторону движения,
                // без начального «доворота» от мирового forward.
                if (!_runtime.IsInitialized || _runtime.FormationHeading.sqrMagnitude < 0.001f)
                    _runtime.FormationHeading = _runtime.Forward;

                _runtime.IsInitialized = true;

                _centerDriver.Begin(speed);
                _state = MoveState.WaitingForPath;
            }
            else
            {
                // ── Уже движемся: ничего не сбрасываем ──────────────────────
                from = _runtime.NavigationCenter;
                _centerDriver.Retarget(speed);
            }

            _awaitingPath = true;
            _serialAtRequest = _centerDriver.PathSerial;
            _pathService.SetTarget(from, targetCenter);

            _squad.SetState(SquadState.Moving);
            _trailRenderer.ShowPath();
        }

        // ── Tick ────────────────────────────────────────────────────────────
        public void Tick()
        {
            _trailRenderer.Tick();

            if (!_pipelineReady) return;

            // Путь пришёл, если драйвер принял новый.
            if (_awaitingPath && _centerDriver.PathSerial != _serialAtRequest)
                _awaitingPath = false;

            switch (_state)
            {
                case MoveState.WaitingForPath:
                    if (!_awaitingPath)
                    {
                        // Путь из одной точки → драйвер сразу Finished.
                        _state = _centerDriver.Finished
                            ? MoveState.WaitingFollowers
                            : MoveState.MovingCenter;
                    }
                    break;

                case MoveState.MovingCenter:
                    TickPipeline();
                    if (_centerDriver.Finished)
                        _state = MoveState.WaitingFollowers;
                    break;

                case MoveState.WaitingFollowers:
                    // Новая команда пришла после окончания пути → снова едем.
                    if (!_centerDriver.Finished)
                    {
                        _state = MoveState.MovingCenter;
                        break;
                    }

                    TickFollowerPipeline();

                    // Пока ждём новый путь — не завершаем движение.
                    if (!_awaitingPath && AreAgentsAtFinalSlots(_formationSlots.Slots))
                        FinishMovement();
                    break;
            }

            if (_state == MoveState.MovingCenter || _state == MoveState.WaitingFollowers)
                _runtime.VisualCenter = _squad.ComputeMassCenter();
        }

        // ── Pipeline steps ──────────────────────────────────────────────────
        private void TickPipeline()
        {
            var slots = _formationSlots.Slots;

            _centerDriver.Tick(slots, Time.deltaTime);
            TickSlots(slots);
        }

        private void TickFollowerPipeline()
        {
            TickSlots(_formationSlots.Slots);
        }

        private void TickSlots(SquadSlot[] slots)
        {
            _smoother.Tick(Time.deltaTime);
            _follower.Tick(_runtime.FormationCenter, _runtime.FormationHeading);
            SlotProjector.Project(slots);
            SlotSeparator.Separate(slots);
            _dispatcher.Dispatch(slots, _currentMode);
        }

        // ── Events ──────────────────────────────────────────────────────────
        private void OnPathFailed()
        {
            _awaitingPath = false;

            // Из покоя путь не построился — просто остаёмся стоять.
            // Если отряд уже едет, он продолжает по старому пути.
            if (_state == MoveState.WaitingForPath)
                AbortToIdle();
        }

        private void AbortToIdle()
        {
            _state = MoveState.Idle;
            _squad.SetState(SquadState.Idle);
            _centerDriver.ClearTrail();
            _trailRenderer.HidePath();
        }

        private void FinishMovement()
        {
            _state = MoveState.Idle;
            _awaitingPath = false;
            _squad.SetState(SquadState.Idle);

            _centerDriver.ClearTrail();
            _trailRenderer.HidePath();
            OnMovementFinished?.Invoke();
        }

        private bool AreAgentsAtFinalSlots(SquadSlot[] slots)
        {
            foreach (var slot in slots)
            {
                if (slot.Occupant == null) continue;

                // Юниты выходят из движения, даже если не смогли встать
                // на своё место из-за препятствия.
                if (slot.Occupant.Mover.IsMoving)
                    return false;
            }

            return true;
        }
    }
}
