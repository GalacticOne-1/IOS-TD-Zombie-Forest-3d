using System;
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Configs
{
    [CreateAssetMenu(fileName = "ProgressionConfig", menuName = "Game Configs/Gameplay/New Progression Config")]
    public class ProgressionConfig : ScriptableObject, IUpdateFromJson
    {
        [field: SerializeField] public List<ProgressLevelData> progress { get; private set; } = new();
        
        
        [Serializable]
        private class Wrapper
        {
            public List<ProgressLevelData> progress;
        }

        /// <summary>
        /// Обновить поля ScriptableObject из JSON.
        /// </summary>
        public void UpdateFromJson(string json)
        {
            try
            {
                var wrapper = JsonUtility.FromJson<Wrapper>(json);
                if (wrapper != null && wrapper.progress != null)
                {
                    progress = wrapper.progress; // перезаписываем список уровней
                }
                else
                {
                    Debug.LogWarning("⚠️ UpdateFromJson: JSON не содержит данных progress.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ UpdateFromJson error: {e.Message}");
            }
        }
    }

    [Serializable]
    public class ProgressLevelData
    {
        public int level;                       // Уровень игрока
        public int requiredXP;                  // Сколько нужно XP

        [Header("Unlocks")] 
        public bool new_survivor;
        public List<string> unlock_equipments;
        public List<string> unlock_builds;      // Башни, которые открываются
        public List<string> unlock_weapons;     // Оружие
        public List<string> unlock_features;    // UI-фичи, меню, режимы

        [Header("Rewards")]
        public int rewardCoins;                 // Базовые награды
        public string rewardChest;              // Тип сундука
    }
}