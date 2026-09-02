using System;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.GameLoop.Tactical;

namespace Galactic1.Code.Systems.Raid.Mission
{
    /// <summary>
    /// Реагирует на игровые события, обновляет MissionContext и просит текущий
    /// Scenario оценить состояние миссии. Не знает ни про LocationType, ни про
    /// конкретный тип сценария — только про факт события и то, во что он
    /// транслируется в контексте (что тоже не зависит от сценария: "здание
    /// разрушено" — универсальный факт для любого combat-рейда).
    /// </summary>
    public class MissionObjectiveService : IDisposable
    {
        private readonly GameLoopContext _context;
        private readonly RaidRuntime _raid;
        private readonly MissionStateProvider _stateProvider;
        private readonly MissionContext _missionContext = new();

        private readonly EventBinding<UnitKilledEvent> _unitKilledBinding;
        private readonly EventBinding<BuildingDestroyedEvent> _buildingDestroyedBinding;
        private readonly EventBinding<WaveCompletedEvent> _waveCompletedBinding;
        private readonly EventBinding<ExitReachedEvent> _exitReachedBinding;
        private readonly EventBinding<AllWavesCompletedEvent> _allWavesBinding;
        
        

        private bool _finished;

        public MissionObjectiveService(
            GameLoopContext context,
            RaidRuntime raid, 
            MissionStateProvider stateProvider)
        {
            _context = context;
            _raid = raid;
            _stateProvider = stateProvider;

            _unitKilledBinding = new EventBinding<UnitKilledEvent>(OnUnitKilled);
            _buildingDestroyedBinding = new EventBinding<BuildingDestroyedEvent>(OnBuildingDestroyed);
            _waveCompletedBinding = new EventBinding<WaveCompletedEvent>(OnWaveCompleted);
            _exitReachedBinding = new EventBinding<ExitReachedEvent>(OnExitReached);
            _allWavesBinding = new EventBinding<AllWavesCompletedEvent>(OnAllWavesCompleted);

            EventBus<UnitKilledEvent>.Register(_unitKilledBinding);
            EventBus<BuildingDestroyedEvent>.Register(_buildingDestroyedBinding);
            EventBus<WaveCompletedEvent>.Register(_waveCompletedBinding);
            EventBus<ExitReachedEvent>.Register(_exitReachedBinding);
            

            EventBus<AllWavesCompletedEvent>.Register(_allWavesBinding);
        }

        // ── Трансляция событий в MissionContext ──────────────────────────────
        // Каждый метод знает только про форму конкретного события — не про сценарии.

        private void OnUnitKilled(UnitKilledEvent e)
        {
            _missionContext.PlayerForcesDestroyed = _raid.Scenario.ArePlayerForcesDestroyed(_stateProvider);
            Evaluate();
        }

        private void OnBuildingDestroyed(BuildingDestroyedEvent e)
        {
            _missionContext.HeadquartersDestroyed = _stateProvider.IsHeadquartersDestroyed();
            Evaluate();
        }

        private void OnWaveCompleted(WaveCompletedEvent e)
        {
            //_missionContext.AllWavesCompleted = _stateProvider.AreAllWavesCompleted();
            //Evaluate();
        }
        private void OnAllWavesCompleted(AllWavesCompletedEvent e)
        {
            _missionContext.AllWavesCompleted = true;
            Evaluate();
        }

        private void OnExitReached(ExitReachedEvent e)
        {
            _missionContext.ExitReached = true;
            Evaluate();
        }

        // ── Оценка ────────────────────────────────────────────────────────────
        
        
        /*
         *  Raid
         *  - если игро покадает локацию через зону выхода то это всегда победа и идет через ForceFinished
         *  - если отряд гибнет то сработает внутренний Evaluate через событие гибели юнитов.
         *
         *  Camp Defense
         *  - при этом сценарии и победа и поражение срабатывает только через внутренный Evaluate,
         *    т.к зон выхода из лагеря не существует
         */

        private void Evaluate()
        {
            if (_finished)
                return;

            var result = _raid.Scenario.EvaluateMission(_missionContext);

            if (!result.IsFinished)
                return;

            // #1 блокируем экран
            _finished = true;
            ServiceLocator.Current.Get<UIRootView>().EnableBlockScreen();

            // #2 после вызова освобождаем
            ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait(1, () =>
            {
                ServiceLocator.Current.Get<UIRootView>().DisableBlockScreen();
                EventBus<MissionCompletedEvent>.Raise(new()
                {
                    Result = result,
                    NextState = TacticalTransitionResolver.GetNext(_context.TacticalStateMachine.Current)
                });
            });
        }

        /// <summary>
        /// Для внешненго вызова
        /// </summary>
        /// <param name="result"></param>
        /// <param name="requiresState"></param>
        public void ForceFinished(MissionResult result, Type requiresState)
        {
            _finished = true;
            EventBus<MissionCompletedEvent>.Raise(new()
            {
                Result = result,
                NextState = requiresState
            });
        }
        
        
        
        

        public void Dispose()
        {
            EventBus<UnitKilledEvent>.Deregister(_unitKilledBinding);
            EventBus<BuildingDestroyedEvent>.Deregister(_buildingDestroyedBinding);
            EventBus<WaveCompletedEvent>.Deregister(_waveCompletedBinding);
            EventBus<ExitReachedEvent>.Deregister(_exitReachedBinding);
            EventBus<AllWavesCompletedEvent>.Deregister(_allWavesBinding);
        }
    }
}