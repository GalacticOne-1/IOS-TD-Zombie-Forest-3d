using System.Collections.Generic;
using System.Linq;
using Gameplay;
using Unity.Collections;
using UnityEngine;

namespace Galactic1.Configs
{
    public class UnlockService : IGameService
    {
        private ProgressionConfig _progressionConfig;
        private ConfigProvider _configManager;

        /// <summary>
        /// Инициализация. Нужно вызвать один раз при старте игры.
        /// </summary>
        public UnlockService(ProgressionConfig config)
        {
            _progressionConfig = config;
            _configManager = ServiceLocator.Current.Get<ConfigProvider>();
        }

        // --------------------------
        // КУМУЛЯТИВНЫЕ РАЗБЛОКИРОВКИ
        // --------------------------

        /// <summary>
        /// All unlocked
        /// </summary>
        /// <param name="playerLevel"></param>
        /// <returns></returns>
        // public List<BuildConfig> GetUnlockedBuilds(int playerLevel)
        // {
        //     var unlocked = new List<BuildConfig>();
        //     foreach (var p in _progressionConfig.progress)
        //     {
        //         if (playerLevel >= p.level)
        //         {
        //             foreach (var id in p.unlock_builds)
        //             {
        //                 var config = _configManager.Builds.Get(id);
        //                 if (config != null) unlocked.Add(config);
        //             }
        //         }
        //     }
        //     return unlocked;
        // }
        
        /// <summary>
        /// All unlocked
        /// </summary>
        /// <param name="playerLevel"></param>
        /// <returns></returns>
        // public List<EquipmentConfig> GetUnlockedEquipments(int playerLevel)
        // {
        //     var unlocked = new List<EquipmentConfig>();
        //     foreach (var p in _progressionConfig.progress)
        //     {
        //         if (playerLevel >= p.level)
        //         {
        //             foreach (var id in p.unlock_equipments)
        //             {
        //                 var config = _configManager.Equipments.Get(id);
        //                 if (config != null) unlocked.Add(config);
        //             }
        //         }
        //     }
        //     return unlocked;
        // }
        
        
        /// <summary>
        /// Проверяет, разблокирован ли конфиг для текущего уровня.
        /// </summary>
        // public bool IsUnlocked(int playerLevel, string configId)
        // {
        //     if (DeveloperConsole.I.game.unlock_all_assets) return true;
        //     
        //     // проверяем все уровни до currentLevel включительно
        //     var l = _progressionConfig.progress.Count;
        //     for (int i = 0; i < l; i++)
        //     {
        //         var progress = _progressionConfig.progress[i];
        //         if (progress.level > playerLevel)
        //             continue;
        //
        //         if (ArrayContains(progress.unlock_builds, configId)) return true;
        //         if (ArrayContains(progress.unlock_weapons, configId)) return true;
        //         if (ArrayContains(progress.unlock_equipments, configId)) return true;
        //     }
        //
        //     return false;
        // }
        
        // private bool ArrayContains(List<string> array, string id)
        // {
        //     if (array == null) return false;
        //     foreach (var a in array)
        //     {
        //         if (a == id)
        //             return true;
        //     }
        //     return false;
        // }


        // --------------------------
        // РАЗБЛОКИРОВКИ ТОЛЬКО ДЛЯ ТЕКУЩЕГО УРОВНЯ
        // --------------------------

        /// <summary>
        /// Unlocked for current level
        /// </summary>
        /// <param name="playerLevel"></param>
        /// <returns></returns>
        // public List<BuildConfig> GetNewBuildsAtLevel(int playerLevel)
        // {
        //     var levelData = _progressionConfig.progress.Find(l => l.level == playerLevel);
        //     var result = new List<BuildConfig>();
        //     if (levelData != null)
        //     {
        //         foreach (var configId in levelData.unlock_builds)
        //         {
        //             var config = _configManager.Builds.Get(configId);
        //             var unlocked = ListSaver.Get<ObjectEntry<bool>, bool>(configId, ref new SAVE().DataGameplay().structures);
        //             
        //             if (config != null && !unlocked)
        //             {
        //                 result.Add(config);
        //             }
        //         }
        //     }
        //     return result;
        // }
        
        
        // --------------------------
        // НАГРАДЫ
        // --------------------------
        public (int coins, string chest) GetLevelRewards(int playerLevel)
        {
            var levelData = _progressionConfig.progress.Find(l => l.level == playerLevel);
            if (levelData != null)
                return (levelData.rewardCoins, levelData.rewardChest);
            return (0, null);
        }

        /// <summary>
        /// Вернуть всё новое содержимое и награды на конкретном уровне.
        /// </summary>
        // public UnlockedContent GetLevelUnlocks(int playerLevel)
        // {
        //     var levelData = _progressionConfig.progress.Find(l => l.level == playerLevel);
        //     if (levelData == null) return null;
        //
        //     return new UnlockedContent
        //     {
        //         towers = new List<string>(levelData.unlock_builds),
        //         weapons = new List<string>(levelData.unlock_weapons),
        //         features = new List<string>(levelData.unlock_features),
        //         rewardCoins = levelData.rewardCoins,
        //         rewardChest = levelData.rewardChest
        //     };
        // }

        
        // --------------------------
        // СЛЕДУЮЩИЙ УРОВЕНЬ ПРОГРЕССИИ
        // --------------------------
        // public ProgressLevelData GetNextUnlock(int playerLevel)
        // {
        //     foreach (var level in _progressionConfig.progress)
        //     {
        //         if (level.level > playerLevel)
        //             return level;
        //     }
        //     return null;
        // }


        /// <summary>
        /// true - для уровня доступен новый выживший
        /// </summary>
        /// <param name="playerLevel"></param>
        /// <returns></returns>
        // public bool UnlockedSurvivor(int playerLevel)
        // {
        //     var inLocation = playerLevel == GAME.DataGameplay().MapData[GAME.DataGameplay().CurrentMapData].CurrentWaveIndex;
        //     if(inLocation)
        //     {
        //         var levelData = _progressionConfig.progress.Find(l => l.level == GAME.CurrentWave + GAME.DataGameplay().CurrentMapData);
        //         if (levelData != null)
        //             return levelData.new_survivor;
        //     }
        //    
        //     return GAME.CurrentWave.EachNumber(5);
        // }
        
    }

    public class UnlockedContent
    {
        public List<string> towers;
        public List<string> weapons;
        public List<string> features;
        public int rewardCoins;
        public string rewardChest;
    }

}