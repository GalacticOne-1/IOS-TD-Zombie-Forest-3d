
using UnityEngine;

namespace Galactic1
{
    public enum EStarterState
    {
        NEW_GAME = 0,
        LOADING = 1
    }
    
    public enum EGameState
    {
        // для лобби
        OFF = 0,
        // для уровня/боя
        LEVEL_PLAY = 1,
        PAUSE = 2,
        RESUME = 3,
        LEVEL_FINISH = 4,
        LEVEL_EXIT = 5
    }
    
    public class Bootstrap : IGameService
    {
        /*
         *    Главное управление в игре на всех этапах (lobby/gameplay)
         *    Когда сцена загружена!!!
         */


        
        
        private EGameState gameState = EGameState.OFF;
        
        public EGameState STATE => gameState;

        

        
        
        // После загрузки сцены создаем все необходимое для уровня
        /// <summary>
        /// *** как стартует игра
        /// </summary>
        /// <param name="starter"></param>
        public void Starter(EStarterState starter)
        {
            DLog.Alert($"***        Starter: {starter}         ***");
            switch (starter)
            {
                case EStarterState.NEW_GAME:
                {
                    gameState = EGameState.OFF;
                    
                    //GAMEPLAY.DataGamestat().gameOver = false;
                    //GAMEPLAY_old.DataGameplay().isGame = true;
                    //RefController.I.dataBase.gameStat.NewGame();
            
            
                    //_PointerHub.player_unit = new PlayerUnit[AppConstants.player_unit_qu];
                    //HUBLink.unit_status = new HUBLink.CUnitsStatus[HUBLink.player_unit.Length];

                    //UIStatController.I.mainMenu.curDays.transform.parent.gameObject.SetActive(false);
                    //UIStatController.I.UpdateCurDayMenu();
                    
                    //GridController.I.NewSaveData();
                    //ServiceLocator.Current.Get<ExtensionPresenter>().NewSaveData();
                    //GeneratorUnit.Inst_base_unit();
                    //WaypointManager.I.ResetPoints();
                    //ConstructRoom.I.NewGame();
                    //TradeSystem.I.UpdateDealList();
                    //ServiceLocator.Current.Get<ViewGameController>().InventoryViewModel.NewSaveData();
                    

                } break;                  

                case EStarterState.LOADING:
                {
                    gameState = EGameState.OFF;
                    
                    //_PointerHub.player_unit = new PlayerUnit[AppConstants.player_unit_qu];
                    //HUBLink.unit_status = new HUBLink.CUnitsStatus[HUBLink.player_unit.Length];
                    //PlayerSTAT.I.INIT();

                    //GeneratorUnit.Loading();
                    //WaypointManager.I.ResetPoints();
                    //ConstructRoom.I.Load();
                    //ServiceLocator.Current.Get<ExtensionPresenter>().Load();
                    
                    //TradeSystem.I.UpdateDealList();


                } break;
            }
            
            
            // ***              SUBSCRIPTION             ***
            BootstrapAssembler.Subscription();
            
            Setup();
            
            // *** 
            //GAMEPLAY_old.Saving();
        }


        void Setup()
        {
            cmd.LoadCampLocation();
            //new UpdateXp(0);
           // new CheckPlayerOnDeath();
            cmd.Player_SpawnUnitOnScene();
            cmd.Player_SpawnDragonOnScene();
            cmd.RestoreController();
            //new UNIT_CAMP_STATE().Check();
            //new UNIT_CAMP_STATE().LockMap();

            // * активация контроллера движения
            // switch (ApplicationSetup.I.PLAYER_CNTR)
            // {
            //     case ApplicationSetup.EPlayerController.MOBILE:
            //         JoystickController.I.Activator();
            //         break;
            //     
            //     case ApplicationSetup.EPlayerController.KEYBOARD:
            //         Player2dController.I.Activator();
            //         break;
            // }
            
            CameraControllerOld.I.Activator();
        }


        
        
        
        
        
        
        
        


        /// <summary>
        /// Для смены состояния игры
        /// </summary>
        /// <param name="request"></param>
        public void SetState(EGameState request)
        {
            //GConsole.ClearLog();
            DLog.Alert($"Bootstrap : current state >> {gameState} / request state {request}");
            switch (request)
            {
                case EGameState.OFF:
                {
                    gameState = EGameState.OFF;
                } break;
                
                case EGameState.LEVEL_PLAY:
                {
                    if (gameState != EGameState.OFF)
                    {
                        
                        Debug.LogError($"You can start game only from {EGameState.OFF} state!");
                        return;
                    }
                    
                    DLog.Alert("STATE LEVEL_PLAY");
                    
                    gameState = EGameState.LEVEL_PLAY;
                    EventBus<StartLevelEvent>.Raise(new StartLevelEvent());
                    
                } break;
                
                case EGameState.PAUSE:
                {
                    if (gameState != EGameState.LEVEL_PLAY)
                    {
                        Debug.LogError($"You can pause game only from {EGameState.LEVEL_PLAY} state!");
                        return;
                    }

                    DLog.Alert("STATE PAUSE");

                    EventBus<PauseGameEvent>.Raise(new PauseGameEvent());
                    gameState = EGameState.PAUSE;
                    
                } break;
                
                case EGameState.RESUME:
                {
                    if (gameState != EGameState.PAUSE)
                    {
                        Debug.LogError($"You can resume game only from {EGameState.PAUSE} state!");
                        return;
                    }

                    DLog.Alert("STATE RESUME");
                    
                    EventBus<ResumeGameEvent>.Raise(new ResumeGameEvent());
                    gameState = EGameState.LEVEL_PLAY;
                    
                } break;
                
                case EGameState.LEVEL_FINISH:
                {
                    if (gameState != EGameState.LEVEL_PLAY)
                    {
                        Debug.LogError($"You can finish game only from {EGameState.LEVEL_PLAY} state!");
                        return;
                    }

                    DLog.Alert("STATE FINISH");

                    EventBus<FinishLevelEvent>.Raise(new FinishLevelEvent());
                    gameState = EGameState.LEVEL_FINISH;
                    
                } break;
                
                case EGameState.LEVEL_EXIT:
                {
                    /*if (gameState != EGameState.FINISH)
                    {
                        Debug.LogError($"You can finish game only from {EGameState.FINISH} state!");
                        return;
                    }*/

                    DLog.Alert("STATE LEVEL_EXIT");

                    EventBus<ExitLevelEvent>.Raise(new ExitLevelEvent());
                    gameState = EGameState.OFF;
                    
                } break;
            }
        }


        
        
    }
}