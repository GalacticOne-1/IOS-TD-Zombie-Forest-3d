using System;
using System.Collections;
using UnityEngine;

namespace Galactic1
{
    [RequireComponent(typeof(CanvasGroup))]
    public class CMN_cg_wave_opacity : MonoBehaviour
    {

        public float smooth = 0.03f;
        
        private void OnEnable()
        {
            StartCoroutine(e());
        }
        
        private void OnDisable()
        {
            StopAllCoroutines();
        }
        
        

        IEnumerator e()
        {
            var cg = gameObject.GetComponent<CanvasGroup>();

            while (true)
            {
                for (float i = 1; i >= 0; i-=smooth)
                {
                    cg.alpha = i;
                    yield return null;
                }
                cg.alpha = 0;
                
                yield return new WaitForSeconds(.1f);

                for (float i = 0; i < 1; i+=smooth)
                {
                    cg.alpha = i;
                    yield return null;
                }
                cg.alpha = 1;

                yield return new WaitForSeconds(1f);
            }
        }

        
    }
}