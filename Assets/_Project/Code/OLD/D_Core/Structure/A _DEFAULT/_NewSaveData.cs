using Galactic1;
using UnityEngine;

namespace Galactic1
{

    /*
     *      Инициализация первого сохранения
     */



    
    public class NEW_SAVE_DATA_SafeBox
    {
        /// <summary>
        /// Ящик накопления
        /// </summary>
        public NEW_SAVE_DATA_SafeBox()
        {
            //GAMEPLAY_old.DataGamestat().safe_box_lv = 0;
            //GAMEPLAY_old.DataGamestat().safe_box_summ = new int[4];
        }
    }
    

    public class NEW_SAVE_DATA_AdShop
    {
        /// <summary>
        /// AD Shop
        /// </summary>
        public NEW_SAVE_DATA_AdShop()
        {
            // amount ad deal
            var qu = 5;
            //GAMEPLAY_old.DataGamestat().ad_shop_limit = new byte[qu];
            //for (int i = 0; i < qu; i++)
                //GAMEPLAY.DataGamestat().ad_shop_limit[i] = GameParam.I.adShopLimit;
        }
    }
    
    





    public class NEW_SAVE_DATA_Units
    {
        /// <summary>
        /// For player units
        /// </summary>
        public NEW_SAVE_DATA_Units()
        {
            // var unitCount = Globals.MAX_PLAYER_UNIT;
            //
            // // #1 первый запуск
            // if (GAMEPLAY_old.DataGameplay().player_unit == null || GAMEPLAY_old.DataGameplay().player_unit.Length == 0)
            // {
            //     GAMEPLAY_old.DataGameplay().player_unit = new CPlayerUnit[unitCount];
            //     for (int i = 0; i < unitCount; i++)
            //     {
            //         GAMEPLAY_old.DataGameplay().player_unit[i] = new CPlayerUnit();
            //         GAMEPLAY_old.DataGameplay().player_unit[i].weaponId = -1;
            //         
            //         // ! for dev !
            //         if (i > 0 && DeveloperConsole.I.game.player_units_spawn_all)
            //         {
            //             new GAMEPLAY_PlayerUnit(i).NewSurvivorAdd(
            //                 new GAMEPLAY_PlayerUnit(i).NewSurvivorData(0, Random.Range(0, 4), new Vector2(26 + i, 8)));
            //         }
            //     }
            //     
            //     // * unlock start heroes
            //     new GAMEPLAY_PlayerUnit(0).NewSurvivorAdd(
            //         new GAMEPLAY_PlayerUnit(0).NewSurvivorData(0, 0, new Vector2(26, 8)));
            // }
            //
            // // #2 для дополнений
            // else if (GAMEPLAY_old.DataGameplay().player_unit.Length < unitCount)
            // {
            //     var cash = GAMEPLAY_old.DataGameplay().player_unit;
            //     GAMEPLAY_old.DataGameplay().player_unit = new CPlayerUnit[unitCount];
            //     for (int i = 0; i < unitCount; i++)
            //     {
            //         // restore data old heroes
            //         if (i < cash.Length)
            //             GAMEPLAY_old.DataGameplay().player_unit[i] = cash[i];
            //         // data for new heroes
            //         else
            //         {
            //             GAMEPLAY_old.DataGameplay().player_unit[i] = new CPlayerUnit();
            //             //GAMEPLAY.DataGameplay().player_unit[i].placeId = -1;
            //         }
            //     }
            // }
        }
    }
    
   

}