
using System;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public interface ITutorialInboxQuery
    {
        bool HasItemInInbox(ItemId itemId);

        /// <summary>Триггер переоценки — тот же принцип, что OnDomainTransition:
        /// событие не является source of truth, HasItemInInbox() перечитывается заново.</summary>
        event Action OnInboxChanged;
    }
}