using UnityEngine;

namespace Galactic1.Code.GameDatabase.Registries
{
    /// <summary>
    /// Type-safe identity asset for a single unlockable piece of content
    /// (a location, a facility, or any future unlockable).
    /// </summary>
    [CreateAssetMenu(fileName = "UnlockId", menuName = "Game Configs/IDs/Unlock Id")]
    public sealed class UnlockId : RuntimeId {}
}
