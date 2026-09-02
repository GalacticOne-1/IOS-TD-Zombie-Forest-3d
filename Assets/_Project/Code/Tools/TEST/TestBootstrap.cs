using System;
using System.Collections.Generic;
using System.Linq;
using DEV;
using Galactic1.Code.Dev;
using Galactic1.Code.GameDatabase;
using Galactic1.Code.Gameplay.BaseBuilding;
using Galactic1.Code.Gameplay.Combat.Data;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Configs;
using Galactic1.Items;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Code.Gameplay.Damage;
using Galactic1.Code.Gameplay.Enemies.Repositories;
using Galactic1.Code.Gameplay.Survivors.Repositories;
using Galactic1.Code.UI.Inventory;
using Galactic1.Code.Gameplay.Units.Stats;
using Galactic1.Code.Inventory.Abstractions;
using Galactic1.Code.Systems.Economy;
using Galactic1.Game.Meta.Items;
using Galactic1.Structs;
using Galactic1.Systems;
using Galactic1.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Galactic1.Test
{
    public class TestBootstrap : MonoBehaviour
    {
        /*
         *      Для теста машины состояния
         */


        private byte n;



        private void Update()
        {
            if (SystemRepository.IsRelease) return;


            if (Input.GetKey(KeyCode.RightShift))
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))   // main
                {
                    AudioService.PlayMusic("Main");
                }

                if (Input.GetKeyDown(KeyCode.Alpha2))   // combat
                {
                    AudioService.PlayMusic("Combat");
                }
            }


            // level state
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.O))
                ServiceLocator.Current.Get<Bootstrap>().SetState(EGameState.OFF);

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.P))
                ServiceLocator.Current.Get<Bootstrap>().SetState(EGameState.LEVEL_PLAY);

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.U))
                ServiceLocator.Current.Get<Bootstrap>().SetState(EGameState.PAUSE);

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
                ServiceLocator.Current.Get<Bootstrap>().SetState(EGameState.RESUME);

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.F))
                ServiceLocator.Current.Get<Bootstrap>().SetState(EGameState.LEVEL_FINISH);

            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.E))
                ServiceLocator.Current.Get<Bootstrap>().SetState(EGameState.LEVEL_EXIT);


            if (Input.GetKey(KeyCode.C) && Input.GetKeyDown(KeyCode.V))
            {
                //GAMEPLAY.Victory();
                //new Level_CheckVictory();
            }
            //


            // SCENE LOADER
            if (Input.GetKey(KeyCode.M))
            {
                /*if(Input.GetKeyDown(KeyCode.Alpha1))
                    ServiceLocator.Current.Get<SceneService>().LoadScene(Scenes.MAP, true);

                if(Input.GetKeyDown(KeyCode.Alpha2))
                    ServiceLocator.Current.Get<SceneService>().LoadScene(Scenes.CAMP, true);

                if(Input.GetKeyDown(KeyCode.Alpha3))
                    ServiceLocator.Current.Get<SceneService>().LoadScene(Scenes.LOCATION, true);*/
            }
            //







            // scene management
            if (Input.GetKey(KeyCode.Tab))
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) // map
                    EventBus<WorldMapSceneRequestEvent>.Raise(new WorldMapSceneRequestEvent());

                if (Input.GetKeyDown(KeyCode.Alpha2)) // home
                    EventBus<HomeSceneRequestEvent>.Raise(new HomeSceneRequestEvent());

                if (Input.GetKeyDown(KeyCode.Alpha3)) // location
                    EventBus<LocationSceneRequestEvent>.Raise(new LocationSceneRequestEvent()
                    {
                        LocationId = ServiceLocator.Current
                            .Get<ConfigProvider>()
                            .Get<ApplicationConfig>().startingLocationId

                    });
            }

            // MAP
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.M))
                ServiceLocator.Current.Get<GameMachine>().Map_Enter();

            // to test location
            // if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.L))
            // {
            //     CameraFollow.I.STOP = true;
            //     ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.PlayerUnitData[0].OnDragon.Value = true;    // что бы юнит не падал
            //     cmd.Player_RemoveUnitFromScene();
            //     cmd.Player_RemoveDragonFromScene();
            //     cmd.LoadLocation(1);
            // }



            if (Input.GetKey(KeyCode.LeftAlt))
            {
                // if (Input.GetKeyDown(KeyCode.Alpha1))
                //     ServiceLocator.Current.Get<GameTimeService>().AdvanceByRaid(new RaidTimeData()
                //     {
                //         DurationDays = .4f
                //     });
                // if (Input.GetKeyDown(KeyCode.Alpha2))
                //     ServiceLocator.Current.Get<GameTimeService>().AdvanceByRaid(new RaidTimeData()
                //     {
                //         DurationDays = 1.3f
                //     });
                // if (Input.GetKeyDown(KeyCode.Alpha3))
                //     ServiceLocator.Current.Get<GameTimeService>().AdvanceByRaid(new RaidTimeData()
                //     {
                //         DurationDays = 3f
                //     });

                // TIME SERVICE
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    ServiceLocator.Current.Get<GameTimeService>().SkipToNextDay(TimeAdvanceReason.ManualSkip);
                }

                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    ServiceLocator.Current.Get<GameTimeService>().SpendHours(2, TimeAdvanceReason.ManualSkip);
                }

                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    ServiceLocator.Current.Get<GameTimeService>().SpendHours(9, TimeAdvanceReason.ManualSkip);
                }

                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    ServiceLocator.Current.Get<GameTimeService>().SpendHours(60, TimeAdvanceReason.ManualSkip);
                }
            }


            // if (Input.GetKeyDown(KeyCode.A))
            // {
            //     EventBus<ADSViewEvent>.Raise(new ADSViewEvent());
            // }



            // * GAME EVENT
            // -- battle reward
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Space))
            {
                new FinishLevel();
            }




            if (Input.GetKey(KeyCode.E))
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) // damage one
                {
                    var enemies = ServiceLocator.Current.Get<EnemyRepository>().ActiveEnemies.ToList();

                    foreach (var e in enemies)
                    {
                        if (!e.EnemyAdapter.Stats.IsDead)
                        {
                            DamageService.ApplyDamage(
                                null,
                                e.EnemyAdapter,
                                10,
                                DamageType.Bullet,
                                new() { BodyPart = BodyPartType.Torso });
                            break;
                        }
                    }
                }
                
                if (Input.GetKeyDown(KeyCode.Alpha2)) // kill one
                {
                    var enemies = ServiceLocator.Current.Get<EnemyRepository>().ActiveEnemies.ToList();

                    foreach (var e in enemies)
                    {
                        if (!e.EnemyAdapter.Stats.IsDead)
                        {
                            DamageService.ApplyDamage(
                                null,
                                e.EnemyAdapter,
                                1000,
                                DamageType.Bullet,
                                new() { BodyPart = BodyPartType.Torso });
                            break;
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.Alpha3)) // kill all in scene
                {
                    var enemies = ServiceLocator.Current.Get<EnemyRepository>().ActiveEnemies.ToList();

                    foreach (var e in enemies)
                    {
                        if (!e.EnemyAdapter.Stats.IsDead)
                        {
                            DamageService.ApplyDamage(
                                null,
                                e.EnemyAdapter,
                                1000,
                                DamageType.Bullet,
                                new() { BodyPart = BodyPartType.Torso });
                        }
                    }
                }
            }


            if (Input.GetKey(KeyCode.D))
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) // damage
                {
                    var player = ServiceLocator.Current.Get<SurvivorRepository>().ActiveSurvivors.ToList();

                    foreach (var p in player)
                    {
                        if (!p.UnitAdapter.Stats.IsDead)
                        {
                            DamageService.ApplyDamage(
                                null,
                                p.UnitAdapter,
                                10,
                                DamageType.Bullet,
                                new() { BodyPart = BodyPartType.Torso });
                            break;
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.Alpha2)) // kill
                {
                    var player = ServiceLocator.Current.Get<SurvivorRepository>().ActiveSurvivors.ToList();

                    foreach (var p in player)
                    {
                        if (!p.UnitAdapter.Stats.IsDead)
                        {
                            DamageService.ApplyDamage(
                                null,
                                p.UnitAdapter,
                                1000,
                                DamageType.Bullet,
                                new() { BodyPart = BodyPartType.Torso });
                            break;
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.Alpha3)) // kill all survivors
                {
                    var player = ServiceLocator.Current.Get<SurvivorRepository>().ActiveSurvivors.ToList();

                    foreach (var p in player)
                    {
                        if (!p.UnitAdapter.Stats.IsDead)
                        {
                            DamageService.ApplyDamage(
                                null,
                                p.UnitAdapter,
                                1000,
                                DamageType.Bullet,
                                new() { BodyPart = BodyPartType.Torso });
                        }
                    }
                }


                // спавн нового выжевшего
                // if (Input.GetKeyDown(KeyCode.S))
                // {
                //     var playerStatsBase = ServiceLocator.Current.Get<ConfigProvider>().Get<PlayerStatsBase>();
                //
                //     var stats = playerStatsBase.GetBaseStats();
                //     stats[StatId.Health] -= Random.Range(50, 0);
                //     
                //     PlayerProxy pp = new PlayerProxy(new PlayerData()
                //     {
                //         Id = Guid.NewGuid().ToString(),
                //         Stats = DictionaryUtility.ToList(stats),
                //         Inventory = new List<InventorySlotData>(),
                //         Equipment = new List<InventorySlotData>()
                //         
                //     });
                //
                //     //ServiceLocator.Current.Get<IGameStateProvider>()
                //         //.GameStateProxy.GameLoopContext.PlayerUnitData.Add(pp);
                //     ServiceLocator.Current.Get<GameSession>().GameLoopContext.CreateUnitCompletely(pp);
                //     
                //     DLog.Alert($"New unit {pp.Id}");
                // }
            }


            if (Input.GetKey(KeyCode.F))
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) // damage
                {
                    GConsole.ClearLog();
                    var facility =
                        ServiceLocator.Current.Get<BaseFacilityRepository>().All.Values.ToList();

                    foreach (var f in facility)
                    {
                        if (f.SceneContext == null)
                            continue;

                        DLog.Alert($"Facility damaged: {f.name}");
                        if (!f.SceneContext.Stats.IsDead)
                        {
                            DamageService.ApplyDamage(
                                null,
                                f.SceneContext,
                                10,
                                DamageType.Hit,
                                new() { BodyPart = BodyPartType.Torso });
                            break;
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.Alpha2)) // damage
                {
                    var facility =
                        ServiceLocator.Current.Get<BaseFacilityRepository>().All.Values.ToList();

                    foreach (var f in facility)
                    {
                        if (f.SceneContext == null || f is CampHQInstance)
                            continue;

                        DLog.Alert($"Facility damaged: {f.name}");
                        if (!f.SceneContext.Stats.IsDead)
                        {
                            DamageService.ApplyDamage(
                                null,
                                f.SceneContext,
                                30,
                                DamageType.Hit,
                                new() { BodyPart = BodyPartType.Torso });
                            break;
                        }
                    }
                }

            }


            // === Spawn one enemy ===
            if (Input.GetKey(KeyCode.D) && Input.GetKeyDown(KeyCode.E))
            {
                DEV_polygon.I.SpawnTarget();
            }






            if (Input.GetKey(KeyCode.Q))
            {
                // clear camp inventory
                if (Input.GetKeyDown(KeyCode.C))
                {
                    DevTestResolver.ClearInventory();
                }


                // INBOX
                if (Input.GetKeyDown(KeyCode.LeftCommand))
                {
                    // Dictionary<string, (int, int)> items = new()
                    // {
                    //     { "weapon.rifle.ak78", (1, Random.Range(23, 80))}, { "resource.steel.plate", (20, 0) }, 
                    //     { "consumable.first.aid", (3, 0) },  { "resource.water", (20, 0) },
                    // };
                    // GConsole.ClearLog();
                    // var inbox = ServiceLocator.Current.Get<InboxService>();
                    // foreach (var it in items)
                    // {
                    //     inbox.AddReward(it.Key, it.Value.Item1, it.Value.Item2);
                    // }
                    //
                    //
                }


                // INVENTORY
                if (Input.GetKeyDown(KeyCode.Space))    // curreent starter kit
                {
                    DevTestResolver.LoadStarterKit();
                }

                if (Input.GetKeyDown(KeyCode.R))        // all resources
                {
                    DevTestResolver.LoadAllResources();
                }
                
                if (Input.GetKeyDown(KeyCode.K))        // construction kit
                {
                    DevTestResolver.LoadConstructionKit();
                }
            }






            // 
            // if (Input.GetKeyDown(KeyCode.X))
            // {
            //     var l = GAMEPLAY.DataGameplay().gridObj.Length;
            //     for (int i = 0; i < l; i++)
            //     {
            //         GAMEPLAY.DataGameplay().gridObj[i].fuel.duration = 0;
            //     }
            //     DLog.Alert("Clear workstations!");
            // }
            //
            // // add time
            // if (Input.GetKeyDown(KeyCode.Alpha1))
            // {
            //     DLog.Alert("---------------     ADDED 10 sec");
            //     TimeManagement.currDateInSeconds += 10;
            // }
            //
            // if (Input.GetKeyDown(KeyCode.Alpha2))
            // {
            //     DLog.Alert("---------------     ADDED 60 sec");
            //     TimeManagement.currDateInSeconds += 60;
            // }
            //
            // if (Input.GetKeyDown(KeyCode.Alpha3))
            // {
            //     DLog.Alert("---------------     ADDED 1800 sec");
            //     TimeManagement.currDateInSeconds += 1800;
            // }
            //
            // if (Input.GetKeyDown(KeyCode.Alpha4))
            // {
            //     DLog.Alert("---------------     ADDED 3600 sec");
            //     TimeManagement.currDateInSeconds += 3600;
            // }
            //


            // add to inbox
            /*if (Input.GetKeyDown(KeyCode.I))
            {
                new Inbox_ADD(new CPlayerInventory()
                {
                    unlock = true,
                    type = 0,
                    category = 0,
                    id = 0,
                    volume = 25,
                    strength = 100
                });
                new Inbox_ADD(new CPlayerInventory()
                {
                    unlock = true,
                    type = 1,
                    category = 0,
                    id = 19,
                    volume = 3,
                    strength = 100
                });
            }*/


            // unlock bunker
            // if (Input.GetKeyDown(KeyCode.B))
            // {
            //     GAMEPLAY.DataGameplay().location_bunker[0].unlocked = true;
            //     GAMEPLAY.DataGameplay().location_bunker[0].timerForLock = TimeManagement.GetTimeFinish(30);
            // }



            /*if (Input.GetKeyDown(KeyCode.L))
            {
                ServiceLocator.Current.Get<ViewGameController>().GetWindow(EWindow.NEW_UNIT, new NewUnitModel.CData()
                {
                    title = "Kokok",
                    klass = "Kraken"
                });
            }*/

            /*if (Input.GetKeyDown(KeyCode.L))
            {
                ServiceLocator.Current.Get<ViewGameController>().GetWindow(EWindow.LEVEL_UP_UNIT, new LevelUpUnitModel.CData()
                {

                });
            }*/
            /*if (Input.GetKeyDown(KeyCode.L))
            {
                ServiceLocator.Current.Get<ViewGameController>().GetWindow(EWindow.NEW_MODIFICATOR, new NewModificatorModel.CData()
                {

                });
            }*/



            /*if (Input.GetKeyDown(KeyCode.N))
            {
                var e = ServiceLocator.Current.Get<ViewGameController>().MainMenuViewModel.GetViewItem(0);
                ServiceLocator.Current.Get<ScreenGFXController>().FXNewContent(new ScreenGFXController.CGFXData()
                {
                    size = e.item.GetComponent<RectTransform>().sizeDelta,
                    coord = e.item.transform.position,
                    button = e.item.sprite,
                    icon = e.icon.sprite
                });
            }
            if (Input.GetKeyDown(KeyCode.M))
            {
                ServiceLocator.Current.Get<ScreenGFXController>().FXResetNewContent();
            }*/


            // if (Input.GetKeyDown(KeyCode.N))
            // {
            //     GAMEPLAY.DataGamestat().playerLevel++;
            //     ServiceLocator.Current.Get<AccessController>().CheckScrapEnrichment(new ReachNewPlayerLevelEvent()
            //     {
            //         new_level = GAMEPLAY.DataGamestat().playerLevel
            //     });
            // }

            // имитация выхода из боя (снятие чернеого экрана)
            // if (Input.GetKeyDown(KeyCode.N))
            // {
            //     ServiceLocator.Current.Get<GameMachine>().ar_reward = new AddingReward.CReward[]
            //     {
            //         new () { type = EStat.SOFT, volume = 100},
            //         new () { type = EStat.HARD, volume = 5},
            //         new () { type = EStat.PLAYER_XP, volume = 100}
            //     };
            //     ServiceLocator.Current.Get<StatController>().Add(EStat.PLAYER_XP, 20, FBA.EPayment.not_payment, false, false); 
            //     EventBus<IsGameEvent>.Raise(new IsGameEvent());
            // }



            // if (Input.GetKeyDown(KeyCode.Q))
            // {
            //     ServiceLocator.Current.Get<ViewGameController>().GetWindow(EWindow.EQUIPMENT_UPGRADE,
            //         new ScrapLotteryFinishModel.CData()
            //         {
            //             score = 500,
            //             unitId = 0,
            //             type = 0,
            //             id = 0
            //         });
            // }


            // -- popup message
            // if (Input.GetKeyDown(KeyCode.X))
            // {
            //     ServiceLocator.Current.Get<PopupController>().AddMessage($"Wave Done +{Random.Range(5, 20)}coin");
            // }



            // -- fortress damage
            // if (Input.GetKeyDown(KeyCode.D))
            // {
            //     HUBLink.playerBase.hpRef.CMD_DAMAGE(new CHPData()
            //     {
            //         status = EStatusDamage.hit,
            //         damage = 5
            //     });
            // }


            // -- silver per kill
            // if (Input.GetKeyDown(KeyCode.X))
            // {
            //     ServiceLocator.Current.Get<CoreEconomicController>().RewardForKill(1,1, Vector2.zero, true);
            // }





            // *** FORMULAS

            /*if (Input.GetKeyDown(KeyCode.Space))
            {
                lv++;
                Form();
                DLog.Alert($">>> {val}");
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                lv = 0;
                Form();
                DLog.Alert($">>> {val}");
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                GConsole.ClearLog();
                lv = 0;
                for (int i = 0; i < 20; i++)
                {
                    Form();
                    lv++;
                    DLog.Alert($">>> {i} __ {val}", EDlogColor.YELLOW);
                }
            }*/

        }

        void Form()
        {
            //val = Convert.ToInt32(b * Math.Pow(coeff, lv) + start);
            //val = Convert.ToInt32(start * Math.Pow(coeff, lv));
            //val = Convert.ToInt32(Math.Floor(lv + start * Math.Pow(coeff, lv / b)));
            //val =  start + startCoeff + (startCoeff + (coeff + coeff * lv) / 2) * lv;
            //val = ServiceLocator.Current.Get<UpgradeController>().UpgradeFormula(start,startCoeff, coeff, lv);
        }


        public float start, startCoeff;
        public float coeff;
        public float b, val;
        public int lv;
    }
}