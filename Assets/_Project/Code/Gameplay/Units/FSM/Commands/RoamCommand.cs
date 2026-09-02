namespace Galactic1.Code.Gameplay.Units.Zombie
{
    /// <summary>
    /// Команда перехода в роуминг.
    /// Brain выдаёт её когда цель потеряна.
    /// </summary>
    public sealed class RoamCommand : IUnitCommand
    {
        public UnitStateId TargetState => UnitStateId.Roaming;

        public bool CanExecute(UnitStateId currentState)
        {
            // Из любого активного состояния кроме смерти
            return currentState != UnitStateId.Dying
                   && currentState != UnitStateId.Dead;
        }
    }
}