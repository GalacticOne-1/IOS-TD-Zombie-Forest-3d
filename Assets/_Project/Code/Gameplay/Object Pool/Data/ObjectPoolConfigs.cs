using System.Collections.Generic;
using Galactic1.Game.Meta.Items;
using UnityEngine;

namespace Galactic1.PoolObject
{
    [CreateAssetMenu(
        fileName = "ObjectPoolConfigs",
        menuName = "Game Configs/Object Pool/Object Pool Registry")]
    public class ObjectPoolConfigs : ScriptableObject
    {
        [field: SerializeField] public List<EffectConfig> EffectConfigs { get; private set; }
        [field: SerializeField] public List<AmmoDefinition> BulletConfigs { get; private set; }
        [field: SerializeField] public List<ItemConfig> GrenadeConfigs { get; private set; }
        [field: SerializeField] public UnitIndicatorWidgetConfig UnitIndicatorWidgetConfig { get; private set; }
    }
}