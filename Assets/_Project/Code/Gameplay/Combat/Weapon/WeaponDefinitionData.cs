using Galactic1.Code.Gameplay.Audio.Weapons;
using Galactic1.Core.Enums;
using Galactic1.Game.Meta.Items;

namespace Galactic1.Code.Gameplay.Weapons.Logic
{
    // ─────────────────────────────────────────────
    //  Data transfer object — плоская структура из SO
    //  (SO конвертирует себя в это при загрузке)
    // ─────────────────────────────────────────────

    public sealed class WeaponDefinitionData
    {
        public WeaponType WeaponType;
        public FireMode FireMode;
        public FireType FireType;
        public AmmoType AmmoType;
        public AmmoDefinition SupportedAmmo;

        public float Damage;
        public int ProjectilesPerShot;
        public float DamageVariance;
        public float Range;
        public float ArmorPiercing;

        public float RoundsPerMinute;
        public int BurstCount;
        public float BurstPauseSec;

        public int MagazineSize;
        public float ReloadTimeSec;

        public bool HasHeat;
        public float HeatPerShot;
        public float HeatCoolRate;
        public float OverheatThreshold;
        public float CooldownSec;

        public bool HasSuppression;
        public float SuppressionAngle;
        public float SuppressionRange;

        public float BaseSpreadDeg;
        public float MovingSpreadMul;
        public float StressSpreadMul;

        // Система эффективной дальности.
        // До EffectiveRange — штрафов нет.
        // От EffectiveRange до MaxRange — разброс плавно растёт до MaxRangeSpreadPenalty.
        // После MaxRange — штраф больше не растёт.
        public float EffectiveRange;
        public float MaxRange;
        public float MaxRangeSpreadPenalty;
        public float MinDamageMultiplierAtMaxRange;
        
        // Трассеры.
        // 1 = каждый выстрел, 4 = каждый четвёртый, 0 или < 0 = отключены.
        public int TracerEveryNthShot;
        public float TracerPelletFraction;

        public WeaponAudioData Audio;
    }
}