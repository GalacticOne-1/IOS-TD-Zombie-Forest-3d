using Galactic1.Code.Systems.Tutorial.Presentation;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    
    [CreateAssetMenu(fileName = "TutorialStepWallActionDefinition",
        menuName = "Game Configs/Tutorial/Actions/StepWallAction_")]
    public  class TutorialStepWallActionDefinition : TutorialActionDefinition
    {
        [SerializeField] private TutorialTargetId targetId;
        
        
        /// <summary>
        /// Отключаем коллайдер который блокирует зомби
        /// </summary>
        public override void Evaluate()
        {
            var targetRegistry = ServiceLocator.Current.Get<TutorialTargetRegistry>();
            if (targetRegistry.TryGetTarget(targetId, out var target))
            {
                target.WorldAnchor.gameObject.SetActive(false);
                AstarPath.active.Scan();
            }
        }
    }
}