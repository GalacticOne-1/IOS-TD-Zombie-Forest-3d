using Galactic1;
using UnityEngine;

namespace Galactic1
{
    public class FreeGems : Singleton<FreeGems>
    {
        public GameObject widget;









        public void ShowWidget()
        {
            //widget.SetActive(true);
            //MainMenu.I.SelectMenu(0);
        }

        public void CloseWidget()
        {
            widget.SetActive(false);
        }

    }
}