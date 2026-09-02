using Galactic1.Code.Systems.GameTime;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.TimeWorld
{
    /// <summary>
    /// UI-контроллер кнопки "Пропустить дни".
    ///
    /// Отвечает за:
    /// - запрос на ручной пропуск времени
    /// - блокировку кнопки во время продвижения времени
    /// - базовую валидацию пользовательского ввода
    ///
    /// ВАЖНО:
    /// - Контроллер НЕ управляет логикой времени
    /// - Вся логика проходит через GameTimeService
    /// </summary>
    public sealed class SkipDayButton : BaseUIButton
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text currentDayLabel;
        [SerializeField] private TMP_Text remainingHourLabel;
        [SerializeField] private Image dayBar;

        public GameObject DayBarRoot => dayBar.transform.parent.gameObject;

        private GameTimeService _gameTime;

        
        
        
        
        public void Activate()
        {
            events.onClick.AddListener(OnSkipButtonClicked);
            
            EventBus<SceneActivateEvent>.Register(new EventBinding<SceneActivateEvent>(() =>
            {
                _gameTime = ServiceLocator.Current.Get<GameTimeService>();
                _gameTime.TimeAdvanceStarted += OnTimeAdvanceStarted;
                _gameTime.TimeAdvanceFinished += OnTimeAdvanceFinished;
                
                RefreshDayLabel();
                UpdateButtonState();
            }));
            
            EventBus<SceneClearEvent>.Register(new EventBinding<SceneClearEvent>(() =>
            {
                _gameTime.TimeAdvanceStarted -= OnTimeAdvanceStarted;
                _gameTime.TimeAdvanceFinished -= OnTimeAdvanceFinished;
            }));
        }


        /// <summary>
        /// Обработка нажатия кнопки "Пропустить дни"
        /// </summary>
        private void OnSkipButtonClicked()
        {
            if (_gameTime.IsTimeAdvancing)
                return;

            _gameTime.SkipToNextDay(TimeAdvanceReason.ManualSkip);
        }


        /// <summary>
        /// Блокировка UI при начале продвижения времени
        /// </summary>
        private void OnTimeAdvanceStarted(TimeAdvanceStartedEvent evt)
        {
            DLog.Alert("Time Advance Started");
            UpdateButtonState();
        }

        /// <summary>
        /// Разблокировка UI после завершения продвижения времени
        /// </summary>
        private void OnTimeAdvanceFinished(TimeAdvanceFinishedEvent evt)
        {
            RefreshDayLabel();
            UpdateButtonState();
        }

        /// <summary>
        /// Обновление текста текущего дня
        /// </summary>
        private void RefreshDayLabel()
        {
            currentDayLabel.text = $"Day {_gameTime.CurrentDay} | {_gameTime.RemainingHour}h Remaining";
            //remainingHourLabel.text = $"{_gameTime.RemainingHour}h Remaining";

            dayBar.fillAmount = (float)_gameTime.RemainingHour / 24;
        }

        /// <summary>
        /// Обновление состояния кнопки
        /// </summary>
        private void UpdateButtonState()
        {
            //skipButton.interactable = !_gameTime.IsTimeAdvancing;
            //daysInput.interactable = !_gameTime.IsTimeAdvancing;
        }
    }
}
