using System;
using System.Collections;
using UnityEngine;

namespace Galactic1
{
    public class CircleRotator : MonoBehaviour
    {
        private void OnEnable()
        {
            StartCoroutine(rotate());
        }

        IEnumerator rotate()
        {
            var tr = transform;
            while (true)
            {
                
                tr.Rotate(new Vector3(0, 0, 20) * Time.deltaTime);
                yield return null;
            }
        }
    }
}