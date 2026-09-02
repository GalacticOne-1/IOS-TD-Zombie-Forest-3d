using UnityEngine;

namespace Galactic1
{
    /*
     *      Награда для всей рекламы в игре
     */






    public class PARAM_AD_REWARD_AfterBattle
    {
        /// <summary>
        /// Процент для рекламы
        /// </summary>
        /// <param name="bonus_standart">x2</param>
        /// <param name="bonus_auto">for auto battle 20%</param>
        public PARAM_AD_REWARD_AfterBattle(out float bonus_standart, out float bonus_auto)
        {
            bonus_standart = 2;
            bonus_auto = 1.2f;
        }
    }




    public class PARAM_AD_REWARD_AdShopData
    {
        
        private CStatData[] ar = new CStatData[]
        {
            new () { BankResourceType = EBankResourceType.CurrencyPremium, volume = 50 },
            new () { BankResourceType = EBankResourceType.CurrencySoft, volume = 22100 },
            // new () { stat = EStat.runes_levelUp, volume = 12730 },
            // new () { stat = EStat.runes_ascension, volume = 9},
            // new () { stat = EStat.scroll_hero, volume = 40 },
        };

        /// <summary>
        /// Передает массив награды с учетом текущего прогресса игры
        /// </summary>
        /// <param name="data"></param>
        public PARAM_AD_REWARD_AdShopData(out CStatData[] data)
        {
            data = new CStatData[5];
            // for (byte i = 0; i < 5; i++)
            // {
            //     data[i] = new CStatData();
            //     data[i].stat = ar[i].stat;
            //     var volume = ar[i].volume;
            //     GetVolume(i, ref volume);
            //     data[i].volume = volume;
            // }
        }
        
        // * update reward volume
        void GetVolume(byte id, ref int volume)
        {
            if (id == 0)         // for ruby
            {
                int[] qu =
                {
                    100, 150, 175, 200, 250, 300, 350, 375, 400, 450, 500, 550, 600, 650, 700, 725, 750, 800, 900, 1000
                };

                volume = Random.Range(10, 51);
            }
            
            else
            {
                // * каждые пройденные Globals.STEP_STAGE_AD_SHOP увеличиваем объем награды на 30%
                //volume += (int)(volume * (int)(GAMEPLAY_old.CurrentStage / Globals.STEP_STAGE_AD_SHOP) * .3f);
            }
        }
    }

}