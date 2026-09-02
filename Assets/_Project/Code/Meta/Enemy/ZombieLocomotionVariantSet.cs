
using UnityEngine;

namespace Galactic1.Game.Meta.Enemy
{
    [CreateAssetMenu(
        fileName = "ZombieLocomotionVariantSet",
        menuName = "Game Configs/Enemy/Zombie Locomotion Variant Set")]
    public sealed class ZombieLocomotionVariantSet : ScriptableObject
    {
        public AnimatorOverrideController[] Controllers;
    }
}