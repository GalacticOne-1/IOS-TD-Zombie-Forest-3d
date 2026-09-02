using UnityEngine;

namespace Galactic1.Code.Gameplay.Weapons.Logic
{
    /// <summary>
    /// Боевой контекст выстрела — передаётся от AI/Targeting в оружие.
    ///
    /// Оружие не знает кто цель, как она выбрана и как считается дистанция.
    /// Оно знает только то, что здесь — "стреляем на 42 метра".
    ///
    /// Расширяется без изменения сигнатур по всей системе:
    ///   TargetDistance, Visibility, TargetCover, LightLevel, WeatherPenalty...
    /// </summary>
    public readonly struct FireContext
    {
        public readonly float TargetDistance;
        public readonly Vector3 TargetAimPoint;

        // Будущие поля — добавляются сюда, сигнатуры Execute/RequestFire не меняются:
        // public readonly float Visibility;
        // public readonly float TargetCover;
        // public readonly float LightLevel;
        // public readonly float WeatherPenalty;

        public FireContext(
            float targetDistance,
            Vector3 targetAimPoint)
        {
            TargetDistance = targetDistance;
            TargetAimPoint = targetAimPoint;
        }
    }
}