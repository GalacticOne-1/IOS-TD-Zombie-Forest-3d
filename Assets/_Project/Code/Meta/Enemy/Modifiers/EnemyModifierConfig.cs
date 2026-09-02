using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Game.Meta.Enemy.Modifiers
{
    [CreateAssetMenu(
        fileName = "EnemyModifier",
        menuName = "Game Configs/Enemy/Enemy Modifier")]
    public sealed class EnemyModifierConfig : ScriptableObject
    {
        [Header("Identity")]
        public string ModifierId;

        public string DisplayName;

        [TextArea]
        public string Description;

        [Header("Stats")]
        public List<StatMultiplierConfig> StatMultipliers = new();

        [Header("Movement")]
        public float WalkSpeedMultiplier = 1f;

        public float RunSpeedMultiplier = 1f;

        [Header("Presentation")]
        public string ForceVariantId;

        public string PrefabIdOverride;

        public RuntimeAnimatorController AnimatorOverride;

        [Header("Flags")]
        public bool SetsEliteFlag;

        public float ThreatMultiplierBonus;
    }

    [Serializable]
    public struct StatMultiplierConfig
    {
        public StatId StatId;
        public float Multiplier;
    }
}