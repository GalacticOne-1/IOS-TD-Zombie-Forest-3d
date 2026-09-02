using Galactic1;
using UnityEngine;

namespace Galactic1
{
    /*
     *     инвентарь игрока/ящики/ и тд
     */




    public class Global_Resource_Minus
    {
        
        /// <summary>
        /// Вычтет необходимое кол-во ресурса у игрока
        /// <br/>(Проверяет все места хранения)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="category"></param>
        /// <param name="id"></param>
        /// <param name="only_inventory">true - ресурсы будут взяты только из инвентаря</param>
        /// <param name="volume"></param>
        public Global_Resource_Minus(int type, byte category, int id, bool only_inventory, short volume)
        {
            int l;
            
            // #1 сначала берем из ящиков
            // if (!only_inventory)
            // {
            //     l = GAMEPLAY_old.DataGameplay().crate.Length;
            //     for (int i = 0; i < l; i++)
            //     {
            //         var ll = GAMEPLAY_old.DataGameplay().crate[i].slot.Length;
            //         for (int j = 0; j < ll; j++)
            //         {
            //             new Inventory_COMPARE(new CPlayerInventory()
            //                 {
            //                     type = type,
            //                     category = category,
            //                     id = id
            //                 },
            //                 GAMEPLAY_old.DataGameplay().crate[i].slot[j],
            //                 out bool equall);
            //     
            //             if (GAMEPLAY_old.DataGameplay().crate[i].slot[j].unlock && GAMEPLAY_old.DataGameplay().crate[i].slot[j].volume > 0 && equall)
            //             {
            //                 GAMEPLAY_old.DataGameplay().crate[i].slot[j].volume -= volume;
            //
            //                 if (GAMEPLAY_old.DataGameplay().crate[i].slot[j].volume >= 0)
            //                 {
            //                     return;
            //                 }
            //
            //                 // * если не ресурсов не хватило с одного слота, двигаемся к след.
            //                 volume = (short)Mathf.Abs(GAMEPLAY_old.DataGameplay().crate[i].slot[j].volume);
            //                 GAMEPLAY_old.DataGameplay().crate[i].slot[j].volume = 0;
            //             }
            //         }
            //     }
            // }
            //
            //
            // // #2 потом из инветоря
            // l = GAMEPLAY_old.DataGameplay().inventory.Length;
            // for (int i = 0; i < l; i++)
            // {
            //     new Inventory_COMPARE(new CPlayerInventory()
            //         {
            //             type = type,
            //             category = category,
            //             id = id
            //         },
            //         GAMEPLAY_old.DataGameplay().inventory[i],
            //         out bool equall);
            //     
            //     if (GAMEPLAY_old.DataGameplay().inventory[i].unlock && GAMEPLAY_old.DataGameplay().inventory[i].volume > 0 && equall)
            //     {
            //         GAMEPLAY_old.DataGameplay().inventory[i].volume -= volume;
            //
            //         if (GAMEPLAY_old.DataGameplay().inventory[i].volume >= 0)
            //         {
            //             return;
            //         }
            //
            //         // * если не ресурсов не хватило с одного слота, двигаемся к след.
            //         volume = (short)Mathf.Abs(GAMEPLAY_old.DataGameplay().inventory[i].volume);
            //         GAMEPLAY_old.DataGameplay().inventory[i].volume = 0;
            //     }
            // }
        }
    }
}