using Galactic1.Code.Systems.Tutorial.Authoring;
using Galactic1.Code.Systems.Tutorial.Presentation;
using UnityEngine;

namespace Galactic1.Code.UI.UnitCard
{
    public class TutorialUnitCardView : MonoBehaviour
    {
        [SerializeField] private TutorialTargetId targetGunEmpty;
        [SerializeField] private TutorialTargetId targetGun;
        [SerializeField] private TutorialTargetId targetAbility;
        [SerializeField] private TutorialTargetId targetAbilitySlot;

        [Space]
        [SerializeField] private GameObject gunEmptyButton;
        [SerializeField] private GameObject gunButton;
        [SerializeField] private GameObject abilityButton;
        [SerializeField] private GameObject abilitySlotButton;

        
        
        public void Initialize()
        {
            var targetBehaviour = gunEmptyButton.AddComponent<TutorialTargetProgramaticBehaviour>();
            targetBehaviour.Initialize(targetGunEmpty);
            
            targetBehaviour = gunButton.AddComponent<TutorialTargetProgramaticBehaviour>();
            targetBehaviour.Initialize(targetGun);
            
            targetBehaviour = abilityButton.AddComponent<TutorialTargetProgramaticBehaviour>();
            targetBehaviour.Initialize(targetAbility);
            
            targetBehaviour = abilitySlotButton.AddComponent<TutorialTargetProgramaticBehaviour>();
            targetBehaviour.Initialize(targetAbilitySlot);
        }
    }
}