using Galactic1;
using TMPro;
using UnityEngine;

namespace Galactic1
{
    public class Energy : Singleton<Energy>
    {
        public GameObject widget;

        public TextMeshProUGUI costGems;









        public void ShowWidget()
        {
            widget.SetActive(true);
        }

        public void CloseWidget()
        {
            widget.SetActive(false);
        }






        
        
    }
}