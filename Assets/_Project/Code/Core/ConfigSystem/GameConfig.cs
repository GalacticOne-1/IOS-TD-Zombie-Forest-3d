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

        [field: SerializeField] public CGeneral General { get; private set; }
        
        public bool SetRequiresDeviceSetup
        {
            set
            {
                var general = General;
                general.requiresDeviceSetup = value;
                General = general;
            }
        }

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

        [field: SerializeField] public CAd Ad { get; private set; }
        
        #endregion


        
        [Serializable]
        private class Wrapper
        {
            public CGeneral general;
            public CIOS ios;
            public CAd ad;
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
                    Ad = wrapper.ad;
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
        
        

        public void Save()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

    }
    
    
    [Serializable]
    public struct CGeneral
    {
        public bool requiresDeviceSetup;            // подгон канвасов под айпад
        public bool tutorialActive;
        public bool alwaysShowPayOffer;             // экраны от магазина и рекламы в начале сцены
    }
    
    [Serializable]
    public struct CIOS
    {
        public byte statusATT { set; get; }         // то что выбрал пользователь
    }
    
    [Serializable]
    public struct CAd
    {
        public byte dailyLimit;                     // available ad per day
        public byte interDelayTimer;                // pause between show 
        public byte interRequiresGames;             // requires for show interstitial
            
    }
    
}