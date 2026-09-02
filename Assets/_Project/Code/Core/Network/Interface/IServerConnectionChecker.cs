using System.Collections;

namespace Galactic1
{
    public interface IServerConnectionChecker : IGameService
    {
        IEnumerator CheckRoutine();
    }
}