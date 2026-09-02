using System;
using Galactic1.Code.AbstractFactory;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.Meta;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Game.Runtime.Production
{
    public interface IFacilityRuntime : ISceneEntityRuntime
    {
        string ConfigId { get; }
        FacilityModule Config {get; }
        
        FacilityType Type { get; }
        int Level { get; }
        Vector2Int Position { get; }
        int Rotation { get; }
        
        event Action OnStateChanged;
        event Action<Vector2Int> OnPositionChanged;
        event Action<int> OnRotationChanged;
        void SetPosition(Vector2Int cell);
        void SetRotation(int rotation);
        
        int FacilityLimit { get; }

        FacilityUpgradeConfig GetUpgrade(int toLevel);
        void Upgrade();
    }
}