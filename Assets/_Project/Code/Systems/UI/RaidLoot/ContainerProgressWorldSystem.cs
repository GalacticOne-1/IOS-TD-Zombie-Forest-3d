using System.Collections.Generic;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Enums;
using Galactic1.RaidLoot.Runtime;
using Galactic1.RaidLoot.Scene;
using Galactic1.RaidLoot.Scene.Lifecycle;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Code.UI.RaidLoot
{
    /// <summary>
    /// Global pooled progress UI for loot containers.
    ///
    /// Runtime
    ///   -> proximity
    ///   -> progress
    ///
    /// System
    ///   -> pooled screen-space bars
    /// </summary>
    public sealed class ContainerProgressWorldSystem :
        MonoBehaviour,
        IGameService
    {
        [Header("Pool")] 
        [SerializeField] private ContainerProgressView _prefab;
        [SerializeField] private int _poolSize = 10;

        [Header("UI")]
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Camera _camera;
        [SerializeField] private Vector3 stackOffsetPosition;
        
        private Transform _root;

        private readonly Queue<ContainerProgressView> _pool = new();

        private readonly Dictionary<string, LootContainerView> _views = new();

        private readonly Dictionary<string, LootContainerRuntime> _runtimes = new();

        private readonly Dictionary<string, ContainerProgressView> _activeBars = new();

        // ------------------------------------------------

        public void Initialize(IEnumerable<LootContainerSceneData> containers)
        {
            _root = ServiceLocator.Current.Get<UIManager>().TransformRoot.floatWorldRoot;

            foreach (var container in containers)
            {
                _views[container.RuntimeId] = container.View;
                _runtimes[container.RuntimeId] = container.Runtime;

                container.Runtime.OnProximityChanged += value => OnProximityChanged(container.RuntimeId, value);

                container.Runtime.OnOpenProgressChanged += value => OnProgressChanged(container.RuntimeId, value);

                container.Runtime.OnStateChanged += state => OnStateChanged(container.RuntimeId, state);
            }

            PrewarmPool();
            
            // всегда регистрируем очистку при смене сцены
            EventBus<SceneServicesClearEvent>.Register(new EventBinding<SceneServicesClearEvent>(() =>
            {
                _root.MakeEmpty();
            }));
        }

        // ------------------------------------------------

        private void PrewarmPool()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                var view = Instantiate(_prefab, _root);

                view.gameObject.SetActive(false);

                _pool.Enqueue(view);
            }
        }

        private ContainerProgressView Get()
        {
            if (_pool.Count > 0)
                return _pool.Dequeue();

            var view = Instantiate(_prefab, _root);

            view.gameObject.SetActive(false);

            return view;
        }

        private void Return(ContainerProgressView view)
        {
            view.ResetView();
            view.gameObject.SetActive(false);

            _pool.Enqueue(view);
        }

        // ------------------------------------------------

        private void OnProximityChanged(
            string id,
            bool inProximity)
        {
            if (!_runtimes.TryGetValue(id, out var runtime))
                return;

            if (runtime.IsOpened)
                return;

            if (inProximity)
                ShowBar(id);
            else
                HideBar(id);
        }

        private void OnProgressChanged(
            string id,
            float progress)
        {
            if (_activeBars.TryGetValue(id, out var bar))
            {
                bar.SetProgress(progress);
            }
        }

        private void OnStateChanged(
            string id,
            ContainerState state)
        {
            if (state == ContainerState.Open ||
                state == ContainerState.FullyLooted)
            {
                HideBar(id);
            }
        }

        // ------------------------------------------------

        private void ShowBar(string id)
        {
            DLog.Alert("Showing bar " + id);
            if (_activeBars.ContainsKey(id))
                return;

            if (!_views.TryGetValue(id, out var view))
                return;

            var bar = Get();

            bar.Attach(view.GetFeedbackAnchor()+stackOffsetPosition, _camera);

            bar.gameObject.SetActive(true);

            _activeBars.Add(id, bar);
        }

        private void HideBar(string id)
        {
            if (!_activeBars.TryGetValue(id, out var bar))
                return;

            _activeBars.Remove(id);

            Return(bar);
        }

    }
}