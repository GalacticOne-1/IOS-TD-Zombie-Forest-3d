using UnityEngine;

namespace Galactic1.UI.Core
{
    [CreateAssetMenu(
        fileName = "ButtonStyleConfig", 
        menuName = "Game Configs/Style/Button Style Config")]
    public class ButtonStyleConfig : ScriptableObject, IUIStyleConfig
    {
        [field: SerializeField] public string ConfigId { get; private set; }

        public string Id
        {
            get => ConfigId;
            set => ConfigId = value;
        }
    
        public Sprite normal;
        public Sprite highlighted;
        public Sprite pressed;
        public Sprite selected;
        public Sprite disabled;

        public Color textNormal;
        public Color textDisabled;
    }

}