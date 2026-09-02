using R3;

namespace Galactic1.Core
{
    public class GameSettingStateProxy
    {
        public ReactiveProperty<int> MusicVolume;
        public ReactiveProperty<int> SFXVolume;
        
        public GameSettingStateProxy(GameSettingState settingState)
        {
            // R3
            MusicVolume = new(settingState.MusicVolume);
            SFXVolume = new(settingState.SFXVolume);
            
            // subscription
            MusicVolume.Skip(1).Subscribe(_ => settingState.MusicVolume = _);
            SFXVolume.Skip(1).Subscribe(_ => settingState.SFXVolume = _);
        }
    }
}