using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1
{
    public class WidgetCycleSystem : Singleton<WidgetCycleSystem>
    {
        /*
         *   Система очереди панелей
         */



        List<CWidgetSystem> after_battle;
        
        /// Для добавления метода запуска панели в очередь
        public void Add_after_battle(CWidgetSystem d) => after_battle.Add(d);
        
        public struct CWidgetSystem
        {
            public GameObject widget;
            public DFunc func;
        }
        



        /// <summary>
        /// Перед новой битвой
        /// </summary>
        public void Clear()
        {
            after_battle = new List<CWidgetSystem>();
        }





        public void LaunchCycle_after_battle()
        {
            StartCoroutine(e());
        }
        IEnumerator e()
        {
            for (int i = 0; i < after_battle.Count; i++)
            {
                after_battle[i].func?.Invoke();

                while (after_battle[i].widget.activeInHierarchy) yield return null;

                if (i == after_battle.Count - 1)
                    yield return new WaitForSeconds(.1f);
            }
            
            // что бы метод запускался не сразу после закрытия последней панели
            //yield return new WaitForSeconds(.5f); 
        }

    }
}