using UnityEngine;

namespace Galactic1
{
    public class _ID : MonoBehaviour, Iid
    {
        /*
         *      Легкий идентификатор
         */
        
        public int ID { set; get; }
        
        public GameObject obj => gameObject;
        
        
    }

    public interface Iid
    {
        public int ID { set; get; }

        public GameObject obj { get; }
    }
}