using System;
using System.Collections.Generic;
using Galactic1.Game.UI.Buildings.DTO;
using Galactic1.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Game.UI.Inbox
{
    /// <summary>
    /// Список предметов Inbox.
    /// </summary>
    public class InboxListView : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private InboxCardView cardPrefab;

        private readonly List<InboxCardView> _cards = new();

        public event Action<string> OnTakeClicked;

        public void Build(IReadOnlyList<InboxItemDTO> items, bool openeing)
        {
            scrollRect.content.MakeHidden();
            Clear();

            var styleResolver = ServiceLocator.Current.Get<UIStyleResolver>();

            foreach (var dto in items)
            {
                var card = Instantiate(cardPrefab, scrollRect.content);
                card.Bind(dto, styleResolver);

                card.OnTakeClicked += HandleTake;

                _cards.Add(card);
            }

            scrollRect.SetSizeContentLayoutGroup(true, null, true, true);
            if (openeing) scrollRect.ScrollRectResetV();
        }

        private void HandleTake(string slotId)
        {
            OnTakeClicked?.Invoke(slotId);
        }

        public void Clear()
        {
            scrollRect.content.MakeEmpty();
            _cards.Clear();
        }
    }
}