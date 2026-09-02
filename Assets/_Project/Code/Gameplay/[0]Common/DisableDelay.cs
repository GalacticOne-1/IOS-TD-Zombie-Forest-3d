
using UnityEngine;


namespace Galactic1
{
    public class DisableDelay : MonoBehaviour
    {

        public float timer = 1;
        void OnEnable()
        {
            Invoke("Hide", timer);
        }

        void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}