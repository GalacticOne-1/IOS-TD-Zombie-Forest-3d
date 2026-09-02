using UnityEngine;

namespace Galactic1.Code.Gameplay.Weapons.View
{
    public class TracerProjectile : BaseProjectile
    {
        
        
        
        protected override void ProcessHit(Collider collider, Vector3 point, Vector3 normal)
        {
            if (!IsSpawned || !_launched) return;
            

            ReturnToPool();
        }
    }
}