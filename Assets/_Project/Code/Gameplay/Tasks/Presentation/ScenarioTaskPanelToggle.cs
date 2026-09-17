using UnityEngine;

namespace Galactic1.Code.Gameplay.Tasks.Presentation
{
    /// <summary>
    /// Управляет ручным открытием/закрытием ScenarioTaskPanel.
    ///
    /// Не содержит логики самих задач и не вмешивается
    /// в lifecycle ScenarioTaskPanel.
    ///
    /// PanelRoot — отдельный контейнер с CanvasGroup.
    ///
    /// OpenButton  — показывается, когда панель скрыта.
    /// CloseButton — показывается, когда панель открыта.
    ///
    /// Внешняя ScenarioTaskPanel управляет только собственной
    /// visibility-логикой и может принудительно свернуть/развернуть
    /// эту панель через ForceHidePanel / ForceShowPanel.
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


        /// <summary>
        /// Текущее ручное состояние панели.
        ///
        /// true  — панель развёрнута.
        /// false — панель свёрнута.
        /// </summary>
        public bool IsOpen => _isOpen;


        private void Awake()
        {
            if (openButton != null)
                openButton.RegisterButtonClick(Toggle);

            if (closeButton != null)
                closeButton.RegisterButtonClick(Toggle);

            ApplyState();
        }


        /// <summary>
        /// Управляет видимостью контейнера кнопок.
        ///
        /// ScenarioTaskPanel вызывает этот метод, когда
        /// обычные экраны открываются/закрываются.
        /// </summary>
        public void RootButtons(bool show)
        {
            if (buttonsRoot != null)
                buttonsRoot.SetActive(show);
        }


        /// <summary>
        /// Переключает ручное состояние панели.
        ///
        /// Open  -> Closed
        /// Closed -> Open
        /// </summary>
        public void Toggle()
        {
            _isOpen = !_isOpen;

            ApplyState();
        }


        /// <summary>
        /// Принудительно разворачивает панель.
        ///
        /// Используется ScenarioTaskPanel при activity задачи.
        ///
        /// Не зависит от текущего состояния _isOpen.
        /// </summary>
        public void ForceShowPanel(bool showButtons)
        {
            _isOpen = true;

            ApplyState();

            /*
             * Во время автоматического показа задач
             * кнопки ручного управления не нужны.
             */
            RootButtons(showButtons);
        }


        /// <summary>
        /// Принудительно сворачивает панель.
        ///
        /// Используется когда список задач пуст.
        ///
        /// OpenButton остаётся активным.
        /// CloseButton скрывается.
        /// </summary>
        public void ForceHidePanel(bool showButtons)
        {
            _isOpen = false;

            ApplyState();

            /*
             * Кнопка открытия должна быть доступна,
             * даже когда задач нет.
             */
            RootButtons(showButtons);
        }


        private void ApplyState()
        {
            if (panelRoot != null)
            {
                panelRoot.alpha = _isOpen ? 1f : 0f;

                panelRoot.interactable = _isOpen;

                panelRoot.blocksRaycasts = _isOpen;
            }


            if (openButton != null)
                openButton.SetActive(!_isOpen);


            if (closeButton != null)
                closeButton.SetActive(_isOpen);


            RootButtons(true);
        }
    }
}