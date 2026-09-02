using Galactic1.Code.Gameplay.Effect;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Gameplay.Units
{
    public sealed class AbilityCommand : IUnitCommand
    {
        public UnitStateId TargetState => UnitStateId.UsingAbility;

        public ItemUseContext Context { get; }

        public AbilityCommand(ItemUseContext ctx)
        {
            Context = ctx;
        }

        public bool CanExecute(UnitStateId s) =>
            s == UnitStateId.Idle ||
            s == UnitStateId.SquadMoving ||
            s == UnitStateId.Engaging ||
            s == UnitStateId.MeleeEngaging;
    }
}