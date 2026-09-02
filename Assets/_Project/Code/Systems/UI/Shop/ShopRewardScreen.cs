using System.Collections;
using System.Collections.Generic;
using Galactic1.Code;
using Galactic1.Code.Dev;
using Galactic1.Code.GameDatabase;
using Galactic1.Configs;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.UI.Shop.Rewards
{
    /// <summary>
    /// Экран/попап наград магазина.
    /// Владеет корутиной последовательного показа карточек.
    /// Карточки отвечают только за визуал.
    /// </summary>
    public class ShopRewardScreen : UIScreenPanel, IGameService
    {
        [SerializeField] private GameObject bgCloseButton;
        [SerializeField] private Transform cardsRoot;
        [SerializeField] private GameObject continueLabel;

        private UIRootView _uiRoot;
        private readonly List<ShopRewardCard> activeCards = new();
        
        
        
        
        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);
            ServiceLocator.Current.Register(this);
            
            _uiRoot = ServiceLocator.Current.Get<UIRootView>();

            bgCloseButton.RegisterButtonClick(OnHide);

            //DebugScreen();


            var l = cardsRoot.childCount;
            for (int i = 0; i < l; i++)
            {
                activeCards.Add(cardsRoot.GetChild(i).GetComponent<ShopRewardCard>());
            }
            
            OnHide();
        }

        public override void Remove()
        {
            base.Remove();
            ServiceLocator.Current.Unregister<ShopRewardScreen>();
        }

        public override void OnHide()
        {
            base.OnHide();
            HideCards();
            gameObject.SetActive(false);
            continueLabel.SetActive(false);
        }

        public override void OnShow(object data = null)
        {
            base.OnShow(data);
            if(data is List<ShopRewardItemData> rewards)
            {
                gameObject.SetActive(true);
                StartCoroutine(ShowRewards(rewards));
            }
        }

        public IEnumerator ShowRewards(List<ShopRewardItemData> rewards)
        {
            _uiRoot.EnableBlockScreen();
            
            // #1 передаем награду
            int count = Mathf.Min(rewards.Count, activeCards.Count);
            for (int i = 0; i < count; i++)
            {
                activeCards[i].Bind(rewards[i]);
            }
            
            // #2 включаем карточку
            for (int i = 0; i < count; i++)
            {
                bool finished = false;
                activeCards[i].Show(() => finished = true);

                yield return new WaitUntil(() => finished);
                //yield return new WaitForSeconds(revealDelay);
            }
            
            yield return new WaitForSeconds(.5f);
            
            continueLabel.SetActive(true);
            _uiRoot.DisableBlockScreen();
        }

        private void HideCards()
        {
            for (int i = activeCards.Count - 1; i >= 0; i--)
                activeCards[i].Hide();
        }
        
        
        
        
        // для теста панели
        private void DebugScreen()
        {
            DebugInputService.I.On(KeyCode.R, () =>
            {
                var startKitData = ServiceLocator.Current.Get<ConfigProvider>().Get<StartKitData>();
                var kit = startKitData.GetKit(EStartKit.PurchaseReward).Items;
                var rew = new List<ShopRewardItemData>();

                foreach (var item in kit)
                    rew.Add(new ShopRewardItemData(null, GameContent.Items.Get(item.configId), item.amount));
                
                OnShow(rew);
            });
        }
    }
}