using System;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    [Serializable]
    public sealed class TutorialUnitSearchQuery : TutorialTargetQuery
    {
        public TutorialUnitSearchCriteria criteria = new();

#if UNITY_EDITOR
        public override bool Validate(out string error)
        {
            if (criteria == null) { error = "TutorialUnitSearchQuery: criteria is empty."; return false; }
            error = null;
            return true;
        }
#endif
    }
}