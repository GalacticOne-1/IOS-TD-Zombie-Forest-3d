using R3;
using UnityEngine;

namespace Galactic1
{
    /// <summary>
    /// Proxy >> EntityData
    /// </summary>
    public abstract class EntityProxy
    {
        public EntityData Origin { get; }

        public string UniqueId => Origin.UniqueId;
        public string ConfigId => Origin.ConfigGuid;
        public string PrefabPath => Origin.PrefabPath;
        public EntityType Type => Origin.Type;
        public readonly ReactiveProperty<Vector2Int> Position;

        protected EntityProxy(EntityData origin)
        {
            Origin = origin;

            Position = new(Origin.Position);
            Position.Skip(1).Subscribe(_ => Origin.Position = _);
        }
    }
}