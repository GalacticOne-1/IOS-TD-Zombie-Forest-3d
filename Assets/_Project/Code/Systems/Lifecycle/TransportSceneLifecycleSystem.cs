using System;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Core.GameSession;
using Galactic1.Core.Systems.Factories;
using UnityEngine;

namespace Galactic1.Code.Systems.Lifecycle
{
    /// <summary>
    /// Отвечает за создание и обновление визуальной модели транспорта.
    /// </summary>
    public class TransportSceneLifecycleSystem
    {
        private readonly GameLoopContext _gameLoopContext;
        private readonly TransportFactory _factory;
        
        private SceneSessionDefinition _session;
        private SceneUnitSource _mode;
        private bool _sceneReady;
        
        private TransportInstance instance;


        public TransportSceneLifecycleSystem(
            GameLoopContext gameLoopContext,
            TransportFactory factory)
        {
            _gameLoopContext = gameLoopContext;
            gameLoopContext.transportSceneLifecycleSystem = this;
            _factory = factory;
        }

        /// <summary>
        /// Вызывается SceneSessionManager когда сцена готова
        /// </summary>
        public void InitializeScene(SceneSessionDefinition session, SceneUnitSource mode)
        {
            _session = session;
            _mode = mode;
            _sceneReady = true;

            //_gameLoopContext.ClearDisplayUnits();

            InitialSync();
        }
        
        /// <summary>
        /// Полная синхронизация сцены с доменом
        /// </summary>
        private void InitialSync()
        {
            _session.Transport = null;

            SpawnIfMissing(GetSourceRuntime());
        }

        private ITransportRuntime GetSourceRuntime()
        {
            return _mode switch
            {
                SceneUnitSource.Camp => _gameLoopContext.PlayerTransport,
                SceneUnitSource.Raid => _gameLoopContext.PlayerTransport,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        
        

        /// <summary>
        /// Спавн scene-представления юнита
        /// </summary>
        public void HandleTransportCreated(ITransportRuntime runtime)
        {
            if (!_sceneReady) return;
            SpawnIfMissing(runtime);
        }
        
        private void SpawnIfMissing(ITransportRuntime runtime)
        {
            // 1️⃣ UI DTO
            //var displayData = new UnitDisplayData(runtime);
            //_gameLoopContext.RegisterDisplayUnit(displayData);

            // 2️⃣ scene instance
            var instance = _factory.Create(
                0,
                _session,
                runtime,
                runtime.GetPrefab()
            );
            
            
            // === отключаем коллайдеры для гаража
            if(_mode == SceneUnitSource.Camp)
            {
                var _colliders = instance.GetComponentsInChildren<Collider>(true);
                for (int i = 0; i < _colliders.Length; i++)
                    _colliders[i].enabled = false;
            }
            

#if UNITY_EDITOR
            DLog.Alert($"[Lifecycle] Spawned scene Transport: {runtime.Id}", EDlogColor.YELLOW);
#endif
        }

        /// <summary>
        /// Удаление scene-инстанса при удалении юнита из домена
        /// </summary>
        public void HandleTransportDeleted()
        {
            if (!_sceneReady) return;
                

            _session.Transport.Entity_Destroy();
            
#if UNITY_EDITOR
            DLog.Alert($"[Lifecycle] Removed scene Transport", EDlogColor.YELLOW);
#endif
        }
    }
}