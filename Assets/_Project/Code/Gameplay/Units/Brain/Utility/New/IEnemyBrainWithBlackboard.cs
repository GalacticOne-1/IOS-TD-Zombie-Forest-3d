namespace Galactic1.Code.Gameplay.Units.Brain.Core
{
    /// <summary>
    /// Реализуется любым брейном, у которого есть EnemyBlackboard (или наследник).
    ///
    /// Введён для EnemyInstance, которому нужен доступ к blackboard независимо
    /// от того, UtilityUnitBrain это (Raid) или SiegeUtilityBrain (Siege).
    /// UtilityUnitBrain реализует этот интерфейс без изменения тела —
    /// у него уже есть публичное свойство Blackboard.
    /// </summary>
    public interface IEnemyBrainWithBlackboard
    {
        Blackboard.EnemyBlackboard Blackboard { get; }
    }
}
