using System.Collections;
using Galactic1;
using Galactic1.Mobile;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1
{
    public class FinishLevelModel : MVVMModel
    {
        
        
        public class CData
        {
            public string status;
            public string night;
            
            public AddingReward.CReward[] reward;
        }
        
        public DFunc onAddingReward;
        private bool bonus;
        
        
        public FinishLevelModel(MVVMView _view) : base(_view)
        {
            view = _view;
            
            (view as FinishLevelView).BDealClose.EventBtn_old(() =>
            {
                (view as FinishLevelView).CloseBonus.SetActive(true);
                (view as FinishLevelView).CDeal.SetActive(false);
                (view as FinishLevelView).BConfirm.SetActive(true);
            });
        }
        



        public void Load(CData data)
        {
            var vw = view as FinishLevelView;
            
            bonus = false;
            onAddingReward = null;
            
            
            // * обычное получение награды
            ServiceLocator.Current.Get<GameMachine>().ar_reward = data.reward;
            
            // * увеличение всей награды для рекламы
            vw.BAds.GetComponent<CoreBtn>()._event.RemoveAllListeners();
            vw.BAds.GetComponent<CoreBtn>()._event.AddListener(() =>
            {
                // if (GAMEPLAY_old.CurrentStage > 1)
                // {
                //     Monetization.AD_Regular_Launch(ANALITICS_FOR_ADS.ERequest.After_Battle_Money,
                //         () =>
                //         {
                //             Monetization.rewardedADAfterBattle = true;
                //             //ServiceLocator.Current.Get<ADSController>().PauseInterReset();
                //             bonus = true;
                //             //new IncreaseReward(ref data.reward[0], 2);
                //             //ServiceLocator.Current.Get<GameMachine>().ar_reward = data.reward;
                //             CloseWindow();
                //         });
                // }
                // else
                // {
                //     bonus = true;
                //     CloseWindow();
                // }
            });
            // --
            
            
            vw.Circle.SetActive(false);
            vw.HoldReward.SetActive(false);
            vw.HoldReward2.SetActive(false);
            vw.BgBonus.SetActive(false);
            vw.CloseBonus.SetActive(false);
            vw.CDeal.SetActive(false);
            vw.BConfirm.SetActive(false);

            // load info
            vw.TStatus.text = data.status;
            vw.TNight.text = data.night;
            
            // *** load reward BONUS
            var l = vw.HoldReward2.transform.childCount;
            if (data.reward == null) data.reward = new AddingReward.CReward[0];
            
            for (int i = 0; i < 3; i++)
            {
                vw.HoldReward2.GetChild(i).SetActive(false);

                //if (i < data.reward.Length && data.reward[i].volume > 0)
                {
                    vw.HoldReward2.GetChild(i).SetActive(true);
                    
                    new LIB_Convert_AssetKey_To_Id(i == 1 ? 1 : 0,
                        i == 1 ? (int)data.reward[i].equipKey : (int)data.reward[i].itemKey, 
                        out int id);
                    new LIB_GetAsset_id(
                        i == 1 ? 1 : 0,
                        0,
                        id,
                        out AssetItems assetItems,
                        out InventoryConfigs equipment);
                    
                    
                    
                    if (i == 0)     // hard
                    {
                        vw.HoldReward2.GetChild(i,0).SetActive(false);
                        vw.HoldReward2.GetChild(i,2).GetComponent<TextMeshProUGUI>().text = data.reward[i].volume.ToString();
                        // vw.HoldReward2.GetChild(i, 1).GetComponent<Image>().sprite =
                        //     ServiceLocator.Current.Get<IconHub>().GetSpriteStat(data.reward[i].type);
                        
                        onAddingReward += () =>
                        {
                            //if (bonus)
                                //new AddingReward(ServiceLocator.Current.Get<GameMachine>().ar_reward[0]);
                        };
                    }
                    
                    else if (i == 1) // medikit / repair
                    {
                        vw.HoldReward2.GetChild(i,0).SetActive(false);
                        vw.HoldReward2.GetChild(i,2).GetComponent<TextMeshProUGUI>().text = data.reward[i].volume.ToString();
                        // vw.HoldReward2.GetChild(i, 1).GetComponent<Image>().sprite = !data.reward[i].isEquipment
                        //     ? ServiceLocator.Current.Get<IconHub>().GetSpriteStat(data.reward[i].type)
                        //     : equipment.Header.Icon;
                        
                        // * item hint
                        var _item = vw.HoldReward2.GetChild(i);
                        //_item.EventBtn_DOWN(() => new GetItemHint(assetItems ? assetItems : equipment, _item));
                        
                        // * для добавления награды
                        if (data.reward[i].equipKey == EEquipment.Repair_Complect)
                        {
                            onAddingReward += () =>
                            {
                                //if (bonus)
                                    //new AddingReward(ServiceLocator.Current.Get<GameMachine>().ar_reward[1]);
                            };
                        }
                        else
                        {
                            onAddingReward += () =>
                            {
                                // if (bonus)
                                //     new Inbox_ADD(new CPlayerInventory()
                                //     {
                                //         type = 1,
                                //         category = 0,
                                //         id = id,
                                //         volume = (short)data.reward[1].volume
                                //     });
                            };
                        }
                    }

                    else
                    {
                        
                        // new SetRegularItem(new SetRegularItem.CData()
                        // {
                        //     item = vw.HoldReward2.GetChild(i),
                        //     iHeader = assetItems,
                        //     volume = (byte)data.reward[i].volume
                        // });
                    
                        // * item hint
                        var _item = vw.HoldReward2.GetChild(i);
                        //_item.EventBtn_DOWN(() => new GetItemHint(assetItems ? assetItems : equipment, _item));
                    
                    

                        // * для добавления награды
                        short volume = (short)data.reward[i].volume;
                        onAddingReward += () =>
                        {
                            // if (bonus)
                            //     new Inbox_ADD(new CPlayerInventory()
                            //     {
                            //         type = 0,
                            //         category = 0,
                            //         id = id,
                            //         volume = volume //(short)(bonus ? volume * 2 : volume)
                            //     });
                        };
                    }
                }
            }
            
            // *** regular reward
            for (int i = 0; i < 2; i++)
            {
                new LIB_Convert_AssetKey_To_Id(0, (int)data.reward[i+3].itemKey, out int id);
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
                //     volume = (byte)data.reward[i+3].volume
                // });
                    
                // * item hint
                var _item = vw.HoldReward.GetChild(i);
               //_item.EventBtn_DOWN(() => new GetItemHint(assetItems ? assetItems : equipment, _item));
                    
                    

                // * для добавления награды
                short volume = (short)data.reward[i+3].volume;
                onAddingReward += () =>
                {
                    // new Inbox_ADD(new CPlayerInventory()
                    // {
                    //     type = 0,
                    //     category = 0,
                    //     id = id,
                    //     volume = volume//(short)(bonus ? volume * 2 : volume)
                    // });
                };
            }
            //vw.HoldReward.transform.SetSizeGroupH(true);
            
            vw.Show();
            vw.StartCoroutine(open(data));
        }

        IEnumerator open(CData data)
        {
            var vw = view as FinishLevelView;

            yield return new WaitForSeconds(.5f);
            
            vw.Circle.SetActive(true);
            vw.HoldReward.SetActive(true);
            
            vw.HoldReward2.SetActive(true);
            vw.BgBonus.SetActive(true);
            yield return new WaitForSeconds(.5f);
            
            // * предложение рекламы если есть мин. сумма
            //if(data.reward[2].volume >= ServiceLocator.Current.Get<CoreEconomicController>().min_gold_for_ads)
            // if(new AD_Request().Available())
            // {
            //     vw.CDeal.SetActive(true);
            //     //yield return new WaitForSeconds(.5f);
            // }
            // else
            // {
            //     vw.BConfirm.SetActive(true);
            // }
        }

        // после закрытия окна, передаем награду без обновления GUI
        // обновление будет после снятия загрзочного экрана в лобби
        public void CloseWindow()
        {
            // *** при закрытии передаем награду
            //if (bonus)
                //new AddingReward(ServiceLocator.Current.Get<GameMachine>().ar_reward[0]);
            onAddingReward?.Invoke();
            //GAMEPLAY_old.Saving();
            view.Hide();
            
            //ServiceLocator.Current.Get<GameMachine>().Camp_Exit();
            
            //new EVENT_CUTSCENE().Begin();
            
            //new AD_Request().Rewarded(AnalyticsService.ERequestAd.After_Battle_Money, null);
        }
        
    }
}