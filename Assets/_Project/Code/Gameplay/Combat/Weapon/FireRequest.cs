
using UnityEngine;

namespace Galactic1.Code.Gameplay.Weapons.Logic
{
    public readonly struct FireRequest
    {
        public readonly float[] SpreadAngles; // x,y в градусах на каждый снаряд
        public readonly int ProjectilesCount;
        public readonly float Damage;
        public readonly float ArmorPiercing;
        public readonly FireType FireType;
        public readonly Vector3 TargetAimPoint;

        public FireRequest(
            float[] spreadAngles,
            int projectilesCount,
            float damage, 
            float armorPiercing,
            FireType fireType, 
            Vector3 targetAimPoint)
        {
            SpreadAngles = spreadAngles;
            ProjectilesCount = projectilesCount;
            Damage = damage;
            ArmorPiercing = armorPiercing;
            FireType = fireType;
            TargetAimPoint = targetAimPoint;
        }
    }
}