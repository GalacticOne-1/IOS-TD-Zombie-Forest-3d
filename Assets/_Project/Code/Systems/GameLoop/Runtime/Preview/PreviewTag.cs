using UnityEngine;

namespace Galactic1.Preview
{
    /// <summary>
    /// Маркерный компонент.
    /// Если присутствует на prefab — объект участвует в Preview Baking.
    /// </summary>
    public class PreviewTag : MonoBehaviour
    {
        public PreviewType type;
    }
}