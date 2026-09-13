using UnityEngine;
using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Authoring.Guidance
{
    /// <summary>Условие вида "сейчас находимся в домене X" — переиспользует ту же
    /// retroactive-семантику, что GameLoopDomainReachedObjective (не transition-семантику
    /// DomainTransitionObjective): guidance должен корректно резолвиться и в случае, если
    /// игрок уже находится в нужном домене на момент активации шага.</summary>
    [CreateAssetMenu(fileName = "Guidance_GameLoopDomain",
        menuName = "Game Configs/Tutorial/Guidance Conditions/Game Loop Domain")]
    public sealed class GameLoopDomainGuidanceConditionDefinition : TutorialGuidanceConditionDefinition
    {
        public override string ConditionTypeId => "GameLoopDomain";

        public TutorialStepDomain domain;
    }
}
