using System;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    [Serializable]
    public sealed class TutorialFixedTargetQuery : TutorialTargetQuery
    {
        public TutorialTargetId targetId;

#if UNITY_EDITOR
        public override bool Validate(out string error)
        {
            if (targetId == null) { error = "TutorialFixedTargetQuery: targetId is empty."; return false; }
            error = null;
            return true;
        }
#endif
    }
}