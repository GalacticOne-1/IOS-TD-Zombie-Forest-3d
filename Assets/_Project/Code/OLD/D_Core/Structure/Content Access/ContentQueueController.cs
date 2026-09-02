using System.Collections;
using System.Collections.Generic;
using Galactic1;
using UnityEngine;

namespace Galactic1
{
    public class ContentQueueController : IGameService
    {
        /*
         *   Система очереди панелей
         */

        
        public enum EContent
        {
            WIDGET,
            WIDGET_LOAD,
            WIDGET_LOAD_ARRAY
        }

        private List<CWidgetSystem> queue_list = new List<CWidgetSystem>();
        
        public struct CWidgetSystem
        {
            public byte order;
            public MainMenuViewModel.EMainMenu menu;

            public EContent typeContent;
            public bool blockScreen;
            
            public GameObject widget;
            public DFunc func;
            public DFuncObj funcObj;
            public DFuncObjAr funcObjAr;
            
            public bool used;
        }
        
        
        
        
        
        
        
        



        #region LEVEL UP DELAY

        /// Для добавления метода запуска панели в очередь
        public void AddQueue(CWidgetSystem d)
        {
            if (queue_list == null) 
                queue_list = new List<CWidgetSystem>();
            queue_list.Add(d);
            DLog.Alert($">>> add queue #{queue_list.Count} _ {d.widget}");
        }
        
        /// <summary>
        /// Для запуска 
        /// </summary>
        public void LaunchQueueDelay()
            => LaunchQueueDelay(ServiceLocator.Current.Get<ViewGameController>().MainMenuViewModel.CurMenu);
        
        /// <summary>
        /// Для запуска 
        /// </summary>
        public void LaunchQueueDelay(MainMenuViewModel.EMainMenu menu)
        {
            //DLog.Alert("***         Content QUEUE");
            //DLog.Alert($">>> {queue_list.Count}", "yellow");
            if(queue_list != null && queue_list.Count > 0)
            {
                List<byte> avail = new List<byte>();
                int n = 20;
                while (true)
                {
                    //DLog.Alert($"*** Queue start", "yellow");
                    n--;
                    
                    // 1 ищем панели
                    bool have_el = false;
                    byte cur_order = 0;
                    byte id = 0;
                    var l = queue_list.Count;
                    for (byte i = 0; i < l; i++)
                    {
                        // поиск подходящей панели для открытого меню
                        if (!queue_list[i].used && queue_list[i].order >= cur_order && queue_list[i].menu == menu)
                        {
                            cur_order = queue_list[i].order;
                            id = i;
                            have_el = true;
                            //DLog.Alert($"use "+i, "yellow");
                        }
                    }

                    // защита от зациклинности
                    if (n <= 0)
                    {
                        Debug.LogError("COLAPSE ContentQueueController");
                        return;
                    }

                    // 2 добавляем в список и запускаем поиск по новой (1)
                    if (have_el)
                    {
                        var el = queue_list[id];
                        el.used = true;
                        queue_list[id] = el;
                        //DLog.Alert($"Added {id} {queue_list[id].used} {queue_list[id].log}", "yellow");
                        avail.Add(id);
                    }
                    // или останавливаем
                    else  
                    {
                        break;
                    }
                }
                
                
                if(avail.Count > 0)
                {
                    ServiceLocator.Current.Get<ViewGameController>().StartCoroutine(newLevel(avail));
                }
            }
        }
        
        IEnumerator newLevel(List<byte> avail)
        {
            var l = avail.Count;
            for (int i = 0; i < l; i++)
            {

                switch (queue_list[avail[i]].typeContent)
                {
                    case EContent.WIDGET:           // виджет уже есть
                    {
                        queue_list[avail[i]].func();
                        
                        while (queue_list[avail[i]].widget.activeSelf) yield return null;
                        
                    } break;

                    case EContent.WIDGET_LOAD:      // виджет передается при вызове метода
                    {
                        var el = queue_list[avail[i]]; 
                        el.widget = queue_list[avail[i]].funcObj();
                        queue_list[avail[i]] = el;
                        
                        while (queue_list[avail[i]].widget.activeSelf) yield return null;
                    } break;

                    case EContent.WIDGET_LOAD_ARRAY:     // массив сигналов
                    {
                        if (queue_list[avail[i]].blockScreen)
                            CORT.BlockScreen(true);
                        
                        var el = queue_list[avail[i]]; 
                        var g = queue_list[avail[i]].funcObjAr();
                        queue_list[avail[i]] = el;

                        var ll = g.Length;
                        bool complete;
                        while (true)
                        {
                            // как только все сигналы будут удалены, сонтент считается завершенным
                            // переходим к след. в очереди
                            complete = true;
                            for (int j = 0; j < ll; j++)
                                if (g[j] != null) complete = false;
                            
                            if(complete) break;
                            
                            yield return null;
                        }
                        
                        if (queue_list[avail[i]].blockScreen)
                            CORT.BlockScreen(false);
                    } break;
                }
                
                /*if (queue_list[avail[i]].func != null)
                {
                    queue_list[avail[i]].func();
                }
                
                // функция после запуска передает виджет
                else if (queue_list[avail[i]].funcObj != null)  
                {
                    var el = queue_list[avail[i]]; 
                    el.widget = queue_list[avail[i]].funcObj();
                    queue_list[avail[i]] = el;
                }
                
                // функция после запуска передает массив пустых объектов
                // которые используются как сигналы для отслеживания окончания (аналог закрытия виджета)
                else if (queue_list[avail[i]].funcObjAr != null)  
                {
                    var el = queue_list[avail[i]]; 
                    el.widget = queue_list[avail[i]].funcObjAr();
                    queue_list[avail[i]] = el;
                }

                while (queue_list[avail[i]].widget.activeSelf) yield return null;*/

                CORT.BlockScreen(true);
                if (i < avail.Count-1)  //if (i == avail.Count - 1)
                    yield return new WaitForSeconds(.5f);       // пауза между новыми панелями
                CORT.BlockScreen(false);
            }

            //DLog.Alert($"*** Queue RMOVE: {queue_list.Count}", "yellow");
            // *** в конце удаляем использованные элементы
            for (int i = queue_list.Count - 1; i >= 0; i--)
            {
                DLog.Alert($"Panel {i} Status: {queue_list[i].used} ", EDlogColor.YELLOW);
                if (queue_list[i].used) queue_list.RemoveAt(i);
            }
            DLog.Alert($"*** Queue FINISH: {queue_list.Count}", EDlogColor.YELLOW);
        }
        
        #endregion
    }
}