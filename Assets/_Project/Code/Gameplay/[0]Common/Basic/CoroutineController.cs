using System.Collections;
using System.Collections.Generic;
using Galactic1;
using UnityEngine;


namespace Galactic1
{
    public class CoroutineController : MonoBehaviour, IGameService
    {
        public WaitForSeconds wait_d1s = new WaitForSeconds(.1f);
        public WaitForSeconds wait_d2s = new WaitForSeconds(.2f);
        public WaitForSeconds wait_d5s = new WaitForSeconds(.5f);
        public WaitForSeconds wait_1s = new WaitForSeconds(1);
        public WaitForSeconds wait_2s = new WaitForSeconds(2);
        public WaitForSeconds wait_4s = new WaitForSeconds(4);
        public WaitForSeconds wait_6s = new WaitForSeconds(6);
        
        
        
        
        
        
        
        /// <summary>
        /// Запуск корутины
        /// </summary>
        /// <param name="cor"></param>
        public void LaunchCoroutine(IEnumerator cor)
        {
            StartCoroutine(cor);
        }

        
        
        
        /// <summary>
        /// Запуск метода после ожидания (2 null)
        /// </summary>
        /// <param name="func"></param>
        public void Coroutine_wait(DFunc func)
        {
            StartCoroutine(coroutine_wait(func));
        }
        IEnumerator coroutine_wait(DFunc func)
        {
            yield return null;
            yield return null;
            func?.Invoke();
        }
        public void Coroutine_wait1(DFunc func)
        {
            StartCoroutine(coroutine_wait(func));
        }
        IEnumerator coroutine_wait1(DFunc func)
        {
            yield return null;
            func?.Invoke();
        }



        /// <summary>
        /// Запуск метода после ожидания
        /// </summary>
        /// <param name="timeWait"></param>
        /// <param name="func"></param>
        public Coroutine Coroutine_wait(float timeWait, DFunc func) => StartCoroutine(coroutine_wait(timeWait, func));
        IEnumerator coroutine_wait(float timeWait, DFunc func)
        {
            yield return new WaitForSeconds(timeWait);
            func?.Invoke();
        }

        /// <summary>
        /// Запуск метода после ожидания
        /// </summary>
        /// <param name="timeWait"></param>
        /// <param name="func"></param>
        public Coroutine Coroutine_wait(float timeWait, DFunc[] ar_func) => StartCoroutine(coroutine_wait(timeWait, ar_func));
        IEnumerator coroutine_wait(float timeWait, DFunc[] ar_func)
        {
            var l = ar_func.Length;
            for (int i = 0; i < l; i++)
            {
                yield return new WaitForSeconds(timeWait);
                ar_func[i]?.Invoke();
            }
        }
        
        
        
        /// <summary>
        /// Запускк процесса с блокировкой экрана (готовность по времени)
        /// </summary>
        /// <param name="timeWait"></param>
        /// <param name="func"></param>
        public void Coroutine_process(float timeWait, DFunc func)
        {
            StartCoroutine(coroutine_process(timeWait, func));
        }
        IEnumerator coroutine_process(float timeWait, DFunc func)
        {
            CORT.BlockScreen(true);
            while (timeWait >= 0)
            {
                timeWait -= Time.deltaTime;
                func?.Invoke();
                yield return null;
            }
            CORT.BlockScreen(false);
        }
        
        /// <summary>
        /// Запускк процесса с блокировкой экрана (delegate для проверки готовности)
        /// </summary>
        /// <param name="complete">метод для проверки</param>
        /// <param name="func"></param>
        public void Coroutine_process(DFuncResponse complete, DFunc func)
        {
            StartCoroutine(coroutine_process(complete, func));
        }
        IEnumerator coroutine_process(DFuncResponse complete, DFunc func)
        {
            CORT.BlockScreen(true);
            while (!complete()) 
            {
                func?.Invoke();
                yield return null;
            }
            CORT.BlockScreen(false);
        }
        
        
        
    }
}