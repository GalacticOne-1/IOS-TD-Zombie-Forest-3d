using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.Gameplay.Tasks.Presentation
{
    /// <summary>Один ряд списка задач. Полностью read-only — не знает про Tutorial/Daily
    /// Task/Inbox, только рендерит ScenarioTaskViewData.</summary>
    public sealed class ScenarioTaskView : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private ScenarioTaskProgressView progressView;
        [SerializeField] private ScenarioTaskRewardView rewardView;
        [SerializeField] private GameObject completedBadge;

        [SerializeField] private float baseHeight = 90f;
        public ScenarioTaskId TaskId { get; private set; }

        
        
        public void Bind(ScenarioTaskViewData task)
        {
            TaskId = task.TaskId;

            bool hasTitle = !string.IsNullOrEmpty(task.TitleKey);
            titleText.gameObject.SetActive(hasTitle);
            if (hasTitle)
                titleText.text = task.TitleKey; // TODO: LocalizationService.Resolve, если появится

            
            progressView.Show(task.DescriptionKey, task.Progress, task.InstructionType);
            rewardView.Show(task.Rewards);
            
            RebuildHeight();

            // Completed: прогресс/награда остаются видимыми (финальное 5/5 перед
            // исчезновением задачи), добавляется только бейдж.
            //completedBadge.SetActive(task.State == ScenarioTaskState.Completed);
        }
        
        private void RebuildHeight()
        {
            float height = baseHeight;

            height += progressView.GetHeight();

            if (rewardView.IsVisible)
                height += rewardView.GetHeight();

            RectTransform rectTransform = transform as RectTransform;

            if (rectTransform == null)
                return;

            rectTransform.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                height);

            RectTransform parentRect = rectTransform.parent as RectTransform;

            if (parentRect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
        }
    }
}