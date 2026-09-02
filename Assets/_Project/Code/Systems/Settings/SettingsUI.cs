
using System.Collections.Generic;
using DEV;
using Galactic1.Mobile;
using Galactic1.Systems.Privacy;
using Galactic1.UI.Core;
using GoogleMobileAds.Ump.Api;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Systems
{
    /// <summary>
    /// Связывает UI-элементы с SettingsManager, включая выбор языка.
    /// </summary>
    public class SettingsUI : UIScreenPanel
    {
        
        [SerializeField] private GameObject bClose;
        [SerializeField] private SettingsHiddenWindow _hiddenWindow;
        public TextMeshProUGUI version;
        
        
        
        [Space(20)]
        [Header("Аудио")] 
        public Toggle musicToggle;
        public Slider musicSlider;
        public Toggle sfxToggle;
        public Slider sfxSlider;
        public GameObject bConsentOption;
        public GameObject bPrivacy;

        
        [Header("Графика")] 
        public TMP_Dropdown graphicsDropdown;

        [Header("Управление")] 
        public Slider sensitivitySlider;
        public Text sensitivityValueText;

        [Header("Язык")] 
        public TMP_Dropdown languageDropdown;

        // Список поддерживаемых языков
        private List<string> availableLanguages = new List<string>
            { "English", "Русский", "Español", "Deutsch", "Français" };
        
        
        
        
        public bool PanelShowed { get; private set; }


        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);
            
            DevUpdate.I.SettingsUI = this;
            
            bClose.RegisterButtonClick(ClosePanel);
            version.text = SystemRepository.AppVersion;
            
            
            // GDPR
            bConsentOption.SetActive(ConsentInformation.PrivacyOptionsRequirementStatus ==
                                     PrivacyOptionsRequirementStatus.Required);
            bConsentOption.RegisterButtonClick(() =>
                ServiceLocator.Current.Get<ConsentService>().ShowPrivacyOptions(
                    error =>
                    {
                        if (error != null)
                        {
                            ScreenProfiler.AddMessage(
                                $"GDPR : Failed to show privacy option form >> {error}".SetText(EDlogColor.ORANGE));
                        }
                    }));
            
            SetupUI();
        }


        public override void OnShow(object data = null)
        {
            base.OnShow(data);
            
            PanelShowed = true;
        }


        public void ClosePanel()
        {
            new GAME_Speed().Continue();
            gameObject.SetActive(false);
            PanelShowed = false;
        }
        
        

        private void SetupUI()
        {
            // Аудио
            musicToggle.onValueChanged.AddListener(ServiceLocator.Current.Get<SettingsManager>().SetMusicEnabled);
            //musicSlider.onValueChanged.AddListener(ServiceLocator.Current.Get<SettingsManager>().SetMusicVolume);
            sfxToggle.onValueChanged.AddListener(ServiceLocator.Current.Get<SettingsManager>().SetSFXEnabled);
            //sfxSlider.onValueChanged.AddListener(ServiceLocator.Current.Get<SettingsManager>().SetSFXVolume);

            // Графика
            //graphicsDropdown.onValueChanged.AddListener(ServiceLocator.Current.Get<SettingsManager>().SetGraphicsQuality);

            // Управление
            // sensitivitySlider.onValueChanged.AddListener(value =>
            // {
            //     ServiceLocator.Current.Get<SettingsManager>().SetControlSensitivity(value);
            //     sensitivityValueText.text = value.ToString("F1");
            // });

            // Язык
            languageDropdown.ClearOptions();
            languageDropdown.AddOptions(availableLanguages);
            string currentLang = ServiceLocator.Current.Get<SettingsManager>().GetLanguage();
            int index = availableLanguages.IndexOf(currentLang);
            languageDropdown.value = index >= 0 ? index : 0;
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);

            LoadUIState();
        }

        private void LoadUIState()
        {
            musicToggle.isOn = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
            sfxToggle.isOn = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;

            //musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            //sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

            //graphicsDropdown.value = PlayerPrefs.GetInt("GraphicsQuality", 1);
            //sensitivitySlider.value = PlayerPrefs.GetFloat("ControlSensitivity", 1f);
        }

        private void OnLanguageChanged(int index)
        {
            string lang = availableLanguages[index];
            ServiceLocator.Current.Get<SettingsManager>().SetLanguage(lang);
        }
    }


}