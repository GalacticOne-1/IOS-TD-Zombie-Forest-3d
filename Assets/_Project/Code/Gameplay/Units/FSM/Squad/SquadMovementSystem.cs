using System;
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Interaction;
using Galactic1.Code.Gameplay.Units;
using UnityEngine;

namespace Galactic1.Code.Systems.Squad
{
    /// <summary>
    /// Единственная машина состояний всего процесса движения отряда.
    ///
    /// Состояния:
    ///   Idle            — отряд стоит
    ///   WaitingForPath  — путь запрошен, ещё не пришёл
    ///   MovingCenter    — центр формации движется по пути
    ///   WaitingFollowers— центр дошёл, ждём агентов
    ///
    /// Правило: никакой другой класс не переводит состояние.
    /// FormationCenterDriver, SquadPathService не имеют права
    /// вызывать SetState или любую смену состояния извне.
    ///
    /// Tick pipeline (строгий порядок):
    ///   1. CenterDriver.Tick()       → Runtime.Center, Runtime.Forward
    ///   2. FormationFollower.Tick()  → slot.DesiredWorldPosition
    ///   3. SlotProjector.Project()   → slot.ProjectedWorldPosition
    ///   4. SlotSeparator.Separate()  → slot.FinalWorldPosition
    ///   5. SlotDispatcher.Dispatch() → UnitMover.MoveTo()
    ///   6. Проверка перехода состояния
    /// </summary>
    public sealed class SquadMovementSystem : System.IDisposable
    {
        // ── Movement states ─────────────────────────────────────────────────
        private enum MoveState
        {
            Idle,
            WaitingForPath,
            MovingCenter,
            WaitingFollowers
        }

        // ── Config ──────────────────────────────────────────────────────────
        private const float ArrivalTolerance = 1.0f;

        // ── References ──────────────────────────────────────────────────────
        private readonly SquadSceneRuntime _squad;
        private readonly SquadPathService _pathService;
        private readonly SquadTrailRenderer _trailRenderer;
        private readonly SquadFormationRuntime _runtime;

        // ── Lazy-created pipeline ────────────────────────────────────────────
        private SquadFormationSlots _formationSlots;
        private FormationCenterDriver _centerDriver;
        private FormationCenterSmoother _smoother;
        private FormationFollower _follower;
        private SlotMovementDispatcher _dispatcher;
        private bool _pipelineReady;
        
        public FormationCenterDriver CenterDriver => _centerDriver;

        public Action<FormationCenterDriver> OnInitialized;

        // ── Current mode ─────────────────────────────────────────────────────
        private WorldInputDispatcher.MoveMode _currentMode;
        private MoveState _state = MoveState.Idle;

        public Vector3 Center => _runtime.Center;
        // В SquadMovementSystem, вместо TrailGeometry Geometry:
        public TrailRenderSnapshot RenderSnapshot =>
            _centerDriver?.RenderSnapshot ?? TrailRenderSnapshot.Invalid;
        public Vector3 Forward => _runtime.Forward;

        // ── Constructor ──────────────────────────────────────────────────────
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
        }

        public void Dispose()
        {
            _centerDriver?.Dispose();
            _squad.CompositionChanged -= RebuildFormation;
        }

        // ── Init pipeline ────────────────────────────────────────────────────
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
        
        
        private void RebuildFormation()
        {
            if (!_pipelineReady)
                return;

            BuildFormation();

            // Немедленно пересчитать новую формацию
            _follower.Tick(
                _runtime.FormationCenter,
                _runtime.FormationHeading);

            SlotProjector.Project(_formationSlots.Slots);
            SlotSeparator.Separate(_formationSlots.Slots);

            // Выдать новые цели сразу
            _dispatcher.Dispatch(_formationSlots.Slots, _currentMode);
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

        // ── Public API ───────────────────────────────────────────────────────
        public void IssueMove(Vector3 targetCenter, WorldInputDispatcher.MoveMode mode)
        {
            if (!EnsurePipelineReady()) return;

            _currentMode = mode;
            _dispatcher.Reset();

            // Всегда сбрасываем центр на реальное положение отряда.
            // NavigationCenter начинает путь отсюда, а не с предыдущей позиции.
            Vector3 massCenter = _squad.ComputeMassCenter();
            _runtime.NavigationCenter = massCenter;
            _runtime.FormationCenter = massCenter;
            
            _runtime.FormationHeading = _runtime.IsInitialized
                ? _runtime.FormationHeading // сохраняем текущую ориентацию
                : Vector3.forward;
            
            Vector3 dir = targetCenter - massCenter;
            if (dir.sqrMagnitude > 0.001f)
                _runtime.Forward = dir.normalized;
            
            _runtime.IsInitialized = true;

            float speed = _squad.Agents.Count == 0
                ? 0
                : mode == WorldInputDispatcher.MoveMode.Walk
                    ? _squad.Agents[0].Mover.WalkSpeed
                    : _squad.Agents[0].Mover.RunSpeed;

            _centerDriver.Begin(speed);
            _pathService.SetTarget(massCenter, targetCenter); // путь строится от реального места

            _state = MoveState.WaitingForPath;
            _squad.SetState(SquadState.Moving);
            _trailRenderer.ShowPath();
        }

        // ── Tick ─────────────────────────────────────────────────────────────
        public void Tick()
        {
            _trailRenderer.Tick();

            if (!_pipelineReady) return;

            switch (_state)
            {
                case MoveState.WaitingForPath:
                    // Как только CenterDriver получит путь через OnPathReady,
                    // его Finished станет false. Переходим в MovingCenter.
                    if (!_centerDriver.Finished)
                        _state = MoveState.MovingCenter;
                    break;

                case MoveState.MovingCenter:
                    TickPipeline();
                    if (_centerDriver.Finished)
                        _state = MoveState.WaitingFollowers;
                    break;

                case MoveState.WaitingFollowers:
                    // Продолжаем гнать агентов к последним слотам,
                    // но центр уже не двигается.
                    TickFollowerPipeline();
                    if (AreAgentsAtFinalSlots(_formationSlots.Slots))
                        FinishMovement();
                    break;
            }
            
            // VisualCenter обновляется в обоих движущихся состояниях,
            // а не только пока двигается центр пути.
            if (_state == MoveState.MovingCenter || _state == MoveState.WaitingFollowers)
                _runtime.VisualCenter = _squad.ComputeMassCenter();
        }

        // ── Pipeline steps ───────────────────────────────────────────────────

        /// <summary>Полный пайплайн: центр + слоты + диспетчер.</summary>
        private void TickPipeline()
        {
            var slots = _formationSlots.Slots;

            _centerDriver.Tick(slots, Time.deltaTime); // 1. NavigationCenter
            _smoother.Tick(Time.deltaTime); // 2. FormationCenter догоняет
            _follower.Tick( // 3. Слоты вокруг FormationCenter
                _runtime.FormationCenter,
                _runtime.FormationHeading);
            SlotProjector.Project(slots); // 4.
            SlotSeparator.Separate(slots); // 5.
            _dispatcher.Dispatch(slots, _currentMode); // 6.
        }

        /// <summary>
        /// Центр уже на месте — только пересчитываем слоты и гоним агентов.
        /// </summary>
        private void TickFollowerPipeline()
        {
            var slots = _formationSlots.Slots;
            _smoother.Tick(Time.deltaTime);
            _follower.Tick(_runtime.FormationCenter, _runtime.FormationHeading);
            SlotProjector.Project(slots);
            SlotSeparator.Separate(slots);
            _dispatcher.Dispatch(slots, _currentMode);
        }

        private void FinishMovement()
        {
            // foreach (var agent in _squad.Agents)
            // {
            //     agent.StopSquadMovement();
            // }

            _state = MoveState.Idle;
            _squad.SetState(SquadState.Idle);

            _centerDriver.ClearTrail();
            _trailRenderer.HidePath();
        }

        private bool AreAgentsAtFinalSlots(SquadSlot[] slots)
        {
            foreach (var slot in slots)
            {
                if (slot.Occupant == null) continue;
                // if (Vector3.Distance(
                //         slot.Occupant.transform.position,
                //         slot.FinalWorldPosition) > ArrivalTolerance)
                //     return false;
                
                // так юниты всегда выходят из состояния движения
                // даже если не могут встать на свое место из-за препятствия
                if (slot.Occupant.Mover.IsMoving) 
                    return false;
            }

            return true;
        }
    }
}