using System.Collections.Generic;
using Galactic1.Code.UI.Inventory;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>
    /// Реестр активных InventoryView — тот же register-in-OnEnable/unregister-in-OnDisable
    /// паттерн, что TutorialTargetRegistry, но без явного TutorialTargetBehaviour на каждом
    /// объекте: InventoryView регистрируется сама (см. её докстринг, минимальная
    /// интеграционная правка — 2 строки). Может содержать 0, 1 или 2+ записи одновременно
    /// (например leftSide/rightSide окна инвентаря) — TutorialInventorySlotTargetProvider
    /// обходит все.
    /// </summary>
    public sealed class TutorialInventoryViewRegistry : IGameService
    {
        private readonly List<InventoryView> _active = new();

        public void Register(InventoryView view)
        {
            if (view != null && !_active.Contains(view))
                _active.Add(view);
        }

        public void Unregister(InventoryView view) => _active.Remove(view);

        public IReadOnlyList<InventoryView> Active => _active;
    }
}
