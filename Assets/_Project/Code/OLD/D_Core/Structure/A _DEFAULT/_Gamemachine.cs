
using Galactic1.Configs;
using Galactic1.Core.Location;
using Galactic1.Gameplay.Player;
using Galactic1.Repository;
using UnityEngine;

namespace Galactic1
{
    /*
     *      Классы для загрузки и остановки уровня в разных режимах
     *      (юниты, объекты и тд)
     */
    
    
    
    public class PLAYER_IN_CAMP
    {
        public PLAYER_IN_CAMP(out bool camp_close) => camp_close = false;//GAMEPLAY_old.CurrentStage < 9;
    }



    #region PLAYER UNIT


    public class PlayerUnitLoad_Camp
    {
        public PlayerUnitLoad_Camp()
        {
            //new TUTORIAL_Status(out bool notActive);
            
            // GameObject.Find("_GROUND_CAMP_").GetComponent<BoxCollider2D>().enabled = true;
            //
            // // *** свободная граница для дракона
            // _PointerHub.locationBorderX = notActive ? Globals.CAMP_BORDER_X : Globals.CAMP_BORDER_CLOSE_X;
            // _PointerHub.locationBorderY = notActive ? Globals.CAMP_BORDER_Y : Globals.CAMP_BORDER_CLOSE_Y;
            // new LOCATION_SETUP().SetGroundBorderX(_PointerHub.locationBorderX);
            // new LOCATION_SETUP().SetGroundBorderY(_PointerHub.locationBorderY); 
            // //JoystickController.I.borderX = notActive ? Globals.CAMP_BORDER : Globals.CAMP_BORDER_CLOSE;
            // DLog.Alert($"Player load : {JoystickController.I.borderX}");
            //
            // // точки выхода из локации
            // ServiceLocator.Current.Get<LevelController>().locationExitLeft.transform.position = new Vector2(-21, 3);
            // ServiceLocator.Current.Get<LevelController>().locationExitRight.transform.position = new Vector2(59, 3);
            
            
            // * PLAYER
            //PlayerSTAT.I.INIT();
            //new UNIT(0).Spawn(ServiceLocator.Current.Get<GlobalRepository>().PlayerSpawnPoint);
            //HUBController.I.INIT();
            

            // camera
            CameraControllerOld.I.STOP = true;
            CameraFollow.I.Activator();
            //ServiceLocator.Current.Get<UnitLife>().Activator();
            //new ParallaxInit(ServiceLocator.Current.Get<ConfigProvider>().Get<LocationsConfigs>().Locations[0].general.BgSetup);
        }
    }
    
    public class PlayerUnitUnload
    {
        public PlayerUnitUnload()
        {
            DLog.Alert("Player : remove", EDlogColor.ORANGE, AppConstants.show_log_core);
            CameraFollow.I.IUpdateClear();
            //ServiceLocator.Current.Get<UnitLife>().IUpdateClear();
            ServiceLocator.Current.Get<HeroStateMachine>().ChangeState(HeroStateMachine.EPlayerController.Empty);
            //new UNIT(0).Remove();
            //PlayerInteractionController.I.Clear();
            new ParallaxClear();
        }
    }
    
    public class PlayerUnitLoad_Location
    {
        /// <summary>
        /// Спавн в локациях через карту
        /// </summary>
        public PlayerUnitLoad_Location()
        {
            //GameObject.Find("_GROUND_CAMP_").GetComponent<BoxCollider2D>().enabled = false;
            //var loc_setup = ServiceLocator.Current.Get<Environment>().location.GetChild(0).GetComponent<LocationSetup>();
            
            // *** свободная граница для дракона
            // _PointerHub.locationBorderX = loc_setup.BorderX;
            // _PointerHub.locationBorderY = loc_setup.BorderY;
            // new LOCATION_SETUP().SetGroundBorderX(_PointerHub.locationBorderX);
            // new LOCATION_SETUP().SetGroundBorderY(_PointerHub.locationBorderY); 
            //JoystickController.I.borderX = loc_setup.Border;
            
            // точки выхода из локации
            //ServiceLocator.Current.Get<LevelController>().locationExitLeft.transform.position = new Vector2(loc_setup.BorderX.x-9, 3);
            //ServiceLocator.Current.Get<LevelController>().locationExitRight.transform.position = new Vector2(loc_setup.BorderX.y+9, 3);
            
            // * PLAYER
            //PlayerSTAT.I.INIT();
            //new UNIT(0).Spawn(ServiceLocator.Current.Get<GlobalRepository>().PlayerSpawnPoint);
            //HUBController.I.INIT();
            

            // camera
            CameraControllerOld.I.STOP = true;
            CameraFollow.I.Activator();
            //ServiceLocator.Current.Get<UnitLife>().Activator();
            //new ParallaxInit(ServiceLocator.Current.Get<ConfigProvider>().Get<LocationsConfigs>()
                //.Locations[ServiceLocator.Current.Get<GlobalRepository>().CurrLocation].general.BgSetup);
        }
    }
    
    
    




    // public class GAMEMACHINE_Units
    // {
    //     public void Activate()
    //     {
    //         if (!DeveloperConsole.I.game.player_units_activate_NO)
    //         {
    //             var l = _PointerHub.player_unit.Length;
    //             for (int i = 0; i < l; i++)
    //             {
    //                 if (_PointerHub.player_unit[i] != null)
    //                 {
    //                     new UNIT(i).Activate();
    //                 }
    //             }
    //         }
    //     }
    //     
    //     /// <summary>
    //     /// Для правильной остановки
    //     /// </summary>
    //     public void Stop()
    //     {
    //         var l = _PointerHub.player_unit.Length;
    //         for (int i = 0; i < l; i++)
    //         {
    //             if (_PointerHub.player_unit[i] != null && _PointerHub.player_unit[i].STATE != EUnitStateType.DIE)
    //             {
    //                 //GAMEPLAY.DataGameplay().playerUnit[i].experience += HUBLink.player_unit[i].atrbRef.experinceForEnemies;
    //                 _PointerHub.player_unit[i].Entity_Deactivate(false);
    //             }
    //         }
    //     }
    //     
    //     public void Reset()
    //     {
    //         var l = _PointerHub.player_unit.Length;
    //         for (int i = 0; i < l; i++)
    //         {
    //             if (_PointerHub.player_unit[i] != null && _PointerHub.player_unit[i].STATE != EUnitStateType.DIE)
    //             {
    //                 //HUBLink.player_unit[i].ResetAnim();
    //             }
    //         }
    //     }
    //     
    //     /// <summary>
    //     /// Удаляет юниты из сцены
    //     /// </summary>
    //     public void Remove()
    //     {
    //         var l = _PointerHub.player_unit.Length;
    //         for (int i = 0; i < l; i++)
    //         {
    //             if (_PointerHub.player_unit[i] != null)
    //             {
    //                 new UNIT(i).Remove();
    //             }
    //         }
    //     }
    // }
    
    
    
    
    /*
     *      Группа методов для управления юнитами игрока
     */
    public class GAMEMACHINE_PlayerUnits
    {
        /// <summary>
        /// Удаляет юниты из сцены
        /// </summary>
        public void Remove()
        {
            // var l = _PointerHub.player_unit.Length;
            // for (int i = 0; i < l; i++)
            // {
            //     if (_PointerHub.player_unit[i] != null)
            //     {
            //         new UNIT(i).Remove();
            //     }
            // }
        }
        
        public void Loading()
        {
            // if (!DeveloperConsole.I.game.player_units_spawn_NO)
            // {
            //     for (int i = 0; i < Globals.MAX_PLAYER_UNIT; i++)
            //     {
            //         new SAVE_Hero().Get(i, out CPlayerUnit sd);
            //         if ((EAssetState)sd.state == EAssetState.UNLOCK)
            //         {
            //             new SAVE().DataGameplay().player_unit[i].died = false;
            //             //new UNIT(i).Create(new Vector2(sd.coordX, sd.coordY));
            //             new UNIT(i).Spawn(new Vector2(sd.coordX, sd.coordY));
            //         }
            //     }
            //
            // }
        }
        
        
        /// <summary>
        /// Перевод в активное состояние
        /// </summary>
        public void Launch()
        {
            // if (!DeveloperConsole.I.game.player_units_activate_NO)
            // {
            //     var l = _PointerHub.player_unit.Length;
            //     for (int i = 0; i < l; i++)
            //     {
            //         if (_PointerHub.player_unit[i] != null)
            //         {
            //             new UNIT(i).Activate();
            //         }
            //     }
            // }
        }
        
        /// <summary>
        /// Для правильной остановки и сохраняем значения
        /// </summary>
        public void Stop()
        {
            // var l = _PointerHub.player_unit.Length;
            // for (int i = 0; i < l; i++)
            // {
            //     if (_PointerHub.player_unit[i] != null && _PointerHub.player_unit[i].STATE != EUnitStateType.DIE)
            //     {
            //         //GAMEPLAY.DataGameplay().playerUnit[i].experience += HUBLink.player_unit[i].atrbRef.experinceForEnemies;
            //         _PointerHub.player_unit[i].Entity_Deactivate(false);
            //         GAME.DataGameplay().player_unit[i].hp = _PointerHub.player_unit[i].Features.curHp;
            //         GAME.DataGameplay().player_unit[i].armor = _PointerHub.player_unit[i].Features.curArmor;
            //     }
            // }
        }
        
        public void Reset()
        {
            // var l = _PointerHub.player_unit.Length;
            // for (int i = 0; i < l; i++)
            // {
            //     if (_PointerHub.player_unit[i] != null && _PointerHub.player_unit[i].STATE != EUnitStateType.DIE)
            //     {
            //         //HUBLink.player_unit[i].ResetAnim();
            //     }
            // }
        }
        
        /// <summary>
        /// Блокирует мертвые юниты
        /// </summary>
        public void Locking()
        {
            // for (int i = 0; i < Globals.MAX_PLAYER_UNIT; i++)
            // {
            //     new SAVE_Hero().Get(i, out CPlayerUnit sd);
            //     if (sd.died)
            //     {
            //         new SAVE().DataGameplay().player_unit[i].state = (byte)EAssetState.LOCK;
            //     }
            // }
        }
        
    }


    #endregion



    #region DRAGON

    public class SpawnDragon
    {
        /// <summary>
        /// Спавн в лагере
        /// </summary>
        public void SpawnInCamp()
        {
            new TUTORIAL_Status(out bool notActive);
            //JoystickController.I.borderX = notActive ? Globals.CAMP_BORDER : Globals.CAMP_BORDER_CLOSE;
            //DLog.Alert($"Dragon load : {JoystickController.I.borderX}");
            
            // точки выхода из локации
            //ServiceLocator.Current.Get<LevelController>().locationExitLeft.transform.position = new Vector2(-21, 3);
            //ServiceLocator.Current.Get<LevelController>().locationExitRight.transform.position = new Vector2(59, 3);
            
            
            // * DRAGON
            //PlayerSTAT.I.INIT();
            // var v = ServiceLocator.Current.Get<GlobalRepository>().PlayerSpawnPoint;
            // v.y += 2;
            // new DRAGON().Spawn(v);
            //HUBController.I.INIT();



            // camera
            //CORT.CameraOff();
            //CameraController.I.Activator();
            //CameraFollow.I.Activator();new ParallaxInit(ServiceLocator.Current.Get<LibController>().mapData.Mission[0].BgSetup);

            //ServiceLocator.Current.Get<UnitLive>().Activator();
        }

        /// <summary>
        /// Спавн в локации
        /// </summary>
        public void SpawnInLocation()
        {
            //var loc_setup = ServiceLocator.Current.Get<Environment>().location.GetChild(0).GetComponent<LocationSetup>();
            
            //JoystickController.I.borderX = loc_setup.Border;
            
            // точки выхода из локации
            //ServiceLocator.Current.Get<LevelController>().locationExitLeft.transform.position = new Vector2(loc_setup.Border.x-9, 3);
            //ServiceLocator.Current.Get<LevelController>().locationExitRight.transform.position = new Vector2(loc_setup.Border.y+9, 3);
            
            // * PLAYER
            //PlayerSTAT.I.INIT();
            //var v = ServiceLocator.Current.Get<GlobalRepository>().PlayerSpawnPoint;
            //v.y += 2;
            //new DRAGON().Spawn(v);
            //HUBController.I.INIT();
            
            // camera
            //CORT.CameraOff();
            //CameraController.I.Activator();
            //CameraFollow.I.Activator();
            //new ParallaxInit(ServiceLocator.Current.Get<LibController>().mapData.Mission[_PointerHub.CUR_LOCATION].BgSetup);
        }


        public void RemoveFromScene()
        {
            DLog.Alert("Dragon : remove");
            //CameraFollow.I.IUpdateClear();
            //new DRAGON().Remove();
        }
    }
    

    #endregion



    #region PLAYER CAMP

    public class LoadDeveloperLocation
    {
        /// <summary>
        /// Для загрузки локации вместо лагеря (DEV)
        /// </summary>
        public LoadDeveloperLocation()
        {
            // if (ServiceLocator.Current.Get<GlobalRepository>().CurrLocation == 0)
            // {
            //     Debug.LogError("Developer location must be > 0");
            //     return;
            // }
            
            // #1 disable all founds location in scene
            //var locations = GameObject.FindObjectsOfType<LocationSetup>(true);
            // foreach (var loc in locations)
            // {
            //     loc.gameObject.SetActive(false);
            // }
            
            // #2 load requires location
            cmd.LoadLocation();
            //new SAVE().DataGameplay().playerUnit[0].onDragon = true;        // юнит всегда на драконе (земля есть только в лагере) 
        }
    }

    
    public class PlayerCamp_Loading
    {
        /// <summary>
        /// Восстановление всех объектов на базе игрока
        /// </summary>
        public PlayerCamp_Loading()
        {
            new TUTORIAL_Status(out bool notActive);
            
            // #1 устанавливаем границы
            //GameObject.Find("_GROUND_CAMP_").GetComponent<BoxCollider2D>().enabled = true;
            //var globalRepository = ServiceLocator.Current.Get<GlobalRepository>();
            
            // *** сохраняем границу локации
            // globalRepository.LocationBorderX = 
            //     new Vector2(-10, 
            //         ServiceLocator.Current.Get<ConfigProvider>().Get<LocationsConfigs>().Locations[0].general.locationBorder.x+10);
            // globalRepository.LocationBorderY = 
            //     new Vector2(1.5f, 
            //         ServiceLocator.Current.Get<ConfigProvider>().Get<LocationsConfigs>().Locations[0].general.locationBorder.y+10);
            // применяем границу локации т.к есть земля
            //new LOCATION_SETUP().SetGroundBorderX(globalRepository.LocationBorderX);
            //new LOCATION_SETUP().SetGroundBorderY(globalRepository.LocationBorderY); 
            // ****************************************************************************************************
            
            // spawn for player
            //globalRepository.PlayerSpawnPoint = 
               // ServiceLocator.Current.Get<ConfigProvider>().Get<LocationsConfigs>().Locations[0].general.PlayerSpawnPoint;
            
            // точки выхода из локации
            //ServiceLocator.Current.Get<LevelController>().locationExitLeft.transform.position = new Vector2(-21, 3);
            //ServiceLocator.Current.Get<LevelController>().locationExitRight.transform.position = new Vector2(59, 3);
            
            
            // #2 комнаты и объекты игрока
            //GridController.I.Load();
            // * запускаем производство
            //ServiceLocator.Current.Get<MonoBehaviourMaster>().update_sec.Add(ServiceLocator.Current.Get<WorkbechProduction>());
            
            
            // #3 доп объекты для обычной игры
            if (notActive)
            {
                //ServiceLocator.Current.Get<ViewGameController>().CampBonusViewModel.LoadObject();
                //ServiceLocator.Current.Get<ViewGameController>().EventLocationViewModel.LoadObject();
            }
            
            // доп объекты для обучения 
            else
            {
                //ServiceLocator.Current.Get<Tutorial_camp>().CreateObjects();
            }
        }
    }

    public class PlayerCamp_Clear
    {
        /// <summary>
        /// Очищение базы игрока
        /// </summary>
        public PlayerCamp_Clear()
        {
            // * останавливаем производство
            //ServiceLocator.Current.Get<MonoBehaviourMaster>().update_sec.Remove(ServiceLocator.Current.Get<WorkbechProduction>());
            
            var l = ServiceLocator.Current.Get<Environment>().playerObj.childCount;
            for (int i = 0; i < l; i++)
            {
                // if (ServiceLocator.Current.Get<Environment>().playerObj.GetChild(i).GetComponent<PlayerObj>() != null && 
                //     ServiceLocator.Current.Get<Environment>().playerObj.GetChild(i).GetComponent<PlayerObj>().Fsm != FSM.die)
                // {
                //     ServiceLocator.Current.Get<Environment>().playerObj.GetChild(i).GetComponent<PlayerObj>().CMD_DEACTIVATE();
                // }
            }
            ServiceLocator.Current.Get<Environment>().playerObj.MakeEmpty();
        }
    }



    

    #endregion
    




    #region LEVEL SETUP

    /*
     *      Общие настройки для разных режимов
     */
    public class LEVEL_SETUP
    {
        /// <summary>
        /// Для старта
        /// </summary>
        public void Enter()
        {
            //GConsole.ClearLog();
            ServiceLocator.Current.Get<GameMachine>().STATUS = GameMachine.EStatus.NON;
            //Screen.sleepTimeout = SleepTimeout.NeverSleep;
            //ServiceLocator.Current.Get<GameMachine>().gold_battle = 0;
            //ServiceLocator.Current.Get<GameMachine>().experience_battle = 0;
            //ServiceLocator.Current.Get<GameMachine>().ar_reward = null;
            //GAMEPLAY.ResetWave();
            //SpeedBattle.I.RestoreSpeed();
        }

        /// <summary>
        /// Для завершения 
        /// </summary>
        public void Exit()
        {
            // энерго сберегательный режим
            // так же нужно включать при паузе или меню
            //Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
    }

    #endregion
    
    

    #region REGULAR LEVEL

    public class GetPlayerSpawnPoint
    {
        public GetPlayerSpawnPoint(out Vector2 coord)
        {
            coord = Vector2.zero;
            // ServiceLocator.Current.Get<ConfigProvider>().Get<LocationsConfigs>()
            //     .Locations[ServiceLocator.Current.Get<GlobalRepository>().CurrLocation].general
            //     .GetLocationPoints(out Vector2 locationCenter, out Vector2 locationBorder);
            //coord = FUNC.GetRandomPointInsideSquare2D(locationCenter, locationBorder, 6);
        }
    }

    
    /// <summary>
    /// Подготавляваем дату для нового запуска уровня/битвы
    /// </summary>
    public class StartLevelData
    {
        public StartLevelData()
        {
            //GConsole.ClearLog();
            ServiceLocator.Current.Get<GameMachine>().STATUS = GameMachine.EStatus.NON;
            //ServiceLocator.Current.Get<GameMachine>().gold_battle = 0;
            //ServiceLocator.Current.Get<GameMachine>().experience_battle = 0;
            //ServiceLocator.Current.Get<GameMachine>().ar_reward = null;
            //GAMEPLAY.ResetWave();
            //SpeedBattle.I.RestoreSpeed();
        }
    }

    /// <summary>
    /// Для обычной битвы
    /// </summary>
    public class LevelSetting_Regular
    {
        public LevelSetting_Regular()
        {
            DLog.Alert("***         Level Setting", EDlogColor.YELLOW);
            
            ServiceLocator.Current.Get<MonoBehaviourMaster>().isPause = false;
            //ServiceLocator.Current.Get<MusicManagement>().MusicBattle();
            
            
            // устанавливаем границы ******************************************************************************
            //GameObject.Find("_GROUND_CAMP_").GetComponent<BoxCollider2D>().enabled = false;
            //var globalRepository = ServiceLocator.Current.Get<GlobalRepository>();
            
            // *** сохраняем границу локации
            // globalRepository.LocationBorderX = new Vector2(-10, ServiceLocator.Current.Get<ConfigProvider>().Get<LocationsConfigs>().Locations[globalRepository.CurrLocation].general.locationBorder.x+10);
            // globalRepository.LocationBorderY = new Vector2(-10, ServiceLocator.Current.Get<ConfigProvider>().Get<LocationsConfigs>().Locations[globalRepository.CurrLocation].general.locationBorder.y+10);
            // // применяем границу локации т.к игрок спавнится в локации только на драконе
            // new LOCATION_SETUP().SetGroundBorderX(globalRepository.LocationBorderX);
            // new LOCATION_SETUP().SetGroundBorderY(globalRepository.LocationBorderY);
            // ****************************************************************************************************
            
            // spawn for player
            //new GetPlayerSpawnPoint(out globalRepository.PlayerSpawnPoint);
            //DLog.Alert($"prefab >> {ServiceLocator.Current.Get<LibController>().mapData.Locations[globalRepository.CUR_LOCATION].general.PrefabPath}");
            
            // location
            // var createdLocation = 
            //     $"Prefabs/Gameplay/Locations/{ServiceLocator.Current.Get<ConfigProvider>().Get<LocationsConfigs>().Locations[globalRepository.CurrLocation].general.PrefabPath}".CreateGO(ServiceLocator.Current.Get<Environment>().location.transform);
            
            // загрузка ящиков
            //createdLocation.GetComponent<LocationSetup>().LoadCrateItems(
                //ServiceLocator.Current.Get<LibController>().mapData.Mission[globalRepository.CUR_LOCATION].PossibleReward);
            
            // спавним существ
            //createdLocation.GetComponent<LocationSpawner>().LoadCreatures();
        }
    }

    public class LevelStop_Regular
    {
        public LevelStop_Regular()
        {
            DLog.Alert("***         Level Stoped", EDlogColor.YELLOW);
            CORT.BlockScreen(true);
            
            // ServiceLocator.Current.Get<LevelController>().Gameplay_stop(() =>
            // {
            //     // что-то после остановки всех юнитов и спавнера
            //     // ...
            //    
            //     CORT.BlockScreen(false);
            // });
        }
    }

    public class LevelClear_Regular
    {
        public LevelClear_Regular()
        {
            DLog.Alert("***         Level Clear", EDlogColor.YELLOW);
            
            // #1 clear creatures in locatioon
            //if(ServiceLocator.Current.Get<Environment>().location.transform.childCount > 0)
                //ServiceLocator.Current.Get<Environment>().location.GetChild(0).GetComponent<LocationSpawner>().UnloadCreatures();
            //Pool.I.Clear();
            //ServiceLocator.Current.Get<AssetController>().Units_remove();
            
            // #2 remove location
            if (ServiceLocator.Current.Get<Environment>().location.transform.childCount > 0)
                ServiceLocator.Current.Get<Environment>().location.GetChild(0).DestroyGO();
            
            switch (ServiceLocator.Current.Get<GameMachine>().STATUS)
            {
                case GameMachine.EStatus.DEFEAT:
                {
                    
                } break;
                
                case GameMachine.EStatus.VICTORY:
                {
                    
                } break;
            }
        }
    }
    //
    //
    // public class GameOver_Regular
    // {
    //     /*
    //      *      восстанавливаем юниты и объекты в полном объеме
    //      */
    //     public GameOver_Regular()
    //     {
    //         DLog.Alert("***         Game Over Regular", EDlogColor.YELLOW);
    //         
    //         ServiceLocator.Current.Get<ViewGameController>().GetStats( new[] { EStat.SOFT});
    //         
    //         
    //         // #1 CLEAR SCENE
    //         new GAMEMACHINE_PlayerUnits().Remove();
    //         
    //         // #2 clear destroed objects
    //         new GAMEMACHINE_PlayerDefStructure().ClearDestroyed();
    //         ServiceLocator.Current.Get<ViewGameController>().ConstructViewModel.Model.CheckBuildsToRepair();
    //         
    //         // #3 NEW LOAD
    //         new GAMEMACHINE_PlayerUnits().Loading();
    //         
    //         // #4
    //         DEV_polygon.I.ClearPolygon();
    //
    //         // reset pool
    //         ServiceLocator.Current.Get<LevelController>().regularSpawn.ClearUnitsPool();
    //         Pool.I.Deactivate();
    //         
    //         
    //     }
    // }
    

    #endregion



    


    
}