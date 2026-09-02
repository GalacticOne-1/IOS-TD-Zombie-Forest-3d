using System.Collections;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;

namespace Galactic1.Mobile
{
    public class AdAlert : UIPopup
    {
        [SerializeField] private TextMeshProUGUI tMessage;


        public override void OnShow(object data = null)
        {
            if (data is string message)
            {
                tMessage.text = message;
                gameObject.GetChild(0).SetActive(false);
                gameObject.GetChild(0).SetActive(true);
                ServiceLocator.Current.Get<CoroutineController>().StartCoroutine(hide());
            }
        }

        public override void OnHide() {}

        IEnumerator hide()
        {
            yield return new WaitForSeconds(2.3f);
            OnCloseAction?.Invoke(Config.id);
        }
    }

}