using System.Collections.Generic;
using Galactic1.AbstractFactory;
using Galactic1.Code.Gameplay.Enemies.Repositories;
using Galactic1.Code.Gameplay.Enemies.Visuals;
using Galactic1.Code.Gameplay.Units.Repositories;
using Galactic1.Code.Gameplay.Units.Zombie;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.Systems.Raid.Enemies;
using Galactic1.Core.Systems.Factories;
using UnityEngine;

namespace Galactic1.Code.Systems.Lifecycle
{
    /// <summary>
    /// Мост между EnemyRegistry (runtime) и сценой (EnemyInstance).
    ///
    /// Слушает события Raid.Enemies.OnRegistered / OnUnregistered.
    /// При регистрации — спавнит сцен-объект и бросает EnemySceneCreatedEvent.
    /// При отмене — бросает EnemySceneDestroyedEvent и удаляет сцен-объект.
    ///
    /// ПРАВИЛО: Lifecycle не знает об UI-системах.
    /// Связь с EnemyHealthBarSystem — только через EventBus.
    ///
    /// ПРАВИЛО: Lifecycle никогда не обращается к ConfigProvider или ScriptableObject.
    /// Вся информация (prefabId, анимации) содержится в runtime.Definition.Presentation,
    /// подготовленном EnemySpawnPipeline.
    /// </summary>
    public sealed class EnemySceneLifecycleSystem
    {
        private readonly Dictionary<string, ZombieSceneBinder> _binders = new();
        private readonly ZombieFactory _factory;
        private readonly RaidRuntime _raid;
        private readonly EnemyRepository _repository;
        private readonly EnemyVisualAssembler _visualAssembler;
        private Transform _instanceRoot;

        private bool _sceneReady;

        public EnemySceneLifecycleSystem(
            ZombieFactory factory,
            RaidRuntime raid,
            EnemyRepository repository)
        {
            _factory = factory;
            _raid = raid;
            _repository = repository;

            _visualAssembler = new EnemyVisualAssembler();
            _instanceRoot = ServiceLocator.Current.Get<Environment>().enemies;

            _raid.Enemies.OnRegistered += HandleZombieRegistered;
            _raid.Enemies.OnUnregistered += HandleZombieUnregistered;
        }

        /// <summary>Вызывается когда сцена полностью готова к спавну.</summary>
        public void SetSceneReady()
        {
            _sceneReady = true;
        }

        // ── РЕГИСТРАЦИЯ ───────────────────────────────────────────────

        private void HandleZombieRegistered(EnemyRuntime runtime)
        {
            if (!_sceneReady) return;
            SpawnSceneEntity(runtime);
        }

        private void SpawnSceneEntity(EnemyRuntime runtime)
        {
            var presentation = runtime.Definition.Presentation;

            if (presentation == null)
            {
                Debug.LogError($"[EnemySceneLifecycle] Presentation == null для {runtime.EnemyId}. " +
                               "Убедись что EnemySpawnPipeline заполнил Definition.Presentation.");
                return;
            }

            var spawnPos = runtime.SpawnPosition;

            var instance = _factory.Create(
                _raid.Enemies.TotalCount,
                presentation.GameplayPrefabId,
                spawnPos);
            
            instance.transform.parent = _instanceRoot;
            instance.UniqueId = runtime.Id;

            _visualAssembler.Apply(instance, presentation);

            var binder = new ZombieSceneBinder(runtime);
            binder.Attach(instance);
            _binders.Add(runtime.Id, binder);

            ServiceLocator.Current.Get<UnitSceneRepository>().Register(runtime.Id, instance);
            _repository.Register(runtime.Id);

            // Уведомляем UI-системы о появлении врага на сцене.
            // Lifecycle не знает кто подписан — только бросает факт.
            // EnemyHealthBarSystem подписан и зарегистрирует Transform.
            EventBus<EnemySceneCreatedEvent>.Raise(new EnemySceneCreatedEvent(runtime.Id, instance.UIAnchor));

            EntityFactory.LoadDataAndActivateEntity<EnemyInstance>(null, instance.gameObject);

#if UNITY_EDITOR
            DLog.Alert($"[EnemySceneLifecycle] Заспавнен {runtime.Id} | PrefabId={presentation.GameplayPrefabId}",
                EDlogColor.YELLOW);
#endif
        }

        // ── ОТМЕНА РЕГИСТРАЦИИ ────────────────────────────────────────

        private void HandleZombieUnregistered(string unitId)
        {
            // Уведомляем до фактического удаления —
            // подписчики ещё могут обратиться к объекту если нужно.
            EventBus<EnemySceneDestroyedEvent>.Raise(new EnemySceneDestroyedEvent(unitId));

            if (_binders.TryGetValue(unitId, out var binder))
            {
                binder.Dispose();
                _binders.Remove(unitId);
            }

            var rep = _repository.TryGet(unitId);
            if (!rep.done) return;

            ServiceLocator.Current.Get<UnitSceneRepository>().Unregister(unitId, rep.instance);
            _repository.Unregister(unitId);
            rep.instance.Entity_Destroy();

#if UNITY_EDITOR
            DLog.Alert($"[EnemySceneLifecycle] Деспавнен {unitId}", EDlogColor.YELLOW);
#endif
        }

        // ── Dispose ───────────────────────────────────────────────────

        public void Dispose()
        {
            _raid.Enemies.OnRegistered -= HandleZombieRegistered;
            _raid.Enemies.OnUnregistered -= HandleZombieUnregistered;

            foreach (var pair in _binders)
                pair.Value.Dispose();

            _binders.Clear();
        }
    }
}