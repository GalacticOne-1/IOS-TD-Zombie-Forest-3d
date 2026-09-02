using Galactic1.Code.Gameplay.Weapons.Logic;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Weapons.Infrastructure
{
    [CreateAssetMenu(fileName = "MeleeDef_", menuName = "Game Configs/Inventory/Melee Definition")]
    public sealed class MeleeDefinition : ScriptableObject
    {
        public FireMode fireMode;

        [Header("Damage")] 
        public float damage = 25f;
        public float damageVariance = 0.1f;
        public float range = 30f;
        public float armorPiercing = 0f;

        [Header("Fire Rate")] 
        public float roundsPerMinute = 600f;

        [Header("Ammo")] 
        public int clipSize = 30;
        public float reloadTimeSec = 2.2f;


        [Header("View")]
        public string muzzleFlashKey = "FX_MuzzleFlash";

        public AudioClip fireSfx;
        public AudioClip emptySfx;

        // Кэш — конвертация происходит один раз
        private WeaponDefinitionData _cached;

        public WeaponDefinitionData ToData()
        {
            return _cached ??= new WeaponDefinitionData
            {
                FireMode = fireMode,
                Damage = damage,
                DamageVariance = damageVariance,
                Range = range,
                ArmorPiercing = armorPiercing,
                RoundsPerMinute = roundsPerMinute,
                MagazineSize = clipSize,
                ReloadTimeSec = reloadTimeSec,
            };
        }
    }
}