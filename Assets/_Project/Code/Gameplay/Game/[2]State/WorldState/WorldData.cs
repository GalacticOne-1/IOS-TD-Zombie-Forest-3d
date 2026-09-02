
using System.Collections.Generic;

namespace Galactic1
{
    public class WorldData
    {
        public int Id { get; set; }
        
        // все объекты на сцене (ресурсы, сундуки, телепорты, строения и тд)
        // что построил игрок или заспавненны локациями
        public List<EntityData>  Entities { get; set; }             
        public List<CrateEntityData> Crates { get; set; } 
        //public List<BuildingData> Facilities { get; set; } 
    }
}