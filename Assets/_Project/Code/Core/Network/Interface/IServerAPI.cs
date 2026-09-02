using System.Collections;

namespace Galactic1
{
    public interface IServerAPI : IGameService
    {
        IEnumerator PingServer(System.Action<bool> callback);
        IEnumerator GetServerTime(System.Action<double> callback);
    }
}