
using TMPro;
using UnityEngine.UI;

namespace Galactic1.Core.UI
{
    public struct StatSlotParam
    {
        public StatId StatId;
        public TMP_Text valueText;
        public Image fillBar;
        public float currValue;
        public float maxValue;
        public bool textAnimation;
    }
}