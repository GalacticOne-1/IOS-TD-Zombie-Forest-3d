using UnityEngine;

namespace Galactic1
{
    public class Cutscene : MonoBehaviour
    {




        public void AnimShow(bool y)
        {
            GetComponent<Animator>().SetBool("Hide", !y);
        }
    }
}