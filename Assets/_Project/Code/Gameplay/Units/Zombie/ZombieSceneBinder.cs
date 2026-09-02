using System;
using Galactic1.Code.Gameplay.Units.Zombie;
using Galactic1.Code.Systems.Runtime.Enemy;
using Galactic1.Gameplay.Player;

namespace Galactic1.Code.Systems.Raid.Enemies
{
    /// <summary>
    /// Lifecycle binder:
    /// ZombieRuntime <-> ZombieInstance
    /// </summary>
    public sealed class ZombieSceneBinder : IDisposable
    {
        public EnemyRuntime Runtime { get; }

        public EnemyInstance Instance { get; private set; }

        private EnemySceneAdapter _sceneAdapter;

        public bool IsSpawned => Instance != null;

        public ZombieSceneBinder(EnemyRuntime runtime)
        {
            Runtime = runtime;
        }

        public void Attach(EnemyInstance instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (IsSpawned)
            {
                throw new InvalidOperationException(
                    $"[{Runtime.Id}] already attached.");
            }

            Instance = instance;

            Bind();
        }

        private void Bind()
        {
            _sceneAdapter = new EnemySceneAdapter(Runtime);
            
            _sceneAdapter.OnDeath += HandleDeath;

            Instance.Bind(_sceneAdapter);

            
            var loadData = new EnemyLoadData
            {
                Runtime = Runtime
            };

            Instance.Entity_Setup(loadData);

        }
        
        private void HandleDeath()
        {
            Instance?.HandleDeath();
        }


        private void Unbind()
        {

            if (_sceneAdapter != null)
            {
                _sceneAdapter.OnDeath -= HandleDeath;
            }
            
            _sceneAdapter?.Dispose();
            _sceneAdapter = null;
        }

        public void Despawn()
        {
            if (!IsSpawned)
                return;

            var instance = Instance;

            Unbind();

            Instance = null;

            instance?.Entity_Destroy();
        }

        public void Dispose()
        {
            Despawn();
        }
    }
}