namespace Galactic1.Code.Gameplay.AI.LOD
{
    /// <summary>
    /// Уровень AI-симуляции юнита, назначаемый AILODSystem.
    ///
    /// ВАЖНО: этот enum не содержит поведения — он только маркер состояния.
    /// Вся логика перехода живёт в UnitInstance (исполнение) и AILODSystem (решение).
    /// </summary>
    public enum SimulationLevel
    {
        /// <summary>Полностью заморожен: Brain, Perception, Navigation, Combat, Noise, Animator — выключены.</summary>
        Sleeping,

        /// <summary>Урезанная симуляция: Brain/Perception тикают редко, Combat/Animator выключены.</summary>
        Low,

        /// <summary>Полная симуляция — поведение как сегодня, без изменений.</summary>
        Full
    }
}