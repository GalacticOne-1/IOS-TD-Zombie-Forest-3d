
using System;
using UnityEngine;

namespace Galactic1
{
    // удалениe объекта по задержке
    public class DeleteDelay : MonoBehaviour
    {
        public float delay = 1f;


        private void Awake()
        {
            Destroy(gameObject, delay);
        }
    }
}
