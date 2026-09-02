
using System;
using UnityEngine;

namespace Galactic1.Gameplay.Interaction
{
    /// <summary>
    /// Интерфейс объектов, на которые можно навести при атаке (враги, боссы).
    /// </summary>
    public interface ITargetable
    {
        float Health { get; }
        float MaxHealth { get; }
        bool IsAlive { get; }
        
        void ReceiveAttack(Transform attacker);
        event Action<float> OnHealthChanged;
        event Action OnDied;
    }
}