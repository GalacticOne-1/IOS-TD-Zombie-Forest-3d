using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Meta.Configs.Recruitment
{
    [CreateAssetMenu(
        fileName = "SkillConfig",
        menuName = "Game Configs/Player/SkillConfig")]
    public sealed class SkillConfig : ScriptableObject
    {
        [Header("Identity")]
        public string Id;
        public string DisplayName;
        [TextArea] public string Description;
        public Sprite Icon;

        [Header("Classification")]
        public SkillType Type;
        public SkillRarity Rarity;
        public SkillTargetType TargetType;

        [Header("Level Scaling")]
        public AnimationCurve PowerByLevel = AnimationCurve.Linear(1, 1, 10, 10);

        [Header("Cooldown")]
        public float BaseCooldown;
        public bool HasCooldown;

        [Header("Costs")]
        public int EnergyCost;
        public int ActionPointsCost;

        [Header("Tags")]
        public List<SkillTag> Tags = new();

        public float EvaluatePower(int level)
        {
            if (PowerByLevel == null)
                return 0f;

            return PowerByLevel.Evaluate(level);
        }

        public bool HasTag(SkillTag tag)
        {
            return Tags != null && Tags.Contains(tag);
        }
    }
    
    public enum SkillType
    {
        Passive,
        Active,
        Aura,
        Ultimate
    }
    
    public enum SkillRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }
    
    public enum SkillTargetType
    {
        Self,
        Ally,
        Enemy,
        Area,
        Global
    }
    
    public enum SkillTag
    {
        Damage,
        Healing,
        Buff,
        Debuff,
        Utility,
        Economy,
        Production,
        Combat,
        Support
    }
}