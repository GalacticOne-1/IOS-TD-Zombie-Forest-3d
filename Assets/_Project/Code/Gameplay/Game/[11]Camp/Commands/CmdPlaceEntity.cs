using Galactic1;
using UnityEngine;

namespace Galactic1
{
    public class CmdPlaceEntity : ICommand
    {
        public readonly EntityType EntityType;
        public readonly string EntityConfigId;
        public readonly string PrefabPath;
        public readonly int Level;
        public readonly Vector2Int Position;

        public CmdPlaceEntity(
            EntityType entityType, 
            string entityConfigId, 
            string prefabPath,
            int level, 
            Vector2Int position)
        {
            EntityType = entityType;
            EntityConfigId = entityConfigId;
            PrefabPath = prefabPath;
            Level = level;
            Position = position;
            
        }
    }
}