
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.Gameplay.Tasks.Presentation
{
    /// <summary>
    /// Read-only presentation of scenario task description and progress.
    ///
    /// Root height is calculated from the preferred heights of
    /// Description and Progress texts.
    ///
    /// Цвет текста передаётся владельцем View:
    /// Active    -> activeColor
    /// Completed -> completedColor
    ///
    /// Сам View не знает о lifecycle задачи и не содержит
    /// никакой логики completion-delay.
    /// </summary>
    public sealed class ScenarioTaskProgressView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text progressText;
        [SerializeField] private Image fillBar;

        [SerializeField] private float spacing;

        [Header("Colors")] [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color completedColor = Color.green;

        
        
        /// <summary>
        /// Основной метод отображения.
        ///
        /// Цвет передаётся явно, чтобы View оставался простым
        /// presentation-компонентом.
        /// </summary>
        public void Show(
            string description,
            ScenarioTaskProgress progress,
            ScenarioTaskInstructionType instructionType,
            Color textColor)
        {
            root.SetActive(true);

            // Description
            descriptionText.text = description;
            //descriptionText.color = textColor;

            // Progress
            bool hasProgress = progress.HasProgress;

            progressText.gameObject.SetActive(hasProgress);

            if (!hasProgress)
            {
                progressText.text = string.Empty;
            }
            else
            {
                progressText.text =
                    instructionType == ScenarioTaskInstructionType.Timer
                        ? FormatAsTimer(progress.Current)
                        : $"[{progress.Current}/{progress.Required}]";

                progressText.color = textColor;
            }

            // Fill bar
            if (fillBar != null)
            {
                fillBar.gameObject.SetActive(hasProgress);

                if (hasProgress && progress.Required > 0)
                {
                    fillBar.fillAmount =
                        Mathf.Clamp01(
                            (float)progress.Current /
                            progress.Required);
                }
            }

            RebuildLayout();
        }

        public void Hide()
        {
            root.SetActive(false);
        }

        private void RebuildLayout()
        {
            Canvas.ForceUpdateCanvases();

            LayoutRebuilder.ForceRebuildLayoutImmediate(
                descriptionText.rectTransform);

            if (progressText.gameObject.activeSelf)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(
                    progressText.rectTransform);
            }

            float height =
                LayoutUtility.GetPreferredHeight(
                    descriptionText.rectTransform);

            height += 20;

            if (progressText.gameObject.activeSelf)
            {
                height +=
                    LayoutUtility.GetPreferredHeight(
                        progressText.rectTransform);

                height += spacing;
            }

            RectTransform rootRect =
                root.transform as RectTransform;

            if (rootRect != null)
            {
                rootRect.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    height);
            }
        }

        private static string FormatAsTimer(int remainingSeconds)
        {
            remainingSeconds = Mathf.Max(0, remainingSeconds);

            return $"{remainingSeconds / 60:00}:{remainingSeconds % 60:00}";
        }

        public float GetHeight()
        {
            return ((RectTransform)root.transform).rect.height;
        }
    }
}