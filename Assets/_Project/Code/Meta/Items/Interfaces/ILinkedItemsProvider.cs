using System.Collections.Generic;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Game.Meta.Items
{
    public interface ILinkedItemsProvider
    {
        (StatId, List<RuntimeId>) LinkedItems();
    }
}