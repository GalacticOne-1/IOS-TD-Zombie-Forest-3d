using Galactic1;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1
{
    public class MainMenuView : MVVMView
    {

        [SerializeField] private GameObject holderMid;
        public GameObject HolderMid => holderMid;


        [SerializeField] private bool staticMenu;
        public bool StaticMenu => staticMenu;
        
        [SerializeField] private CMenu[] menu;
        public CMenu[] Menu => menu;
        
        [System.Serializable]
        public struct CMenu
        {
            public Sprite icon;
            public int widgetCoord;
        }

        [SerializeField] private bool useFlags;
        public bool UseFlags => useFlags;
        
        [SerializeField] private int[] widgetCoord;
        //public int[] WidgetCoord => widgetCoord;


        [Space]
        [SerializeField] private GameObject prefab;
        public GameObject Prefab => prefab;
        [SerializeField] private GameObject holder;
        public GameObject Holder => holder;

        [SerializeField] private Color dark, close;
        public Color Dark => dark;
        public Color Close => close;

        
        [Tooltip("Плавность изменения кнопки")]
        public float durationButton = .4f;

        public float durationMovement = .7f;
        
        
        
        
        public CView[] ar_item;
        public struct CView
        {
            public RectTransform rt;
            public Image item, icon;
            public TextMeshProUGUI title;
            public GameObject flag;
        }

        


        public void SelectMenu(sbyte id)
        {
            (presenter as MainMenuViewModel).SelectMenu(id);
        }

    }
}