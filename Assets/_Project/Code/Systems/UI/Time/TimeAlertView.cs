
using Galactic1.Code.Systems.CampDefense.Preparation;
using Galactic1.Code.Systems.GameTime;
using Galactic1.Code.Systems.World.Threats;
using TMPro;
using UnityEngine;

namespace Galactic1.Code.UI.TimeWorld
{
    /// <summary>
    /// TimeAlertView — UI-компонент для отображения сигналов о угрозах мира.
    ///
    /// Используется для:
    /// - предупреждений о приближающихся угрозах (любого типа)
    /// - отображения стадии угрозы: Brewing, Imminent, Active
    ///
    /// Принцип:
    /// - компонент реагирует на события ThreatStageChanged / ThreatActivated
    /// - не содержит игровой логики
    /// - не управляет кнопками
    /// </summary>
    public sealed class TimeAlertView : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text alertText;
        [SerializeField] private TMP_Text dayText;
        [SerializeField] private GameObject timeBox;

        
        string brewingText = "Zombie Activity Rising";
        string imminentText = "Horde Is Near";
        string activeText = "Camp Under Attack!";
        string noThreatText = "No Threats";

        
        
        
        private GameTimeService _gameTime;
        private WorldThreatService _worldThreatService;
        private CampDefensePreparationService campDefensePreparationService;
        
        
        
        public void Activate(CampDefensePreparationService defensePreparationService)
        {
            campDefensePreparationService = defensePreparationService;
            
            
            EventBus<SceneActivateEvent>.Register(new EventBinding<SceneActivateEvent>(() =>
            {
                _gameTime = ServiceLocator.Current.Get<GameTimeService>();
                _worldThreatService = ServiceLocator.Current.Get<WorldThreatService>();
                

                // Подписка на события времени
                _gameTime.DayPassed += OnDayPassed;
                _gameTime.TimeAdvanceFinished += OnTimeAdvanceFinished;

                // Подписка на события угроз
                _worldThreatService.ThreatStageChanged += OnThreatStageChanged;
                _worldThreatService.ThreatActivated += OnThreatActivated;

                Refresh();
            }));
            
            EventBus<SceneClearEvent>.Register(new EventBinding<SceneClearEvent>(() =>
            {
                _gameTime.DayPassed -= OnDayPassed;
                _gameTime.TimeAdvanceFinished -= OnTimeAdvanceFinished;
                
                _worldThreatService.ThreatStageChanged -= OnThreatStageChanged;
                _worldThreatService.ThreatActivated -= OnThreatActivated;
            }));
        }



        /// <summary>
        /// Полное обновление состояния алерта
        /// </summary>
        public void Refresh()
        {
            Threat highestPriorityThreat = _worldThreatService.GetCurrentThreat();

            int remainingHours = highestPriorityThreat?.GetRemainingDays(_gameTime.TotalWorldHours) ?? 0;
            
            
            // день атаки
            if (campDefensePreparationService.IsDefenseAvailable)
            {
                Show(activeText, remainingHours);
                return;
            }
            
            // скрываем оповещение если угроза не активная
            if (remainingHours <= 0)
            {
                Hide();
                return;
            }

            // оповещение отображается 
            // 1. угроза появилась
            // 2. угроза активна (день атаки)
            string alertMessage = highestPriorityThreat.Stage switch
            {
                ThreatStage.Brewing => brewingText,
                ThreatStage.Imminent => imminentText,
                ThreatStage.Active => activeText,
                _ => noThreatText
            };

            
            Show(alertMessage, remainingHours);
        }

        #region Event Handlers

        private void OnDayPassed(DayPassedEvent evt)
        {
            Refresh();
        }

        private void OnTimeAdvanceFinished(TimeAdvanceFinishedEvent evt)
        {
            Refresh();
        }

        private void OnThreatStageChanged(Threat threat)
        {
            Refresh();
        }

        private void OnThreatActivated(Threat threat)
        {
            Refresh();
        }

        #endregion

        #region UI Helpers

        private void Show(string text, int hours)
        {
            timeBox.SetActive(hours > 0);
            alertText.text = text;
            var d = hours / 24;
            hours -= d*24;
            dayText.text = $"{d}\n{hours}";
            gameObject.SetActive(true);
        }

        private void Hide()
        {
            gameObject.SetActive(false);
        }

        #endregion
    }
}
