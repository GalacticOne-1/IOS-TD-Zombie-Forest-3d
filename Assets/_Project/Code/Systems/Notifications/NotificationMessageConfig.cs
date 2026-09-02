
using System;
using System.Collections.Generic;
using Galactic1.Core.Results;
using UnityEngine;
using UnityEngine.Serialization;

namespace Galactic1.Code.Notification
{
    [CreateAssetMenu(menuName = "Game Configs/Notification/Message Config")]
    public sealed class NotificationMessageConfig : ScriptableObject
    {
        [SerializeField] private List<Entry> entries = new();
        
        [Space]
        [SerializeField] private List<NotificationStyle> styles; // todo перенести в конфиг стилей

        
        
        [Serializable]
        public struct Entry
        {
            public string Id;
            public NotificationFailReason Reason;
            public string Message;
            public bool UsePopup;

            [Space]
            public NotificationStyleCategory StyleCategory;
            [HideInInspector] public NotificationStyle Style;
        }
        
        [Serializable]
        public struct NotificationStyle
        {
            public NotificationStyleCategory Category;
            public Color BackgroundColor;
            public Color TextColor;
            public Sprite Icon;
        }

        public bool TryGet(NotificationFailReason reason, out Entry entry)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].Reason == reason)
                {
                    entry = entries[i];
                    entry.Style = styles.Find(s => s.Category == entries[i].StyleCategory);
                    return true;
                }
            }

            entry = default;
            return false;
        }

        public NotificationStyle GetStyle(NotificationStyleCategory category)
            => styles.Find(s => s.Category == category);
    }
}