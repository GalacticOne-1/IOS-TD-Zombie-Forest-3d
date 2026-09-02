using R3;
using UnityEngine;

namespace Galactic1
{
    public class StructureViewModel
    {
        private readonly StructureEntityProxy _structureEntityProxy;
        private readonly BuildableConfig _structureConfig;
        private readonly StructureService _structureService;
        
        public readonly string ConfigId;
        public readonly int StructureId;
        public readonly string PrefabPath;
        
        public ReadOnlyReactiveProperty<int> Level { get; }
        public ReadOnlyReactiveProperty<Vector2Int> Position { get; }
        
        
        public StructureViewModel(
            StructureEntityProxy structureEntityProxy, 
            BuildableConfig structureConfig,
            StructureService structureService)
        {
            ConfigId = structureEntityProxy.ConfigId;
            //StructureId = structureEntityProxy.UniqueId;
            PrefabPath = structureEntityProxy.PrefabPath;
            
            _structureEntityProxy = structureEntityProxy;
            _structureConfig = structureConfig;
            _structureService = structureService;

            Level = structureEntityProxy.Level;
            Position = structureEntityProxy.Position;
        }
    }
}