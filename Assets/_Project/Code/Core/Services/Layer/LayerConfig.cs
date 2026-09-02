using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1.Core.Gameplay
{

    [CreateAssetMenu(fileName = "LayerConfig", menuName = "Game Configs/Core/Layer Config")]
    public sealed class LayerConfig : ScriptableObject
    {
        [Header("=== Perception ===")]
        public LayerMask Detectable;
        public LayerMask DamageableAll;
        public LayerMask Occlusion;         // что блокирует LOS
        

        [Header("=== Environment ===")]
        public LayerMask Environment;       // стены, укрытия
        public LayerMask Destructible;      // бочки, двери
        public LayerMask Terrain;           // земля

        [Header("=== Combat Queries ===")]
        public LayerMask BulletHit;         // что может попасть под пулю
        public LayerMask ExplosionHit;      // что дамажит AoE


        [Header("=== Navigation / Misc ===")]
        public LayerMask Ground;
        public LayerMask Interactable;

    }
}