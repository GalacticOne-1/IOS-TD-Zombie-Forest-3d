using System;
using Galactic1.Code.AbstractFactory;
using Galactic1.Code.Gameplay.Combat.Cover;
using Galactic1.Code.Gameplay.Units;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.UI.Units.Presentation;
using UnityEngine;

namespace Galactic1.Code.Systems.Runtime.Building
{
    /// <summary>
    /// Scene adapter для любого боевого сооружения.
    ///
    /// Работает через IDamageableFacilityRuntime и поэтому
    /// одинаково подходит для:
    /// - CombatFacilityRuntime
    /// - RaidCombatFacilityRuntime
    /// </summary>
    public sealed class DamageableFacilitySceneAdapter :
        ISceneFacility,
        IUnitSceneContext,
        IDamageableSceneFacility,
        IFacilitySceneAdapter,
        IDisposable
    {
        private readonly IRaidFacilityRuntime _runtime;

        public IRaidFacilityRuntime Runtime => _runtime;

        // --------------------------------------------------------------------
        // ISceneFacility
        // --------------------------------------------------------------------

        public string Id => RuntimeBase.Id;
        public FacilityType Type => _runtime.Type;

        public Vector2Int Position => _runtime.Position;

        public int Rotation => _runtime.Rotation;

        public event Action<Vector2Int> OnPositionChanged;

        public event Action<int> OnRotationChanged;
        
        public event Action OnStateChanged;

        // --------------------------------------------------------------------
        // IUnitSceneContext
        // --------------------------------------------------------------------

        public IUnitRuntimeBase RuntimeBase => _runtime;

        public IUnitStatsScene Stats { get; }

        public UnitCoverState Cover => UnitCoverState.None_;

        // --------------------------------------------------------------------
        // IDamageableSceneFacility
        // --------------------------------------------------------------------

        public float CurrentHP => _runtime.CurrentHP;

        public float MaxHP => _runtime.MaxHP;

        public event Action<float, float> OnHealthChanged;

        public event Action OnDestroyed;

        // --------------------------------------------------------------------

        public DamageableFacilitySceneAdapter(IRaidFacilityRuntime runtime)
        {
            _runtime = runtime;
            
            Stats = new UnitStatsSceneAdapter(runtime.Stats);

            _runtime.OnPositionChanged += RuntimePositionChanged;
            _runtime.OnRotationChanged += RuntimeRotationChanged;
            _runtime.OnHealthChanged += RuntimeHealthChanged;
            _runtime.OnDestroyed += RuntimeDestroyed;
        }

        public void Dispose()
        {
            _runtime.OnPositionChanged -= RuntimePositionChanged;
            _runtime.OnRotationChanged -= RuntimeRotationChanged;
            _runtime.OnHealthChanged -= RuntimeHealthChanged;
            _runtime.OnDestroyed -= RuntimeDestroyed;

            Stats.Dispose();
        }

        void ISceneEntityRuntime.Dispose()
        {
            Dispose();
        }

        // --------------------------------------------------------------------
        // Runtime -> Scene
        // --------------------------------------------------------------------

        private void RuntimePositionChanged(Vector2Int cell)
        {
            OnPositionChanged?.Invoke(cell);
        }

        private void RuntimeRotationChanged(int rotation)
        {
            OnRotationChanged?.Invoke(rotation);
        }

        private void RuntimeHealthChanged(float currentHp, float maxHp)
        {
            OnHealthChanged?.Invoke(currentHp, maxHp);
        }

        private void RuntimeDestroyed()
        {
            OnDestroyed?.Invoke();
        }

        // --------------------------------------------------------------------
        // Scene -> Runtime
        // --------------------------------------------------------------------

        public void SetPosition(Vector2Int cell)
        {
            _runtime.SetPosition(cell);
        }

        public void SetRotation(int rotation)
        {
            _runtime.SetRotation(rotation);
        }
    }
}