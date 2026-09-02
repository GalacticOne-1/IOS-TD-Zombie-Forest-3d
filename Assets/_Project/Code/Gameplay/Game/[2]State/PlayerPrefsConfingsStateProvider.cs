using System.Collections.Generic;
using Galactic1;
using R3;
using UnityEngine;

namespace Galactic1.Core
{
    public class PlayerPrefsConfingsStateProvider : IGameSettingsStateProvider
    {
        private const string GAME_SETTINGS_STATE_KEY = nameof(GAME_SETTINGS_STATE_KEY);
        
        public GameSettingStateProxy GameSettings { get; private set; }

        private GameSettingState _gameSettingsStateOrigin;
        
        
        
        public Observable<GameSettingStateProxy> LoadGameSettings()
        {
            if (!PlayerPrefs.HasKey(GAME_SETTINGS_STATE_KEY))
            {
                GameSettings = CreateSettingsStateFromConfing();
                DLog.Alert("Game BasicSettings State created from default basicSettings", EDlogColor.YELLOW, AppConstants.show_log_core);

                SaveGameSettings();        // сохраняем состояние при первом старте аппки
            }
            
            else
            {
                // загружаем существующее состояние
                var json = PlayerPrefs.GetString(GAME_SETTINGS_STATE_KEY);
                _gameSettingsStateOrigin = JsonUtility.FromJson<GameSettingState>(json);
                GameSettings = new GameSettingStateProxy(_gameSettingsStateOrigin);
                
                DLog.Alert("Game BasicSettings State loaded", EDlogColor.YELLOW, AppConstants.show_log_core);
            }

            return Observable.Return(GameSettings);
        }

        public Observable<bool> SaveGameSettings()
        {
            var json = JsonUtility.ToJson(_gameSettingsStateOrigin, true);
            PlayerPrefs.SetString(GAME_SETTINGS_STATE_KEY, json);

            return Observable.Return(true);
        }

        public Observable<bool> ResetGameSettings()
        {
            GameSettings = CreateSettingsStateFromConfing();
            SaveGameSettings();        
            
            return Observable.Return(true);
        }
        
        
        /// <summary>
        /// Создание базового сотояния из конфига
        /// </summary>
        /// <returns></returns>
        GameSettingStateProxy CreateSettingsStateFromConfing()
        {
            // фейк состояние для тестов
            _gameSettingsStateOrigin = new GameSettingState
            {
                
            };

            return new GameSettingStateProxy(_gameSettingsStateOrigin);
        }
    }
}