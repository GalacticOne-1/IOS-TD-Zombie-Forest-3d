using System;
using Galactic1.Code.AbstractFactory;
using Galactic1.Game.Runtime.Production;
using UnityEngine;

namespace Galactic1.Code.Systems.Runtime.Building
{
    /// <summary>
    /// Адаптер между Runtime здания и Scene объектом.
    /// </summary>
    public sealed class SceneFacilityAdapter : ISceneFacility, IDisposable
    {
        private readonly IFacilityRuntime _runtime;

        public IRaidFacilityRuntime Runtime => null;
        

        public string Id => _runtime.Id;
        public Vector2Int Position => _runtime.Position;
        public int Rotation => _runtime.Rotation;

        public event Action<Vector2Int> OnPositionChanged;
        public event Action<int> OnRotationChanged;
        
        

        public SceneFacilityAdapter(IFacilityRuntime runtime)
        {
            _runtime = runtime;

            _runtime.OnPositionChanged += RuntimePositionChanged;
            _runtime.OnRotationChanged += RuntimeRotationChanged;
        }
        
        public void Dispose()
        {
            _runtime.OnPositionChanged -= RuntimePositionChanged;
            _runtime.OnRotationChanged -= RuntimeRotationChanged;
        }

        private void RuntimePositionChanged(Vector2Int cell)
        {
            OnPositionChanged?.Invoke(cell);
        }
        
        private void RuntimeRotationChanged(int rotation)
        {
            OnRotationChanged?.Invoke(rotation);
        }
        
        
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