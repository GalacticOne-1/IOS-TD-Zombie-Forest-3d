using Galactic1;

namespace Galactic1
{
    public class ServerTimeConfigs
    {
        
        
        
        /// <summary>
        /// Все что должно откатыватся каждые сутки
        /// </summary>
        public void Reset_24h()
        {
            //GAMEPLAY_old.DataGamestat().gems_ad_free = false;           // кристалы за рекламу раз в сутки
            //ServiceLocator.Current.Get<ViewGameController>().DailyBonusViewModel.CheckDay();
            //ServiceLocator.Current.Get<LibController>().campBonus.NewSaveData();
        }
    }
}