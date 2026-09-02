using R3;

namespace Galactic1.Core
{
    public interface IGameStateProvider : IGameService
    {
        public GameStateProxy GameStateProxy { get; }

        public Observable<GameStateProxy> LoadGameState();
        public Observable<bool> SaveGameState();
        public Observable<bool> ResetGameState();
    }
}