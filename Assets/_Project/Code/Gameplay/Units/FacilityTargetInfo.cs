using Galactic1.Code.Systems.Raid;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Units
{
    public class FacilityTargetInfo : TargetInfoBase
    {
        private Collider _collider;


        public override void Initialize(IUnitSceneContext unit)
        {
            base.Initialize(unit);

            _collider = GetComponent<Collider>();
        }

        public override Vector3 GetClosestPoint(Vector3 fromPosition)
        {
            if (_collider == null)
                return transform.position;

            return _collider.ClosestPoint(fromPosition);
        }
    }
}