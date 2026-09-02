
using Galactic1;
using UnityEngine;

namespace Galactic1
{
    /*
     *      Награда для разных режимов боя
     */


    
    



    /// <summary>
    /// Формирование награды для обычного боя
    /// </summary>
    public class PARAM_BATTLE_REWARD_Regular
    {
        public void GetList(out CStatData[] list)
        {
            list = new CStatData[4];
            list[0].BankResourceType = EBankResourceType.CurrencySoft;
            // list = new[]
            // {
            //     new CStatData() { stat = EStat.SOFT},
            //     // new CStatData() { stat = EStat.runes_levelUp},
            //     // new CStatData() { stat = EStat.runes_ascension},
            //     // new CStatData() { stat = EStat.scroll_hero},
            // };
        }

        public void GetReward(int numberMission, out CStatData[] list)
        {
            GetList(out list);

            // money
            list[0].volume = 200 + 5 * numberMission;

            // loot
            for (int i = 1; i < 4; i++)
            {
                list[i].isItem = true;
                //list[i].item = (EItem)Random.Range(0, 5);
                list[i].volume = 1;
            }
            
            
            // *************************************************************************************
            // ! коэффициент !
            //float multiplier = ServiceLocator.Current.Get<SettingsProvider>().GameSettings.Gameplay.REWARD_GAME_LOOP.multiplier;

            //var l = list.Length;
            //for (int i = 0; i < 1; i++)
                //list[i].volume = (int)(list[i].volume * multiplier);            // only money
        }
    }

    
    
}