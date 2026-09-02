using System;
using Galactic1.Code.Systems.Runtime.Building;
using UnityEngine;

namespace Galactic1.Code.AbstractFactory
{
    public interface ISceneFacility : ISceneEntityRuntime
    {
        IRaidFacilityRuntime Runtime { get; }
        
        Vector2Int Position { get; }
        int Rotation { get; }

        event Action<Vector2Int> OnPositionChanged;
        event Action<int> OnRotationChanged;
        void SetPosition(Vector2Int cell);
        void SetRotation(int rotation);
    }
}