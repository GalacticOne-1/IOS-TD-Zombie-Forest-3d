using UnityEngine;

namespace Galactic1.Code.Gameplay.AoE
{
    /// <summary>
    /// Горящая зона (Molotov / fire patch)
    /// </summary>
    [CreateAssetMenu(fileName = "FireAoEConfig", menuName = "Game Configs/Inventory/Fire AoE Config")]
    public sealed class FireAoEConfig : AreaEffectConfig
    {
        [Header("Fire Specific")]
        public float burnDamageMultiplier = 1f;
    }
}