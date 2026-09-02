using UnityEngine;

namespace Galactic1
{
    public class EntityData
    {
        public string UniqueId { get; set; } // уникальный ид сущности
        public string ConfigGuid { get; set; } // Для поиска настроек сущности
        public string PrefabPath { get; set; } // Для загрузки объекта из resources
        public EntityType Type { get; set; } // Тип сущности, для быстрого понимания что это
        public int Level { get; set; }
        public Vector2Int Position { get; set; } // Позиция в x,y которые конвертируются в x,z на плоскости
    }
}