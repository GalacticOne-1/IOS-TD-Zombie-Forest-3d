using System.Collections.Generic;
using Galactic1.Code.Gameplay.Tasks;
using Galactic1.Code.Systems.Tutorial.Runtime;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>
    /// Контейнер наград рядом с инструкцией шага. Пул виджетов — награды на шаг
    /// немногочисленны (единицы), пересоздавать инстансы на каждый Show() не нужно.
    /// Полностью скрывается, если наград нет — не занимает места (см. TutorialHUDController).
    /// </summary>
    public sealed class TutorialRewardView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private RectTransform itemsContainer;
        [SerializeField] private TutorialRewardItemWidget itemWidgetPrefab;

        private readonly List<TutorialRewardItemWidget> _pool = new();

        public void Show(IReadOnlyList<ScenarioTaskReward> rewards)
        {
            if (rewards == null || rewards.Count == 0)
            {
                Hide();
                return;
            }

            EnsurePoolSize(rewards.Count);
            for (int i = 0; i < _pool.Count; i++)
                _pool[i].gameObject.SetActive(i < rewards.Count);

            for (int i = 0; i < rewards.Count; i++)
                _pool[i].Set(rewards[i]);

            root.SetActive(true);
        }

        public void Hide() => root.SetActive(false);

        private void EnsurePoolSize(int count)
        {
            while (_pool.Count < count)
                _pool.Add(Instantiate(itemWidgetPrefab, itemsContainer));
        }
    }
}