using Galactic1.Code.GameDatabase.Registries;
using UnityEngine;

namespace Galactic1.PoolObject
{
    public struct EffectRequest
    {
        public VfxId Id;
        public Vector3 Position;
        public Transform AttachTo; // если нужно "привязать" (например, к оружию)
        public Quaternion Rotation;
        
        /// по дефолту продолжительность берется
        /// <br/>из конфига ObjectPoolParam
        public float Duration;
    }

}