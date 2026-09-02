
namespace Galactic1.Gameplay.Locations
{
    public class HomeLoader : ILocationLoaderMode
    {
        public void Load(LocationContext ctx, DIContainer container)
        {
            //new TUTORIAL_Status(out bool notActive);
            
            
            // location
            var prefabPath = ctx.LocationConfig.PrefabPath;
            ctx.LocationInstance = prefabPath.CreateGO(ServiceLocator.Current.Get<Environment>().location.transform);
        }
    }
}