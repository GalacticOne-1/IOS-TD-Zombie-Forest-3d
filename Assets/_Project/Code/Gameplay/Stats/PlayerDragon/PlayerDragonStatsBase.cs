using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1.Code.Gameplay.Units.Stats
{
    [CreateAssetMenu(fileName = "DragonStatsBase", menuName = "Game Configs/Player/Dragon Stats Base")]
    public class PlayerDragonStatsBase : CharacterStatsBase
    {
        
        [Serializable]
        public struct CBaseStats
        {
            public float level;
            public int experience;
            
            public float health;
            public float hunger;
            public float thirst;
            public float hungerDecay;
            public float thirstDecay;
            public float move_speed;
            
            public float damage;
            public float armor;
            public float attack_speed;
        }

        [field: SerializeField] public CBaseStats BaseStats { get; private set; }
        
        
        
        [Serializable]
        private class Wrapper
        {
            public CBaseStats base_stats;
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
            result[StatId.HungerDecay] = BaseStats.hungerDecay;
            result[StatId.ThirstDecay] = BaseStats.thirstDecay;
            
            result[StatId.MoveSpeed] = BaseStats.move_speed;
            
            result[StatId.Damage] = BaseStats.damage;
            result[StatId.Armor] = BaseStats.armor;
            result[StatId.ReloadSpeed] = BaseStats.attack_speed;

            return result;
        }
    }
}