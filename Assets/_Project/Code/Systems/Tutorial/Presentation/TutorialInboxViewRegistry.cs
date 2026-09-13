using System.Collections.Generic;
using Galactic1.Game.UI.Inbox;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>Реестр активных InboxCardView — тот же register/unregister паттерн,
    /// что TutorialInventoryViewRegistry, для того же класса задач (живой поиск UI-элемента
    /// по ItemId, без персистентной регистрации по TutorialTargetId).</summary>
    public sealed class TutorialInboxViewRegistry : IGameService
    {
        private readonly List<InboxCardView> _active = new();

        public void Register(InboxCardView card)
        {
            if (card != null && !_active.Contains(card))
                _active.Add(card);
        }

        public void Unregister(InboxCardView card) => _active.Remove(card);

        public IReadOnlyList<InboxCardView> Active => _active;
    }
}