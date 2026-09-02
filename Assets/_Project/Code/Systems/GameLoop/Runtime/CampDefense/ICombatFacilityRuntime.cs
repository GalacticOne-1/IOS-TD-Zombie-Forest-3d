using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Systems.Runtime.Building
{
    public interface ICombatFacilityRuntime
    {
        FacilityType Type { get; }
        FacilityModule Config { get; }
    }
}