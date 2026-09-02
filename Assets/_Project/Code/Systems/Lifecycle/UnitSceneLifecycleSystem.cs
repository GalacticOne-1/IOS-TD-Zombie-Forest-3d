
using System;
using System.Collections.Generic;
using Galactic1.AbstractFactory;
using Galactic1.Code.Cameras;
using Galactic1.Code.Gameplay.Animation;
using Galactic1.Code.Gameplay.Survivors.Repositories;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Gameplay.Units.Repositories;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.Systems.Raid.Survivors;
using Galactic1.Code.UI.Units;
using Galactic1.Configs;
using Galactic1.Core.GameSession;
using Galactic1.Core.Systems.Factories;
using Galactic1.Meta.Configs.Recruitment;

namespace Galactic1.Code.Systems.Lifecycle
{
    /// <summary>
    /// UnitSceneLifecycleSystem
    /// ------------------------------------------------------------
    /// МОСТ между доменным слоем (GameLoopContext) и сценой.
    ///
    /// Отвечает ТОЛЬКО за визуальное существование юнитов:
    /// - Спавнит scene-инстансы при создании юнита в домене
    /// - Удаляет scene-инстансы при удалении юнита из домена
    ///
    /// НЕ содержит игровой логики.
    /// НЕ принимает решений о жизни/смерти юнитов.
    /// </summary>
    public sealed class UnitSceneLifecycleSystem
    {
        private readonly GameLoopContext _gameLoopContext;
        private readonly PlayerFactory _factory;
        private readonly UnitIdentityPoolConfig _identityConfig;
        private readonly WeaponAnimLibrary _animLibrary;
        private readonly SurvivorRepository _repository;
        private readonly CameraTargetGroup _cameraGroup;
        
        private readonly Dictionary<string, SurvivorSceneBinder> _binders = new();

        private SceneSessionDefinition _session;
        private SceneUnitSource _mode;
        private bool _sceneReady;

        
        

        public UnitSceneLifecycleSystem(
            GameLoopContext gameLoopContext,
            PlayerFactory factory,
            UnitIdentityPoolConfig identityConfig,
            WeaponAnimLibrary animLibrary,
            SurvivorRepository repository,
            CameraTargetGroup cameraGroup)
        {
            _gameLoopContext = gameLoopContext;
            _gameLoopContext.unitSceneLifecycleSystem = this;
            _factory = factory;
            _identityConfig = identityConfig;
            _animLibrary = animLibrary;
            _repository = repository;
            _cameraGroup = cameraGroup;
            

            EventBus<SceneServicesResetReusableEvent>.Register(new EventBinding<SceneServicesResetReusableEvent>(() =>
            {
                _sceneReady = false;
            }));
        }
        
        
        /// <summary>
        /// Вызывается SceneSessionManager когда сцена готова
        /// </summary>
        public void InitializeScene(SceneSessionDefinition session, SceneUnitSource mode)
        {
            _session = session;
            _mode = mode;
            _sceneReady = true;


            InitialSync();
        }

        
        /// <summary>
        /// Полная синхронизация сцены с доменом
        /// </summary>
        private void InitialSync()
        {
            _cameraGroup.Clear();
            _session.Survivors = new();
            _binders.Clear();
            
            foreach (var unit in GetSourceUnits())
                SpawnIfMissing(unit);
            
            // === спавн всех юнитов для защиты лагеря
            if(_gameLoopContext.CurrentRaid?.Scenario.Options.UseDefenseFacilities ?? false)
            {
                var units= _gameLoopContext.CurrentRaid.CampDefenders.Units;
                foreach (var unit in units)
                    SpawnIfMissing(unit);
            }
        }

        private IEnumerable<IUnitRuntime> GetSourceUnits()
        {
            return _mode switch
            {
                SceneUnitSource.Camp => _gameLoopContext.PlayerUnits,
                SceneUnitSource.Raid => _gameLoopContext.CurrentRaid.Squad.Units,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        
        
        /// <summary>
        /// Активация моделей на сцене
        /// </summary>
        /// <param name="mode"></param>
        public void ActivateScene()
        {
            var l = _session.Survivors.Count;
            for (int i = 0; i < l; i++)
            {
                EntityFactory.LoadDataAndActivateEntity<SurvivorInstance>(null, _session.Survivors[i].gameObject);
            }
        }

        
        

        /// <summary>
        /// Спавн scene-представления юнита
        /// </summary>
        public void HandleUnitCreated(IUnitRuntime runtime)
        {
            if (!_sceneReady) return;
            SpawnIfMissing(runtime);
        }
        
        private void SpawnIfMissing(IUnitRuntime runtime)
        {
            if (HasSceneUnit(runtime.Id))
                return;

            // 1️⃣ scene instance
            var instance = _factory.Create(
                _session.Survivors.Count,
                _session,
                _animLibrary,
                runtime,
                _identityConfig.GetSurvivorEntry(runtime.ArchetypeId)
            );
            
            var binder = new SurvivorSceneBinder(
                runtime,
                _session,
                _animLibrary);

            binder.Attach(instance);
            _binders.Add(runtime.Id, binder);
            
            // сервис фокуса камеры на отряде
            _cameraGroup.Add(instance.Tr);
            instance.OnDeath += _ => _cameraGroup.Remove(instance.Tr);
            instance.OnDestory += () => _cameraGroup.Remove(instance.Tr);


#if UNITY_EDITOR
            DLog.Alert($"[Lifecycle] Spawned scene unit: {runtime.Id}", EDlogColor.YELLOW);
#endif
        }

        /// <summary>
        /// Удаление scene-инстанса при удалении юнита из домена
        /// </summary>
        public void HandleUnitDeleted(string unitId)
        {
            if (!_sceneReady) 
                return;
            
            if (_binders.TryGetValue(unitId, out var binder))
            {
                binder.Dispose();
                _binders.Remove(unitId);
            }
            
                
            var rep = _repository.TryGet(unitId);
            if (!rep.done) 
                return;
            
            ServiceLocator.Current.Get<UnitSceneRepository>().Unregister(unitId, rep.instance);
            _repository.Unregister(unitId);
            
            rep.instance.Entity_Destroy();
            
#if UNITY_EDITOR
            DLog.Alert($"[Lifecycle] Removed scene unit: {unitId}", EDlogColor.YELLOW);
#endif
        }
        
        private bool HasSceneUnit(string unitId)
            => _binders.ContainsKey(unitId);
    }
}