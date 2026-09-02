using Galactic1.Configs;
using Galactic1.Meta.Configs.Recruitment;
using UnityEngine;

namespace Galactic1.Code.UI.Units.Presentation
{
    /// <summary>
    /// Для получения prefab по archetypeId for UI.
    /// </summary>
    public sealed class UnitPreviewResolver : IGameService // todo пока не используется

    {
        private readonly ConfigProvider _config;

        public UnitPreviewResolver(ConfigProvider config)
        {
            _config = config;
        }

        public (string prefabPath, UnitIdentityPoolConfig.ArchetypePrefabEntry variant) GetPreviewPrefab(string archetypeId)
        {
            return _config.Get<UnitIdentityPoolConfig>().GetSurvivorEntry(archetypeId);
        }
    }
}