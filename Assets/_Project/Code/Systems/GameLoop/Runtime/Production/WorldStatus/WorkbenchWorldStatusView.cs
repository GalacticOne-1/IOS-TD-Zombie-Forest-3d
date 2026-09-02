
using System;
using Galactic1.Code.Utility;
using Galactic1.Runtime.UI.WorldStatus;
using Galactic1.UI.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.UI.WorldStatus
{
    /// <summary>
    /// Screen Space UI над верстаком.
    ///
    /// Позиционируется через WorldToScreenPoint каждый кадр.
    /// Render() — единственная точка обновления данных.
    /// Update() — только слежение за позицией здания в мире.
    /// </summary>
    public sealed class WorkbenchWorldStatusView : MonoBehaviour
    {
        [Header("Root")] 
        [SerializeField] private GameObject panel;

        [Header("Item")] 
        [SerializeField] private Image itemIcon;

        [SerializeField] private GameObject compoleteLabel;

        [Header("Orders")] 
        [SerializeField] private TextMeshProUGUI ordersLabel;

        [Header("Time")] 
        [SerializeField] private TextMeshProUGUI timeLabel;

        [Header("Progress")] 
        [SerializeField] private Image progressFill;

        
        [Header("World Tracking")] [SerializeField]
        private Vector3 worldOffset = new(0f, 2.5f, 0f); // высота над зданием

        // =========================================================
        // Runtime
        // =========================================================

        private WorkbenchWorldStatusPresenter _presenter;
        private Transform _worldTarget; // Transform здания
        private Camera cam;
        private RectTransform rectTransform;
        private Canvas canvas;

        // =========================================================
        // Unity lifecycle
        // =========================================================

        /// <summary>
        /// Update — строго только позиция.
        /// Никакого состояния, никакого Presenter.
        /// </summary>
        private void Update()
        {
            if (_worldTarget == null || cam == null) return;

            var worldPos = _worldTarget.position + worldOffset;
            var screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldPos);

            // Переводим screen point в local point внутри Canvas
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    screenPos,
                    canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
                    out var localPoint))
            {
                rectTransform.localPosition = localPoint;
            }
        }

        private void OnDestroy()
        {
            _presenter?.Dispose();
        }

        // =========================================================
        // PUBLIC
        // =========================================================

        /// <summary>
        /// Вызывается из FacilityFactory.
        /// worldTarget — Transform здания для слежения позиции.
        /// </summary>
        public void Bind(
            WorkbenchWorldStatusPresenter presenter,
            Transform worldTarget,
            Camera camera)
        {
            rectTransform = GetComponent<RectTransform>();
            panel?.SetActive(false);

            cam = camera;
            canvas = GetComponentInParent<Canvas>();

            _presenter = presenter ?? throw new ArgumentNullException(nameof(presenter));
            _worldTarget = worldTarget ?? throw new ArgumentNullException(nameof(worldTarget));

            // Presenter уже создан снаружи и подписан —
            // вручную запрашиваем первый Render после того как View готов
            _presenter.ForceRefresh();
        }

        /// <summary>
        /// Единственная точка обновления данных.
        /// Вызывается Presenter-ом по OnStateChanged.
        /// </summary>
        public void Render(WorkbenchStatusDTO dto)
        {
            panel?.SetActive(dto.HasAnyJob);

            if (!dto.HasAnyJob) return;

            itemIcon.sprite = dto.ItemIcon;
            itemIcon.enabled = dto.ItemIcon != null;

            var completed = !dto.IsWorking && dto.CompletedStack > 0;
            
            compoleteLabel.SetActive(completed);
            
            if (!completed)
                ordersLabel.text = TextBuilder.Start()
                    .Color(Color.green)
                    .Size(115)
                    .Text(dto.CompletedStack)
                    .End() // size
                    .End() // color
                    .Text("/")
                    .Size(100)
                    .Text(dto.TotalStack)
                    .End()
                    .ToString();
            else
                ordersLabel.text = TextBuilder.Start()
                    .Color(Color.green)
                    .Size(100)
                    .Text(dto.CompletedStack)
                    .ToString();
            
            timeLabel.text = TimeUtils.FormatTime(dto.RemainingTime);
            timeLabel.transform.parent.gameObject.SetActive(dto.RemainingTime > 0);

            progressFill.fillAmount = dto.Progress;
        }

    }
}