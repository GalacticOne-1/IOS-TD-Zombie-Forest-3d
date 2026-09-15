using System;
using TMPro;
using UnityEngine;

namespace Galactic1.UI.Core
{
    public class TaskPopup : UIPopup
    {
        [SerializeField] private TMP_Text titleText, messageText;
        [SerializeField] private GameObject closeButton, closeButton2;


        public override void OnShow(object data = null)
        {
            if (data is TaskPopupData popupData)
            {
                titleText.text = popupData.Title;
                messageText.text = popupData.Message;
                OnClosed += popupData.OnClose;
            }
            else
            {
                Debug.LogWarning("[TaskPopup] No data passed!");
            }

            closeButton.RegisterButtonClick(OnCloseClicked);
            closeButton2.RegisterButtonClick(OnCloseClicked);
        }

        private void OnCloseClicked()
        {
            OnCloseAction?.Invoke(Config.id);
        }
    }

    public class TaskPopupData
    {
        public readonly string Title;
        public readonly string Message;
        public readonly Action OnClose;

        public TaskPopupData(string title, string message, Action onClose)
        {
            Title = title;
            Message = message;
            OnClose = onClose;
        }
    }
}