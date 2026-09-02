using UnityEngine;


namespace Galactic1.UI.Core
{

    [CreateAssetMenu(fileName = "UIPopupConfig", menuName = "Game Configs/UI/UI Popup Config")]
    public class UIPopupConfig : ScriptableObject
    {
        public UIScreenId id;
        public PopupLayer layer = PopupLayer.Regular;

        [Tooltip("If using Addressables, set reference here. Otherwise leave null and set fallbackPrefab.")]
        public GameObject prefab;

        [Tooltip("Fallback prefab when not using Addressables or for edit-time preview.")]
        public string addressableKey;

        public bool blockUnderlying = true;
    }
}