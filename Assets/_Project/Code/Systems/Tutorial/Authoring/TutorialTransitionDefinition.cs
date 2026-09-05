using System;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    /// <summary>
    /// Один переход из шага в графе тутора. Список переходов шага упорядочен: первый
    /// переход, чьё condition == null или Evaluate() == true, побеждает.
    /// </summary>
    [Serializable]
    public sealed class TutorialTransitionDefinition
    {
        [Tooltip("stepId следующего шага. Null = терминальный переход.")]
        public TutorialStepId nextStepId;

        [SerializeReference]
        public ITutorialTransitionCondition condition;

        public bool IsTerminal => nextStepId == null;
        public bool IsSatisfied() => condition == null || condition.Evaluate();
    }
}
