namespace Galactic1.Code.Gameplay.Units
{
    /// <summary>
    /// Единственная «умная» часть юнита — принимает решения и отдаёт команды в StateMachine.
    /// Shared-системы (FSM, Mover, Combat, Perception) знать не знают о том,
    /// кто конкретно управляет юнитом.
    ///
    /// Реализации:
    ///   PlayerCommandBrain — транслирует приказы игрока + реактивный AI отряда.
    ///   ZombieAIBrain      — роуминг, погоня, атака (Phase 3).
    ///   BossAIBrain        — фазы, угрозы, тактика (Phase 4).
    /// </summary>
    public interface IUnitBrain
    {
        /// <summary>
        /// Позволяет состояниям FSM временно отключить принятие решений мозгом.
        /// Например: SuppressedState и UsingAbilityState выключают мозг на время анимации.
        /// </summary>
        bool IsEnabled { get; set; }
        
        /// <summary>
        /// Вызывается один раз сразу после создания мозга,
        /// до первого Tick. Здесь можно кешировать ссылки на компоненты юнита.
        /// </summary>
        void Initialize(UnitInstance unit);

        /// <summary>
        /// Вызывается каждый кадр из UnitInstance.UpdateM() ПОСЛЕ StateMachine.Tick().
        /// Мозг читает состояние мира и при необходимости выдаёт команды в StateMachine.
        /// </summary>
        void Tick(float dt);

        /// <summary>
        /// Внешний приказ от игрока / SquadController.
        /// AI-мозги обычно игнорируют или реагируют на угрозу.
        /// </summary>
        void OnPlayerCommand(IUnitCommand command);

        /// <summary>
        /// Уведомление об успешной смене состояния FSM.
        /// Мозг может сбросить внутренние таймеры или разблокировать действия.
        /// </summary>
        void OnStateChanged(UnitStateId newState);

        /// <summary>
        /// Вызывается из UnitInstance.Entity_Destroy() перед разрушением объекта.
        /// Отписки, очистка ресурсов.
        /// </summary>
        void Dispose();
    }
}