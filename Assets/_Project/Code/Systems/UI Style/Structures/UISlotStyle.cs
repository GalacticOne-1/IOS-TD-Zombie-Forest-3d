using UnityEngine;

namespace Galactic1.Structs.UI
{
    [System.Serializable]
    public struct UISlotStyle
    {
        public Color normal;
        public Color selected;
        public Color highlight;
        public Color disabled;

        public Sprite normalSprite;
        public Sprite selectedSprite;
        public Sprite highlightSprite;
        public Sprite disabledSprite;
    }
}