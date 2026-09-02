using System;
using UnityEngine;

namespace Galactic1
{
    public class AutoDisable : MonoBehaviour
    {
        private void OnDisable()
        {
            gameObject.SetActive(false);
        }
    }
}