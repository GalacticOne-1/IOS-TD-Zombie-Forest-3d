
using System;
using Galactic1;

namespace Galactic1
{
    public static class EntitiesProxyFactory
    {
        public static EntityProxy CreateEntity(EntityData entityData)
        {
            switch (entityData.Type)
            {
                case EntityType.Furniture:
                    return new StructureEntityProxy(entityData as StructureEntityData);
                
                
                
                default:
                    throw new Exception("Unsupported entity type: " + entityData.Type);
            }
        }
    }
}