using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Structure;
using UnityEngine;

namespace Galactic1.PoolObject
{
    [CreateAssetMenu(
        fileName = "UnitIndicatorWidgetConfig", 
        menuName = "Game Configs/Object Pool/Unit Indicator Widget Config")]
    public class UnitIndicatorWidgetConfig : ScriptableObject, IObjectPoolConfig
    {
        [field: SerializeField] public RuntimeId Id { get; private set; }
        [field: SerializeField] public string PrefabPath { get; private set; }
        [field: SerializeField] public ObjectPoolParam ObjectPoolParam { get; private set; }
    }
}