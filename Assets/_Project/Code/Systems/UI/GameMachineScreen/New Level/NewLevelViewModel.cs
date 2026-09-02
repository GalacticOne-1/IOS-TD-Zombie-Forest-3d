using System.Collections;
using Galactic1.Mobile;
using Galactic1;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1
{
    public class NewLevelViewModel : MVVMViewModel, IScreenT
    {
        private DFunc onAddingReward;

        private bool bonus;
        
        public NewLevelViewModel(MVVMModel _model, MVVMView _view) : base(_model, _view)
        {
            view = _view;
            model = _model;
            
            
            var vw = view as NewLevelView;
            var md = model as NewLevelModel;
            
            
            // subscribtion button
            vw.BConfirm.GetComponent<CoreBtn>()._event.AddListener(CloseWindow);
            vw.BCloseDeal.EventBtn_old(() => vw.CBonusDeal.SetActive(false));
        }


        
        
        
        public void OpenWindow<T>(T data)
        {
            //GAMEPLAY_old.GamePause();
            var vw = view as NewLevelView;
            var d = data as NewLevelModel.CNewLevel;
            
            // * обычное получение награды
            bonus = false;
            onAddingReward = null;
            /*onAddingReward = () =>
            {
                new AddingReward(d.reward, StatController.EUpdateGUI.Off);
                GAMEPLAY.Saving();
                //FX();
            };*/
            // --
            
            // * увеличение всей награды для рекламы
            vw.BAds.GetComponent<CoreBtn>()._event.RemoveAllListeners();
            // vw.BAds.GetComponent<CoreBtn>()._event.AddListener(() => new AD_Request().Rewarded(AnalyticsService.ERequestAd.Player_Rank,
            //     () =>
            //     {
            //         // обnовляем награду после получения бонуса
            //         ServiceLocator.Current.Get<AudioController>().Sound_UI(18);
            //         bonus = true;
            //         new IncreaseReward(ref d.reward, 2);
            //         var l = vw.HoldReward.transform.childCount;
            //         for (int i = 0; i < l; i++)
            //         {
            //             if (vw.HoldReward.GetChild(i).activeSelf)
            //             {
            //                 vw.HoldReward.GetChild(i, 2).GetComponent<TextMeshProUGUI>().text = $"{d.reward[i].volume}";
            //                 vw.HoldReward.GetChild(i, 2).GetComponent<Animator>().SetTrigger("action");
            //             }
            //         }
            //         (view as NewLevelView).CBonusDeal.SetActive(false);
            //         
            //         
            //         /*onAddingReward = () =>
            //         {
            //             new IncreaseReward(ref d.reward, 2);
            //             new AddingReward(d.reward, StatController.EUpdateGUI.Off);
            //             GAMEPLAY.Saving();
            //             FX();
            //             
            //         };
            //         CloseWindow();*/
            //     }));
            // --


            // float fx
            void FX()
            {
                var gfx_data = new ScreenGFXController.CHeapData[d.reward.Length];
                for (int i = 0; i < gfx_data.Length; i++)
                {
                    gfx_data[i].type = d.reward[i].type;
                    gfx_data[i].start = vw.HoldReward.GetChild(i,0).transform.position;
                }
                ServiceLocator.Current.Get<ScreenGFXController>().FloatingHeap(gfx_data);
            }
            
            
            // * load info
            var l = vw.PBox.Length;
            for (byte i = 0; i < l; i++)
                vw.PBox[i].SetActive(false);
            
            // * предложение для бонуса
            new TUTORIAL_Status(out bool notActive);
            //vw.CBonusDeal.SetActive(notActive && new AD_Request().Available());
            vw.BCloseDeal.SetActive(false);

            
            vw.TLevel.text = $"{d.level + 1}";
            vw.TH2.text = d.h2;
            l = vw.HoldReward.transform.childCount;
            if (d.reward == null) d.reward = new AddingReward.CReward[0];
            float width = 0;
            for (int i = 0; i < l; i++)
            {
                vw.HoldReward.GetChild(i).SetActive(false);

                if (i < d.reward.Length)
                {
                    vw.HoldReward.GetChild(i).SetActive(true);

                    new LIB_Convert_AssetKey_To_Id(0, (int)d.reward[i].itemKey, out int id);
                    new LIB_GetAsset_id(
                        0,
                        0,
                        id,
                        out AssetItems assetItems,
                        out InventoryConfigs equipment);
                    // new SetRegularItem(new SetRegularItem.CData()
                    // {
                    //     item = vw.HoldReward.GetChild(i),
                    //     iHeader = assetItems,
                    //     volume = (byte)d.reward[i].volume
                    // });
                    
                    // * item hint
                    var _item = vw.HoldReward.GetChild(i);
                    //_item.EventBtn_DOWN(() => new GetItemHint(assetItems ? assetItems : equipment, _item));
                    
                    

                    // * для добавления награды
                    short volume = (short)d.reward[i].volume;
                    onAddingReward += () =>
                    {
                        // new Inbox_ADD(new CPlayerInventory()
                        // {
                        //     type = 0,
                        //     category = 0,
                        //     id = id,
                        //     volume = (short)(bonus ? volume * 2 : volume)
                        // });
                    };
                }
            }
            //vw.HoldReward.transform.SetSizeGroupH(true);
            
            
            
            
            // * new blueprints
            bool new_blueprints = d.newBlueprints.Count > 0;

            l = vw.CBlueprints.transform.childCount;
            for (int i = 0; i < l; i++)
            {
                if (i >= d.newBlueprints.Count)
                {
                    vw.CBlueprints.GetChild(i).SetActive(false);
                    continue;
                }

                if (i == l - 1)
                {
                    vw.CBlueprints.GetChild(i).SetActive(true);
                    vw.CBlueprints.GetChild(i, 0).GetComponent<TextMeshProUGUI>().text =
                        $"...and {d.newBlueprints.Count - i} more blueprints";
                    break;
                }

                new LIB_GetAssetEquipment(d.newBlueprints[i], out InventoryConfigs equipment);
                vw.CBlueprints.GetChild(i).SetActive(true);
                vw.CBlueprints.GetChild(i, 0).GetComponent<Image>().sprite = equipment.Header.Icon;
                vw.CBlueprints.GetChild(i, 0).GetComponent<Image>().gameObject.SetUISize(Vector2.one *  equipment.Header.SizeUI*1.2f);
                
                // * item hint
                var _item = vw.CBlueprints.GetChild(i);
               // _item.EventBtn_DOWN(() => new GetItemHint(equipment, _item));
            }
            //vw.CBlueprints.transform.SetSizeGroupH(true);
            
            
            
            
            // ***      добавляем в очередь контента
            /*ServiceLocator.Current.Get<ContentQueueController>().AddQueue(new ContentQueueController.CWidgetSystem()
            {
                order = 9,
                //menu = EMainMenu.HOME,
                widget = vw.gameObject,
                typeContent = ContentQueueController.EContent.WIDGET,
                func = () =>
                {
                    vw.Show();
                    view.StartCoroutine(open(unlocked_unit));
                }
            });*/
            vw.Show();
            view.StartCoroutine(open(new_blueprints));
        }

        public void CloseWindow()
        {
            //GAMEPLAY_old.GameContinue();
            (view as NewLevelView).Hide();
            //GAMEPLAY_old.GameContinue();
            
            //Review.I.RequestReview();
            
            // *** при закрытии передаем награду
            onAddingReward?.Invoke();
        }


        IEnumerator open(bool new_blueprints)
        {
            var vw = view as NewLevelView;

            yield return new WaitForSeconds(.3f);

            var l = vw.PBox.Length;
            for (byte i = 0; i < l; i++)
            {
                if (i != 2 || new_blueprints)
                {
                    vw.PBox[i].SetActive(true);
                    yield return new WaitForSeconds(.6f);
                }
            }
            
            yield return new WaitForSeconds(1f);
            
            vw.BCloseDeal.SetActive(true);
        }
    }
}