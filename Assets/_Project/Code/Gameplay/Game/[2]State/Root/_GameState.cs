namespace Galactic1.Core
{
    public static class _GameState
    {
        public static bool AppLoaded { get; private set; }
        public static bool FirstStart { get; private set; }


        public static void AppLoaded_() => AppLoaded = true;
        public static void FirstStart_() => FirstStart = true;
        
        
        
        public static void Save() => ServiceLocator.Current.Get<IGameStateProvider>().SaveGameState();
    }
}