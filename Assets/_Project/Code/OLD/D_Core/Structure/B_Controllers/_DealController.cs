using Galactic1.Mobile;
using Galactic1.Core;

namespace Galactic1
{
    /*
     *      Логика показа предложений AD Box/ Pay Wall/ Pay Offer / Game Event
     */




    
    public class GetDeal_DailyBonus
    {
        /// <summary>
        /// Ежедневная награда
        /// </summary>
        public GetDeal_DailyBonus()
        {
            DLog.Alert("* Deal Daily Bonus");
            new TUTORIAL_Status(out bool not_active);
            if(!ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.DailyReward.Value.dailyBonusShowed && not_active)
            {
                ServiceLocator.Current.Get<ContentQueueController>().AddQueue(new ContentQueueController.CWidgetSystem()
                {
                    order = 5,
                    //widget = ServiceLocator.Current.Get<ViewGameController>().DailyBonusViewModel.GetScreen(),
                    typeContent = ContentQueueController.EContent.WIDGET,
                    func = () =>
                    {
                        //ServiceLocator.Current.Get<ViewGameController>().DailyBonusViewModel.OpenWindow();
                    }
                });
            }
        }
    }

    


    public class GetDeal_ADEquipmentBox
    {
        /// <summary>
        /// Реклама для получения бокса снаряжения
        /// </summary>
        public GetDeal_ADEquipmentBox()
        {
            DLog.Alert("* Deal AD Equipment Box");
            new TUTORIAL_Status(out bool not_active);
            // if (Monetization.RewardedADS_Available() && not_active)
            // {
            //     
            //     ServiceLocator.Current.Get<ContentQueueController>().AddQueue(new ContentQueueController.CWidgetSystem()
            //     {
            //         order = 0,
            //         widget = ServiceLocator.Current.Get<ViewGameController>().EquipmentADBoxViewModel.GetScreen(),
            //         typeContent = ContentQueueController.EContent.WIDGET,
            //         func = () =>
            //         {
            //             ServiceLocator.Current.Get<ViewGameController>().EquipmentADBoxViewModel.ShowDeal();
            //         }
            //     });
            // }
        }
    }
}