using System;
using System.Collections;
using UnityEngine;

namespace Galactic1
{
    public class Impact : MonoBehaviour
    {
        public float wait;

        protected virtual void OnEnable()
        {
            Invoke("Hide", wait);
        }

        protected void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}