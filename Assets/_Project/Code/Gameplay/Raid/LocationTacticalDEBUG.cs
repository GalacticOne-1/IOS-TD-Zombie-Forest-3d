using System;
using DEV;
using Galactic1.Code.Systems.GameLoop.Tactical;
using Galactic1.Code.Systems.Raid.Mission;
using Galactic1.Core.Systems.GameLoopSession;

namespace Galactic1.Code.Systems.Raid.Test
{
    public static class LocationTacticalDEBUG
    {




        /// <summary>
        /// Выход из локации без последствия
        /// </summary>
        public static void RaidCancel()
        {
            var _context = ServiceLocator.Current.Get<GameSession>().GameLoopContext;

            _context.CurrentRaid.Status = RaidStatus.Failed;
            //_context.CurrentRaid.EndReason = RaidEndReason.DebugCancel;
            //_context.TacticalStateMachine.ChangeState(typeof(SUB_RaidCleanupState));

            DevUpdate.I.missionObjectiveService.ForceFinished(new()
                {
                    Status = MissionStatus.Victory,
                    EndReason = RaidEndReason.DebugCancel
                },
                typeof(SUB_RaidCleanupState));
        }

        public static void RaidComplete()
        {
            var _context = ServiceLocator.Current.Get<GameSession>().GameLoopContext;


            _context.CurrentRaid.Status = RaidStatus.Completed;
            // _context.CurrentRaid.EndReason = RaidEndReason.ObjectivesCompleted;
            // _context.TacticalStateMachine.ChangeState(
            //     TacticalTransitionResolver.GetNext(_context.TacticalStateMachine.Current));

            DevUpdate.I.missionObjectiveService.ForceFinished(new()
                {
                    Status = MissionStatus.Victory,
                    EndReason = RaidEndReason.ObjectivesCompleted
                },
                TacticalTransitionResolver.GetNext(_context.TacticalStateMachine.Current));
        }

        public static void RaidDefeat()
        {
            
        }
        
        
        /// <summary>
        /// Выход из локации
        /// <br/>- без награды
        /// <br/>- с применением состояния юнитов
        /// </summary>
        public static void RaidEvacuation()
        {
            var _context = ServiceLocator.Current.Get<GameSession>().GameLoopContext;

            _context.CurrentRaid.Status = RaidStatus.Failed;
            // _context.CurrentRaid.EndReason = RaidEndReason.Evacuated;
            // _context.TacticalStateMachine
            //     .ChangeState(TacticalTransitionResolver.GetNext(_context.TacticalStateMachine.Current));

            DevUpdate.I.missionObjectiveService.ForceFinished(new()
                {
                    Status = MissionStatus.Defeat,
                    EndReason = RaidEndReason.Evacuated
                },
                TacticalTransitionResolver.GetNext(_context.TacticalStateMachine.Current));
        }



        static Type GetNextTacticalState(ITacticalState current)
        {
            return current switch
            {
                SUB_RaidActiveState => typeof(SUB_RaidCheckObjectivesState),
                SUB_RaidCheckObjectivesState => typeof(SUB_RaidCleanupState),
                SUB_RaidCleanupState => null, // финальное состояние
                _ => throw new ArgumentOutOfRangeException(nameof(current), "Unknown tactical state")
            };
        }
    }
}