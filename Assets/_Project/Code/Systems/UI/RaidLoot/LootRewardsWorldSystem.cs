using System.Collections.Generic;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Events;
using Galactic1.RaidLoot.Scene;
using Galactic1.RaidLoot.Scene.Lifecycle;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Code.UI.RaidLoot
{
    /// <summary>
    /// Global raid loot UI system.
    ///
    /// Receives ContainerLootCollectedEvent,
    /// spawns LootRewardsStack above container,
    /// then returns stack back to pool.
    /// </summary>
    public sealed class LootRewardsWorldSystem :
        MonoBehaviour,
        IGameService
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Vector3 stackOffsetPosition;
        
        [Header("Pool")]
        [SerializeField] private LootRewardsStack _stackPrefab;
        [SerializeField] private int _poolSize = 10;

        
        
        private Transform _root;

        private readonly Queue<LootRewardsStack> _pool = new();

        private readonly Dictionary<string, LootContainerView> _views = new();


        // ------------------------------------------------

        public void Initialize(IEnumerable<LootContainerSceneData> containers)
        {
            _root = ServiceLocator.Current.Get<UIManager>().TransformRoot.floatWorldRoot;

            _views.Clear();

            foreach (var container in containers)
                _views[container.RuntimeId] = container.View;

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
                var stack = Instantiate(
                        _stackPrefab,
                        _root);

                stack.gameObject.SetActive(false);

                stack.Setup(ReturnToPool);

                _pool.Enqueue(stack);
            }
        }

        private LootRewardsStack GetStack()
        {
            if (_pool.Count == 0)
            {
                var stack = Instantiate(
                        _stackPrefab,
                        _root);

                stack.Setup(ReturnToPool);

                return stack;
            }

            return _pool.Dequeue();
        }

        private void ReturnToPool(LootRewardsStack stack)
        {
            stack.gameObject.SetActive(false);

            _pool.Enqueue(stack);
        }

        // ------------------------------------------------

        public void OnLootCollected(ContainerLootCollectedEvent e)
        {
            if (!_views.TryGetValue(e.ContainerId, out var view))
            {
                return;
            }

            cash = e;
            var stack = GetStack();

            stack.Attach(view.GetFeedbackAnchor()+stackOffsetPosition, _camera);
            stack.gameObject.SetActive(true);

            stack.Show(e.Loot);
        }
        
        private ContainerLootCollectedEvent cash;
        // private void Update()
        // {
        //     if (Input.GetKeyDown(KeyCode.A))
        //     {
        //         OnLootCollected(cash);
        //     }
        // }
    }
}