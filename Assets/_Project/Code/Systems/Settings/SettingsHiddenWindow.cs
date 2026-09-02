using System;
using Galactic1.Configs;
using Galactic1.Mobile;
using Galactic1.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Systems
{
    public class SettingsHiddenWindow : MonoBehaviour
    {
        [SerializeField] private GameObject bOpen;
        [SerializeField] private GameObject panel, bResetData;
        [SerializeField] private GameObject adStatus, adLimit;


        private void Awake()
        {
            bOpen.EventBtn_old(ShowHiddenPanel);
            panel.SetActive(false);
        }


        public void ShowHiddenPanel()
        {
            panel.SetActive(true);
            
            bResetData.EventBtnOne_old(ResetGameData);

            // ad work
            //adStatus.GetComponent<TMP_Text>().text =
               // ServiceLocator.Current.Get<AdController>().AD_AVAILABLE() ? "AD Status [ON]" : "AD Status [OFF]";
            //adStatus.GetChild(1).GetComponent<Image>().color =
               // ServiceLocator.Current.Get<AdController>().AD_AVAILABLE() ? Color.green : Color.red;

            // ad limit
            adLimit.GetComponent<TMP_Text>().text =
                $"AD Daily Limit {ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.ADState.Value.RemainingLimit}/{ServiceLocator.Current.Get<ConfigProvider>().Get<GameConfig>().Ad.dailyLimit}";
        }
        
        /// <summary>
        /// Сброс всей дата в игре
        /// </summary>
        void ResetGameData()
        {
            //SaveManagement.I.ClearSaveData();
            Application.Quit();
        }
        

        
    }
}