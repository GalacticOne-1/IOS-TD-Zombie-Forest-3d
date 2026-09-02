using System;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.Runtime.Production;

namespace Galactic1.Code.Systems.Runtime
{
    public interface IFacilitySceneAdapter
    {
        FacilityType Type { get; }
        event Action OnStateChanged;
    }
}