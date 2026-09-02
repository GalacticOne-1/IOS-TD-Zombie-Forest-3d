using System.Collections.Generic;
using Galactic1;
using Galactic1.Mobile;
using UnityEngine;

namespace Galactic1
{
    public class AddingReward
    {
        public struct CReward
        {
            public EBankResourceType type;
            public int volume;
            public Vector2 start;

            public bool isEquipment;
            public EItems itemKey;
            public EEquipment equipKey;
        }
        
        /// <summary>
        /// для передачи наград игроку
        /// <br/>(новый лвл/конец боя/реклама и тд)
        /// </summary>
        /// <param name="list">[ ] reward</param>
        // public AddingReward(CReward[] list, StatController.EUpdateGUI updateGUI = StatController.EUpdateGUI.Basic)
        // {
        //     var l = list.Length;
        //     for (int i = 0; i < l; i++)
        //     {
        //         //DLog.Alert($"Adding reward {list[i].type} [{list[i].volume}]; update gui [{updateGUI}]");
        //
        //         ServiceLocator.Current.Get<StatController>().Add(
        //             list[i].type,
        //             list[i].volume,
        //             false,
        //             updateGUI);
        //     }
        // }
        /// <summary>
        /// для передачи одной награды игроку <br/>(новый лвл/конец боя/реклама и тд)
        /// </summary>
        //public AddingReward(CReward reward, StatController.EUpdateGUI updateGUI = StatController.EUpdateGUI.Basic) : this(new CReward[] { reward }, updateGUI) {}
    }


    public class IncreaseReward
    {
        /// <summary>
        /// Увеличениe значения в награде 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="percent">must have > 1; 1.5f, 1.75f, 2, 3 etc</param>
        public IncreaseReward(ref AddingReward.CReward[] list, float percent)
        {
#if UNITY_EDITOR
            if (percent <= 1)
            {
                Debug.LogError("Процент для бонуса должен быть > 1");
                return;
            }
#endif
            var l = list.Length;
            for (int i = 0; i < l; i++)
            {
                if (list[i].type != EBankResourceType.PlayerXp)
                    list[i].volume = (int)(list[i].volume * percent);
            }
        }

        /// <summary>
        /// Увеличениe значения в награде (must have > 1; 1.5f, 1.75f, 2, 3 etc)
        /// </summary>
        /// <param name="reward"></param>
        /// <param name="percent"></param>
        public IncreaseReward(ref AddingReward.CReward reward, float percent)
        {
#if UNITY_EDITOR
            if (percent <= 1)
            {
                Debug.LogError("Процент для бонуса должен быть > 1");
                return;
            }
#endif
            reward.volume = (int)(reward.volume * percent);
        }
    }


    /*
     *    Выдача награды по выходу из боя
     *    (сами значения не передаем, только обновляем отображение,
     *      передача значений происходит по закрытии экрана FinishLevelModel)
     */
    public class RewardAfterLevel
    {
        public RewardAfterLevel()
        {
            var ar_reward = ServiceLocator.Current.Get<GameMachine>().ar_reward;
            ServiceLocator.Current.Get<GameMachine>().ar_reward = null;
            var l = ar_reward.Length;
            var gfx_data = new ScreenGFXController.CHeapData[l];
            for (int i = 0; i < l; i++)
            {
                gfx_data[i].type = ar_reward[i].type;
                gfx_data[i].start = ar_reward[i].start.GetCenterScreen();
            }
            
            GameObject[] ClaimReward()
            {
                var g = new GameObject[l];
                ServiceLocator.Current.Get<ScreenGFXController>().FloatingHeap(gfx_data, out g);
                return g;
            }
            
            ServiceLocator.Current.Get<ContentQueueController>().AddQueue(new ContentQueueController.CWidgetSystem()
            {
                order = 98,
                //menu = EMainMenu.HOME,
                typeContent = ContentQueueController.EContent.WIDGET_LOAD_ARRAY,
                blockScreen = true,
                funcObjAr = ClaimReward
            });
        }
    }
    
    
    public class FinishLevel
    {
        public void Check()
        {
            if(ServiceLocator.Current.Get<GameMachine>().STATUS == GameMachine.EStatus.VICTORY)
            {
                ServiceLocator.Current.Get<ContentQueueController>().AddQueue(new ContentQueueController.CWidgetSystem()
                {
                    order = 20,
                    typeContent = ContentQueueController.EContent.WIDGET,
                    widget = ServiceLocator.Current.Get<ViewGameController>().FinishLevelPresenter.GetScreen(),
                    func = Show
                });
            }
        }
        
        
        /// <summary>
        /// для вызова окна окончания игры
        /// </summary>
        public void Show()
        {
            // #1 выдача снаряжения здесь
            // ,,,
            
            
            
            ServiceLocator.Current.Get<ViewGameController>().GetWindow(EWindow.FINISH, new FinishLevelModel.CData()
            {
                status = "Camp Clear!",
                //night = $"Day {GAMEPLAY_old.CurrentStage}",
                //reward = ServiceLocator.Current.Get<CoreEconomicController>().GetRewardCampDefense(),
            });
        }
    }
    
    public class FinishRaid
    {
        public void Check()
        {
            ServiceLocator.Current.Get<ContentQueueController>().AddQueue(new ContentQueueController.CWidgetSystem()
            {
                order = 9,
                typeContent = ContentQueueController.EContent.WIDGET,
                //widget = ServiceLocator.Current.Get<ViewGameController>().RaidCompleteViewModel.GetScreen(),
                func = Show
            });
        }
        
        
        /// <summary>
        /// для вызова окна окончания игры
        /// </summary>
        public void Show()
        {
            
            // ServiceLocator.Current.Get<ViewGameController>().GetWindow(EWindow.RAID_COMPLETE, new RaidCompleteModel.CData()
            // {
            //     //status = "Raid Complete!",
            //     reward = ServiceLocator.Current.Get<CoreEconomicController>().GetRewardRaid(),
            // });
        }
    }
    
    
    public class NewPlayerLevel
    {
        /// <summary>
        /// Проверка потребности для открытия панели
        /// </summary>
        public void Check()
        {
            // if (GAMEPLAY_old.DataGameplay().require_player_rank_screen)
            // {
            //     ServiceLocator.Current.Get<ContentQueueController>().AddQueue(new ContentQueueController.CWidgetSystem()
            //     {
            //         order = 10,
            //         typeContent = ContentQueueController.EContent.WIDGET,
            //         widget = ServiceLocator.Current.Get<ViewGameController>().NewLevelViewModel.GetScreen(),
            //         func = Show
            //     });
            // }
        }
        
        
        
        /// <summary>
        /// для вызова окна получения нового уровня
        /// </summary>
        public void Show()
        {
            /*
             *      даем фиксированную награду за новый лвл
             *      для дол места рекламы
             */

            //GAMEPLAY_old.DataGameplay().require_player_rank_screen = false;
            
            // * проверяем новые предметы
            new LIB_GetEquipments(out InventoryConfigs[] list);
            List<byte> newItems = new List<byte>();
            var l = list.Length;
            for (byte i = 0; i < l; i++)
            {
                //if(list[i].levelRequired == GAMEPLAY_old.PlayerRank)
                    //newItems.Add(i);
            }

            
            ServiceLocator.Current.Get<ViewGameController>().GetWindow(EWindow.LEVEL_UP_PLAYER, new NewLevelModel.CNewLevel()
            {
                //h2 = $"You are now a Rank {GAMEPLAY_old.PlayerRank+1} Survivor!",
                //level = GAMEPLAY_old.PlayerRank,
                reward = new AddingReward.CReward[]
                {
                    new (){ itemKey = EItems.Common_Plank, volume = 3},         
                    new (){ itemKey = EItems.Scrap_Metal, volume = 3},
                    new (){ itemKey = EItems.Piece_Cloth, volume = 3},
                    new (){ itemKey = EItems.Raw_Meat, volume = 3},
                },
                newBlueprints = newItems
            });
        }
    }

    
    /// <summary>
    /// Для проверки прохождения локации
    /// </summary>
    public class CheckNewLocation
    {
        public CheckNewLocation()
        {
            DLog.Alert("check New Location");
            /*if (ServiceLocator.Current.Get<GameMachine>().STATUS == GameMachine.EStatus.VICTORY 
                && GAMEPLAY.DataGameplay().curLocation < GAMEPLAY.DataGameplay().location.Length)
            {
                // ServiceLocator.Current.Get<ViewGameController>().GetWindow(EWindow.NEW_LOCATION, new NewLocationModel.CData()
                // {
                //     loc = $"Location {GAMEPLAY.DataGameplay().curLocation} Complete!"
                //});
                DLog.Alert("New Location");
                ServiceLocator.Current.Get<ContentQueueController>().AddQueue(new ContentQueueController.CWidgetSystem()
                {
                    order = 99,      // должно показывается самым первым
                    //menu = EMainMenu.HOME,
                    //widget = ServiceLocator.Current.Get<ViewGameController>().NewLocationPresenter.GetScreen(),
                    typeContent = ContentQueueController.EContent.WIDGET,
                    func = () =>
                    {
                        /*ServiceLocator.Current.Get<ViewGameController>().GetWindow(EWindow.NEW_LOCATION,
                            new NewLocationModel.CData()
                            {
                                loc = $"Location {GAMEPLAY.DataGameplay().curLocation} Complete!"
                            });#1#
                    }
                });
            }*/
        }
    }

    

}