
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Structure;

namespace Galactic1.PoolObject
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "EffectConfig", menuName = "Game Configs/Object Pool/Effect Config")]
    public class EffectConfig : ScriptableObject, IObjectPoolConfig
    {
        [field: SerializeField] public RuntimeId Id { get; private set; }
        [field: SerializeField] public string PrefabPath { get; private set; }
        [field: SerializeField] public ObjectPoolParam ObjectPoolParam { get; private set; }
    }
}