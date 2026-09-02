
using UnityEngine;

namespace Galactic1.Structure
{
    [System.Serializable]
    public struct ObjectPoolParam
    {
        public int InitialSize;
        public int MaxSize; // hard cap — overflow guard
        
        [Header("0 = объект сам вернется в пул (Duration игнорируется)")]
        public float Duration; 
        
    }
}