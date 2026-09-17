using System;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    [Serializable]
    public sealed class TutorialInboxItemQuery : TutorialTargetQuery
    {
        public ItemId itemId;

#if UNITY_EDITOR
        public override bool Validate(out string error)
        {
            if (itemId == null) { error = "TutorialInboxItemQuery: itemId is empty."; return false; }
            error = null;
            return true;
        }
#endif
    }
}