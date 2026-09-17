using System;
using Galactic1.Code.GameDatabase.Registries;

namespace Galactic1.Code.Systems.Tutorial.Authoring
{
    [Serializable]
    public sealed class TutorialFacilityCardQuery : TutorialTargetQuery
    {
        public ItemId facilityItemId;

#if UNITY_EDITOR
        public override bool Validate(out string error)
        {
            if (facilityItemId == null) { error = "TutorialFacilityCardQuery: facilityItemId is empty."; return false; }
            error = null;
            return true;
        }
#endif
    }
}