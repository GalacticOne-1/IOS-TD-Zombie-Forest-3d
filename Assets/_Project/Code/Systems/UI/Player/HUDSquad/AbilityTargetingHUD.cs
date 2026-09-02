using System;
using Galactic1.Code.UI.UnitCard;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Core.UI.HUD
{
    public sealed class AbilityTargetingHUD : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Image itemIcon;
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private GameObject cancelZoneButton;

        private Action _onCancel;
        
        
        
        public void Bind(SquadUICoordinator squadUI)
        {
            squadUI.OnTargetingStarted += OnTargetingStarted;
            squadUI.OnTargetingStopped += OnTargetingStopped;
            
            cancelZoneButton.RegisterButtonClick(() => _onCancel?.Invoke());
            
            
            
            Hide();
        }

        private void OnTargetingStarted(TargetingUIData data)
        {
            Show(data.Icon, data.ItemName, data.OnCancel);
        }

        private void OnTargetingStopped()
        {
            Hide();
        }
        

        public void Show(Sprite icon, string itemName, Action onCancel)
        {
            _onCancel = onCancel;
            itemIcon.sprite = icon;
            itemNameText.text = itemName;
            root.SetActive(true);
        }

        public void Hide()
        {
            root.SetActive(false);
            _onCancel = null;
        }


    }
}