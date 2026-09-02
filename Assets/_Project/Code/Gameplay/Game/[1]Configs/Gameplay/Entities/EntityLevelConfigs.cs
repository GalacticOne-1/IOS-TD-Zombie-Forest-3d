using UnityEngine;

namespace Galactic1
{
    [CreateAssetMenu(fileName = "EntityLevelConfigs", menuName = "Game Configs/Entities/New Entity Level Configs")]
    public class EntityLevelConfigs : ScriptableObject
    {
        [field: SerializeField] public string Level { get; private set; }
        [field: SerializeField] public string PrefabSkinPath { get; private set; }
    }
}