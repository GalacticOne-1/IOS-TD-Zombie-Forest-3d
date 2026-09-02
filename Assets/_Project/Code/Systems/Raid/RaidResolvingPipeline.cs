using System.Collections.Generic;
using System.Linq;

namespace Galactic1.Code.Systems.Raid
{
    /// <summary>
    /// Единый pipeline применения результатов рейда
    /// к meta-runtime объектам.
    /// </summary>
    public static class RaidResolvingPipeline
    {
        public static void Resolve<TMeta>(
            IEnumerable<IRaidResolvable> raidObjects,
            IEnumerable<TMeta> metaObjects,
            System.Func<TMeta, string> idSelector)
        {
            var metaById = metaObjects.ToDictionary(idSelector);

            foreach (var raid in raidObjects)
            {
                if (!metaById.TryGetValue(raid.Id, out var meta))
                    continue;

                raid.ApplyToMeta(meta);
            }
        }
        
        /// <summary>
        /// Resolve для ОДНОГО meta-объекта (транспорт, база, корабль)
        /// </summary>
        public static void Resolve<TMeta>(
            IRaidResolvable raidObject,
            TMeta metaObject,
            System.Func<TMeta, string> idSelector)
        {
            if (raidObject == null || metaObject == null)
                return;

            if (raidObject.Id != idSelector(metaObject))
                return;

            raidObject.ApplyToMeta(metaObject);
        }
    }
}