namespace Galactic1.Game.Meta.Enemy
{
    /// <summary>
    /// AI-профиль юнита. Определяет, какой брейн строит UtilityBrainFactory.
    ///
    /// Профиль задаёт СЦЕНАРИЙ рейда (IRaidScenario.AIProfile), а не архетип
    /// врага — один и тот же зомби ведёт себя как Raid AI в exploration
    /// и как Siege AI в Camp Defense.
    /// </summary>
    public enum EnemyAIProfile
    {
        Raid,
        Siege,
        Boss, // задел на будущее — UtilityBrainFactory уже готова к switch-расширению
    }
}
