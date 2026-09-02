using TMPro;
using UnityEngine.UI;

namespace Galactic1.Game.UI.Stats
{
    [System.Serializable]
    public sealed class StaticDescriptorSlot
    {
        public string slotId;
        public TMP_Text labelText;
        public Image icon;
        public TMP_Text valueText;


        public void Set(string label, string value)
        {
            if (labelText != null)
                labelText.text = label;
            if (valueText != null)
                valueText.text = value;
            
            SetActive(true);
        }

        public void SetActive(bool y)
        {
            if (labelText != null)
                labelText.gameObject.SetActive(y);
            if (valueText != null)
                valueText.gameObject.SetActive(y);
        }
    }
}