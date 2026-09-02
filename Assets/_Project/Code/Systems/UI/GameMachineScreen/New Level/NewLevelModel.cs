using System.Collections.Generic;
using Galactic1;
using UnityEngine;

namespace Galactic1
{
    public class NewLevelModel : MVVMModel
    {

        public class CNewLevel
        {
            public string h2;
            public int level;

            public AddingReward.CReward[] reward;

            public List<byte> newBlueprints;
        }
        
        
        public NewLevelModel(MVVMView _view) : base(_view)
        {
            view = _view;
        }
    }
}