
using Galactic1;
using UnityEngine;
using UniRx;
using UnityEngine.UI;

namespace Galactic1
{
    public class MainMenuModel : MVVMModel
    {
        
        public sbyte cur_mark;
        

        private CMainMenu[] menu_data;
        public CMainMenu[] MenuData => menu_data;
        
        public struct CMainMenu
        {
            public ReactiveProperty<bool> available;
            public ReactiveProperty<short> flag_request;
            public short required_level;
        }
        
        public float widthPanel => (view as MainMenuView).Holder.GetComponent<RectTransform>().rect.width;

        
        
        
        
        
        public MainMenuModel(MVVMView _view) : base(_view)
        {
            view = _view;

            // var required = ServiceLocator.Current.Get<ProgressController>().main_menu;
            // menu_data = new CMainMenu[required.Length];
            // for (int i = 0; i < menu_data.Length; i++)
            // {
            //     menu_data[i] = new CMainMenu();
            //     menu_data[i].available = new();
            //     menu_data[i].flag_request = new();
            //     menu_data[i].required_level = required[i];
            // }
        }

        
        /// <summary>
        /// Загружает доступ к кнопкам
        /// </summary>
        /// <param name="level"></param>
        public override void LoadAccess(int level)
        {
            var l = menu_data.Length;
            for (byte i = 0; i < l; i++)
            {
                menu_data[i].available.Value = menu_data[i].required_level <= level;
            }
        }

        
        
        
        
        
        
        /// <summary>
        /// Состояние кнопки (открыта/закрыта)
        /// </summary>
        public void SetAccess(sbyte i)
        {
            var el = (view as MainMenuView).ar_item[i];
            if (menu_data[i].available.Value)
            {
                //if (!GAMEPLAY_old.DataGameplay().mainMenu_flag_access[i])
                    menu_data[i].flag_request.Value = 1;
                
                // el.icon.sprite = (view as MainMenuView).Menu[i].icon;
                // el.item.SetMaterialFlash(0);
                // el.icon.SetMaterialFlash(0);
                // el.item.gameObject.GetChild(0).GetComponent<Image>().SetMaterialFlash(0);
                // el.item.gameObject.GetChild(1).GetComponent<Image>().SetMaterialFlash(0);
            }
            else
            {
                // el.icon.sprite = ServiceLocator.Current.Get<IconHub>().closeLock;
                // el.item.SetMaterialFlash(.4f, Globals.color_lock_black);
                // el.icon.SetMaterialFlash(.4f, Globals.color_lock_black);
                // el.item.gameObject.GetChild(0).GetComponent<Image>().SetMaterialFlash(.4f, Globals.color_lock_black);
                // el.item.gameObject.GetChild(1).GetComponent<Image>().SetMaterialFlash(.4f, Globals.color_lock_black);
            }
        }

        /// <summary>
        ///  !новое!
        /// </summary>
        public void SetFlag(sbyte i) 
            => (view as MainMenuView).ar_item[i].flag.SetActive(menu_data[i].flag_request.Value > 0);

    }
}