using Galactic1.Code.Dev;
using Galactic1.Configs;
using UnityEngine;

namespace Galactic1.EntryPoint
{
    public class NewGameEntry
    {
        
        private const string NEW_GAME_KEY = nameof(NEW_GAME_KEY);

        
        
        public NewGameEntry()
        {
            if (PlayerPrefs.HasKey(NEW_GAME_KEY))
                return;

            PlayerPrefs.SetString(NEW_GAME_KEY, "y");

            // === спавн стартовых предметов на базе игрока
            ServiceLocator.Current.Get<ConfigProvider>().Get<StartKitData>().GetKit(EStartKit.StartGame_01).Apply();

        }
    }
}