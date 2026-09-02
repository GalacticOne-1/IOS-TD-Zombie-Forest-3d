using System.Collections;
using Galactic1;
using Galactic1.Gameplay.Player;
using Galactic1.Repository;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Galactic1
{
    public class GameMachine : MonoBehaviour, IGameService
    {

        #region STAT

        public SGameState STATE = SGameState.GAME;
        public enum SGameState
        {
            GAME,
            LEVEL,
        }

        public EMode MODE = EMode.REGULAR;
        
        public enum EMode
        {
            REGULAR,  
            MAP,
            CAMP_SURVIVE,
            RAID
        }



        public EStatus STATUS;
        public enum EStatus
        {
            NON,
            CANCEL,
            DEFEAT,
            VICTORY
        }

        
        // заработанное в бою
        public float gold_battle;
        public float experience_battle;
        
        // для передачи наград
        public AddingReward.CReward[] ar_reward;

        #endregion

        
        
        
        /// <summary>
        /// Утсановка значений и состояний для начала игры
        /// </summary>
        public void GameInit()
        {
            ServiceLocator.Current.Get<ViewGameController>().MainMenuViewModel.ResetState();

        }





        
        /*
         *      Загрузочный экран, подготовка сцены, запуск уровня
         */
        public void Level_Start()
        {
            STATE = SGameState.LEVEL;
            
            //ServiceLocator.Current.Get<GUIAssist>().LoadScreenShow(ServiceLocator.Current.Get<SceneController>().loadMapScreen, true, () =>
            {
                // #1 меняем канвасы
                ServiceLocator.Current.Get<ViewGameController>().GetCanvas(new [] { ECanvas.GAME, ECanvas.LEVEL, ECanvas.OVER });
                // disable button construct
                ServiceLocator.Current.Get<ViewGameController>().MainMenuViewModel.View.Holder.GetChild(0).SetActive(false);

                new LOCATION_SETUP().LocationExit(true);
                
                // #2 раздельный вызов для очистки и новой загрузки
                ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait(.2f, new DFunc[]
                {
                    // * очищаем виджеты нужного экрана
                    /*() =>
                    {
                        if(MODE == EMode.REGULAR)
                            EventBus<ScreenClearRegularEvent>.Raise(new ScreenClearRegularEvent());
                        else if(MODE == EMode.MAP)
                            EventBus<ScreenClearMapEvent>.Raise(new ScreenClearMapEvent());
                    },*/
                    () => { EventBus<ScreenClearMapEvent>.Raise(new ScreenClearMapEvent()); },
                    cmd.ClearCampLocation,
                    //() => { EventBus<ClearGameEvent>.Raise(new ClearGameEvent()); },
                    cmd.LoadLocation,
                    () =>
                    {
                        cmd.Player_SpawnUnitOnSceneForLocation();
                        cmd.Player_SpawnDragonOnSceneForLocation();
                        cmd.RestoreController();
                        CameraFollow.I.STOP = false;
                    },

                    // -- ! в конце ! --
                    //GAMEPLAY_old.Saving
                });
                
                // #3 убираем черный экран
                ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait(3f,() =>
                {
                    // включаем звуки юнитов
                    EventBus<SoundUnitsEnableEvent>.Raise(new SoundUnitsEnableEvent());
                    //ServiceLocator.Current.Get<GUIAssist>().LoadScreenHide(ServiceLocator.Current.Get<SceneController>().loadMapScreen, null);
                });
            }//);
            
        }

        

        /*
         *      Останавливаем уровень, готовим сцену для выхода
         */
        public void Level_Finish()
        {
            //ServiceLocator.Current.Get<ViewGameController>().GetWindow(EWindow.FINISH);
            new FinishLevel();
        }


        
        /*
         *      Очищаем левел, убираем окна и переходим в игру (бывшее лобби)
         */
        public void Level_Exit()
        {
            STATE = SGameState.GAME;

            ServiceLocator.Current.Get<MonoBehaviourMaster>().isPause = true;
            
            //ServiceLocator.Current.Get<GUIAssist>().LoadScreenShow(ServiceLocator.Current.Get<SceneController>().loadMapScreen, true, () =>
            {
                // #1 останавливаем звуки юнитов
                EventBus<SoundUnitsDisableEvent>.Raise(new SoundUnitsDisableEvent());
                
                // #2 меняем канвасы
                ServiceLocator.Current.Get<ViewGameController>().GetCanvas(new [] { ECanvas.MAP,  ECanvas.OVER });
                // disable button construct
                ServiceLocator.Current.Get<ViewGameController>().MainMenuViewModel.View.Holder.GetChild(0).SetActive(true);
                
                // #3 раздельный вызов для очистки и новой загрузки
                ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait(.2f, new DFunc[]
                {
                    // * загружаем виджеты нужного экрана
                    /*() =>       
                    {
                        if(MODE == EMode.REGULAR)
                            EventBus<ScreenLoadRegularEvent>.Raise(new ScreenLoadRegularEvent());
                        else if(MODE == EMode.MAP)
                            EventBus<ScreenLoadMapEvent>.Raise(new ScreenLoadMapEvent());
                    },*/
                    
                    //() => JoystickController.I.ResetTouch(),
                    () => cmd.LoadCampLocation(),
                    () =>
                    {
                        cmd.Player_RemoveUnitFromScene();
                        cmd.Player_RemoveDragonFromScene();
                    },
                    cmd.ClearLocation,
                    () => { EventBus<ScreenLoadMapEvent>.Raise(new ScreenLoadMapEvent()); },
                    
                    // -- ! в конце ! --
                    () => ServiceLocator.Current.Get<MonoBehaviourMaster>().isPause = false,
                    //GAMEPLAY_old.Saving
                });
                
                // #4 убираем черный экран
                ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait(3f,() =>
                {
                    //ServiceLocator.Current.Get<GUIAssist>().LoadScreenHide(ServiceLocator.Current.Get<SceneController>().loadMapScreen,null); 
                });
            }//);
        }



        
        /// <summary>
        /// Для входа на карту
        /// </summary>
        public void Map_Enter()
        {
            //ServiceLocator.Current.Get<GUIAssist>().LoadScreenShow(ServiceLocator.Current.Get<SceneController>().loadMapScreen, true, () =>
            {
                CameraFollow.I.STOP = true;
                
                // #1 останавливаем звуки юнитов
                EventBus<SoundUnitsDisableEvent>.Raise(new SoundUnitsDisableEvent());
                
                // #2 меняем канвасы
                ServiceLocator.Current.Get<ViewGameController>().GetCanvas(new [] { ECanvas.MAP, ECanvas.OVER });
                
                
                // #3 раздельный вызов для очистки и новой загрузки
                ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait(.2f, new DFunc[]
                {
                    //() => JoystickController.I.ResetTouch(),
                    () =>
                    {
                        ServiceLocator.Current.Get<HeroStateMachine>().ChangeState(HeroStateMachine.EPlayerController.Dragon);
                        cmd.Player_RemoveUnitFromScene();
                        cmd.Player_RemoveDragonFromScene();
                    },
                    () => { EventBus<ScreenClearRegularEvent>.Raise(new ScreenClearRegularEvent()); },
                    () => { EventBus<ScreenLoadMapEvent>.Raise(new ScreenLoadMapEvent()); },
                    
                    // -- ! в конце ! --
                    //GAMEPLAY_old.Saving
                });
                
                // #4 убираем черный экран
                ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait(1f,() =>
                {
                    //ServiceLocator.Current.Get<GUIAssist>().LoadScreenHide(ServiceLocator.Current.Get<SceneController>().loadMapScreen, null);
                });
            }//);
        }
        
        
        /// <summary>
        /// Для выходы с карты
        /// </summary>
        public void Map_Exit()
        {
            //ServiceLocator.Current.Get<GUIAssist>().LoadScreenShow(ServiceLocator.Current.Get<SceneController>().loadMapScreen, true, () =>
            {
                // #1 меняем канвасы
                ServiceLocator.Current.Get<ViewGameController>().GetCanvas(new [] { ECanvas.GAME, ECanvas.LEVEL , ECanvas.OVER });
                
                new LOCATION_SETUP().LocationExit(true);
                
                // #2 раздельный вызов для очистки и новой загрузки
                ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait(.2f, new DFunc[]
                {
                    () =>
                    {
                        cmd.Player_SpawnUnitOnScene();
                        cmd.Player_SpawnDragonOnScene();
                        cmd.RestoreController();
                        CameraFollow.I.STOP = false;
                    },
                    () => { EventBus<ScreenClearMapEvent>.Raise(new ScreenClearMapEvent()); },
                    () => { EventBus<ScreenLoadRegularEvent>.Raise(new ScreenLoadRegularEvent()); },
                    
                    // -- ! в конце ! --
                    //GAMEPLAY_old.Saving
                });
                
                // #3 убираем черный экран
                ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait(1f,() =>
                {
                    // включаем звуки юнитов
                    EventBus<SoundUnitsEnableEvent>.Raise(new SoundUnitsEnableEvent());
                    //ServiceLocator.Current.Get<GUIAssist>().LoadScreenHide(ServiceLocator.Current.Get<SceneController>().loadMapScreen, null);
                });
            }//);
        }


        /// <summary>
        /// Оживление игрока на базе
        /// </summary>
        public void Level_Exit_Revive()
        {
            STATE = SGameState.GAME;
            
            //ServiceLocator.Current.Get<GUIAssist>().LoadScreenShow(ServiceLocator.Current.Get<SceneController>().loadMapScreen, true, () =>
            {
                // #1 меняем канвасы
                ServiceLocator.Current.Get<ViewGameController>().GetCanvas(new [] { ECanvas.GAME, ECanvas.LEVEL , ECanvas.OVER });
                // disable button construct
                ServiceLocator.Current.Get<ViewGameController>().MainMenuViewModel.View.Holder.GetChild(0).SetActive(true);
                
                new LOCATION_SETUP().LocationExit(true);

                //ServiceLocator.Current.Get<GlobalRepository>().CurrLocation = 0;
                
                // #2 раздельный вызов для очистки и новой загрузки
                ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait(.2f, new DFunc[]
                {
                    
                    //  удаляем локацию
                    cmd.ClearLocation,
                    // загружаем базу
                    cmd.LoadCampLocation,
                    
                    // создаем юнит
                    () =>
                    {
                        cmd.Player_SpawnUnitOnScene();
                        cmd.Player_SpawnDragonOnScene();
                        cmd.RestoreController();
                    },
                    // возврат границ этажа
                    //() => new UNIT_DEFUALT_FLOOR(),
                    // загружаем экраны
                    () => { EventBus<ScreenLoadRegularEvent>.Raise(new ScreenLoadRegularEvent()); },

                    
                    // -- ! в конце ! --
                    () => ServiceLocator.Current.Get<MonoBehaviourMaster>().isPause = false,
                    //GAMEPLAY_old.Saving
                });
                
                // #3 убираем черный экран
                ServiceLocator.Current.Get<CoroutineController>().Coroutine_wait(2f,() =>
                {
                    // включаем звуки юнитов
                    EventBus<SoundUnitsEnableEvent>.Raise(new SoundUnitsEnableEvent());
                    //ServiceLocator.Current.Get<GUIAssist>().LoadScreenHide(ServiceLocator.Current.Get<SceneController>().loadMapScreen,
                        // () =>
                        // {
                        //     if (MODE == EMode.CAMP_SURVIVE)
                        //         EventBus<IsFinishBattleEvent>.Raise(new IsFinishBattleEvent());
                        // });
                });
            }//);
        }





    }
}