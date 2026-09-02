using Galactic1;
using UnityEngine;

namespace Galactic1
{
    /*
     *      Сложность в игре (вражеские волны/ враги и пр)
     */






    /// <summary>
    /// Таблица с настройками для волн
    /// </summary>
    public class PARAM_GAME_DIFICULT_Table
    {
        [System.Serializable]
        public struct CData
        {
            public float timer;                         // длительность работы спавна
            public CCreature[] creature;                // какие юниты спавнятся
        }
        [System.Serializable]
        public struct CCreature
        {
            public byte id;
            public byte volume;
            public float hp, def, atk;
        }


        public PARAM_GAME_DIFICULT_Table(int battle_stage, int stage, out CData instruction)
        {
            instruction = new CData();


            // время для спавна обычных волн
            instruction.timer = 10;

            // поднимаем каждые 10 лвл
            instruction.timer += 5 * ((stage + 1) / 10);
            if (instruction.timer > 40) instruction.timer = 40;
            

            
            switch (battle_stage)
            {
                
                case 1:
                {
                    instruction.creature = new CCreature[]
                    {
                        new() { id = 3, volume = 1 },
                        new() { id = 4, volume = 1 },
                        new() { id = 5, volume = 1 }
                    };

                } break;

                case 2:
                {
                    instruction.creature = new CCreature[]
                    {
                        new() { id = 2, volume = 4 },
                        new() { id = 6, volume = 2 },
                        new() { id = 7, volume = 1 }
                    };

                } break;

                case 3:
                {
                    instruction.creature = new CCreature[]
                    {
                        new() { id = 8, volume = 4 },
                        new() { id = 9, volume = 3 },
                        new() { id = 10, volume = 2 }
                    };

                } break;

                case 4:
                {
                    instruction.creature = new CCreature[]
                    {
                        new() { id = 11, volume = 1 },
                        new() { id = 12, volume = 1 },
                        new() { id = 13, volume = 1 }
                    };

                } break;

                case 5:
                {
                    instruction.creature = new CCreature[]
                    {
                        new() { id = 8, volume = 4 },
                        new() { id = 4, volume = 2 },
                        new() { id = 14, volume = 1 }
                    };

                } break;
                
                case 6:
                default:
                {
                    instruction.creature = new CCreature[]
                    {
                        new() { id = 0, volume = 4 },
                        new() { id = 1, volume = 3 },
                        new() { id = 2, volume = 2 }
                    };

                } break;
            }
        }
    }

    public class PARAM_GAME_DIFICULT_Boss
    {
        public PARAM_GAME_DIFICULT_Boss(int stage, ref PARAM_GAME_DIFICULT_Table.CCreature creature)
        {
            //var asset = ServiceLocator.Current.Get<LibController>().GetEntity((byte)EEnemyDifficult.boss, creature.id);


            // float hp_total = asset.HpDefault;
            // float def_total = asset.DefDefault;
            // float atk_total = asset.AtkDefault;
            // float hp_progress = asset.DifficultProgress.hp.startValue;
            // float def_progress = asset.DifficultProgress.def.startValue;
            // float atk_progress = asset.DifficultProgress.atk.startValue;
            // bool hp_freeze = false;
            // bool atk_freeze = false;
            // int step_freeze_hp = 50;
            // int step_freeze_atk = 7;
            //
            // for (int j = 0; j < stage; j++)
            // {
            //     // *** доп. ступени для уменьшения сложности
            //     if (j != 0 && j % 40 == 0 && !hp_freeze)
            //     {
            //         if (stage > 200)
            //             step_freeze_hp += 20;
            //         if (stage > 400)
            //             step_freeze_hp += 20 * (stage / 200);
            //         //if (stage > 600)
            //             //step_freeze_hp += 10;
            //         
            //         hp_freeze = true;
            //         hp_progress *= .85f;
            //         //DLog.Alert($">>> Progress freeze", EDlogColor.ORANGE);
            //     }
            //     else if (j % step_freeze_hp == 0 && hp_freeze)
            //     {
            //         hp_freeze = false;
            //         //DLog.Alert($">>> Progress continue", EDlogColor.ORANGE);
            //     }
            //     //
            //     
            //     if (j != 0 && j % 7 == 0 && !hp_freeze)         // увеличение каждый 7 этап
            //     {
            //         hp_progress += asset.DifficultProgress.hp.progressValue;
            //         def_progress += asset.DifficultProgress.def.progressValue;
            //         //DLog.Alert($"Progress HP : #{j / 7} => {hp_progress}");
            //     }
            //
            //
            //     
            //     
            //     
            //     // ATK
            //     if (j > 100 && j % 7 == 0 && !atk_freeze)
            //     {
            //         atk_freeze = true;
            //         step_freeze_atk += 7;
            //     }
            //     else if (j % step_freeze_atk == 0)
            //     {
            //         atk_freeze = false;
            //     }
            //     
            //     
            //     if(j >= 13 && j % 7 == 0 && !atk_freeze)
            //     {
            //         atk_progress += asset.DifficultProgress.atk.progressValue;
            //         atk_total += atk_progress;
            //     }
            //     //
            //     
            //     
            //     
            //     // считаем в float чтобы хвосты не пропадали
            //     hp_total += hp_progress;
            //     def_total += def_progress;
            //     
            // }
            //
            // // применяем конечное значения
            // creature.hp = hp_total;
            // creature.def = def_total;
            // creature.atk = atk_total;
            //
            // DLog.Alert($"PARAM DIFICULT BOSS : entity", EDlogColor.YELLOW);
            // DLog.Alert($"hp: {creature.hp} / atk : {creature.atk}", EDlogColor.YELLOW);
        }
    }



    public class PARAM_GAME_DIFICULT_Regular
    {
        /// <summary>
        /// Настройка волн для обычной битвы
        /// </summary>
        /// <param name="instruction"></param>
        // public PARAM_GAME_DIFICULT_Regular(int stage, out PARAM_GAME_DIFICULT_Table.CData instruction)
        // {
        //     if (GAMEPLAY_old.DataGameplay().regular_battle_stage > 6) GAMEPLAY_old.DataGameplay().regular_battle_stage = 0;
        //     
        //     // #1 получаем установку для волны
        //     new PARAM_GAME_DIFICULT_Table(GAMEPLAY_old.DataGameplay().regular_battle_stage, stage, out instruction);
        //     
        //     // #2 считаем хар-ки под текущий этап
        //     var l = instruction.creature.Length;
        //     for (int i = 0; i < l; i++)
        //     {
        //         var asset = ServiceLocator.Current.Get<LibController>().GetEntity(0, instruction.creature[i].id);
        //
        //
        //         float hp_total = asset.HpDefault;
        //         float def_total = asset.DefDefault;
        //         float atk_total = asset.AtkDefault;
        //         float hp_progress = asset.DifficultProgress.hp.startValue;
        //         float def_progress = asset.DifficultProgress.def.startValue;
        //         float atk_progress = asset.DifficultProgress.atk.startValue;
        //         bool hp_freeze = false;
        //         bool atk_freeze = false;
        //         int step_freeze_hp = 50;
        //         int step_freeze_atk = 7;
        //         
        //         for (int j = 0; j < stage; j++)
        //         {
        //             // *** доп. ступени для уменьшения сложности
        //             if (j != 0 && j % 40 == 0 && !hp_freeze)
        //             {
        //                 if (stage > 200)
        //                     step_freeze_hp += 20;
        //                 if (stage > 400)
        //                     step_freeze_hp += 20 * (stage / 200);
        //                 //if (stage > 600)
        //                     //step_freeze_hp += 10;
        //                 
        //                 hp_freeze = true;
        //                 hp_progress *= .85f;
        //                 //DLog.Alert($">>> Progress freeze", EDlogColor.ORANGE);
        //             }
        //             else if (j % step_freeze_hp == 0 && hp_freeze)
        //             {
        //                 hp_freeze = false;
        //                 //DLog.Alert($">>> Progress continue", EDlogColor.ORANGE);
        //             }
        //             //
        //             
        //             if (j != 0 && j % 7 == 0 && !hp_freeze)         // увеличение каждый 7 этап
        //             {
        //                 hp_progress += asset.DifficultProgress.hp.progressValue;
        //                 def_progress += asset.DifficultProgress.def.progressValue;
        //                 //DLog.Alert($"Progress HP : #{j / 7} => {hp_progress}");
        //             }
        //
        //
        //             
        //             
        //             
        //             // ATK
        //             if (j > 100 && j % 7 == 0 && !atk_freeze)
        //             {
        //                 atk_freeze = true;
        //                 step_freeze_atk += 7;
        //             }
        //             else if (j % step_freeze_atk == 0)
        //             {
        //                 atk_freeze = false;
        //             }
        //             
        //             
        //             if(j >= 13 && j % 7 == 0 && !atk_freeze)
        //             {
        //                 atk_progress += asset.DifficultProgress.atk.progressValue;
        //                 atk_total += atk_progress;
        //             }
        //             //
        //             
        //             
        //             
        //             // считаем в float чтобы хвосты не пропадали
        //             hp_total += hp_progress;
        //             def_total += def_progress;
        //             
        //         }
        //
        //         // применяем конечное значения
        //         instruction.creature[i].hp = hp_total;
        //         instruction.creature[i].def = def_total;
        //         instruction.creature[i].atk = atk_total;
        //         
        //         DLog.Alert($"PARAM DIFICULT REGULAR : entity {i}", EDlogColor.YELLOW);
        //         DLog.Alert($"hp: {instruction.creature[i].hp} / atk : {instruction.creature[i].atk}", EDlogColor.YELLOW);
        //     }
        // }
    }
    
    
    
}