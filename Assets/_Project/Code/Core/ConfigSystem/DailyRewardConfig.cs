using System;
using System.Collections.Generic;
using UnityEngine;


namespace Galactic1.Configs
{
    [CreateAssetMenu(fileName = "DailyRewardConfig", menuName = "Game Configs/Economics/New Daily Reward Configs")]
    public class DailyRewardConfig : ScriptableObject, IUpdateFromJson
    {
        [field: SerializeField] public List<DailyReward> rewards { get; private set; } = new();
        
        
        [Serializable]
        private class DailyRewardWrapper
        {
            public List<DailyReward> rewards;
        }
        
        
        
        public void NewSaveData()
        {
            // GAMEPLAY_old.DataGamestat().dailyBonus = new CSaveDailyBonus[rewards.Count];
            //
            // var l = rewards.Count;
            // for (int i = 0; i < l; i++)
            // {
            //     GAMEPLAY_old.DataGamestat().dailyBonus[i] = new CSaveDailyBonus();
            // }
            //
            // GAMEPLAY_old.DataGamestat().dailyBonus[0].state = (byte)EState.COLLECT;
        }
        

        public void UpdateFromJson(string json)
        {
            // Создаём временный контейнер
            var wrapper = JsonUtility.FromJson<DailyRewardWrapper>(json);

            if (wrapper != null && wrapper.rewards != null)
            {
                rewards = wrapper.rewards;
            }
            else
            {
                Debug.LogWarning("⚠ Failed to parse DailyRewardConfig json!");
            }
        }

        
    }
    
    
    [Serializable]
    public class DailyReward
    {
        public int day;
        public string item_id;
        public string item_type;
        public int amount;
    }
}