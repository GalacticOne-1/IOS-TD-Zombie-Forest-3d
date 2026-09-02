
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Systems.Runtime
{
    public interface ITransportRuntime
    {
        string Id { get; }
        ItemConfig Item { get; }
        string GetPrefab();
    }
}