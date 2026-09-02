using System;

namespace Galactic1.UI.Core
{
    public class ConfirmPopupData
    {
        public string title;
        public string message;
        public string confirmButton;
        public Action onOk;
        public Action onClose;

        public ConfirmPopupData(
            string title, 
            string message,  
            string confirmButton,  
            Action onOk = null,
            Action onClose = null)
        {
            this.message = message;
            this.title = title;
            this.confirmButton = confirmButton;
            this.onOk = onOk;
            this.onClose = onClose;
        }
    }
}