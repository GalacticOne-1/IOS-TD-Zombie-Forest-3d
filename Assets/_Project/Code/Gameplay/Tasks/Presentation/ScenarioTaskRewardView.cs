using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Tasks.Presentation
{
    /// <summary>Контейнер наград одной задачи — пул ScenarioTaskRewardIconWidget.
    /// Полностью скрывается при отсутствии наград (см. ScenarioTaskView.Bind).</summary>
    public sealed class ScenarioTaskRewardView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private GameObject itemsContainer;


        public void Show(IReadOnlyList<ScenarioTaskReward> rewards)
        {
            if (rewards == null || rewards.Count == 0)
            {
                Hide();
                return;
            }

            
            var l = itemsContainer.transform.childCount;
            for (int i = 0; i < l; i++)
                itemsContainer.GetChild(i).SetActive(i < rewards.Count);

            var rewardQu = rewards.Count <= l ? rewards.Count : l;
            for (int i = 0; i < rewardQu; i++)
                itemsContainer.GetChild(i).GetComponent<ScenarioTaskRewardItem>().Set(rewards[i]);

            root.SetActive(true);
        }

        public void Hide() => root.SetActive(false);
        
        public bool IsVisible => root.activeSelf;
        public float GetHeight()
            => ((RectTransform)root.transform).rect.height;

    }
}