
using System;
using Galactic1.AbstractFactory;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Damage
{
    /*
     *     Компонент для добавления на сущность
     */

    public class TargetPriorityComponent : MonoBehaviour
    {
        [Tooltip("Профиль приоритета, который определяет формулу расчёта.")]
        public TargetPriorityProfile Profile;

        [Tooltip("Является ли этот враг боссом?")]
        public bool IsBoss;

        [Tooltip("Является ли этот враг элитным?")]
        public bool IsElite;

        public bool IsCarryingBomb { get; private set; }

        //private _HP health;
        private Transform cachedTransform;

        void Awake()
        {
            cachedTransform = transform;
            //health = GetComponent<_HP>();
        }

        public void SetCarryingBomb(bool carrying)
        {
            IsCarryingBomb = carrying;
        }

        public float GetPriority(Vector3 origin)
        {
            if (Profile == null) return 0;

            float priority = Profile.BasePriority;

            // Расстояние (нормализуем)
            float dist = Vector3.Distance(origin, cachedTransform.position);
            float normDist = Mathf.Clamp01(dist / Profile.MaxDistance);
            priority += Profile.DistanceWeight * normDist;

            // Здоровье
            // if (health != null)
            // {
            //     float healthPercent = health.CurrentHealth / health.MaxHealth;
            //     priority += Profile.HealthWeight * healthPercent;
            // }

            // Бонусы за роль
            if (IsBoss) priority += Profile.BossBonus * Profile.RoleWeightMultiplier;
            if (IsElite) priority += Profile.EliteBonus * Profile.RoleWeightMultiplier;
            if (IsCarryingBomb) priority += Profile.CarryingBombBonus;

            return priority;
        }
        
        public float GetPriorityWithOverride(Vector3 origin, TargetPriorityProfile overrideProfile)
        {
            if (overrideProfile == null) return GetPriority(origin);

            float priority = overrideProfile.BasePriority;

            float dist = Vector3.Distance(origin, transform.position);
            float normDist = Mathf.Clamp01(dist / overrideProfile.MaxDistance);
            priority += overrideProfile.DistanceWeight * normDist;

            // if (health != null)
            // {
            //     float healthPercent = health.CurrentHealth / health.MaxHealth;
            //     priority += overrideProfile.HealthWeight * healthPercent;
            // }

            if (IsBoss) priority += overrideProfile.BossBonus * overrideProfile.RoleWeightMultiplier;
            if (IsElite) priority += overrideProfile.EliteBonus * overrideProfile.RoleWeightMultiplier;
            if (IsCarryingBomb) priority += overrideProfile.CarryingBombBonus;

            return priority;
        }

    }


}