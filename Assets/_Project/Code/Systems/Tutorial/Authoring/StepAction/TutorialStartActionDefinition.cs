
using Galactic1.EntryPoint;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{

    [CreateAssetMenu(fileName = "TutorialStartActionDefinition",
        menuName = "Game Configs/Tutorial/Actions/Tutorial Start")]
    public class TutorialStartActionDefinition : TutorialActionDefinition
    {
        public override void Evaluate()
        {
            new NewGameEntry().StartBattle("");
        }
    }
}