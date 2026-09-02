using Galactic1.Systems;
using UnityEngine;

namespace Galactic1.Systems
{

    /// <summary>
    /// Менеджер игровых настроек: звук, музыка, графика, язык, управление.
    /// Сохраняет параметры в PlayerPrefs и применяет их при старте.
    /// </summary>
    public class SettingsManager : MonoBehaviour, IGameService
    {

        // PlayerPrefs keys
        private const string MUSIC_VOLUME_KEY = "MusicVolume";
        private const string SFX_VOLUME_KEY = "SFXVolume";
        private const string MUSIC_ENABLED_KEY = "MusicEnabled";
        private const string SFX_ENABLED_KEY = "SFXEnabled";
        private const string GRAPHICS_QUALITY_KEY = "GraphicsQuality";
        private const string SENSITIVITY_KEY = "ControlSensitivity";
        private const string LANGUAGE_KEY = "Language";

        public string CurrentLanguage { get; private set; } = "English";



        
        
        
        
        
        
        
        public void Activate()
        {
            LoadSettings();
        }

        #region 🎵 Аудио

        public void SetMusicEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(MUSIC_ENABLED_KEY, enabled ? 1 : 0);
            ApplyMusicVolume();
        }

        public void SetSFXEnabled(bool enabled)
        {
            PlayerPrefs.SetInt(SFX_ENABLED_KEY, enabled ? 1 : 0);
            ApplySFXVolume();
        }

        public void SetMusicVolume(float volume)
        {
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, Mathf.Clamp01(volume));
            ApplyMusicVolume();
        }

        public void SetSFXVolume(float volume)
        {
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, Mathf.Clamp01(volume));
            ApplySFXVolume();
        }

        private void ApplyMusicVolume()
        {
            bool enabled = PlayerPrefs.GetInt(MUSIC_ENABLED_KEY, 1) == 1;
            float volume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
            ServiceLocator.Current.Get<AudioManager>()?.SetMusicVolume(enabled ? volume : 0f);
        }

        private void ApplySFXVolume()
        {
            bool enabled = PlayerPrefs.GetInt(SFX_ENABLED_KEY, 1) == 1;
            float volume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
            ServiceLocator.Current.Get<AudioManager>()?.SetSFXVolume(enabled ? volume : 0f);
        }

        #endregion

        #region 🎨 Графика

        public void SetGraphicsQuality(int index)
        {
            index = Mathf.Clamp(index, 0, QualitySettings.names.Length - 1);
            PlayerPrefs.SetInt(GRAPHICS_QUALITY_KEY, index);
            QualitySettings.SetQualityLevel(index, true);
        }

        #endregion

        #region 🎮 Управление
        
        /// <summary>
        /// Вызов вибрации
        /// </summary>
        public void Vibro()
        {
            //DLog.Alert("Vibro", "blue");
            //if(vibro) Vibration.VibratePop();
        }

        public void SetControlSensitivity(float value)
        {
            PlayerPrefs.SetFloat(SENSITIVITY_KEY, Mathf.Clamp(value, 0.1f, 5f));
        }

        public float GetControlSensitivity() => PlayerPrefs.GetFloat(SENSITIVITY_KEY, 1f);

        #endregion

        #region 🌐 Язык

        /// <summary>
        /// Устанавливает язык по названию (например, "English" или "Русский")
        /// </summary>
        public void SetLanguage(string language)
        {
            CurrentLanguage = language;
            PlayerPrefs.SetString(LANGUAGE_KEY, language);
            PlayerPrefs.Save();

            // Если используется Unity Localization, можно вызвать:
            // LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales
            //     .Locales.FirstOrDefault(l => l.name == language);

            Debug.Log($"Language changed to: {language}");
        }

        public string GetLanguage() => PlayerPrefs.GetString(LANGUAGE_KEY, Application.systemLanguage.ToString());

        #endregion

        #region 💾 Загрузка

        public void LoadSettings()
        {
            ApplyMusicVolume();
            ApplySFXVolume();

            int quality = PlayerPrefs.GetInt(GRAPHICS_QUALITY_KEY, 1);
            QualitySettings.SetQualityLevel(quality, true);

            CurrentLanguage = GetLanguage();
        }

        #endregion
    }



}