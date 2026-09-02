using System;
using UnityEngine;
using UnityEngine.Events;

namespace Galactic1
{
    public class LaunchFunc : MonoBehaviour
    {
        /*
         *  Запускает метод при каждом включении 
         */
        
        
        public UnityEvent func;


        private void OnEnable()
        {
            func?.Invoke();
        }
    }
}