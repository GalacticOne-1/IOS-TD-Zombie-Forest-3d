
namespace Galactic1.Code.Systems.Lifecycle
{
    /// <summary>
    /// Враг деспавнен со сцены.
    ///
    /// Бросается из EnemySceneLifecycleSystem до фактического удаления объекта,
    /// чтобы подписчики успели выполнить cleanup.
    /// </summary>
    public readonly struct EnemySceneDestroyedEvent : IEvent
    {
        public readonly string UnitId;

        public EnemySceneDestroyedEvent(string unitId) => UnitId = unitId;
    }
}