using Galactic1.Core.Enums;
using Galactic1.UI.Core;

namespace Galactic1.Game.UI.Stats
{
    /// <summary>
    /// Определяет какой UI-стиль должен использоваться для конкретного типа стата.
    /// </summary>
    public sealed class StatStyleResolver
    {
        private static StatStyleConfig _config;

        public StatStyleResolver(StatStyleConfig config)
        {
            _config = config;
            _config.Initialize();
        }

        // todo подключить локализацию ...

        public static StatStyleEntry Resolve(StatId id)
            => _config.GetStat(id);

        public static DescriptorStyleEntry Resolve(DescriptorId id)
            => _config.GetDescriptor(id);
    }
}