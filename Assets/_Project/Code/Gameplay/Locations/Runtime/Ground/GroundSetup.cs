
using System;
using UnityEngine;

namespace Galactic1
{
    public abstract class GroundSetup : MonoBehaviour, IGroundSetup, IGroundY
    {
        private float y;
        public float Y => y;


        public struct CData
        {
            public float xMin, xMax;
            public float y;
        }


        private void Awake()
        {
            y = transform.position.y + transform.localScale.y / 2;
        }


        public abstract CData GetSetup();
    }
    
    
    interface IGroundSetup
    {
        /// <summary>
        /// Для получения настроек этажа
        /// </summary>
        /// <returns></returns>
        GroundSetup.CData GetSetup();
    }

    interface IGroundY
    {
        /// <summary>
        /// Поверхность объекта
        /// <br/>(position.y + scale.y / 2)
        /// </summary>
        float Y { get; }
    }
}


