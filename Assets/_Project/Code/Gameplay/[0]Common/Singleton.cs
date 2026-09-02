
using UnityEngine;

namespace Galactic1
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        static T instance;
        public static T I{
            get{
                if(instance == null)
                    instance = FindObjectOfType<T>();

                return instance;
            }
            
        }

        private void Awake()
        {
            instance = FindObjectOfType<T>();
        }
    }
    
}
