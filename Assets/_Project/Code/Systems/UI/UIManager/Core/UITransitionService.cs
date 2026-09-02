using System.Collections;
using UnityEngine;

namespace Galactic1.UI.Core
{
    public class UITransitionService : MonoBehaviour
    {
        [SerializeField] private float defaultDuration = 0.25f;
        [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0,0,1,1);


        public IEnumerator FadeIn(CanvasGroup cg, float duration = -1f)
        {
            if (duration <= 0) duration = defaultDuration;
            cg.gameObject.SetActive(true);
            // float t = 0f;
            // while (t < duration)
            // {
            //     t += Time.deltaTime;
            //     var p = curve.Evaluate(t / duration);
            //     cg.alpha = Mathf.Lerp(0f, 1f, p);
                 yield return null;
            // }
            cg.alpha = 1f;
        }


        public IEnumerator FadeOut(CanvasGroup cg, float duration = -1f)
        {
            if (duration <= 0) duration = defaultDuration;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                var p = curve.Evaluate(t / duration);
                cg.alpha = Mathf.Lerp(1f, 0f, p);
                yield return null;
            }
            cg.alpha = 0f;
            cg.gameObject.SetActive(false);
        }


        public IEnumerator ScaleIn(Transform t, float duration = -1f)
        {
            if (duration <= 0) duration = defaultDuration;
            float elapsed = 0f;
            Vector3 from = Vector3.one * 0.8f;
            Vector3 to = Vector3.one;
            t.localScale = from;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var p = curve.Evaluate(elapsed / duration);
                t.localScale = Vector3.Lerp(from, to, p);
                yield return null;
            }
            t.localScale = to;
        }
    }
}