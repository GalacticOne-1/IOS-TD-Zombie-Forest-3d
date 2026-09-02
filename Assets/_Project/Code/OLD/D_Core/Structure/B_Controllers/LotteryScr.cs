using Galactic1;
using UnityEngine;

namespace Galactic1
{
    
    
    public class PlayLottery
    {
        public PlayLottery(int qu_items, out int winner, out int[] hide)
        {
            winner = Random.Range(0, qu_items);


            hide = new int[qu_items - 1];
            for (int i = 0; i < qu_items-1; i++)
                hide[i] = -1;

            for (int i = 0; i < qu_items; i++)
            {
                // пропускаем победителя
                if(i == winner) continue;

                var _try = 0;
                while (true)
                {
                    // начало попытки
                    var n = Random.Range(0, qu_items-1);
                    
                    // если за отведенные попытки не смогли добавить значение
                    // добавляем в первый свободный элемент
                    if (_try > 5)
                    {
                        n = hide.FindEmptyElement();
                    }
                    
                    // если выбранный элемент уже занят, делаем новую попытку
                    else if (hide[n] != -1)
                    {
                        _try++;
                        continue;
                    }
                    
                    // успешное добавление
                    hide[n] = i;
                    break;
                }
            }
        }
    }
    
    
    public class Lottery_AD_Box
    {
        /// <summary>
        /// Для бокса со снаряжением
        /// </summary>
        public Lottery_AD_Box(int qu_items, out int winner, out int[] hide)
        {
            // #1 выбираем предмет
            winner = 0;
            
            // *** рандом подгоняется под номер запуска лотереи
            
            // if (GAMEPLAY_old.DataGameplay().adEquipmentBox.number_launch > 6)
            // {
            //     GAMEPLAY_old.DataGameplay().adEquipmentBox.number_launch = 0;
            //     GAMEPLAY_old.DataGameplay().adEquipmentBox.received_reward = new byte[GAMEPLAY_old.DataGameplay().adEquipmentBox.received_reward.Length];
            // }
            //
            // switch (GAMEPLAY_old.DataGameplay().adEquipmentBox.number_launch)
            // {
            //     case 0:
            //         winner = Random.Range(0, 2);
            //     break;
            //     case 1:
            //         winner = GAMEPLAY_old.DataGameplay().adEquipmentBox.received_reward[0] == 0 ? 0 : 1;
            //         break;
            //     
            //     case 4:
            //     case 3:
            //     case 2:
            //         winner = Random.Range(2, 5);
            //         break;
            //     
            //     case 5:
            //         winner = 5;
            //         break;
            //     
            //     case 6:
            //         winner = Random.Range(6, 9);
            //
            //         // #1 выдаем рюкзак если у игрока такого нет
            //         new Inventory_FIND(1, (int)EEquipment.Military_Backpack, out int slot);
            //         new InventoryBox_FIND(1, (int)EEquipment.Military_Backpack, out int crate, out int slotCrate);
            //         if (winner == 7 && (slot != -1 || slotCrate != -1))
            //         {
            //             winner = 6;
            //             DLog.Alert(">>> AD Equipment Box: Player have <Military Backpack>", EDlogColor.ORANGE);
            //         }
            //         
            //         // #2 выдаем автомат если у игрока его нет и он провел в игре достаточно времени
            //         new Inventory_FIND(1, (int)EEquipment.AK_47, out slot);
            //         new InventoryBox_FIND(1, (int)EEquipment.AK_47, out crate, out slotCrate);
            //         if (winner == 8 && (slot != -1 || slotCrate != -1 ))
            //         {
            //             winner = 6;
            //             DLog.Alert(">>> AD Equipment Box: Player denied <AK 47>", EDlogColor.ORANGE);
            //         }
            //
            //         break;
            // }
            //
            // GAMEPLAY_old.DataGameplay().adEquipmentBox.received_reward[winner]++;
            // GAMEPLAY_old.DataGameplay().adEquipmentBox.number_launch++;
            
            
            
            
            // #2 
            hide = new int[qu_items - 1];
            for (int i = 0; i < qu_items-1; i++)
                hide[i] = -1;

            for (int i = 0; i < qu_items; i++)
            {
                // пропускаем победителя
                if(i == winner) continue;

                var _try = 0;
                while (true)
                {
                    // начало попытки
                    var n = Random.Range(0, qu_items-1);
                    
                    // если за отведенные попытки не смогли добавить значение
                    // добавляем в первый свободный элемент
                    if (_try > 5)
                    {
                        n = hide.FindEmptyElement();
                    }
                    
                    // если выбранный элемент уже занят, делаем новую попытку
                    else if (hide[n] != -1)
                    {
                        _try++;
                        continue;
                    }
                    
                    // успешное добавление
                    hide[n] = i;
                    break;
                }
            }
        }
    }


    
    public class RarityChance
    {
        /// <summary>
        /// Шанс для редких предметов
        /// </summary>
        public RarityChance(ERarities rare, out bool is_win)
        {
            is_win = false;
            switch (rare)
            {
                case ERarities.Standard: is_win = Random.Range(0, 100) < 60;            // 80%
                    break;
                
                case ERarities.Superior: is_win = Random.Range(0, 100) < 40;            // 80%
                    break;
                    
                case ERarities.High_End: is_win = Random.Range(0, 100) < 25;            // 50%
                    break;
                
                case ERarities.Exotic: is_win = Random.Range(0, 100) < 10;              // 30%
                    break;
            }
        }
    }
}