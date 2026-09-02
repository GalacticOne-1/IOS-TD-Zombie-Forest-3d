using System;
using TMPro;
using UnityEngine;

namespace Galactic1.UI.Core
{
    public class ConfirmPaidPopup : UIPopup
    {
        [SerializeField] private TMP_Text titleText, messageText, confirmText, costText;
        [SerializeField] private GameObject okButton;
        [SerializeField] private GameObject closeButton, closeButton2;

        private Action onOk;
        private Action onClose;

        public override void OnShow(object data = null)
        {
            if (data is ConfirmPaidPopupData popupData)
            {
                titleText.text = popupData.title;
                messageText.text = popupData.message;
                confirmText.text = popupData.confirmButton;
                costText.text = popupData.costButton;
                onOk = popupData.onOk;
                onClose = popupData.onClose;
            }
            else
            {
                Debug.LogWarning("[ConfirmPopup] No data passed!");
            }

            okButton.RegisterButtonClick(OnOkClicked);
            closeButton.RegisterButtonClick(OnCloseClicked);
            closeButton2.RegisterButtonClick(OnCloseClicked);
        }

        public override void OnHide()
        {
            base.OnHide();
            //okButton.onClick.RemoveListener(OnOkClicked);
            //closeButton.onClick.RemoveListener(OnCloseClicked);
        }

        private void OnOkClicked()
        {
            onOk?.Invoke();
            OnCloseAction?.Invoke(Config.id);
        }

        private void OnCloseClicked()
        {
            onClose?.Invoke();
            OnCloseAction?.Invoke(Config.id);
        }
    }

}