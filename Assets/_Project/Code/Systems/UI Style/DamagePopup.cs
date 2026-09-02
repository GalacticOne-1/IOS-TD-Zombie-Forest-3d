using UnityEngine;
using TMPro;
using System;
using Gameplay;
using Random = UnityEngine.Random;

namespace Galactic1
{
    public class DamagePopup : MonoBehaviour
    {
        [Header("UI")] public TMP_Text textMesh;


        [SerializeField] private float scale;
        [Header("Motion BasicSettings")] public float baseMoveSpeed = 40f;
        public float fadeSpeed = 2f;
        public float lifetime = 1.5f;
        public float holdDuration = 0.25f;

        [Header("Shake & Spread")] public float shakeIntensity = 5f;
        public float directionSpread = 25f; // угол отклонения движения
        public float speedVariation = 0.2f; // разброс скорости

        [Header("Scale Animation")] public float scaleInSpeed = 8f;
        public float scaleOutSpeed = 2f;
        public float maxScale = 1.2f;

        private Transform tr;
        private float timer;
        private float holdTimer;
        private Color color;
        private Vector3 moveDir;
        private float moveSpeed;
        private float currentScale = 0.5f;
        private bool reachedMaxScale = false;
        private bool fading = false;

        public Action OnHide;

        
        
        
        
        
        public void Setup(DamagePopupStyleConfig.CStyle style, int damage, bool isCritical)
        {
            
            tr = transform;
            textMesh.text = damage.ToString();
            textMesh.fontSize = isCritical ? 6 : 4;

            // небольшая вариация цвета
            float brightness = Random.Range(0.9f, 1.1f);
            color = style.color * brightness;
            color.a = 1f;
            textMesh.color = color;

            // движение — не строго вверх
            float angle = Random.Range(-directionSpread, directionSpread);
            moveDir = Quaternion.Euler(0, 0, angle) * Vector3.up;

            // случайная вариация скорости
            moveSpeed = baseMoveSpeed * Random.Range(1f - speedVariation, 1f + speedVariation);

            timer = lifetime;
            holdTimer = holdDuration;
            fading = false;
            reachedMaxScale = false;
            transform.localScale = Vector3.one * currentScale;

            // критический урон выглядит мощнее
            if (isCritical)
            {
                maxScale = 1.5f;
                moveSpeed *= 1.25f;
                color = Color.Lerp(style.color, Color.yellow, 0.5f);
                textMesh.outlineWidth = 0.2f;
            }
            else
            {
                textMesh.outlineWidth = 0.1f;
            }
        }

        void Update()
        {
            timer -= Time.deltaTime;

            // движение
            transform.position += moveDir * moveSpeed * Time.deltaTime;

            // дрожь
            float shake = Mathf.Sin(Time.time * 40f) * shakeIntensity;
            transform.position += new Vector3(shake * Time.deltaTime, 0, 0);

            // масштаб (выпрыгивание)
            if (!reachedMaxScale)
            {
                currentScale += scaleInSpeed * Time.deltaTime;
                if (currentScale >= maxScale)
                {
                    currentScale = maxScale;
                    reachedMaxScale = true;
                }
            }
            else
            {
                currentScale -= scaleOutSpeed * Time.deltaTime;
                if (currentScale <= 1f)
                    currentScale = 1f;
            }

            transform.localScale = Vector3.one * currentScale;

            // зависание → исчезновение
            if (timer < holdDuration && !fading)
            {
                holdTimer -= Time.deltaTime;
                if (holdTimer <= 0)
                {
                    fading = true;
                }
            }

            // плавный fade-out
            if (fading)
            {
                color.a -= fadeSpeed * Time.deltaTime;
                textMesh.color = color;
                if (color.a <= 0)
                {
                    OnHide?.Invoke();
                }
            }
        }
    }

}