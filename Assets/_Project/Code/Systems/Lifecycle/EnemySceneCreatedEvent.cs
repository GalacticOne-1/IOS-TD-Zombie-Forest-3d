using UnityEngine;

namespace Galactic1.Code.Systems.Lifecycle
{
    /// <summary>
    /// Враг заспавнен на сцене и готов к взаимодействию с UI-системами.
    ///
    /// Бросается из EnemySceneLifecycleSystem после binder.Attach().
    /// Позволяет UI-системам регистрировать Transform без прямой зависимости
    /// от EnemySceneLifecycleSystem.
    /// </summary>
    public readonly struct EnemySceneCreatedEvent : IEvent
    {
        public readonly string UnitId;
        public readonly Transform UIAnchor;

        public EnemySceneCreatedEvent(string unitId, Transform uiAnchor)
        {
            UnitId = unitId;
            UIAnchor = uiAnchor;
        }
    }
}
