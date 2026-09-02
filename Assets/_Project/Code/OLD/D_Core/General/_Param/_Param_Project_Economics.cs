using UnityEngine;

namespace Galactic1
{
    /*
     *      Экономка для компонентов использующих HARD / AD
     */



    public class PARAM_PROJECT_ECONOMICS_SafeBox
    {
        public struct CData
        {
            public int soft,
                runes_levelUp,
                runes_ascension,
                scroll_hero;
        }
        
        /// <summary>
        /// Весь список макс хранения 
        /// </summary>
        public void GetLimit(byte level, out CData limit)
        {
            CData[] ar =
            {
                new() { soft = 10000, runes_levelUp = 10000, runes_ascension = 200, scroll_hero = 25 },
                new() { soft = 25000, runes_levelUp = 20000, runes_ascension = 350, scroll_hero = 50 },
                new() { soft = 50000, runes_levelUp = 35000, runes_ascension = 500, scroll_hero = 100 },
                new() { soft = 100000, runes_levelUp = 75000, runes_ascension = 750, scroll_hero = 150 },
                new() { soft = 200000, runes_levelUp = 125000, runes_ascension = 1250, scroll_hero = 250 },
                new() { soft = 500000, runes_levelUp = 250000, runes_ascension = 1750, scroll_hero = 400 },
                new() { soft = 1000000, runes_levelUp = 500000, runes_ascension = 2250, scroll_hero = 500 },
                new() { soft = 2000000, runes_levelUp = 1000000, runes_ascension = 2500, scroll_hero = 600 },
                new() { soft = 3000000, runes_levelUp = 1500000, runes_ascension = 2700, scroll_hero = 700 },
                new() { soft = 5000000, runes_levelUp = 2500000, runes_ascension = 3000, scroll_hero = 750 },
            };

            limit = ar[level];
        }

        /// <summary>
        /// Для увеличения макс хранения 
        /// </summary>
        public void SetNewLimit()
        {
            // GAMEPLAY_old.DataGamestat().safe_box_lv++;
            // if (GAMEPLAY_old.DataGamestat().safe_box_lv >= 9)
            //     GAMEPLAY_old.DataGamestat().safe_box_lv = 9;
        }

        /// <summary>
        /// Стоимость опустошения сейфа
        /// </summary>
        /// <returns></returns>
        public byte GetCostPurchase() => 49;
    }
    
    


    
    
    
    
    
    
    




    public class PARAM_PROJECT_ECONOMICS_CombatBonus
    {
        public struct CData
        {
            public string title, des;
            
            public string buyH2;
            public short buyResult;
            public byte buyCost;
            
            public string adH2;
            public short adResult;
            public byte adCost;
        }
        
        /// <summary>
        /// Установка бонусов для боя
        /// </summary>
        /// <param name="variant">list of bonuses</param>
        public PARAM_PROJECT_ECONOMICS_CombatBonus(out CData[] variant)
        {
            
            // если надо исключить бонус, не забудь изменить id => variant[??] = d
            
            variant = new CData[3];     // и здесь ^^
            
            
            // #1 auto battle
            CData d = new CData();
            d.title = "Auto-Battle";
            d.des = "Automatically starts the battle and takes the reward";
            
            // buy
            d.buyH2 = "Get 50 auto-battles";
            d.buyCost = 50;
            d.buyResult = 50;
            
            // ad
            d.adH2 = "Get 10 auto-battles";
            d.adCost = 1;
            d.adResult = 10;

            variant[0] = d;
            //
            
            // #2 bonus reward
            d = new CData();
            d.title = "Bonus Reward";
            d.des = "Increases battle reward by 25%";
            
            // buy
            d.buyH2 = "Get for 50 battles";
            d.buyCost = 100;
            d.buyResult = 50;
            
            // ad
            d.adH2 = "Get for 10 battles";
            d.adCost = 2;
            d.adResult = 10;

            variant[1] = d;
            //
            
            // #3 x2 speed
            d = new CData();
            d.title = "x2 Speed";
            d.des = "x2 speed of the battle";
            
            // buy
            d.buyH2 = "Get 60 min";
            d.buyCost = 20;
            d.buyResult = 3600;
            
            // ad
            d.adH2 = "Get 60 min";
            d.adCost = 1;
            d.adResult = 3600;

            variant[2] = d;
            //
        }
    }
}