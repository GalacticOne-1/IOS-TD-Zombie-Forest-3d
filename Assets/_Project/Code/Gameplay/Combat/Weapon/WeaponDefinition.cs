using Galactic1.Code.Gameplay.Audio.Weapons;
using Galactic1.Code.Gameplay.Weapons.Logic;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Weapons.Infrastructure
{
    [CreateAssetMenu(fileName = "WeaponDef_", menuName = "Game Configs/Inventory/Weapon Definition")]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [Header("Weapon Class")]
        [Tooltip("Класс оружия — используется для хардкода эффективной дальности через WeaponRangeConfig")]
        public WeaponType weaponType;
        
        public FireMode fireMode;
        public FireType fireType;
        public AmmoType ammoType;
        public AmmoDefinition supportedAmmo;
        public WeaponAudioConfig audio;

        [Header("Damage (урон одной пули)")] 
        public float damage = 25f;
        [Tooltip("Кол-во пуль за выстрел"), Min(1)]
        public int projectilesPerShot = 1;
        public float damageVariance = 0.1f;
        public float range = 30f;
        public float armorPiercing = 0f;

        [Header("Fire Rate")] 
        public float roundsPerMinute = 600f;
        public int burstCount = 3;
        public float burstPauseSec = 0.4f;


        [Header("Ammo")] 
        public int magazineSize = 30;
        public float reloadTimeSec = 2.2f;

        [Header("Heat")] 
        public bool hasHeat;
        public float heatPerShot = 8f;
        public float heatCoolRate = 20f;
        public float overheatThreshold = 100f;
        public float cooldownSec = 3f;

        [Header("Suppression")] 
        public bool hasSuppression;
        public float suppressionAngle = 60f;
        public float suppressionRange = 15f;


        // [Header("Effective Range")] 
        // [Tooltip("До этой дистанции штрафов к разбросу нет")]
        // public float effectiveRange = 35f;
        //
        // [Tooltip("На этой дистанции достигается максимальный штраф к разбросу. Должно быть больше effectiveRange")]
        // public float maxRange = 80f;
        //
        // [Tooltip("Максимальный множитель разброса на дистанции maxRange и дальше.\n" +
        //          "Пистолет: 5  |  SMG: 6  |  Дробовик: 8  |  Карабин/AR: 3  |  Снайперка: 1.5")]
        // public float maxRangeSpreadPenalty = 4f;
        
            
        [Header("Spread")] 
        [Tooltip("Механика точности как в играх AAA")]
        public float baseSpreadDeg = 1.5f;      // базовый разброс оружия
        public float movingSpreadMul = 2.5f;    // разброс в движении
        public float stressSpreadMul = 1.5f;    // разброс от стресса
        
        [Header("Damage Falloff")]
        [Tooltip("Multiplier at MaxRange. 1.0 = no falloff. 0.25 = 25% damage at MaxRange.")]
        [Range(0.01f, 1f)]
        public float minDamageMultiplierAtMaxRange = 1f;

        [Header("Tracers")]
        [Tooltip("Трассер спавнится каждый N-й выстрел.\n1+ = каждый, 0 = отключены.")]
        public int tracerEveryNthShot = 4;
        
        // Доля дробин/пуль, для которых спавнится визуальный трассер.
        // 1.0 = все (пистолет, винтовка), 0.5 = половина (дробовик).
        // Диапазон: 0.0 .. 1.0
        public float tracerPelletFraction = 1f;

        
        
        
        public float GetAccuracyScore()
        {
            // Определяем эталонные значения для всей игры (мин/макс разброс)
            // Например: Снайперка имеет 0.1°, Дробовик 12.0°
            const float minPossibleSpread = 0.1f; 
            const float maxPossibleSpread = 12f; 

            // Инвертируем: чем меньше Spread, тем выше Score
            float normalized = Mathf.InverseLerp(maxPossibleSpread, minPossibleSpread, baseSpreadDeg);
    
            return (int)(normalized * 100f);
        }
        
        
        

        // Кэш — конвертация происходит один раз.
        // Сбрасывается автоматически при любом изменении SO в инспекторе.
        private WeaponDefinitionData _cached;
 
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Сбрасываем кэш при каждом изменении поля в инспекторе.
            // Без этого EditorWindow и runtime видят устаревшие данные.
            _cached = null;
        }
#endif

        public WeaponDefinitionData ToData()
        {
            return _cached ??= new WeaponDefinitionData
            {
                FireMode = fireMode,
                FireType = fireType,
                AmmoType = ammoType,
                SupportedAmmo = supportedAmmo,
                Damage = damage,
                DamageVariance = damageVariance,
                Range = range,
                ArmorPiercing = armorPiercing,
                RoundsPerMinute = roundsPerMinute,
                BurstCount = burstCount,
                BurstPauseSec = burstPauseSec,
                ProjectilesPerShot = projectilesPerShot,
                MagazineSize = magazineSize,
                ReloadTimeSec = reloadTimeSec,
                HasHeat = hasHeat,
                HeatPerShot = heatPerShot,
                HeatCoolRate = heatCoolRate,
                OverheatThreshold = overheatThreshold,
                CooldownSec = cooldownSec,
                HasSuppression = hasSuppression,
                SuppressionAngle = suppressionAngle,
                SuppressionRange = suppressionRange,
                BaseSpreadDeg = baseSpreadDeg,
                MovingSpreadMul = movingSpreadMul,
                StressSpreadMul = stressSpreadMul,

                // Захардкожено по классу оружия — не берётся из SO-полей.
                EffectiveRange = WeaponRangeConfig.GetEffectiveRange(weaponType),
                MaxRange = WeaponRangeConfig.GetMaxRange(weaponType),
                MaxRangeSpreadPenalty = WeaponRangeConfig.GetMaxSpreadPenalty(weaponType),

                MinDamageMultiplierAtMaxRange = minDamageMultiplierAtMaxRange,
                TracerEveryNthShot = tracerEveryNthShot,
                TracerPelletFraction = tracerPelletFraction,
                
                // audio может быть не назначен в инспекторе — это валидная
                // конфигурация ("немое" оружие или ассет ещё не заведён).
                // WeaponCombatBridge/WeaponGunshotAudioSystem обязаны
                // обрабатывать null здесь без исключений.
                Audio = audio != null ? audio.ToData() : null,
            };
        }
    }
}