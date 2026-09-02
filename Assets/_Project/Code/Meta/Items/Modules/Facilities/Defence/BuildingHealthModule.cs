using System;
using UnityEngine;

namespace Galactic1.Game.Meta.Items
{
    /// <summary>
    /// Конфигурация здоровья и разрушения здания.
    /// Отвечает только за живучесть — никакой логики атаки.
    /// </summary>
    [System.Serializable]
    public class BuildingHealthModule : ItemModule
    {
        [SerializeField] private BuildingHealthSettings settings;

        public BuildingHealthSettings Settings => settings;
    }

    [Serializable]
    public struct BuildingHealthSettings
    {
        [Header("Health")]
        public int maxHealth;
        public float armor;

        [Header("Resistances")]
        public ResistanceData resistances;

        [Header("Repair")]
        public bool canRepair;
        public float repairSpeed;

        [Header("Destruction")]
        public bool leaveRuins;
        public float collapseDelay;
    }

    [Serializable]
    public struct ResistanceData
    {
        [Tooltip("Сопротивление огню, %")]
        public float fireResistance;

        [Tooltip("Сопротивление взрывному урону, %")]
        public float explosiveResistance;

        // Легко расширяется под новые типы: ballisticResistance, energyResistance и т.д.
    }
}