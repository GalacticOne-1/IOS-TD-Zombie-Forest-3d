
using Galactic1;
using Galactic1.Configs;
using Galactic1.Mobile;
using Galactic1.Repository;
using UnityEngine;
using Galactic1.Core;
using Galactic1.Code.Systems.GameLoop.Tactical;
using Galactic1.Code.Systems.Raid.Mission;
using Galactic1.Systems;
using Galactic1.UI.Core;

namespace DEV
{
    public class DevUpdate : Singleton<DevUpdate>
    {

        public bool onTop;
        public float startX = 20, startY = 50;
        public Vector2 sizeWindow;
        public float size = 2;
        public GUIStyle style;



        [Space] public int equipmentSlot;
        public EDEVCMD cmd;
        public EDEVCMD cmd2;
        public enum EDEVCMD
        {
            add_experience, add_player_level, add_stage,
            add_unit_experience, add_unit_level,
            
            minus_energy, 
            add_hard, minus_hard,
            add_soft, minuse_soft,
            add_soul, minus_soul,
            
            review_panel, 
            
            check_exp_progress,
            update_quest,
            variant_lerp,
            show_reward,
            
            all_extension,
            
            collect_logs, collect_limestone, collect_berry, mine_iron_ore, mine_limestone,
            collect_scrap_metal, 
            unlock_chest,
        }

        public CDevUnitCMD unitCmd, unitCmd2;
        public enum CDevUnitCMD
        {
            player_kill, player_damage,
            dragon_kill, dragon_damage,
            
            p_broke_weapon, p_broke_armor, p_broke_tool,
            d_broke_weapon, d_broke_armor, d_broke_tool,
        }


        float startWindowX, startWindowY;
        private bool devPanel;



        public MissionObjectiveService missionObjectiveService;
        public SettingsUI SettingsUI;
        
        
        
        

        private void Awake()
        {
#if !UNITY_EDITOR
            size = 1.4f;
            //startY = Screen.height-100;
#endif

            //startWindowX = Screen.width;
            startWindowY = onTop ? sizeWindow.y+80 : Screen.height;
        }


        // regular button
        bool Button(float x, float y, string name) 
            => GUI.Button(new Rect((startX + x)*size, startY + y* (size / 2), 90*size, 30*size), name, style);
        
        
        void OnGUI()
        {
            if (!DeveloperConsole.I.core.showDevPanel
                || !_GameState.AppLoaded
                || !SettingsUI.PanelShowed) return;

            // incremental
            // if (Input.GetKeyDown(KeyCode.A))
            // {
            //     DLog.Alert($"{(long)(baseSumm * Mathf.Pow(percent, level))}","lime");
            // }

            if (GUI.Button(new Rect(100, 20, 40*size, 40*size), devPanel ? "X" : "O", style))
            {
                devPanel = !devPanel;
                //CORT.BLOCK_BUTTTONS = devPanel;
            }

            if (devPanel)
            {
                GUI.Window(0, new Rect(0, startWindowY-sizeWindow.y, sizeWindow.x, sizeWindow.y), 
                    WindowContent, "DEV PANEL");
            }
            
        }

        void WindowContent(int id)
        {
            if (Button(0, 140, $"{cmd}"))
                CMD(cmd);
            if (Button(100, 140, $"{cmd2}"))
                CMD(cmd2);
            
            if (Button(220, 140, $"{unitCmd}"))
                UnitCMD(unitCmd);
            if (Button(320, 140, $"{unitCmd2}"))
                UnitCMD(unitCmd2);
            
            
            //if (Button(0, 70, "Reward AD"))
                //ServiceLocator.Current.Get<AdController>().Test_reward();
            
            //if (Button(100, 70, "Inter AD"))
               // ServiceLocator.Current.Get<AdController>().Test_inter();
            
            if (Button(220, 70, "Clear SCP"))
                ScreenProfiler.ClearMessage();



            if (ServiceLocator.Current.Get<ConfigProvider>().Get<ApplicationConfig>().showCrashButton &&
                Button(320, 0, "CRASH"))
                FBA.CRASH();
            
            


            if (Button(0, 0, "Defeat"))
            {
                missionObjectiveService?.ForceFinished(new MissionResult()
                {
                    Status = MissionStatus.Defeat
                },
                typeof(SUB_RaidCleanupState));
            }

            if (Button(100, 0, "Victory"))
            {
                missionObjectiveService?.ForceFinished(new MissionResult()
                {
                    Status = MissionStatus.Victory
                },
                typeof(SUB_RaidCleanupState));
            }
            
            // ------------ ^ DEFAULT ^
            
            
            


            
            /*if (GUI.Button(new Rect(20, Screen.height/2, 120, 30), "New unit"))
            {
                StartCoroutine(Monetization.PanelNewUnit(null, ""));
            }

            if (GUI.Button(new Rect(150, Screen.height/2, 120, 30), "Gems"))
            {
                HUBStat.SetGems(100);
                StartCoroutine(Monetization.PanelNewUnit(null, ""));
            }
            
            
            if (GUI.Button(new Rect(280, Screen.height/2, 120, 30), "Pass 1 hour"))
            {
                TimeManagement.currDateInSeconds += TimeManagement.hourInSeconds;
            }*/

            


            if (Button(220, 210, "Clear INV"))
            {
                DevTestResolver.ClearInventory();
            }
            if (Button(320, 210, "All Resources"))
            {
                DevTestResolver.LoadAllResources();
            }

            if (Button(420, 210, "Constr. kit"))
            {
                DevTestResolver.LoadConstructionKit();
            }
            
            if (Button(520, 210, "Starter Kit"))
            {
                DevTestResolver.LoadStarterKit();
            }
        }

        

        void CMD(EDEVCMD cmd)
        {
            switch (cmd)
            {
                case EDEVCMD.add_experience:
                    //new UpdateXp(10);
                    break;
                case EDEVCMD.add_player_level:
                    //GAMEPLAY_old.ReachedPlayerRank();
                    break;
                case EDEVCMD.add_unit_experience:
                    //ServiceLocator.Current.Get<ViewGameController>().UnitMngmViewModel.AddExperience(0, 30);
                    break;
                case EDEVCMD.add_unit_level:
                    //ServiceLocator.Current.Get<ViewGameController>().UnitMngmViewModel.AddExperience(0, 1000);
                    break;
                
                case EDEVCMD.add_stage:
                    //GAMEPLAY.DataGameplay().stage += 20;
                    //ServiceLocator.Current.Get<LibController>().adventureMap.CheckProgress();
                    break;
                
                case EDEVCMD.add_hard:
                    //ServiceLocator.Current.Get<StatController>().Add(EBankResourceType.CurrencyPremium,5);
                    break;
                case EDEVCMD.minus_hard:
                    //ServiceLocator.Current.Get<StatController>().Take(EBankResourceType.CurrencyPremium,5);
                    break;
                
                case EDEVCMD.add_soft:
                    //ServiceLocator.Current.Get<StatController>().Add(EBankResourceType.CurrencySoft,100);
                    break;
                case EDEVCMD.minuse_soft:
                    //ServiceLocator.Current.Get<StatController>().Take(EBankResourceType.CurrencySoft,100);
                    break;
                
                
                case EDEVCMD.minus_energy:
                    //HUBStat.SetEnergy(-5);
                    break;
                
                case EDEVCMD.review_panel:
                    ServiceLocator.Current.Get<UIManager>().OpenScreen(
                        UIScreenId.Review,
                        null,
                        _ => { _.GetComponent<Review>().OnShow(); });
                    break;
                
                case EDEVCMD.update_quest:
                    //ServiceLocator.Current.Get<ConfigProvider>().GameConfigsOld.Lib.dailyQuestConfig.UpdateDailyQuest();
                    break;
                
                case EDEVCMD.variant_lerp:
                    DeveloperConsole.I.game.lerpVariant++;
                    if (DeveloperConsole.I.game.lerpVariant > 3) DeveloperConsole.I.game.lerpVariant = 0;
                    ScreenProfiler.AddMessage("***      New Lerp Variant "+DeveloperConsole.I.game.lerpVariant);
                    break;
                
                // ---------------
                
                
                case EDEVCMD.check_exp_progress:
                    DLog.Alert("--------------- REQUIRED EXPERIENCE", EDlogColor.YELLOW);
                    for (byte i = 0; i < 20; i++)
                    {
                        //Debug.Log($"#{i}___{ServiceLocator.Current.Get<ProgressController>().RequiredExperience(i)}");
                    }
                    break;


                case EDEVCMD.show_reward:
                {
                    ServiceLocator.Current.Get<ScreenGFXController>().FXReward(new ScreenGFXController.CGFXData2()
                    {
                        //icon = ServiceLocator.Current.Get<IconHub>().GetSpriteStat(EBankResourceType.CurrencySoft),
                        size = new Vector2(3,3),
                        txt = "Gold x9999"
                    });
                } break;


                
                
            }
            
            //SaveManagement.I.SaveMobile();
        }

        


        void UnitCMD(CDevUnitCMD cmd)
        {
            switch (cmd)
            {

                case CDevUnitCMD.player_damage:
                {
                    // var controller = ServiceLocator.Current.Get<PlayerRepository>().GetController;
                    //
                    // controller.StatsController.ModifyStat(StatType.Hunger, -30);
                    // controller.StatsController.ModifyStat(StatType.Thirst, -30);
                    //
                    // ServiceLocator.Current.Get<DamageSystem>().ApplyDamage(new DamageEvent()
                    // {
                    //     Attacker = null,
                    //     Target = controller,
                    //     Type = DamageType.Hit,
                    //     Amount = 15
                    // });
                }
                    break;
                
                
                

                case CDevUnitCMD.p_broke_armor:
                case CDevUnitCMD.p_broke_weapon:
                {
                    var controller = ServiceLocator.Current.Get<PlayerRepository>().GetController;
                    
                    //controller.EquipmentContainer_old.OnItemUsed(cmd == CDevUnitCMD.p_broke_weapon ? 0 : equipmentSlot, false);
                }
                    break;
                case CDevUnitCMD.p_broke_tool:
                {
                    var controller = ServiceLocator.Current.Get<PlayerRepository>().GetController;
                    
                    //controller.EquipmentContainer_old.OnItemUsed(0, true);
                }
                    break;

                case CDevUnitCMD.d_broke_armor:
                case CDevUnitCMD.d_broke_weapon:
                {
                    // var controller = ServiceLocator.Current.Get<DragonRepository>().GetController;
                    //
                    // controller.EquipmentContainer.OnItemUsed(cmd == CDevUnitCMD.d_broke_weapon ? 0 : equipmentSlot, false);
                }
                    break;
                case CDevUnitCMD.d_broke_tool:
                {
                    var controller = ServiceLocator.Current.Get<PlayerRepository>().GetController;
                    
                    //controller.EquipmentContainer_old.OnItemUsed(0, true);
                }
                    break;
            }
        }



    }
}