namespace Galactic1.Gameplay.Locations.Utils
{
    /// <summary>
    /// Запускает объекты обучения (Tutorial_camp и т.п.)
    /// </summary>
    public class HomeTutorialLoader
    {
        public void TryLoad(DIContainer container)
        {
            // существующая логика:
            // new TUTORIAL_Status(out bool notActive); ... conditional CreateObjects()
            new TUTORIAL_Status(out bool notActive);
            if (!notActive)
            {
                //ServiceLocator.Current.Get<Tutorial_camp>().CreateObjects();
            }
        }
    }
}