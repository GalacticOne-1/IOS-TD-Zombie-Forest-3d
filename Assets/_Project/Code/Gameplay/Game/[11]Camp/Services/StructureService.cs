using System.Collections.Generic;
using Galactic1;
using ObservableCollections;
using R3;
using UnityEngine;

namespace Galactic1
{
    /// <summary>
    /// Такой сервис создается один раз со сценой
    /// <br/>и когда сцена уничтожается  
    /// <br/>сервисы со всеми подписками тоже уничтожаются
    /// </summary>
    public class StructureService
    {
        private readonly ICommandProcessor _cmd;
        private readonly ObservableList<StructureViewModel> _allStructures = new();
        private readonly Dictionary<int, StructureViewModel> _structureMap = new();
        private readonly Dictionary<string, BuildableConfig> _structureConfigsMap = new();
            
        public IObservableCollection<StructureViewModel> AllStructures => _allStructures;
        
        
        public StructureService(
            IObservableCollection<EntityProxy> entities,
            Dictionary<string, BuildableConfig> structureConfigs,
            ICommandProcessor cmd)
        {
            _cmd = cmd;

            // заполняем список 
            foreach (var config in structureConfigs)
            {
                _structureConfigsMap[config.Key] = config.Value;
            }
            
            // синхронизация
            foreach (var entity in entities)
            {
                if (entity is StructureEntityProxy furnitureEntity)
                    CreateStructureViewModel(furnitureEntity);
            }

            entities.ObserveAdd().Subscribe(e =>
            {
                if (e.Value is StructureEntityProxy furnitureEntityProxy)
                    CreateStructureViewModel(furnitureEntityProxy);
            });
            entities.ObserveRemove().Subscribe(e =>
            {
                if (e.Value is StructureEntityProxy furnitureEntityProxy)
                    RemoveStructureViewModel(furnitureEntityProxy);
            });
            //

        }


        public bool PlaceStructure(string buildingConfigId, string prefabPath, int level, Vector2Int position)
        {
            var command = new CmdPlaceEntity(EntityType.Furniture, buildingConfigId, prefabPath, level, position);
            var result = _cmd.Process(command);

            return result;
        }

        public bool ChangePositionStructure(int furnitureEntityId, Vector2Int newPosition)
        {

            return false;
        }

        public bool DeleteStructure(int furnitureEntityId)
        {
            return false;
        }

        
        /// <summary>
        /// Создание объекта в сцене
        /// </summary>
        /// <param name="structure"></param>
        void CreateStructureViewModel(StructureEntityProxy structure)
        {
            var furnitureViewModel = new StructureViewModel(structure, _structureConfigsMap[structure.ConfigId], this);
            
            
            _allStructures.Add(furnitureViewModel);
            //_structureMap[structure.UniqueId] = furnitureViewModel;
        }
        
        /// <summary>
        /// Удаление объекта
        /// </summary>
        /// <param name="structure"></param>
        void RemoveStructureViewModel(StructureEntityProxy structure)
        {
            //if (_structureMap.TryGetValue(structure.UniqueId, out var furnitureViewModel))
            {
                //_allStructures.Remove(furnitureViewModel);
                //_structureMap.Remove(structure.UniqueId);
            }
        }
    }
}