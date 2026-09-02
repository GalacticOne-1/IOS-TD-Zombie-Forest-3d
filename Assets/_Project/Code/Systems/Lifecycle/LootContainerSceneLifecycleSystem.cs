
using System.Collections.Generic;
using Galactic1.RaidLoot.Authoring;
using Galactic1.RaidLoot.Definitions;
using Galactic1.RaidLoot.Services;
using UnityEngine;

namespace Galactic1.RaidLoot.Scene.Lifecycle
{
    /// <summary>
    /// Связывает LootContainerRuntime с LootContainerView.
    ///
    /// Для каждого LootSpawnPoint:
    ///   1. Находит Runtime через Repository
    ///   2. Находит VisualDefinition через VisualDatabase
    ///   3. Инициализирует View (передаёт runtime + visual)
    ///   4. Инициализирует ProximityTrigger
    ///
    /// Хранит список (id, view) для WorldSpaceLootFeedback.
    /// При Dispose() очищает список — View отписываются сами в OnDestroy.
    /// </summary>
    public sealed class LootContainerSceneLifecycleSystem
    {
        private readonly LootContainerRepository _repository;
        private readonly LootContainerFactory _factory;
        private readonly LootContainerOpenService _openService;
        private readonly LootContainerVisualDatabase _visualDatabase;

        
        private readonly List<LootContainerSceneData> _containers = new();
        public IReadOnlyList<LootContainerSceneData> Containers => _containers;
        

        public LootContainerSceneLifecycleSystem(
            LootContainerFactory factory,
            LootContainerOpenService openService,
            LootContainerVisualDatabase visualDatabase)
        {
            _factory = factory;
            _openService = openService;
            _visualDatabase = visualDatabase;
        }

        public void SetSceneReady(LootSpawnPoint[] spawnPoints)
        {
            foreach (var point in spawnPoints)
            {
                if (point?.Config == null) continue;

                if (!_factory.TryGetRuntime(point, out var runtime))
                {
                    Debug.LogWarning($"[LootContainerSceneLifecycleSystem] " +
                                     $"Runtime не найден для контейнера: {point}");
                    continue;
                }

                // Ищем VisualDefinition по VisualId из Runtime
                LootContainerVisualConfig visual = null;
                var visualId = runtime.Definition.VisualId;

                if (visualId != null && !_visualDatabase.TryGet(visualId, out visual))
                {
                    Debug.LogWarning($"[LootContainerSceneLifecycleSystem] " +
                                     $"VisualDefinition не найден для VisualId: {visualId.DebugKey}");
                }

                // Инициализируем View
                var view = point.View;
                if (view != null)
                {
                    view.Init(runtime, visual);
                    _containers.Add(new LootContainerSceneData(runtime, view));
                }

                // Инициализируем ProximityTrigger
                var trigger = point.GetComponent<LootContainerTrigger>();
                trigger?.Init(_openService, runtime, runtime.Definition.OpenTimerDelay);
            }
        }

        public void Dispose()
        {
            _containers.Clear();
        }
    }
}