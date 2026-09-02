using System;

namespace Galactic1.UI.Core
{
    public class ConfirmPaidPopupData : ConfirmPopupData
    {
        public string costButton;
        
        public ConfirmPaidPopupData(
            string title, 
            string message, 
            string confirmButton, 
            string costButton = "", 
            Action onOk = null, 
            Action onClose = null) : base(title, message, confirmButton, onOk, onClose)
        {
            this.costButton = costButton;
        }
    }
}