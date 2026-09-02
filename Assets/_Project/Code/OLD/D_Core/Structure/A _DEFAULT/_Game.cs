using Galactic1;
using UnityEngine;

namespace Galactic1
{



    public static class GAME
    {
        public static EUnitMode UNIT_MODE;
        
        //public static EBuildMode BUILD_MODE;
        //public static EBuildDragType BUILD_DRAG;
        
    }




    public class GAME_Speed
    {
        /// <summary>
        /// Поставить игру на паузу
        /// </summary>
        public void Pause()
        {
            ServiceLocator.Current.Get<MonoBehaviourMaster>().isPause = true;
        }
        /// <summary>
        /// Снять игру с паузы
        /// </summary>
        public void Continue()
        {
            ServiceLocator.Current.Get<MonoBehaviourMaster>().isPause = false;
        }


        /// <summary>
        /// Speed x1
        /// </summary>
        public void Normal()
        {
            Time.timeScale = 1;
            //ServiceLocator.Current.Get<MonoBehaviourMaster>().ChangeTimeScale();
        }

        /// <summary>
        /// Понижение скорости .1
        /// </summary>
        public void Low()
        {
            Time.timeScale = .1f;
            //ServiceLocator.Current.Get<MonoBehaviourMaster>().ChangeTimeScale();
        }
        
        /// <summary>
        /// Восстановление скорости
        /// </summary>
        public void Regular()
        {
            SpeedBattle.I.RestoreSpeed();
        }
    }



    public class GAME_Level_Options
    {
        public void Pause()
        {
            //ServiceLocator.Current.Get<PausePresenter>().OpenWindow();
            ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait(.2f, () =>
            {
                Time.timeScale = 0;
            });
        }

        public void Continue()
        {
            new GAME_Speed().Regular();
            //ServiceLocator.Current.Get<PausePresenter>().CloseWindow();
        }

        public void Exit()
        {
           // ServiceLocator.Current.Get<Bootstrap>().SetState(EGameState.LEVEL_CANCEL);
        }
    }





    /// <summary>
    /// Запуск биtвы под разные режимы
    /// </summary>
    public class GAME_Battle
    {
        public void Regular()
        {
            //_PointerHub.CODE_ENTITY_CLEAR();
            ServiceLocator.Current.Get<GameMachine>().MODE = GameMachine.EMode.REGULAR;
            ServiceLocator.Current.Get<Bootstrap>().SetState(EGameState.LEVEL_PLAY);
        }
        
        // public void RegularNext()
        // {
        //     new COMBAT_Autobattle().Stop();
        //     HUBLink.CODE_ENTITY_CLEAR();
        //     ServiceLocator.Current.Get<GameMachine>().MODE = GameMachine.EMode.REGULAR;
        //     ServiceLocator.Current.Get<Bootstrap>().SetState(EGameState.LEVEL_PLAY_NEXT);
        // }
        //
        // public void AncientRuins()
        // {
        //     HUBLink.CODE_ENTITY_CLEAR();
        //     ServiceLocator.Current.Get<ViewGameController>().FormationViewModel.Model.HideLockedPlace();
        //     ServiceLocator.Current.Get<GameMachine>().MODE = GameMachine.EMode.RUINS;
        //     ServiceLocator.Current.Get<Bootstrap>().SetState(EGameState.LEVEL_PLAY);
        // }
    }
    
    
    public class GAME_Result
    {
        /// <summary>
        /// Call each kill enemy for victory
        /// </summary>
        /// <param name="dev_victory"></param>
        public void CheckVictory(bool dev_victory = false)
        {
            // if (!DeveloperConsole.I.core.test_battle && ServiceLocator.Current.Get<LevelController>().AllUnitsDead() || dev_victory)
            // {
            //     GAMEPLAY_old.GamePause();
            //     ServiceLocator.Current.Get<GameMachine>().STATUS = GameMachine.EStatus.VICTORY;
            //     ServiceLocator.Current.Get<Bootstrap>().SetState(EGameState.LEVEL_FINISH);
            // }
        }

        public void CheckDefeat()
        {
            // all units dead
            // if (_PointerHub.AllUnitsDead())
            // {
            //     Defeat();
            // }
        }

        /// <summary>
        /// Call for defeat
        /// </summary>
        public void Defeat()
        {
            //GConsole.ClearLog();
            //GAMEPLAY_old.GamePause();
            ServiceLocator.Current.Get<GameMachine>().STATUS = GameMachine.EStatus.DEFEAT;
            DLog.Alert($"GAME Result : {ServiceLocator.Current.Get<GameMachine>().STATUS}");
            ServiceLocator.Current.Get<Bootstrap>().SetState(EGameState.LEVEL_FINISH);
        }
    }
}