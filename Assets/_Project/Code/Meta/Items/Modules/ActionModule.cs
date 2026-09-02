using System.Collections.Generic;
using Galactic1.Code.Items;
using Galactic1.Items;
using UnityEngine;

namespace Galactic1.Game.Meta.Items
{
    [System.Serializable]
    public class ActionModule : ItemModule
    {
        [SerializeField]
        private List<ItemActionConfig> actions;
        
        public IReadOnlyList<ItemActionConfig> Actions => actions;

        public void SetActions(List<ItemActionConfig> actionConfigs)
        {
            actions = new(actionConfigs);
        }
        
        
        
        
        
        public override void CollectDescriptors(List<DescriptorDisplayEntry> list)
        {
            
        }
    }
}