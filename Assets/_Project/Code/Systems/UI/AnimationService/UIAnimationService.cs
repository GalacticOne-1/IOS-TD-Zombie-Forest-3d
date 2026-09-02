using System.Collections;
using UnityEngine;
using TMPro;


namespace Galactic1.UI.Core
{

    public class UIAnimationService : MonoBehaviour, IGameService
    {


        // =========================
        //  PUBLIC API
        // =========================

        public void AnimateStatChange(TMP_Text text, float oldValue, float newValue)
        {
            StartCoroutine(AnimateStatRoutine(text, oldValue, newValue));
        }

        public void Pulse(Transform target, float scale = 1.2f, float time = 0.15f)
        {
            StartCoroutine(PulseRoutine(target, scale, time));
        }

        public void FlashColor(TMP_Text text, Color color, float time = 0.25f)
        {
            StartCoroutine(ColorFlashRoutine(text, color, time));
        }

        public void Shake(Transform target, float strength = 10f, float time = 0.25f)
        {
            StartCoroutine(ShakeRoutine(target, strength, time));
        }

        public void Fade(CanvasGroup cg, float to, float time = 0.3f)
        {
            StartCoroutine(FadeRoutine(cg, to, time));
        }

        // =========================
        //  EFFECTS IMPLEMENTATION
        // =========================

        private IEnumerator AnimateStatRoutine(TMP_Text text, float oldValue, float newValue)
        {
            // 1) Цвет (зелёный/красный флэш)
            Color flashColor = newValue > oldValue ? new Color(0.2f, 1f, 0.2f) : new Color(1f, 0.3f, 0.3f);
            StartCoroutine(ColorFlashRoutine(text, flashColor, .6f));

            // 2) Пружинка (как в LDOE)
            StartCoroutine(PulseRoutine(text.transform, 1.2f, 0.6f));

            // 3) Плавное изменение числа
            yield return StartCoroutine(NumberTweenRoutine(text, oldValue, newValue, 0.2f));
        }

        // private IEnumerator PulseRoutine(Transform target, float scale, float time)
        // {
        //     Vector3 start = Vector3.one;
        //     Vector3 peak = start * scale;
        //     float t = 0f;
        //
        //     while (t < 1f)
        //     {
        //         t += Time.unscaledDeltaTime / time;
        //         float k = EaseOutBack(t);
        //         target.localScale = Vector3.LerpUnclamped(start, peak, k);
        //         DLog.Alert("aaaa");
        //         yield return null;
        //     }
        //     DLog.Alert("++++++");
        //     t = 0f;
        //     while (t < 1f)
        //     {
        //         
        //         t += Time.unscaledDeltaTime / time;
        //         float k = EaseInBack(t);
        //         target.localScale = Vector3.Lerp(peak, start, k);
        //         DLog.Alert("mmmmm "+target.localScale);
        //         yield return null;
        //     }
        //     
        //     target.localScale = start;             // 🔒 гарантированный возврат
        // }
        
        private IEnumerator PulseRoutine(Transform target, float scale = 1.2f, float duration = 0.3f)
        {
            Vector3 start = target.localScale;         // текущий размер
            Vector3 peak = start * scale;              // увеличенный размер

            float halfDuration = duration / 2f;
            float t = 0f;

            // Увеличение (линейно)
            while (t < halfDuration)
            {
                t += Time.unscaledDeltaTime / duration;
                float k = EaseOutBack(t);
                target.localScale = Vector3.LerpUnclamped(start, peak, k);
                yield return null;
            }
            yield return null;
            yield return null;
            yield return null;

            t = 0f;

            // Возврат к исходному размеру (линейно)
            while (t < halfDuration)
            {
                t += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(t / halfDuration);
                target.localScale = Vector3.Lerp(peak, start, progress);
                yield return null;
            }

            // Гарантированный возврат к исходному размеру
            target.localScale = start;
        }

        private IEnumerator ColorFlashRoutine(TMP_Text text, Color color, float time)
        {
            Color original = text.color;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / time;
                text.color = Color.Lerp(color, original, t);
                yield return null;
            }
        }

        private IEnumerator NumberTweenRoutine(TMP_Text text, float from, float to, float time)
        {
            float t = 0f;

            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / time;
                float v = Mathf.Lerp(from, to, t);
                text.text = Mathf.RoundToInt(v).ToString();
                yield return null;
            }

            text.text = to.ToString();
        }

        private IEnumerator FadeRoutine(CanvasGroup cg, float to, float time)
        {
            float from = cg.alpha;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / time;
                cg.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }
        }

        private IEnumerator ShakeRoutine(Transform tr, float strength, float time)
        {
            Vector3 start = tr.localPosition;
            float t = 0f;

            while (t < time)
            {
                t += Time.unscaledDeltaTime;
                tr.localPosition = start + (Vector3)Random.insideUnitCircle * strength;
                yield return null;
            }

            tr.localPosition = start;
        }

        
        
        // =====================================================
        // BACK (для пульса и LDOE-style overshoot)
        // =====================================================
        public static float EaseInBack(float t, float c1 = 1.70158f)
        {
            return c1 * t * t * t - c1 * t * t;
        }

        public static float EaseOutBack(float t, float c1 = 1.70158f)
        {
            float p = t - 1f;
            return 1f + c1 * p * p * p + c1 * p * p;
        }

        public static float EaseInOutBack(float t, float c1 = 1.70158f)
        {
            float c2 = c1 * 1.525f;
            return t < 0.5f
                ? (Mathf.Pow(2f * t, 2f) * ((c2 + 1f) * 2f * t - c2)) / 2f
                : (Mathf.Pow(2f * t - 2f, 2f) * ((c2 + 1f) * (t * 2f - 2f) + c2) + 2f) / 2f;
        }
        
        // =====================================================
        // QUAD
        // =====================================================
        public static float EaseInQuad(float t) => t * t;

        public static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

        public static float EaseInOutQuad(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
        }

        // =====================================================
        // CUBIC
        // =====================================================
        public static float EaseInCubic(float t) => t * t * t;

        public static float EaseOutCubic(float t)
        {
            float p = t - 1f;
            return p * p * p + 1f;
        }

        public static float EaseInOutCubic(float t)
        {
            return t < 0.5f ? 4f * t * t * t : 1f + Mathf.Pow(2f * t - 2f, 3f) / 2f;
        }

        // =====================================================
        // SINE
        // =====================================================
        public static float EaseInSine(float t) => 1f - Mathf.Cos((t * Mathf.PI) / 2f);

        public static float EaseOutSine(float t) => Mathf.Sin((t * Mathf.PI) / 2f);

        public static float EaseInOutSine(float t) => -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f;
        
        // =====================================================
        // LINEAR
        // =====================================================
        public static float Linear(float t) => t;
    }

}