using System.Collections.Generic;
using Galactic1.Game.Meta.Enemy;

namespace Galactic1.Code.GameDatabase.Registries
{
    /// <summary>
    /// Runtime lookup registry for enemy archetypes.
    /// </summary>
    public sealed class EnemyRegistry : RegistryBase<RuntimeId, EnemyArchetypeConfig>
    {

        public EnemyId DefaultEnemyId { get; private set; }

        public EnemyRegistry(IReadOnlyList<EnemyArchetypeConfig> configs)
        {
            for (int i = 0; i < configs.Count; i++)
            {
                var config = configs[i];

                if (config == null)
                {
                    DLog.Alert($"[EnemyRegistry] Null config at index {i}", EDlogColor.YELLOW);
                    continue;
                }

                if (config.Id == null)
                {
                    DLog.Alert($"[EnemyRegistry] Item '{config.name}' has NULL ItemId.", EDlogColor.YELLOW);
                    continue;
                }

                if (map.ContainsKey(config.Id))
                {
                    DLog.Alert($"[EnemyRegistry] Duplicate ItemId detected: {config.Id.name}", EDlogColor.YELLOW);
                    continue;
                }

                map.Add(config.Id, config);
            }
            
            // === устанавливаем базового зомби (пока первый в списке, можно искать явно)
            DefaultEnemyId = configs[0].Id;
        }
    }
}