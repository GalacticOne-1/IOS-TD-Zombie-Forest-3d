using UnityEngine;

namespace Galactic1
{
    public class GameSpeedController : MonoBehaviour, IGameService
    {
        [Header("UI")] 
        [SerializeField] private GameObject bSpeed;
        [SerializeField] private Sprite spOn, spOff;
        

        [Header("Настройки")] 
        [SerializeField] private float[] speedOptions = { 0f, 1f, 2f, 4f }; // Индексы: 0 = пауза, 1 = x1, 2 = x2, 3 = x4
        private int currentIndex = 1; // Стартовая скорость — x1

        private bool x2_max = true;
        
        
        
        
        private void Start()
        {
            ResetSpeed();
            bSpeed.EventBtn_old(SetSpeed);
        }

        public void SetSpeed(int index)
        {
            if (index < 0 || index >= speedOptions.Length) return;

            currentIndex = index;
            ApplySpeed();
        }
        
        public void SetSpeed()
        {
            var optionsLength= speedOptions.Length;
            if (x2_max)
                optionsLength--;
            
            int attempts = 0;
            do
            {
                currentIndex = (currentIndex + 1) % optionsLength;
                attempts++;

                // Безопасный выход из бесконечного цикла, если в массиве только пауза
                if (attempts > optionsLength)
                {
                    Debug.LogWarning("No non-zero speeds available in speedOptions!");
                    return;
                }

            } while (Mathf.Approximately(speedOptions[currentIndex], 0f));

            ApplySpeed();
        }

        public void TogglePause()
        {
            if (Time.timeScale > 0f)
            {
                Time.timeScale = 0f;
                UpdateSpeedText();
            }
            else
            {
                Time.timeScale = speedOptions[currentIndex];
                UpdateSpeedText();
            }
        }

        public void ResetSpeed()
        {
            currentIndex = 1; // индекс обычной скорости (x1)
            ApplySpeed();
        }

        private void ApplySpeed()
        {
            Time.timeScale = speedOptions[currentIndex];
            UpdateSpeedText();
        }

        private void UpdateSpeedText()
        {
            if (bSpeed == null) return;

            if (Time.timeScale == 0f)
                bSpeed.GetChild(0).CMP_Text().text = "Speed Paused";
            else
            {
                bSpeed.GetChild(0).CMP_Text().text = $"Speed x{Time.timeScale}";
                bSpeed.CMP_Image().sprite = speedOptions[currentIndex] == 1 ? spOff : spOn;
            }
        }
    }

}