using Galactic1.EntryPoint;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{

    [CreateAssetMenu(fileName = "TutorialStartCampActionDefinition",
        menuName = "Game Configs/Tutorial/Actions/Load Camp")]
    public class TutorialStartCampActionDefinition : TutorialActionDefinition
    {
        public override void Evaluate()
        {
            new NewGameEntry().StartCamp();
        }
    }
}