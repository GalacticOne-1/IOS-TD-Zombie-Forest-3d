using UnityEngine;

namespace Galactic1.Repository
{
    public abstract class Repository
    {
        public abstract void Register(string withId, GameObject entity);
        public abstract void Unregister(string withId, GameObject entity);
        
        public abstract GameObject GetCloseet();
    }
}