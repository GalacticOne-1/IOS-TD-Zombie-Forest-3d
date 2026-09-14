
using Galactic1.Code.Gameplay.Audio;
using Galactic1.Code.Gameplay.Combat.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.Gameplay.Tasks.Presentation
{
    /// <summary>
    /// Один ряд списка задач.
    ///
    /// Полностью read-only — не знает про Tutorial/Daily Task/Inbox,
    /// только рендерит ScenarioTaskViewData.
    ///
    /// State используется только для presentation:
    /// Active    -> обычный цвет
    /// Completed -> зелёный цвет
    /// </summary>
    public sealed class ScenarioTaskView : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private ScenarioTaskProgressView progressView;
        [SerializeField] private ScenarioTaskRewardView rewardView;


        [Header("Layout")] 
        [SerializeField] private float baseHeight = 90f;
        
        
        [Header("Completed")] 
        [SerializeField] private Sprite activeSprite, completedSprite;
        [SerializeField] private Image completedBadge;


        [Header("Colors")] 
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color completedColor = Color.green;

        public ScenarioTaskId TaskId { get; private set; }

        public void Bind(ScenarioTaskViewData task, AudioCueData audioCueData)
        {
            TaskId = task.TaskId;

            bool isCompleted =
                task.State == ScenarioTaskState.Completed;

            Color textColor;
            if (isCompleted) // task completed
            {
                textColor = completedColor;
                completedBadge.sprite = completedSprite;
                EventBus<AudioUIEvent>.Raise(new AudioUIEvent(audioCueData));
            }
            else
            {
                textColor = activeColor;
                completedBadge.sprite = activeSprite;
            }


            // Title
            bool hasTitle = !string.IsNullOrEmpty(task.TitleKey);

            titleText.gameObject.SetActive(hasTitle);

            if (hasTitle)
            {
                titleText.text = task.TitleKey;
                titleText.color = textColor;
            }

            // Description + progress
            progressView.Show(
                task.DescriptionKey,
                task.Progress,
                task.InstructionType,
                activeColor);

            // Rewards
            rewardView.Show(task.Rewards);

            RebuildHeight();
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

            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

            RectTransform parentRect = rectTransform.parent as RectTransform;

            if (parentRect != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
            }
        }
    }
}
