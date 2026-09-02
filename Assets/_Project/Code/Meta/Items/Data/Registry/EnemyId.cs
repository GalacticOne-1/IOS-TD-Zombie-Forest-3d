using UnityEngine;

namespace Galactic1.Code.GameDatabase.Registries
{
    /// <summary>
    /// Type-safe enemy identity asset.
    /// Immutable global identifier.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyId", menuName = "Game Configs/IDs/Enemy Id")]
    public sealed class EnemyId : RuntimeId {}
}