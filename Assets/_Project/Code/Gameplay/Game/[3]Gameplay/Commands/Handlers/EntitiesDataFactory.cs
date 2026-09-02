using System;
using Galactic1.Configs;
using Galactic1;
using UnityEngine;

namespace Galactic1
{
    public static class EntitiesDataFactory
    {
        public static EntityData CreateEntity(EntityInitialStateConfigs initialConfigs, _EntityConfig_ entityConfigs)
        {
            switch (entityConfigs.EntityType)
            {
                // case EntityType.Unknown:
                //     break;
                case EntityType.Furniture:
                    return CreateEntity<StructureEntityData>(initialConfigs, entityConfigs);
                // case EntityType.Npc:
                //     break;
                // case EntityType.Creature:
                //     break;
                default:
                    throw new Exception("Not implemented entity creation: " + entityConfigs.EntityType);
            }
        }



        // заполнение базовых полей для любой сущности
        static T CreateEntity<T>(EntityInitialStateConfigs initialConfigs, _EntityConfig_ entityConfigs) where T : EntityData, new()
        {
            return CreateEntity<T>(
                entityConfigs.EntityType,
                entityConfigs.ConfigId,
                entityConfigs.PrefabPath,
                initialConfigs.Level,
                initialConfigs.InitialPosition);
        }
        
        // заполнение базовых полей для любой сущности
        public static T CreateEntity<T>(
            EntityType type, 
            string configId, 
            string prefabPath,
            int level, 
            Vector2Int postition)
            where T : EntityData, new()
        {
            var entity = new T
            {
                Type = type,
                ConfigGuid = configId,
                PrefabPath = prefabPath,
                Level = level,
                Position = postition
            };

            
            // заполнение остальных полей зависимых от конкретной сущности
            switch (entity)
            {
                case StructureEntityData furnitureEntityData:
                    UpdateFurnitureEntity(furnitureEntityData);
                    break;
                
                // ...
                
                default:
                    throw new Exception("Not implemented entity creation: " + type);
            }

            return entity;
        }

        

        
        static void UpdateFurnitureEntity(StructureEntityData structureEntity)
        {
            structureEntity.SlotSize = 9;
        }
    }
}