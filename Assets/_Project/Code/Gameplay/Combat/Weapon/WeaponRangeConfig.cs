using Galactic1.Core.Enums;

namespace Galactic1.Code.Gameplay.Weapons.Infrastructure
{
    /// <summary>
    /// Хардкодированные параметры дальности по классу оружия.
    /// Редактируются здесь — не в каждом SO отдельно.
    /// </summary>
    public static class WeaponRangeConfig
    {
        private readonly struct RangeProfile
        {
            public readonly float EffectiveRange;
            public readonly float MaxRange;
            public readonly float MaxSpreadPenalty;

            public RangeProfile(float effective, float max, float penalty)
            {
                EffectiveRange = effective;
                MaxRange = max;
                MaxSpreadPenalty = penalty;
            }
        }

        private static readonly System.Collections.Generic.Dictionary<WeaponType, RangeProfile>
            Profiles = new()
            {
                { WeaponType.Shotgun, new RangeProfile(5, 15, 2f) },
                { WeaponType.Pistol, new RangeProfile(7, 15, 2.5f) },
                { WeaponType.SMG, new RangeProfile(9, 15, 2) },
                { WeaponType.AR, new RangeProfile(10, 15, 1.7f) },
                { WeaponType.DMR, new RangeProfile(13, 15, 1.4f) },
                { WeaponType.LMG, new RangeProfile(9, 15, 2) },
                { WeaponType.SniperRifle, new RangeProfile(14, 15, 1.2f) },
            };

        private static readonly RangeProfile Fallback = new(35f, 80f, 3.0f);

        public static float GetEffectiveRange(WeaponType type) => Get(type).EffectiveRange;
        public static float GetMaxRange(WeaponType type) => Get(type).MaxRange;
        public static float GetMaxSpreadPenalty(WeaponType type) => Get(type).MaxSpreadPenalty;

        private static RangeProfile Get(WeaponType type)
            => Profiles.TryGetValue(type, out var p) ? p : Fallback;
    }
}