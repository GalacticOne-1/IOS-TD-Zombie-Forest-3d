using UnityEngine;

namespace Galactic1.Code.UI.Interaction
{
    /// <summary>
    /// Вешать на статичный UI пенали которые могут менять состояние
    /// в зависимости от режима в сцене
    /// </summary>
    public sealed class UILayerElement : MonoBehaviour
    {
        [SerializeField] public UILayerType Layer; // слой принадлежности панели
    }
}