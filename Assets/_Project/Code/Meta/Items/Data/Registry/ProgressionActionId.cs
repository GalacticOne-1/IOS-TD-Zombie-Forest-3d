using UnityEngine;

namespace Galactic1.Code.GameDatabase.Registries
{
    /// <summary>
    /// Type-safe identity asset for an XP action category
    /// (e.g. "Zombie Kill", "Loot", "Location Discovery", "Facility Built").
    /// One asset per category — NOT per individual entity.
    /// </summary>
    [CreateAssetMenu(fileName = "ProgressionActionId", menuName = "Game Configs/IDs/Progression Action Id")]
    public sealed class ProgressionActionId : RuntimeId {}
}
