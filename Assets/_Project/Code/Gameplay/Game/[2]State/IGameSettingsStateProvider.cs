using R3;

namespace Galactic1.Core
{
    public interface IGameSettingsStateProvider
    {
        public GameSettingStateProxy GameSettings { get; }

        public Observable<GameSettingStateProxy> LoadGameSettings();
        public Observable<bool> SaveGameSettings();
        public Observable<bool> ResetGameSettings();
    }
}