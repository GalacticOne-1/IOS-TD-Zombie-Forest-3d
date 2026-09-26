
using System.Collections.Generic;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>
    /// Реестр всех tutorial target-блокировок в игре. Резолвится через
    /// ConfigProvider.Get&lt;TutorialTargetGateRegistry&gt;(), тот же паттерн, что
    /// TutorialCampaignRegistry — O(1) lookup по Guid(TutorialTargetId) вместо линейного
    /// сканирования, кэш строится в OnEnable/OnValidate.
    /// </summary>
    [CreateAssetMenu(
        fileName = "TutorialTargetGateRegistry",
        menuName = "Game Configs/Tutorial/Target Gate Registry")]
    public sealed class TutorialTargetGateRegistry : ScriptableObject
    {
        public List<TutorialTargetGateDefinition> gates = new();

        private Dictionary<string, TutorialTargetGateDefinition> _cacheByTargetGuid;

        private void OnEnable() => RebuildCache();
#if UNITY_EDITOR
        private void OnValidate() => RebuildCache();
#endif

        private void RebuildCache()
        {
            _cacheByTargetGuid = new Dictionary<string, TutorialTargetGateDefinition>();
            if (gates == null) return;

            foreach (var gate in gates)
            {
                if (gate?.targetId == null) continue;
                _cacheByTargetGuid[gate.targetId.Guid] = gate;
            }
        }

        /// <summary>O(1) доступ по typed-ссылке на target. Возвращает false, если для
        /// этого target gate не сконфигурирован — вызывающий (TutorialTargetGateService)
        /// обязан в этом случае считать target разблокированным (см. её докстринг).</summary>
        public bool TryGetGate(TutorialTargetId targetId, out TutorialTargetGateDefinition gate)
        {
            if (_cacheByTargetGuid == null) RebuildCache();
            gate = null;
            return targetId != null && _cacheByTargetGuid.TryGetValue(targetId.Guid, out gate);
        }

#if UNITY_EDITOR
        public bool ValidateAll(out List<string> errors)
        {
            errors = new List<string>();
            var seenTargets = new HashSet<string>();

            foreach (var gate in gates)
            {
                if (gate == null)
                {
                    errors.Add("TutorialTargetGateRegistry: contains a null gate entry.");
                    continue;
                }
                if (!gate.Validate(out var error))
                {
                    errors.Add(error);
                    continue;
                }
                if (!seenTargets.Add(gate.targetId.Guid))
                    errors.Add($"TutorialTargetGateRegistry: duplicate gate for target '{gate.targetId.DebugKey}'.");
            }
            return errors.Count == 0;
        }
#endif
    }
}