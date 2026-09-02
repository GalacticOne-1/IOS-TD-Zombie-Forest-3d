using System.Collections.Generic;

namespace Galactic1.Code.Systems.Runtime
{
    public interface IIdentityGenerator : IGameService
    {
        UnitIdentity Generate(IReadOnlyCollection<string> usedArchetypeIds = null);
    }
}