using System;
using UnityEngine;

namespace Galactic1.Configs
{
    [Serializable]
    public class EntityInitialStateConfigs
    {
        [field: SerializeField] public string ConfigId { get; private set; }
        [field: SerializeField] public int Level { get; private set; }
        [field: SerializeField] public Vector2Int InitialPosition { get; private set; }
    }
}