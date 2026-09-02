using UnityEngine;

namespace Galactic1.Gameplay.Locations.Modes
{
    public class RegularLevelCleaner : ILocationCleanerMode
    {
        public void Clear(LocationContext ctx)
        {
            // отключаем и выгружаем существ
            //ctx.LocationInstance?.GetComponent<LocationSpawner>()?.UnloadCreatures();

            // чистка общих пулов
            //Pool.I.Clear();

            // удаляем инстанс локации
            if (ctx.LocationInstance != null)
            {
                Object.Destroy(ctx.LocationInstance);
                ctx.LocationInstance = null;
            }
        }
    }
}