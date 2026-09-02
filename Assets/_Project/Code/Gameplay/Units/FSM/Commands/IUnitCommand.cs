namespace Galactic1.Code.Gameplay.Units
{
    public interface IUnitCommand
    {
        UnitStateId TargetState { get; }
        bool CanExecute(UnitStateId s);
    }
}