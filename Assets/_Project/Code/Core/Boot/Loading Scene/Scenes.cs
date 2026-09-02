namespace Galactic1
{
    public static class Scenes
    {
        public const string BOOT = "Boot";
        public const string CORE_GAMEPLAY = "CoreGameplay";
        public const string ROOT_PLAYER = "RootPlayer";
        
        
        public const string MAP = "Map";
        public const string HOME = "Home";
        public const string LOCATION = "Location";
        public const string DEV_SCENE = "DevScene";




        /// <summary>
        /// Для получения списка сцен несовместимых с загружаемой
        /// </summary>
        /// <param name="newScene">загружаемая сцена</param>
        /// <returns></returns>
        public static string[] NotCompatibleGroups(string newScene)
        {
            string[] scenes = null;
            switch (newScene)
            {
                case MAP:
                    scenes = new[] { BOOT, MAP, HOME, LOCATION, ROOT_PLAYER, DEV_SCENE };
                    break;
                
                case HOME:
                    scenes = new[] { BOOT, MAP, HOME, LOCATION, DEV_SCENE };
                    break;
                
                case LOCATION:
                    scenes = new[] { BOOT, MAP, HOME, LOCATION, DEV_SCENE };
                    break;
                
                case DEV_SCENE:
                    scenes = new[] { BOOT, MAP, HOME, LOCATION, DEV_SCENE };
                    break;
            }

            return scenes;
        }
    }
}