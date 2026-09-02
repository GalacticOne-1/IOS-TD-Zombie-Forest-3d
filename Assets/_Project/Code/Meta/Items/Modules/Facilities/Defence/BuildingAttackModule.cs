using System;
using Galactic1.Code.Gameplay.Weapons.Infrastructure;
using UnityEngine;

namespace Galactic1.Game.Meta.Items
{
    /// <summary>
    /// Оборонительная установка (турель), использующая существующий
    /// оружейный пайплайн: WeaponDefinition → WeaponDefinitionData → WeaponEntity →
    /// WeaponCombatBridge → WeaponFireService → Projectile → DamagePipeline.
    ///
    /// Сам модуль не хранит damage/RPM/spread/ammo — всё это уже описано
    /// в WeaponDefinition. Здесь только поведение установки как турели.
    /// </summary>
    [System.Serializable]
    public class BuildingAttackModule : ItemModule
    {
        [Header("Weapon")]
        [Tooltip("Оружие турели. Damage, RPM, spread, projectile, ammo, tracer, suppression и fire mode берутся отсюда.")]
        [SerializeField]
        private WeaponDefinition weaponDefinition;

        [SerializeField] private BuildingAttackSettings settings;

        public WeaponDefinition WeaponDefinition => weaponDefinition;
        public BuildingAttackSettings Settings => settings;
    }

    [Serializable]
    public struct BuildingAttackSettings
    {
        [Header("Rotation")] public float rotationSpeed;

        [Tooltip("Вращается ли турель в режиме ожидания (без цели)")]
        public bool idleRotation;

        [Header("Engagement")] [Tooltip("Радиус обнаружения/поражения цели турелью")]
        public float attackRadius;

        [Tooltip("Требуется ли прямая видимость до цели")]
        public bool requireLOS;

        [Tooltip("Слои, по которым турель выбирает цели")]
        public LayerMask targetMask;

        [Header("Ammo / Power")]
        [Tooltip("Требуются ли патроны из инвентаря для стрельбы (иначе — бесконечный боезапас)")]
        public bool requiresAmmo;

        public bool requiresPower;
    }
}