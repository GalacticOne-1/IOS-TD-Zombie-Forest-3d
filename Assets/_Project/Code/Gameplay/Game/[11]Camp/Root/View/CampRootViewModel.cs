using System;
using Galactic1;
using ObservableCollections;
using R3;
using Random = UnityEngine.Random;

namespace Galactic1
{
    public class CampRootViewModel
    {
        public readonly IObservableCollection<StructureViewModel> AllStructures;
        


        public CampRootViewModel(StructureService structureService)
        {
            AllStructures = structureService.AllStructures;

        }



    }
}