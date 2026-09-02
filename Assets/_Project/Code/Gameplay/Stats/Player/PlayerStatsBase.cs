using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units.Stats
{
    [CreateAssetMenu(fileName = "PlayerStatsBase", menuName = "Game Configs/Player/Player Stats Base")]
    public class PlayerStatsBase : CharacterStatsBase
    {
        
        [Serializable]
        public struct CPlayer
        {
            public float level;
            public int experience;
            
            public float health;
            public float hunger;
            public float thirst;
            public float hungerDecay;
            public float thirstDecay;
            
            public float move_speed;
            public float jump_force;
            public float jump_force_big;
            public float wall_jump_force;
            public float wall_slide_speed;

            public float damage;
            public float armor;
            public float attack_speed;
        }

        [field: SerializeField] public CPlayer BaseStats { get; private set; }
        
        
        
        [Serializable]
        private class Wrapper
        {
            public CPlayer base_stats;
        }

        /// <summary>
        /// Обновить поля ScriptableObject из JSON.
        /// </summary>
        public void UpdateFromJson(string json)
        {
            try
            {
                var wrapper = JsonUtility.FromJson<Wrapper>(json);
                if (wrapper != null)
                {
                    BaseStats = wrapper.base_stats;
                }
                else
                {
                    Debug.LogWarning("⚠️ UpdateFromJson: JSON не содержит данных PlayerConfig.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ UpdateFromJson error: {e.Message}");
            }
        }

        public override Dictionary<StatId, float> GetBaseStats()
        {
            var result = new Dictionary<StatId, float>();
            
            result[StatId.Level] = BaseStats.level;
            result[StatId.Experience] = BaseStats.experience;
            
            result[StatId.Health] = BaseStats.health;
            result[StatId.Hunger] = BaseStats.hunger;
            result[StatId.Thirst] = BaseStats.thirst;
            //result[StatId.HungerDecay] = BaseStats.hungerDecay;
            //result[StatId.ThirstDecay] = BaseStats.thirstDecay;
            
            result[StatId.MoveSpeed] = BaseStats.move_speed;
            //result[StatId.JumpForce] = BaseStats.jump_force;
            //result[StatId.JumpForceBig] = BaseStats.jump_force_big;
            //result[StatId.WallJumpForce] = BaseStats.wall_jump_force;
            //result[StatId.WallSlideSpeed] = BaseStats.wall_slide_speed;
            
            // эти статы отображаем в инвентаре
            result[StatId.Damage] = 0;
            result[StatId.Armor] = 0;
            result[StatId.Accuracy] = 0;
            result[StatId.DamagePerSec] = 0;

            return result;
        }
    }
}