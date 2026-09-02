using System.Collections;
using System.Collections.Generic;
using Galactic1.Core;
using UnityEngine;

namespace Galactic1.UI.Core
{
    public class WidgetQueueService : IGameService
    {
        private readonly List<WidgetRequest> _queue = new();
        private bool _isShowing;
        private Coroutines _coroutineRunner;
        
        private const float DelayBetweenWidgets = 0.4f;

        
        public WidgetQueueService(Coroutines coroutineRunner)
        {
            _coroutineRunner = coroutineRunner;
        }

        public void StartShow()
        {
            _isShowing = false;
            TryShowNext();
        }

        public void Enqueue(WidgetRequest request)
        {
            _queue.Add(request);
            _queue.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        private void TryShowNext()
        {
            ServiceLocator.Current.Get<UIRootView>().DisableBlockScreen();
            if (_isShowing || _queue.Count == 0)
            {
                ServiceLocator.Current.Get<IGameStateProvider>().SaveGameState();
                //Debug.LogError("Widget queue completed!");
                return;
            }

            _isShowing = true;
            var request = _queue[0];
            _queue.RemoveAt(0);

            request.Show(() =>
            {
                ServiceLocator.Current.Get<UIRootView>().EnableBlockScreen();
                _isShowing = false;
                _coroutineRunner.StartCoroutine(ShowNextDelayed());
            });
        }

        private IEnumerator ShowNextDelayed()
        {
            yield return new WaitForSeconds(DelayBetweenWidgets);
            TryShowNext();
        }
    }
}