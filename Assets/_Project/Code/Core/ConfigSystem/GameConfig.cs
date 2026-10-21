using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1
{
    [CreateAssetMenu(fileName = "GameConfigs", menuName = "Game Configs/Core/Game Configs")]
    public class GameConfig : ScriptableObject
    {
        /*
         *      Настройки геймплея (сложность/награды/стоимость) в общем баланс игры
         *          - базовая установка не доступна для изменения через сервер 
         */


        #region GENERAL

        [field: SerializeField] public General General { get; private set; }
        
        public bool SetTutorial
        {
            set
            {
                var general = General;
                general.tutorialActive = value;
                General = general;
            }
        }

        #endregion
        
        
        #region AD

        [field: SerializeField] public Ad Ad { get; private set; }
        
        #endregion


        public void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

    }
    
    
    [Serializable]
    public struct General
    {
        public bool tutorialActive;
    }
    
    [Serializable]
    public struct Ad
    {
        public byte dailyLimit;                     // available ad per day
        public byte interDelayTimer;                // pause between show 
        public byte interRequiresGames;             // requires for show interstitial
            
    }
    
}