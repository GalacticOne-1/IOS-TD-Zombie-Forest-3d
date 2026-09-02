using Galactic1.Code.Gameplay.Combat.Data;
using UnityEngine;

namespace Galactic1.Code.Gameplay.Damage
{
    /// <summary>
    /// Контекст попадания (используется для FX, headshot и т.д.)
    /// </summary>
    public struct HitInfo
    {
        public Vector3 Point;
        public Vector3 Normal;
        public Collider Collider;
        public Transform Transform;
        public BodyPartType BodyPart;   
        public SurfaceType Surface; 
    }
}