using Galactic1.AbstractFactory;
using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Damage
{
    /// <summary>
    /// Универсальный приёмник урона.
    /// Объединяет:
    /// - юнитов (ISceneUnit)
    /// - environment (IDamageable)
    /// </summary>
    public sealed class DamageReceiverProxy : MonoBehaviour
    {
        public ISceneEntity Entity { get; private set; }
        public IUnitSceneContext Unit { get; private set; }
        public IDamageable Damageable { get; private set; }
        
        
        public void Bind(ISceneEntity entity)
        {
            Entity = entity;

            if (entity is IDamageable damageable)
                Damageable = damageable;
        }
        
        public void Bind(IUnitSceneContext unit)
        {
            Unit = unit;
        }
        
    }
}