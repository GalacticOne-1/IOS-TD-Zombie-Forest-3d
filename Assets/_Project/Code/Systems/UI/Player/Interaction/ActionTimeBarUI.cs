using UnityEngine;
using UnityEngine.UI;
using Galactic1.Gameplay.Player.StateMachine;

namespace Galactic1.Gameplay.UI
{
    /// <summary>
    /// Тайм-бар прогресса для действия игрока (сундук, кодовый сейф, сбор ресурсов и др.).
    /// Один бар на всю сцену, динамически позиционируется над текущим объектом.
    /// Интегрируется с ActionJob.
    /// </summary>
    public class ActionTimeBarUI : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);

        private Camera mainCamera;
        private Vector3 target;
        private ActionJob job;
        

        private void Awake()
        {
            mainCamera = Camera.main;
            Hide();
        }

        private void Update()
        {
            if (job == null || target == null)
            {
                Hide();
                return;
            }

            // позиция тайм-бара над объектом
            Vector3 screenPos = mainCamera.WorldToScreenPoint(target + offset);
            transform.position = screenPos;

            // обновление fill по таймеру
            float progress = Mathf.Clamp01(1f - (job.RemainingTime / job.Duration));
            fillImage.fillAmount = progress;

            // если job завершился — скрыть
            if (job.IsCompleted || job.IsCancelled)
            {
                Hide();
                //target = null;
                job = null;
            }
        }

        /// <summary>
        /// Привязка тайм-бара к текущему действию
        /// </summary>
        public void StartAction(Vector3 target, ActionJob job)
        {
            this.target = target;
            this.job = job;
            Show();
        }

        public void CancelAction()
        {
            //target = null;
            job = null;
            Hide();
        }

        private void Show()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            fillImage.fillAmount = 0;
        }

        private void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
