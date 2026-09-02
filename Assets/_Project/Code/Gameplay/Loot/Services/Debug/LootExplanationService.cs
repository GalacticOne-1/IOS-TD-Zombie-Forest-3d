
using Galactic1.RaidLoot.Services;
using UnityEngine;

namespace Galactic1.RaidLoot.Diagnostics
{
    /// <summary>
    /// Public API для получения объяснения почему выпал/не выпал предмет.
    /// Используется в: editor tools, QA, designer dashboard.
    /// Не используется в gameplay runtime.
    /// </summary>
    public sealed class LootExplanationService
    {
        private readonly LootGenerationService _generationService;

        public LootExplanationService(LootGenerationService generationService)
            => _generationService = generationService;

        /// <summary>Получить полный trace последней генерации контейнера.</summary>
        public LootGenerationTrace GetTrace(string containerIdKey)
        {
            _generationService.TryGetTrace(containerIdKey, out var trace);
            return trace;
        }

        /// <summary>Вывести читаемый отчёт в Console.</summary>
        public void LogTrace(string containerIdKey)
        {
            var trace = GetTrace(containerIdKey);
            if (trace == null)
            {
                Debug.LogWarning($"[LootExplanation] No trace found for: {containerIdKey}");
                return;
            }

            Debug.Log(trace.ToReadableString());
        }
    }
}