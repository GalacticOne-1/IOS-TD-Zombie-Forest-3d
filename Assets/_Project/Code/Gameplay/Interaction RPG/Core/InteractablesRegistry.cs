
using System.Collections.Generic;

namespace Galactic1.Gameplay.Interaction
{
    /// <summary>
    /// Централизованный реестр активных интерактивов.
    /// Интерракты регистрируются при OnEnable и снимаются при OnDisable.
    /// Это даёт быстрый доступ к списку без FindObjectsOfType.
    /// </summary>
    public static class InteractablesRegistry
    {
        private static readonly List<IInteractable> _all = new List<IInteractable>();

        public static IReadOnlyList<IInteractable> All => _all;

        public static void Register(IInteractable interactable)
        {
            if (interactable == null) return;
            if (!_all.Contains(interactable)) _all.Add(interactable);
        }

        public static void Unregister(IInteractable interactable)
        {
            if (interactable == null) return;
            _all.Remove(interactable);
        }

        public static void Clear() => _all.Clear();
    }
}