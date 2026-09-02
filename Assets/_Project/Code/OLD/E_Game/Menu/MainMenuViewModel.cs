
using System.Collections;
using Galactic1;
using Galactic1.Mobile;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1
{
    public class MainMenuViewModel : MVVMViewModel, IStateMachineGUI, IContentAccess, IFlagController
    {
        
        public enum EMainMenu
        {
            CONSTRUCT, INVENTOY, CRAFT, UNIT, DEPOT, MAP, SHOP
        }
        
        #region BASIC

        public EMainMenu CurMenu => EMainMenu.CONSTRUCT; //(EMainMenu)(model as MainMenuModel).cur_mark;
        
        public GUIBaseState currentState { get; set; }
        
        private MainMenuState1 state1;
        private MainMenuState2 state2;
        private MainMenuState3 state3;
        private MainMenuState4 state4;
        private MainMenuState5 state5;


        public MainMenuView View => view as MainMenuView;
        

        public MainMenuViewModel(MVVMModel _model, MVVMView _view) : base(_model, _view)
        {
            model = _model;
            view = _view;
            
            
            // *** INIT STATES
            state1 = new MainMenuState1();
            state2 = new MainMenuState2();
            state3 = new MainMenuState3();
            state4 = new MainMenuState4();
            state5 = new MainMenuState5();
            //Initialize(state3);
            
            
            
            // *** CREATE BUTTONS
            var vw = view as MainMenuView;
            var md = model as MainMenuModel;
            var l = md.MenuData.Length;
            vw.ar_item = new MainMenuView.CView[l];
            
            for (sbyte i = 0; i < l; i++)
            {
                // card
                var _i = i;
                var item = vw.StaticMenu ? vw.Holder.GetChild(i) : vw.Prefab.CreateGO(vw.Holder.transform);
                item.GetComponent<CoreBtn>()._event.AddListener(() => vw.SelectMenu(_i));

                // загружаем элемент массива 
                vw.ar_item[i].rt = item.GetComponent<RectTransform>();
                vw.ar_item[i].item = item.GetComponent<Image>();
                vw.ar_item[i].icon = item.GetChild(0).GetComponent<Image>();
                //vw.ar_item[i].title = item.GetChild(1).GetComponent<TextMeshProUGUI>();
                
                // flag
                vw.ar_item[i].flag = "Flag_new".CreateGO(vw.ar_item[i].item.transform);
                vw.ar_item[i].flag.SetUIPosition(new Vector2(-30, 0));
                vw.ar_item[i].flag.SetActive(false);
                
                // устанавливаем значeния
                vw.ar_item[i].icon.sprite = vw.Menu[i].icon;
                //vw.ar_item[i].title.text = ServiceLocator.Current.Get<LocalisationController>().GetTextGame($"menu_{i}");
                
                // subscribtion
                md.MenuData[i].available.Subscribe(_ => {  md.SetAccess(_i); }).AddTo(_disposables);
                
                if(vw.UseFlags)
                {
                    md.MenuData[i].flag_request.Subscribe(_ => { md.SetFlag(_i); }).AddTo(_disposables);
                }
            }

            
        }

        public override void ResetState()
        {
            (model as MainMenuModel).cur_mark = -1;
            //SelectMenu((int)EMainMenu.HOME);            // при старте включаем главный экран
        }
        

        public override void LoadAccess(int level) => model.LoadAccess(level);
        
        
        public MainMenuView.CView GetViewItem(sbyte i) => (view as MainMenuView).ar_item[i];

        #endregion



        #region STATE MACHINE


        public void Initialize(GUIBaseState state)
        {
            currentState = state;
            currentState?.Enter();
        }

        
        public void SelectState(GUIBaseState newState)
        {
            if(currentState != null)
            {
                currentState.Exit();
                (currentState as MainMenuStateScr).OutFocus();
            }

            currentState = newState;
            currentState.Enter();
        }
        

        #endregion



        #region FLAG

        public void AddFlag(sbyte i)
        {   
            short request = (model as MainMenuModel).MenuData[i].flag_request.Value;
            //new FlagState(true, ref request, "Main Menu");
            (model as MainMenuModel).MenuData[i].flag_request.Value = (sbyte)request;
        }

        public void RemoveFlag(sbyte i)
        {
            short request = (model as MainMenuModel).MenuData[i].flag_request.Value;
            //new FlagState(false, ref request, "Main Menu");
            (model as MainMenuModel).MenuData[i].flag_request.Value = (sbyte)request;
        }

        #endregion





        #region BUTTONS
        

        public void SelectMenu(sbyte i)
        {
            //DLog.Alert($"selected menu >>> {i}", "yellow");
            var md = model as MainMenuModel;
            if (!md.MenuData[i].available.Value ) return; // || md.cur_filtr == i
            
            md.cur_mark = i;
            
            // отключаем флаг если зашли впервые в открытое меню
            // if (!GAMEPLAY_old.DataGameplay().mainMenu_flag_access[i])
            // {
            //     GAMEPLAY_old.DataGameplay().mainMenu_flag_access[i] = true;
            //     RemoveFlag(i);
            // }

            switch ((EMainMenu)md.cur_mark)
            {
                case EMainMenu.CONSTRUCT:
                {
                    SelectState(state1);
                } break;
                
                case EMainMenu.INVENTOY:
                {
                    SelectState(state2);
                } break;
                
                case EMainMenu.CRAFT:
                {
                    SelectState(state3);
                } break;
            }
            

            // CORT.BlockScreen(true);
            // view.StopAllCoroutines();
            // view.StartCoroutine(content(new Vector2((view as MainMenuView).Menu[i].widgetCoord, 0)));
            // view.StartCoroutine(size_btn());
        }
        
        
        IEnumerator size_btn()
        {
            var vw = view as MainMenuView;
            var md = model as MainMenuModel;
            
            var l = md.MenuData.Length;
            float scr = md.widthPanel;
            float select_w = .3f;
            //Debug.Log("Width "+scr);
            float w = (scr - scr * select_w) / (l - 1);
            //Debug.Log("Width 2 "+w);

            // расчитываем размер кнопок
            float[] sz_x = new float[l];
            float[] sz_y = new float[l];
            for (int i = 0; i < l; i++)
            {
                if (i == md.cur_mark)
                {
                    sz_x[i] = scr * select_w;
                    sz_y[i] = 220;
                    vw.ar_item[i].title.enabled = true;
                    //vw.ar_item[i].item.color = Color.white;
                    vw.ar_item[i].item.SetShaiderFlash(0);
                    vw.ar_item[i].icon.SetShaiderFlash(0);
                    vw.ar_item[i].item.gameObject.GetChild(0).GetComponent<Image>().SetShaiderFlash(0);
                    vw.ar_item[i].item.gameObject.GetChild(1).GetComponent<Image>().SetShaiderFlash(0);
                }
                else
                {
                    sz_x[i] = w;
                    sz_y[i] = 200;
                    if (md.MenuData[i].available.Value)
                    {
                        //vw.ar_item[i].item.color = vw.Dark;
                        vw.ar_item[i].item.SetShaiderFlash(.2f, AppConstants.color_lock_black);
                        vw.ar_item[i].icon.SetShaiderFlash(.2f, AppConstants.color_lock_black);
                        vw.ar_item[i].item.gameObject.GetChild(0).GetComponent<Image>().SetShaiderFlash(.2f, AppConstants.color_lock_black);
                        vw.ar_item[i].item.gameObject.GetChild(1).GetComponent<Image>().SetShaiderFlash(.2f, AppConstants.color_lock_black);
                    }
                    vw.ar_item[i].title.enabled = false;
                }
            }
            
            
            
            // расчитываем позицию
            float[] x = new float[l];
            float[] y = new float[l];
            
            for (int i = 0; i < l; i++)
            {
                x[i] = i == 0 
                    ? sz_x[i] / 2 
                    : (sz_x[i - 1] + sz_x[i]) / 2 + x[i - 1];     // (ширина своя + предыдущего) / 2 + поз предыдущего

                y[i] = i == md.cur_mark ? 10 : 0;
            }

            
            
            // двигаем
            Vector2 size, pos;
            float time = 0;
            
            while (time < vw.durationButton)
            {

                float t = time / vw.durationButton;
                //t = t * t * (3f - 2f * t);
                time += Time.deltaTime;

                for (int i = 0; i < l; i++)
                {
                    size = vw.ar_item[i].rt.sizeDelta;
                    size.x = Mathf.Lerp(vw.ar_item[i].rt.sizeDelta.x, sz_x[i], t);
                    size.y = Mathf.Lerp(vw.ar_item[i].rt.sizeDelta.y, sz_y[i], t);
                    vw.ar_item[i].rt.sizeDelta = size;

                    pos = vw.ar_item[i].rt.anchoredPosition;
                    pos.x = Mathf.Lerp(vw.ar_item[i].rt.anchoredPosition.x, x[i], t);
                    pos.y = Mathf.Lerp(vw.ar_item[i].rt.anchoredPosition.y, y[i], t);
                    vw.ar_item[i].rt.anchoredPosition = pos;
                }

                yield return null;
            }
            
            for (int i = 0; i < l; i++)
            {
                size = vw.ar_item[i].rt.sizeDelta;
                size.x = vw.ar_item[i].rt.sizeDelta.x;
                size.y = vw.ar_item[i].rt.sizeDelta.y;
                vw.ar_item[i].rt.sizeDelta = size;
                    
                pos = vw.ar_item[i].rt.anchoredPosition;
                pos.x = vw.ar_item[i].rt.anchoredPosition.x;
                pos.y = vw.ar_item[i].rt.anchoredPosition.y;
                vw.ar_item[i].rt.anchoredPosition = pos;
            }
            
            //ScreenProfiler.AddMessage(">>>> "+time);
            //DLog.Alert(">>>> "+time);
        }

        IEnumerator content(Vector2 target)
        {
            var vw = view as MainMenuView;
            Vector2 coord = vw.HolderMid.transform.localPosition;
            float time = 0;
            
            //DLog.Alert("<<  >> ");
            while (time < vw.durationMovement)
            {
                
                coord.x = Mathf.Lerp(coord.x, target.x, time / vw.durationMovement);
                time += Time.deltaTime;
                if (time > .3f) time *= 1.2f;       // ускоряем когда осталось небольшое расстояние
                
                vw.HolderMid.transform.localPosition = coord;
                
                yield return null;
            }
            vw.HolderMid.transform.localPosition = target;
            //DLog.Alert(">>>> "+time, "yellow");
            (currentState as MainMenuStateScr).InFocus(); 
            //ServiceLocator.Current.Get<ContentQueueController>().LaunchQueueDelay((EMainMenu)(model as MainMenuModel).cur_filtr);
            CORT.BlockScreen(false);
        }

        

        #endregion


        
    }
}