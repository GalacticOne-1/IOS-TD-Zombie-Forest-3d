
using System.Collections.Generic;
using ObservableCollections;
using R3;
using UnityEngine;

namespace Galactic1
{
    public class CampRootBinder : MonoBehaviour
    {
        private readonly Dictionary<int, StructureBinder> _createStructuresMap = new();

        private readonly CompositeDisposable _disposables = new();

        private CampRootViewModel _viewModel;
        
        public void Bind(CampRootViewModel viewModel)
        {
            _viewModel = viewModel;
                
            
            // текущее состояние
            foreach (var furnitureViewModel in viewModel.AllStructures)
            {
                CreateStructures(furnitureViewModel);
            }

            // подписка на изменения
            _disposables.Add(viewModel.AllStructures.ObserveAdd()
                .Subscribe(e => CreateStructures(e.Value)));

            _disposables.Add(viewModel.AllStructures.ObserveRemove()
                .Subscribe(e => DestroyFurniture(e.Value)));
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }


        void CreateStructures(StructureViewModel structureViewModel)
        {
            var structureLevel = structureViewModel.Level.CurrentValue;
            var structureConfigId = structureViewModel.ConfigId;
            var prefabPath = structureViewModel.PrefabPath;
            var prfabStructurePath = $"{AppConstants.PATH_STRUCTURES}{prefabPath}";

            var createdStructure = prfabStructurePath.CreateGO(null).GetComponent<StructureBinder>();
            createdStructure.Bind(structureViewModel);

            _createStructuresMap[structureViewModel.StructureId] = createdStructure;
        }

        void DestroyFurniture(StructureViewModel structureViewModel)
        {
            if (_createStructuresMap.TryGetValue(structureViewModel.StructureId, out var furnitureBinder))
            {
                Destroy(furnitureBinder.gameObject);
                _createStructuresMap.Remove(structureViewModel.StructureId);
            }
        }


    }
}