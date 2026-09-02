using UnityEngine;

namespace Galactic1.UI.Inventory.Preview
{
    /// <summary>
    /// Конфигурация предпросмотра персонажа для UI:
    /// - базовый 2D prefab
    /// - размер RenderTexture
    /// - минимальная камера
    /// </summary>
    [CreateAssetMenu(fileName = "UICharacterPreviewConfig", menuName = "Game Configs/Inventory/UI Character Preview Config")]
    public class UICharacterPreviewConfig : ScriptableObject
    {
        [field: SerializeField] public GameObject PlayerPrefab { get; private set; }
        [field: SerializeField] public GameObject DragonPrefab { get; private set; }

        public int RenderTextureSize = 512;

       
    }
}