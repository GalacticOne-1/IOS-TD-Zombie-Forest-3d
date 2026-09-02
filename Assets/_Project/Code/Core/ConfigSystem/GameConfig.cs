using System;
using UnityEditor;
using UnityEngine;

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

        public bool SetReview
        {
            set
            {
                var general = General;
                general.review = value;
                General = general;
            }
        }

        public bool SetTutorial
        {
            set
            {
                var general = General;
                general.tutorial = value;
                General = general;
            }
        }

        #endregion
        

        #region IOS

        [field: SerializeField] public CIOS Ios { get; private set; }
        
        public byte SetStatusATT
        {
            set
            {
                var ios = Ios;
                ios.statusATT = value;
                Ios = ios;
            }
        }

        #endregion
        
        
        #region AD

        [field: SerializeField] public CAd Ad { get; private set; }
        
        #endregion



        


        // ожидание нового запроса к серверу
        public float WaitConnect { get; private set; }
        
        
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
                
#if UNITY_EDITOR
                WaitConnect = .4f;
#else
                WaitConnect = 1f;
#endif
                
                var wrapper = JsonUtility.FromJson<Wrapper>(json);
                if (wrapper != null)
                {
                    General = wrapper.general;
                    Ios = wrapper.ios;
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
        
        
        
        #region GAMEPLAY

        [Space(50)]
        [Header("***************************************************************************************************")]
        
        [Space(20)]
        [SerializeField] private CGameplay _gameplay;
        
        
        
        #endregion


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
        public bool review;
        public bool tutorial;
        public bool alwaysShowPayOffer;             // экраны от магазина и рекламы в начале сцены
    }
    
    [Serializable]
    public struct CIOS
    {
        public bool requiresATT;
        public byte statusATT { set; get; }         // то что выбрал пользователь
    }
    
    [Serializable]
    public struct CAd
    {
        public byte dailyLimit;                     // доступная реклама в день
        public byte interDelayTimer;                // пауза в сек. между показами интера 
        public byte interRequiresGames;             // кол-во запусков лвл/битв для показа авто интера
            
    }
    
    
    
    // ***********************************  ^ STRUCTURE GAME ^  **************************************************
    // ***********************************************************************************************************
    // ***********************************************************************************************************


    [Serializable]
    public struct CGameplay
    {
        public CNewGame NEW_GAME;                           // #1 значения для новой игры
        public CRewardGameLoop REWARD_GAME_LOOP;            // #2 награда за прохождение левела/битву и пр
        public CIAP IAP_SHOP;
        public CAdShop AD_SHOP;
    }


    [Serializable]
    public struct CNewGame                          
    {
        public int soft;
        public int hard;
    }

    [Serializable]
    public struct CRewardGameLoop
    {
        public float multiplier;
    }

    [Serializable]
    public struct CIAP
    {
        public int freeMoney;
    }
    
    [Serializable]
    public struct CAdShop
    {
        public byte shopLimit;                      // лимит рекламы в магазине для одной карточки

        public int hard;                            // 50
        public int soft;                            // 22100
        public int runesLevelUp;                    // 12730
        public byte runesAscension;                 // 9
        public byte scrollHero;                     // 40
    }
    
    
    


    
}