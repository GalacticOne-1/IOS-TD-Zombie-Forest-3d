using Galactic1.AbstractFactory;
using Galactic1.Code.Gameplay.Units.Zombie;
using Galactic1.Configs;
using UnityEngine;

namespace Galactic1.Core.Systems.Factories
{
    /// <summary>
    /// Factory только создаёт scene instance.
    /// Никакой runtime binding логики здесь нет.
    /// </summary>
    public sealed class ZombieFactory : IGameService
    {
        private Transform _spawnRoot;


        public EnemyInstance Create(
            int index,
            string prefabId,
            Vector3 position)
        {
            _spawnRoot ??= ServiceLocator.Current.Get<Environment>().enemies;

            var instance =  $"{AppConstants.PATH_ENEMIES}{prefabId}"
                .CreateGO(_spawnRoot)
                .GetComponent<EnemyInstance>();

            instance.Id = index;
            instance.name += $"___({index})";

            var pos = position;
            pos.x += Random.Range(-1f, 1f);
            pos.z += Random.Range(-1f, 1f);

            instance.Tr.position = pos;

#if UNITY_EDITOR
            DLog.Alert(
                $"[ZombieFactory] Created instance",
                EDlogColor.YELLOW);
#endif

            return instance;
        }
    }
}