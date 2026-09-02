namespace Galactic1
{
    public class WorldMapViewModelsRegistrations
    {
        public static void Register(DIContainer container)
        {
            container.RegisterFactory(_ => new UIWorldMapRootViewModel()).AsSingle();
        }
    }
}