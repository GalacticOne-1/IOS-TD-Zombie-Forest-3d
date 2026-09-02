using System;
using Galactic1.Code.Gameplay.Animation;
using Galactic1.Code.Gameplay.Equipment;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Systems.Runtime;
using Galactic1.Core.Enums;
using Galactic1.Core.GameSession;
using Galactic1.Gameplay.Player;

namespace Galactic1.Code.Systems.Raid.Survivors
{
    /// <summary>
    /// Lifecycle binder:
    /// Runtime <-> SceneInstance
    ///
    /// Owner:
    /// - bind
    /// - setup
    /// - unbind
    /// - despawn
    /// </summary>
    public sealed class SurvivorSceneBinder : IDisposable
    {
        // ─────────────────────────────────────────────────────────────
        // Runtime
        // ─────────────────────────────────────────────────────────────

        public IUnitRuntime Runtime { get; }

        // ─────────────────────────────────────────────────────────────
        // Scene
        // ─────────────────────────────────────────────────────────────

        public SurvivorInstance Instance { get; private set; }

        private SceneUnitAdapter _sceneAdapter;

        public bool IsSpawned => Instance != null;

        // ─────────────────────────────────────────────────────────────
        // Dependencies
        // ─────────────────────────────────────────────────────────────

        private readonly SceneSessionDefinition _context;
        private readonly WeaponAnimLibrary _animLibrary;

        private ItemBrokenHandler _itemBrokenHandler;

        // ─────────────────────────────────────────────────────────────
        // Constructor
        // ─────────────────────────────────────────────────────────────

        public SurvivorSceneBinder(
            IUnitRuntime runtime,
            SceneSessionDefinition context,
            WeaponAnimLibrary animLibrary)
        {
            Runtime = runtime;

            _context = context;
            _animLibrary = animLibrary;
        }

        public void Attach(SurvivorInstance instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (IsSpawned)
            {
                throw new InvalidOperationException($"[{Runtime.Id}] already attached.");
            }

            Instance = instance;

            Bind();
        }

        private void Bind()
        {
            _sceneAdapter = new SceneUnitAdapter(Runtime);
            
            _sceneAdapter.OnDeath += HandleDeath;

            Instance.Bind(_sceneAdapter);


            var loadData = new PlayerLoadData
            {
                UnitRuntime = Runtime,
                InventoryPort = _context.InventoryPort,
                AnimLibrary = _animLibrary
            };

            Instance.Entity_Setup(loadData);

            ApplyEquipment(loadData);


            _itemBrokenHandler = new ItemBrokenHandler(
                Runtime.EquipmentService as EquipmentRuntimeService,
                () => Instance.gameObject);
        }

        private void ApplyEquipment(PlayerLoadData loadData)
        {
            Instance.GetComponent<UnitAnimationController>()
                .SetWeapon(WeaponType.Unarmed);

            ServiceLocator.Current.Get<CoroutineController>()
                .Coroutine_wait(
                    .2f,
                    () => PlayerEquipmentApplier.Apply(loadData, Instance));
        }


        private void HandleDeath()
        {
            if (Instance != null)
                Instance.HandleDeath();
        }
        
        private void Unbind()
        {
            _itemBrokenHandler?.Dispose();
            _itemBrokenHandler = null;

            //Instance?.Unbind();

            
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