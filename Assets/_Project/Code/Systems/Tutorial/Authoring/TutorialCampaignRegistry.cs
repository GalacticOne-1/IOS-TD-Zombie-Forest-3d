using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>
    /// Реестр всех кампаний тутора в игре. Резолвится через ConfigProvider.Get&lt;TutorialCampaignRegistry&gt;(),
    /// как и другие глобальные конфиги проекта (ItemDatabase, WaveConfig и т.п.).
    ///
    /// Два способа резолва — по образцу TutorialDefinition.GetStep/GetStepByGuid:
    /// GetCampaign(TutorialCampaignId) — обычный typed lookup (авторинг-время, прямая ссылка
    /// на ассет, например назначенная в инспекторе вызывающего сервиса). GetCampaignByGuid(string) —
    /// ТОЛЬКО для границы persist/restore, где CGameStateTutorial хранит campaignId как string
    /// (ScriptableObject-ссылку нельзя напрямую положить в сохранение).
    /// </summary>
    [CreateAssetMenu(
        fileName = "TutorialCampaignRegistry",
        menuName = "Galactic1/Tutorial/Campaign Registry")]
    public sealed class TutorialCampaignRegistry : ScriptableObject
    {
        public List<TutorialDefinition> campaigns = new();

        private Dictionary<TutorialCampaignId, TutorialDefinition> _cache;
        private Dictionary<string, TutorialDefinition> _cacheByGuid;

        private void OnEnable() => RebuildCache();
#if UNITY_EDITOR
        private void OnValidate() => RebuildCache();
#endif

        private void RebuildCache()
        {
            _cache = new Dictionary<TutorialCampaignId, TutorialDefinition>();
            _cacheByGuid = new Dictionary<string, TutorialDefinition>();
            if (campaigns == null) return;
            foreach (var c in campaigns)
            {
                if (c == null || c.campaignId == null) continue;
                _cache[c.campaignId] = c;
                _cacheByGuid[c.campaignId.Guid] = c;
            }
        }

        /// <summary>O(1) доступ по typed-ссылке. Основной способ резолва кампании.</summary>
        public TutorialDefinition GetCampaign(TutorialCampaignId campaignId)
        {
            if (_cache == null) RebuildCache();
            return campaignId != null && _cache.TryGetValue(campaignId, out var def) ? def : null;
        }

        /// <summary>Резолв из персистентного guid (CGameStateTutorial.campaignId).
        /// Использовать ТОЛЬКО на границе save/restore.</summary>
        public TutorialDefinition GetCampaignByGuid(string guid)
        {
            if (_cacheByGuid == null) RebuildCache();
            return !string.IsNullOrEmpty(guid) && _cacheByGuid.TryGetValue(guid, out var def) ? def : null;
        }
    }
}
