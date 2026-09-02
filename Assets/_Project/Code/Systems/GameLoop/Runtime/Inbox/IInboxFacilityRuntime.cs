using System;
using Galactic1.Code.Systems.Runtime.Building;

namespace Galactic1.Code.Systems.Runtime
{
    public interface IInboxFacilityRuntime
    {
        FacilityType Type { get; }
        InboxRuntime Inbox { get; }
        int TotalWorldHour { get; }
        event Action OnStateChanged;
    }
}