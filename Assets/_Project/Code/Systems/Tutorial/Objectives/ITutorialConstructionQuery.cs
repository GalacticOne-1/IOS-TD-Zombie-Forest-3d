using System;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    /// <summary>Узкий query-интерфейс "построено ли здание X" — по тому же принципу, что
    /// ITutorialInventoryQuery/ITutorialFacilityPanelQuery (см. TutorialGameStateQuery
    /// докстринг): guidance condition получает только то, что ему нужно, не весь
    /// GameLoopContext.</summary>
    public interface ITutorialConstructionQuery
    {
        /// <summary>itemId == null = "есть хотя бы одно построенное здание вообще".</summary>
        bool HasFacilityBuilt(ItemId itemId);

        /// <summary>Триггер переоценки — тот же принцип, что OnDomainTransition/
        /// OnInboxChanged: событие не источник истины, HasFacilityBuilt() перечитывается
        /// заново. Payload — itemId только что построенного здания (для отладки/логов,
        /// подписчик всё равно обязан перечитать своё собственное condition).</summary>
        event Action<ItemId> OnFacilityBuilt;
    }
}
