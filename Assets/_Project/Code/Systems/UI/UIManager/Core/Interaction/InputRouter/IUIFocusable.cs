using UnityEngine;

namespace Galactic1.Code.UI.Interaction
{
    /// <summary>
    /// UI элемент, который может получать фокус
    /// и реагировать на клик вне себя.
    /// </summary>
    public interface IUIFocusable
    {
        int Priority { get; }

        /// Проверяет, попадает ли клик внутрь элемента
        bool ContainsScreenPoint(Vector2 screenPos);

        /// Потеря фокуса (клик вне)
        void OnFocusLost();
    }
}