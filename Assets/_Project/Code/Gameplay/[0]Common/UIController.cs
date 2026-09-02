using System.Collections;
using System.Collections.Generic;
using Galactic1;
using Galactic1.Mobile;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace Galactic1
{
    /*    Единичный класс Gameplay
     *
     *    Управление общим состоянием всех виджетов на сцене с боевым уровнем
     */
    public class UIController : Singleton<UIController>
    {

        #region Public Fields
        /// <summary>
        /// true - курсор нахoдится на UI объекте
        /// </summary>
        public bool UI_ELEMENT { set; get; }



        #endregion




        /// <summary>
        /// Проверяет нет ли под курсором, нужного объекта UI
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool OverUI(string target)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
         
            pointerData.position = Input.mousePosition;
 
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            //Debug.Log("<color=lime>Check</color>");
            var l = results.Count;
            for (int i = 0; i < l; i++)
            {
                //Debug.Log("Ui "+results[i].gameObject.name);
                if (results[i].gameObject.name == target)
                    return true;
            }
            
            return false;
        }
        /// <summary>
        /// true - под курсором есть UI
        /// </summary>
        /// <returns></returns>
        public bool OverUI()
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
         
            pointerData.position = Input.mousePosition;
 
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            
            //if(results.Count > 0)
            //ScreenProfiler.AddMessage($"------------------ OVER UI => {results[0].gameObject.name}");

            return results.Count > 0;
        }

        
    }

    
}