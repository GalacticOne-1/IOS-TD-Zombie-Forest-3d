using System;
using Galactic1.Code.Systems.Tutorial.Authoring;

namespace Galactic1.Code.Systems.Tutorial.Objectives
{
    public interface IGameLoopStateQuery
    {
        TutorialStepDomain CurrentDomain { get; }

        /// <summary>Fires on every GameLoopStateMachine.OnStateChanged, carrying
        /// (previous, current) domain — required for transition-based semantics
        /// (DomainTransitionObjective), distinct from CurrentDomain ("where am I now").</summary>
        event Action<TutorialStepDomain, TutorialStepDomain> OnDomainTransition;
    }
}
