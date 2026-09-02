
using Galactic1.Code.GameDatabase;
using Galactic1.Code.Gameplay.AI.LOD;
using Galactic1.Code.Gameplay.RaidDirector;
using Galactic1.Code.Systems.Enemies;
using Galactic1.Code.Systems.Squad;
using Galactic1.Core.Systems.GameLoopSession;

namespace Galactic1.Code.Systems.GameLoop.Tactical
{
    // =========================================================
    // старт рейда
    // =========================================================
    public sealed class SUB_RaidStartState : ITacticalState
    {
        private readonly DIContainer _container;

        public SUB_RaidStartState(DIContainer container)
        {
            _container = container;
        }


        public void Enter(DIContainer container, GameLoopContext context)
        {
            DLog.Alert("RaidStartState enter: инициализация боевых юнитов и карты", AppConstants.show_log_core);
            // Подготовка рейда: спавн врагов, подготовка целей
            
            var options = context.CurrentRaid.Scenario.Options;
            
            // === Спавним зомби-группы
            if (!DeveloperConsole.I.core.dev_polygon) // зомби спавнер если не тестовая среда !
            {
                // зомби спавнер для рейдовых локаций
                if(options.UseAmbientPopulation)
                {
                    var ambientPopulation = _container.Resolve<AmbientEnemyPopulationSystem>();
                    ambientPopulation.Initialize();
                }
                
                // спавнер волн зомби
                if (options.UseWaveSpawner)
                {
                    // todo
                }
                
                
                // === Запускаем Director
                var director = _container.Resolve<RaidDirectorRuntime>();
                var squadController = ServiceLocator.Current.Get<SquadController>();
                director.Initialize(GameContent.Enemies.DefaultEnemyId, squadController.Squad.GetLeader.transform);
                
                
                
                
                // ==========================================================================================
                // ==========================================================================================
                // AI LOD (в конце)
                // нужно юзать только для статичного спавнера, когда все зомби сразу спавнятся !!!
                if (options.UseAmbientPopulation)
                    container.Resolve<AILODSystem>().Entry();
            }
            
            
            
            // === Немедленно переходим в активную фазу
            context.TacticalStateMachine.ChangeState<SUB_RaidActiveState>();
        }

        public void Update(GameLoopContext context, float deltaTime)
        {
            // Можно сразу перейти к активной фазе
            //context.TacticalStateMachine.ChangeState<SUB_RaidActiveState>();
        }

        public void Exit(GameLoopContext context)
        {
            DLog.Alert("RaidStartState exit", EDlogColor.YELLOW, AppConstants.show_log_core);
        }
    }
}