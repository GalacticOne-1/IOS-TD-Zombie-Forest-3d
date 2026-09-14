using UnityEngine;

namespace Galactic1.Code.Gameplay.Tasks.Presentation
{
    /// <summary>
    /// Управляет ручным открытием/закрытием ScenarioTaskPanel.
    ///
    /// Не содержит никакой логики самих задач и не вмешивается
    /// в lifecycle ScenarioTaskPanel.
    ///
    /// Для корректной работы panelRoot должен быть отдельным
    /// контейнером с CanvasGroup, внутри которого находится ScenarioTaskPanel.
    ///
    /// OpenButton  — показывается, когда панель скрыта.
    /// CloseButton — показывается, когда панель открыта.
    ///
    /// Обе кнопки используют один Toggle().
    ///
    /// Важно: если в момент нажатия OpenButton открыт обычный UIScreen,
    /// ScenarioTaskPanel сама держит свой внутренний CanvasGroup на alpha=0
    /// (см. ScenarioTaskPanel.OnScreenOpened -> HideImmediate()).
    /// В этом случае контейнер станет видимым (alpha=1), но итоговая
    /// видимость на экране всё равно будет 0, т.к. она — произведение
    /// alpha контейнера и alpha самой панели. Это ожидаемое поведение:
    /// ручной toggle работает только тогда, когда нет открытых экранов.
    /// </summary>
    public sealed class ScenarioTaskPanelToggle : MonoBehaviour
    {
        [Header("Panel")] 
        [SerializeField] private CanvasGroup panelRoot;

        [Header("Buttons")] 
        [SerializeField] private GameObject buttonsRoot;
        [SerializeField] private GameObject openButton;
        [SerializeField] private GameObject closeButton;

        private bool _isOpen = true;

        private void Awake()
        {
            if (openButton != null)
                openButton.RegisterButtonClick(Toggle);

            if (closeButton != null)
                closeButton.RegisterButtonClick(Toggle);

            ApplyState();
        }


        public void RootButtons(bool show) 
            => buttonsRoot.SetActive(show);


        /// <summary>
        /// Переключает состояние панели.
        ///
        /// Open  -> Closed
        /// Closed -> Open
        /// </summary>
        public void Toggle()
        {
            _isOpen = !_isOpen;

            ApplyState();
        }

        private void ApplyState()
        {
            if (panelRoot != null)
            {
                panelRoot.alpha = _isOpen ? 1 : 0;
                panelRoot.interactable = _isOpen;
                panelRoot.blocksRaycasts = _isOpen;
            }

            if (openButton != null)
                openButton.gameObject.SetActive(!_isOpen);

            if (closeButton != null)
                closeButton.gameObject.SetActive(_isOpen);

            RootButtons(true);
        }

        public void ForceShowPanel()
        {
            if (!_isOpen)
                Toggle();

            RootButtons(false);
        }
    }
}