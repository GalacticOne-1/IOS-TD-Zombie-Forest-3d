namespace Galactic1.Code.Gameplay.Units
{
    public sealed class AttackCommand : IUnitCommand
    {
        public UnitStateId TargetState { get; }
        public string TargetId { get; }
        

        public AttackCommand(string targetId, UnitStateId targetState = UnitStateId.Engaging)
        {
            TargetId = targetId;
            TargetState = targetState;
            DLog.Alert($"Request attack command to {targetState}");
        }

        // Атаковать можно из Idle, Moving, TakingCover, Engaging (смена цели)
        public bool CanExecute(UnitStateId s) =>
            s == UnitStateId.Idle ||
            s == UnitStateId.Engaging ||
            //s == UnitStateId.MeleeEngaging ||
            s == UnitStateId.Chasing;
    }
}