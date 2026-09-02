namespace Galactic1.Code.Gameplay.Units
{
    public sealed class EquipWeaponCommand : IUnitCommand
    {
        public UnitStateId TargetState { get; }

        public EquipWeaponCommand(UnitStateId restoreState)
        {
            TargetState = restoreState;
        }

        // Разрешаем из любого состояния кроме смерти
        public bool CanExecute(UnitStateId s) =>
            s == UnitStateId.Idle ||
            s == UnitStateId.SquadMoving ||
            s == UnitStateId.Engaging ||
            s == UnitStateId.MeleeEngaging;
    }
}