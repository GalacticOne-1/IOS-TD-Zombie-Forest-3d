
using UnityEngine;

namespace Galactic1.Gameplay.Locations.Modes
{
    public class HomeCleaner : ILocationCleanerMode
    {
        public void Clear(LocationContext ctx)
        {
            // удаляем инстанс локации
            if (ctx.LocationInstance != null)
            {
                Object.Destroy(ctx.LocationInstance);
            }
            else
            {
                var location = Object.FindAnyObjectByType<SceneContext>();
                if (location != null)
                    Object.Destroy(location.gameObject);
            }
            
            ctx.LocationInstance = null;
        }
    }
}