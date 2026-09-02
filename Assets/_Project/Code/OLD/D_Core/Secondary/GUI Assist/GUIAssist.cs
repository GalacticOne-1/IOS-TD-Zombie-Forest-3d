using System;
using System.Collections;
using UnityEngine;

namespace Galactic1
{
    public class GUIAssist : MonoBehaviour, IGameService
    {
        
        /*
         *      Методы для панелей
         */

        public GameObject blockScreen, shortDark;
        



        /// <summary>
        /// Плавная загрузка canvasgroup 
        /// </summary>
        /// <param name="cg"></param>
        /// <param name="show"></param>
        /// <returns></returns>
        public void CanvasGroupAlpha(GameObject cg, bool show, Action func = null, float smooth = .15f)
        {
            StartCoroutine(canvasGroup(cg, show, func, smooth));
        }
        IEnumerator canvasGroup(GameObject obj, bool show, Action func, float smooth)
        {
            blockScreen.SetActive(true);
            CanvasGroup cg = obj.GetComponent<CanvasGroup>();

            if (show)
            {
                func?.Invoke();
                obj.SetActive(true);
                for (float i = cg.alpha; i < 1.1f; i+=smooth)
                {
                    cg.alpha = i;
                    yield return null;
                }
            }
            else
            {
                for (float i = cg.alpha; i > -.9f; i-=smooth)
                {
                    cg.alpha = i;
                    yield return null;
                }
                obj.SetActive(false);
                func?.Invoke();
            }
            blockScreen.SetActive(false);
        }
        
        /// <summary>
        /// Состояние Canvas Group (Show/Hide)
        /// </summary>
        /// <param name="cg"></param>
        /// <param name="show"></param>
        /// <param name="delay">Задержка перед изменением состояния</param>
        /// <param name="smooth"></param>
        public void CanvasGroupAlpha(GameObject cg, bool show, float delay, float smooth = .15f)
        {
            StartCoroutine(canvasGroup(cg, show, delay, smooth));
        }
        IEnumerator canvasGroup(GameObject obj, bool show, float delay, float smooth)
        {
            blockScreen.SetActive(true);
            CanvasGroup cg = obj.GetComponent<CanvasGroup>();
            if (show)
            {
                cg.alpha = 0;
                yield return new WaitForSeconds(delay);
                
                for (float i = cg.alpha; i < 1.1f; i+=smooth)
                {
                    cg.alpha = i;
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(delay);
                
                for (float i = cg.alpha; i > -.9f; i-=smooth)
                {
                    cg.alpha = i;
                    yield return null;
                }
            }
            blockScreen.SetActive(false);
        }



        #region Load Screen

        /// <summary>
        /// Показываем окно (только для смены сцен)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="wait"></param>
        /// <param name="func"></param>
        public void LoadScreenShow(GameObject obj, bool useScreen, Action func = null, float delayFunc = 0)
        {
            StartCoroutine(loadScreenS(obj,useScreen, func, delayFunc));
        }
        IEnumerator loadScreenS(GameObject obj, bool useScreen, Action func, float delay)
        {
            if(useScreen)
            {
                
                obj.SetActive(true);
                CanvasGroup cg = obj.GetComponent<CanvasGroup>();


                for (float i = cg.alpha; i <= 1.1f; i += .15f) //.01f
                {
                    cg.alpha = i;
                    yield return null;
                    //Debug.Log(Time.deltaTime);
                }
            }

            yield return new WaitForSeconds(delay);
            func?.Invoke();
        }
        
        /// <summary>
        /// Убираем окно (только для смены сцен)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="func"></param>
        public void LoadScreenHide(GameObject obj, Action func = null)
        {
            StartCoroutine(loadScreenH(obj, func));
        }
        IEnumerator loadScreenH(GameObject obj, Action func)
        {
            CanvasGroup cg = obj.GetComponent<CanvasGroup>();
            
            
            for (float i = cg.alpha; i > -.9f; i -= .15f)
            {
                cg.alpha = i;
                yield return null;
            }
            
            func?.Invoke();
            obj.SetActive(false);
        }

        /// <summary>
        /// Моментальное отключение 
        /// </summary>
        /// <param name="obj"></param>
        public void LoadScreenHide(GameObject obj)
        {
            obj.SetActive(false);
            obj.GetComponent<CanvasGroup>().alpha = 0;
        }

        #endregion
    }
}
