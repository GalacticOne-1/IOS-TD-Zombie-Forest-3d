namespace Galactic1.Game.Meta.Enemy
{
    /// <summary>
    /// Все возможные типы AI action.
    /// Используется как ключ в ActionWeightEntry и AIActionDefinition.
    /// Search добавлен для SearchAction.
    /// </summary>
    public enum AIActionType
    {
        Attack,
        Chase,
        Search,
        Investigate,
        Roam,
        Flee,
        UseAbility,
        AttackWall,
        AttackHQ,
        AdvanceToHQ
    }
}