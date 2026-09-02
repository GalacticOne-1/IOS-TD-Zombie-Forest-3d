using Galactic1.Code.GameDatabase.Registries;
using Galactic1.PoolObject;
using Galactic1.Structure;
using UnityEngine;

namespace Galactic1.Game.Meta.Items
{
    /// <summary>
    /// Определяет калибр.
    /// Оружие использует ссылку на AmmoDefinition.
    /// Патроны содержат ссылку на тот же AmmoDefinition.
    /// </summary>
    [CreateAssetMenu(fileName = "AmmoDefinition", menuName = "Game Configs/Inventory/Ammo Definition")]
    public class AmmoDefinition : ScriptableObject, IObjectPoolConfig
    {
        [field: SerializeField] public RuntimeId Id { get; private set; }
        
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public string PrefabPath { get; private set; }
        [field: SerializeField] public ObjectPoolParam ObjectPoolParam { get; private set; }
        
    }
}