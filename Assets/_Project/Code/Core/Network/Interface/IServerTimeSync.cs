using System;
using System.Collections;

namespace Galactic1
{
    public interface IServerTimeSync : IGameService
    {
        event Action OnServerTimeSynced;
        IEnumerator SyncServerTime();
        double GetServerNow();
        int GetServerDay();
    }
}