
using Galactic1.Code.Gameplay.Effect;
using Galactic1.Code.Gameplay.Units.Abstractions;
using Galactic1.Code.Gameplay.Units.Definitions;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    public interface IUnitRuntimeBase
    {
        string Id { get; }
        int TeamId { get; } // 🔥 ключ для friendly fire
        
        UnitGameplayDefinition RuntimeDefinition { get; }
        IUnitStatsRuntime Stats { get; }
        ActiveEffectsRuntime Effects { get; }
        bool IsInCombat { get; }
        Vector3 SpawnPosition { get; }
        void Tick(float dt);
        
    }
}