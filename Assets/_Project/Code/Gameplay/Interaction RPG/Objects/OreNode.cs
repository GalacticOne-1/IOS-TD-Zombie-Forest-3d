using UnityEngine;

namespace Galactic1.Gameplay.Interaction.Objects
{
    public class OreNode : ResourceNodeBase, IActionInteractable
    {
        public override ActionType ActionType => ActionType.GatherOre;
        
        protected override void OnGatherHit(Transform interactor)
        {
            // звук рубки
        }

        protected override void OnDepleted()
        {
            // спаун логов
            base.OnDepleted();
        }
    }
}