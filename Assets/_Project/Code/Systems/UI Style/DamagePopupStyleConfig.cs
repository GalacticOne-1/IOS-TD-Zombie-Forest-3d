using UnityEngine;

namespace Galactic1
{
    [CreateAssetMenu(fileName = "DamagePopupStyleConfig", menuName = "Game Configs/Style/Damage Popup Style Config")]
    public class DamagePopupStyleConfig : ScriptableObject
    {
        [field: SerializeField] public string ConfigId { get; private set; }

        public string Id
        {
            get => ConfigId;
            set => ConfigId = value;
        }
        
        [System.Serializable]
        public class CStyle
        {
            public string id;          // например: "normal", "critical", "poison"
            public Color color;
            public float scale = 1f;
            public Font font;
            public AnimationCurve popupCurve;
        }

        public CStyle[] styles;

    }

}