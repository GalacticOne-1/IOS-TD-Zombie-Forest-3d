using Galactic1.Core.Enums;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Animation
{
    [CreateAssetMenu(
        fileName = "WeaponAnimLibrary",
        menuName = "Game Configs/Player/Weapon Anim Library")]
    public sealed class WeaponAnimLibrary : ScriptableObject
    {
        [System.Serializable]
        public struct WeaponAnimEntry
        {
            public WeaponType weaponType;
            public AnimatorOverrideController overrideController;
        }

        [SerializeField] private WeaponAnimEntry[] entries;

        public AnimatorOverrideController GetController(WeaponType type)
        {
            foreach (var entry in entries)
                if (entry.weaponType == type)
                    return entry.overrideController;

            Debug.LogWarning($"[WeaponAnimLibrary] No controller for {type}, using Unarmed");
            return entries[0].overrideController;
        }
    }
}