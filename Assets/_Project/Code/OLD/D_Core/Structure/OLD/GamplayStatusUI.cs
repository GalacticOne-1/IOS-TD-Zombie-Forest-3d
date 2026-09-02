using System.Collections;
using System.Collections.Generic;
using Galactic1.Localisation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1
{
    public class GamplayStatusUI : Singleton<GamplayStatusUI>
    {
        /*
         *    Панели victory, defeat, new level
         */

        [SerializeField] private GameObject widget, widget2;
        [SerializeField] private Image status;
        [SerializeField] private TextMeshProUGUI statusT, waveT, chapterT, gold, gems;
        [SerializeField] private Sprite victory, defeat;
        [SerializeField] private Color colReg, colDefeat;
        
        
        [SerializeField] private GameObject btnContinue;
        
        
        [SerializeField] private GameObject cardPrefab;
        [SerializeField] private ScrollRect scroll;
        

        [Space]
        public CRevive revive;
        [System.Serializable] 
        public class CRevive
        {
            public GameObject bg;
            public GameObject widget;
        }
        

        private DFunc onContinue;

        List<CNewUnitData> arNewUnits = new List<CNewUnitData>();

        
        List<CRewardData> arReward;
        /// <summary>
        /// Для добавления элемента награды
        /// </summary>
        /// <param name="rew"></param>
        public void AddReward(CRewardData rew) => arReward.Add(rew);









        #region REVIVE

        /// <summary>
        /// Когда игрок уничтожен, предлагаем oживить
        /// </summary>
        public void ShowWidgetRevive()
        {
            revive.bg.SetActive(true);
            revive.widget.SetActive(true);
        }
        /// <summary>
        /// Just close
        /// </summary>
        public void CloseWidgetRevive()
        {
            revive.bg.SetActive(false);
            revive.widget.SetActive(false);
        }

        /// <summary>
        /// game over
        /// </summary>
        public void Revive_no()
        {
            revive.widget.SetActive(false);
            //GAMEPLAY_old.CheckDefeat();
        }
        

        #endregion
        
        
        
        public void ShowWidgetVictory()
        {
            Clear();
            status.sprite = victory;
            statusT.text = ServiceLocator.Current.Get<LocalisationService>().Data.victory;
            StartCoroutine(victory_());
        }
        IEnumerator victory_()
        {
            widget.SetActive(true);
            yield return new WaitForSeconds(.2f);
            //GameManager.StopBattle();
            /*MusicManagement.I.MusicStop();
            AudioController.I.Sound_UI(5);
            GameManager.StopBattle();
            BattleManager.I.onVictory?.Invoke();
            widget.SetActive(true);
            
            
            // REWARD--------------------
            GAMEPLAY.LoadRewardVictory();
            ShowRewardCards();
            
            CheckNewUnits();
            yield return new WaitForSeconds(1f);
            
            Monetization.ShowDealRewardedAD();
            btnContinue.SetActive(true);*/
        }
        
        
        public void ShowWidgetDefeat()
        {
            Clear();
            //widget.GetComponent<Image>().color = colDefeat;
            status.sprite = defeat;
            statusT.text = ServiceLocator.Current.Get<LocalisationService>().Data.defeat;
            StartCoroutine(defeat_());
        }
        IEnumerator defeat_()
        {
            widget.SetActive(true);
            yield return new WaitForSeconds(.2f);
            //GameManager.StopBattle();
            //yield return new WaitForSeconds(1f);
            //MusicManagement.I.MusicStop();
            //AudioController.I.Sound_UI(6);
            
            
            
            // REWARD--------------------
            //GAMEPLAY.LoadRewardDefeat();
            //ShowRewardCards();
            
            //onContinue += () => StartCoroutine(continue_finish());
            //yield return new WaitForSeconds(1f);
            
            //btnContinue.SetActive(true);
        }


        void Clear()
        {
            //widget.GetComponent<Image>().color = colReg;
            //chapterT.text = $"{LocalisationManagement.I.Data.chapter} {(HUBStat.selectedChapter + 1).NumberWithNull()}";
            //waveT.text = $"{(HUBStat.curWave + 1).NumberWithNull()}";
            //SOFT.text = $"{LocalisationManagement.I.Data.get} {HUBStat.softCurrencyInBattle}";
            //HARD.text = $"{LocalisationManagement.I.Data.get} {HUBStat.hardCurrencyInBattle}";
            //CORT.SpeedGame_regular();
            //MusicManagement.I.MusicLobby();
            //MusicManagement.I.MusicStop();
            //arReward = new List<CRewardData>();
            //onContinue = null;
            //ADS.I.CloseDealToRewardedAD();
            //btnContinue.SetActive(false);
            //CanvasCntr.I.ScreenToEndBattle();
            //BattleManager.I.ClearBattlefield();
            //revive.bg.SetActive(true);
            
            //Monetization.ShowDealRewardedAD();
        }
        
        public void CloseWidget()
        {
            widget.SetActive(false);
        }

        
        
        
        /// <summary>
        /// Проигрыш при живых юнитах
        /// </summary>
        public void ShowWidgetDefeat_map()
        {
            widget2.SetActive(true);
        }
        public void CloseWidget2()
        {
            widget2.SetActive(false);
        }
        
        
        
        
    }

    public struct CNewUnitData
    {
        public string title, unit;
        public Sprite icon;
    }

    [System.Serializable]
    public struct CRewardData
    {
        public long volume, current, required;
        public ERewardType type;
        public Sprite icon;
        public string title;
    }
    
    public enum ERewardType
    {
        gold, exp, gems, 
        bones, seal, runes
    }
}