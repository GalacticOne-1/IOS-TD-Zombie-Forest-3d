using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.UI.Buildings;
using Galactic1.Configs.Galactic1.Code.GameDatabase;
using Galactic1.UI.Core;
using Galactic1.UI.Core.TabPanel;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.UI.Shop
{
    public class GameStoreWindow : UIScreenPanel
    {

        #region FIELDS

        [field: SerializeField] public bool EnableDebugLogs { get; private set; } = true;

        [SerializeField] private GameObject bClose;

        [field: SerializeField] public ScrollRect ScrollRect { get; private set; }
        [SerializeField] private RectTransform content;

        [Header("Layout BasicSettings")] 
        public float spacing = 30f;
        public float startOffset = 50f;
        public float endOffset = 50f;
        public bool horizontal = true;
        public float scrollDuration = 0.25f;

        [Header("Detail Panels")] 
        public List<ShopDetailsPanelBase> panels;

        [Header("Card Prefabs")] 
        public List<CardPrefabEntry> cardPrefabs;

        [Header("Tabs")] 
        public List<TabButtonEntry> tabButtons;
        public GameObject inboxButton;
        

        private readonly List<GameObject> spawnedCards = new();
        private readonly Dictionary<ShopCategory, float> categoryPositions = new();

        private bool scrolling = false;
        private float targetScrollPos;
        private bool userDragging = false;
        private float scrollElapsed = 0f;

        #endregion





        private GameStoreViewModel _viewModel;
        private readonly Dictionary<int, ShopCardUIBase> _createdCardBindersMap = new();


        public Action onDisable;


        private void OnEnable()
        {
            DLog.Alert("Game store enabled", EnableDebugLogs);

            bClose.RegisterButtonClick(() => onDisable?.Invoke());
        }

        private void OnDisable()
        {
            DLog.Alert("Game store disable", EDlogColor.ORANGE, EnableDebugLogs);
            onDisable?.Invoke();
        }






        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);

            ServiceLocator.Current.Get<TabPanelController>()
                .RegisterTab(new TabPanelController.RegistryEntry()
                {
                    Label = "gameStore",
                    Panel = this,
                    PanelId = UIScreenId.GameStore,
                    Order = 9,
                    OnAction = container.Resolve<GameStoreService>().ShowWindow
                });
        }

        // public override void Remove()
        // {
        //     base.Remove();
        //     ServiceLocator.Current.Unregister<InventoryWindow>();
        // }

        public override void OnShow(object data = null)
        {
            base.OnShow(data);
            _viewModel = data as GameStoreViewModel;

            // foreach (var card in _viewModel._gameStoreService.AllCards)
            // {
            //     CreateCard(card);
            // }
            
            // Настраиваем pivot и anchors контента
            if (horizontal)
            {
                content.anchorMin = new Vector2(0f, 0.5f);
                content.anchorMax = new Vector2(0f, 0.5f);
                content.pivot = new Vector2(0f, 0.5f);
            }
            else
            {
                content.anchorMin = new Vector2(0.5f, 1f);
                content.anchorMax = new Vector2(0.5f, 1f);
                content.pivot = new Vector2(0.5f, 1f);
            }

            GenerateShopCards();
            UpdateContentSize();
            SetupTabs();
            
            inboxButton.GetChild(0,1).SetActive(false);
            inboxButton.RegisterButtonClick(() =>
            {
                _container.Resolve<RuntimeFacilityPanelController>().Open(GameIdProvider.MainContainer);
            });

            // Сразу устанавливаем контент на первую вкладку без плавного скролла
            if (tabButtons.Count > 0)
                ScrollToCategory(tabButtons[0].category, smooth: false);
        }

        public override void OnHide()
        {
            base.OnHide();
        }


        // test
        // private void Update()
        // {
        //     if (Input.GetKeyDown(KeyCode.Alpha0))
        //     {
        //         var cardViewModel =
        //             _viewModel._gameStoreService._cardProxies.First(cardViewModel => cardViewModel.Id == 0);
        //
        //         cardViewModel.Limit.Value++;
        //         _GameState.Save();
        //     }
        // }




        // -------------------- TABS --------------------
        private void SetupTabs()
        {
            foreach (var tab in tabButtons)
            {
                if (tab.button == null) continue;
                var cat = tab.category;
                tab.button.RegisterButtonClick(() => ScrollToCategory(cat, true));
                tab.button.GetChild(0,1).SetActive(false);
            }
        }

        /// <summary>
        /// Запускает плавный скролл к первой карточке категории
        /// </summary>
        public void ScrollToCategory(ShopCategory category, bool smooth = false)
        {
            if (!categoryPositions.ContainsKey(category) || ScrollRect == null || content == null) return;

            ScrollRect.velocity = Vector2.zero;
            float targetPos = categoryPositions[category];
            float viewportSize = horizontal ? ScrollRect.viewport.rect.width : ScrollRect.viewport.rect.height;
            float contentSize = horizontal ? content.sizeDelta.x : content.sizeDelta.y;

            float desiredOffset = targetPos - startOffset;
            float maxOffset = Mathf.Max(0, contentSize - viewportSize);
            float clampedOffset = Mathf.Clamp(desiredOffset, 0, maxOffset);

            scrollElapsed = 0f;

            if (smooth)
            {
                targetScrollPos = clampedOffset;
                scrolling = true;
            }
            else
            {
                Vector2 anchoredPos = content.anchoredPosition;
                if (horizontal) anchoredPos.x = -clampedOffset;
                else anchoredPos.y = clampedOffset;
                content.anchoredPosition = anchoredPos;
                scrolling = false;
            }
        }

        /// <summary>
        /// Вызывается при начале перетаскивания пользователем
        /// </summary>
        public void OnBeginDrag()
        {
            userDragging = true;
            scrolling = false; // останавливаем автоскролл
        }

        /// <summary>
        /// Вызывается при завершении перетаскивания
        /// </summary>
        public void OnEndDrag()
        {
            userDragging = false;
        }

        private void Update()
        {
            if (!scrolling || userDragging) return;

            Vector2 anchoredPos = content.anchoredPosition;
            float current = horizontal ? -anchoredPos.x : anchoredPos.y;
            scrollElapsed += Time.unscaledDeltaTime; // unscaled, чтобы игнорировать Time.timeScale
            float t = Mathf.Clamp01(scrollElapsed / scrollDuration);
            float newVal = Mathf.Lerp(current, targetScrollPos, t);

            if (horizontal)
            {
                anchoredPos.x = -newVal;
                if (Mathf.Abs(newVal - targetScrollPos) < 0.1f) scrolling = false;
            }
            else
            {
                anchoredPos.y = newVal;
                if (Mathf.Abs(newVal - targetScrollPos) < 0.1f) scrolling = false;
            }

            content.anchoredPosition = anchoredPos;
        }

        // -------------------- CARD GENERATION --------------------
        // private GameObject GetPrefabForItem(ShopItemSO item)
        // {
        //     foreach (var entry in cardPrefabs)
        //     {
        //         if (entry.cardType == item.cardType)
        //             return entry.prefab;
        //     }
        //
        //     return null;
        // }

        private void GenerateShopCards()
        {
            ClearShop();
            //if (shopItems == null || shopItems.Length == 0) return;

            float offset = startOffset;
            categoryPositions.Clear();

            // порядок категорий = порядок вкладок в tabButtons
            var categoryOrder = tabButtons
                .Select((tab, index) => (tab.category, index))
                .ToDictionary(x => x.category, x => x.index);

            List<ShopCardViewModel> sorted = _viewModel._gameStoreService.AllCards
                .Where(c => categoryOrder.ContainsKey(c._iapConfig.Category))
                .OrderBy(c => categoryOrder[c._iapConfig.Category])
                .ThenBy(c => c._iapConfig.Header.Order)
                .ToList();

            ShopCategory? currentCategory = null;
            var styleResolver = ServiceLocator.Current.Get<UIStyleResolver>();

            foreach (var cardViewModel in sorted)
            {
                // === пропускаем покупки которые не имеют своей карточки (используют общую карточку)
                if (!cardViewModel._iapConfig.Use) 
                    continue;
                
                
                var createdCard = CreateCard(styleResolver, cardViewModel);
                RectTransform rect = createdCard.GetComponent<RectTransform>();

                rect.anchorMin = new Vector2(0f, 0.5f);
                rect.anchorMax = new Vector2(0f, 0.5f);
                rect.pivot = new Vector2(0f, 0.5f);

                if (horizontal)
                    rect.anchoredPosition = new Vector2(offset, 0f);
                else
                    rect.anchoredPosition = new Vector2(0f, -offset);

                if (currentCategory != cardViewModel._iapConfig.Category)
                {
                    categoryPositions[cardViewModel._iapConfig.Category] = offset;
                    currentCategory = cardViewModel._iapConfig.Category;
                }

                offset += (horizontal ? rect.sizeDelta.x : rect.sizeDelta.y) + spacing;

                var cardUI = createdCard.GetComponent<ShopCardUIBase>();
                //if (cardUI != null)
                    //cardUI.Setup(item, this);

                spawnedCards.Add(createdCard.gameObject);
            }
        }

        ShopCardUIBase CreateCard(UIStyleResolver styleResolver, ShopCardViewModel shopCardViewModel)
        {
            var prefabPath = $"{AppConstants.PATH_UI}{shopCardViewModel._iapConfig.PrefabPath}";

            var createdCard = prefabPath.CreateGO(content).GetComponent<ShopCardUIBase>();
            createdCard.Bind(styleResolver, this, shopCardViewModel, shopCardViewModel.OnBuy);
            //WindowCardFactory.InitializeCard(iapCardViewModel._iapConfig, createdCard);
            _createdCardBindersMap[shopCardViewModel.Id] = createdCard;

            return createdCard;
        }

        void DestroyCard(ShopCardViewModel shopCardViewModel)
        {
            if (_createdCardBindersMap.TryGetValue(shopCardViewModel.Id, out var iapCardBinder))
            {
                iapCardBinder.gameObject.DestroyGO();
                _createdCardBindersMap.Remove(shopCardViewModel.Id);
            }
        }

        private void UpdateContentSize()
        {
            if (!content || ScrollRect == null) return;

            float total = startOffset + endOffset;
            foreach (var card in spawnedCards)
            {
                var rect = card.GetComponent<RectTransform>();
                total += (horizontal ? rect.sizeDelta.x : rect.sizeDelta.y) + spacing;
            }

            Vector2 scrollSize = ScrollRect.viewport.rect.size;
            Vector2 size = content.sizeDelta;

            if (horizontal)
                size.x = Mathf.Max(total, scrollSize.x);
            else
                size.y = Mathf.Max(total, scrollSize.y);

            content.sizeDelta = size;
        }

        private void ClearShop()
        {
            foreach (Transform child in content)
                Destroy(child.gameObject);
            spawnedCards.Clear();
            categoryPositions.Clear();
        }

        public void OpenDetailsPanel(
            UIStyleResolver styleResolver,
            IAPConfig config, 
            ShopCardUIBase view, 
            Action<IAPConfig, ShopCardUIBase> buyCallback)
        {
            foreach (var panel in panels)
            {
                if (panel == null) continue;

                if ((config.ViewType == EViewType.Single1 && panel is ShopDetailsPanelType1) ||
                    (config.ViewType == EViewType.Single2 && panel is ShopDetailsPanelType2) ||
                    (config.ViewType == EViewType.Multiple && panel is ShopDetailsPanelType3))
                {
                    _viewModel._gameStoreService._shopController.OnPurchaseSuccess = panel.Hide;
                    panel.Show(styleResolver, config, view, buyCallback);
                }
                else
                {
                    panel.Hide();
                }
            }
        }
    }

    [System.Serializable]
    public struct CardPrefabEntry
    {
        public EViewType cardType;
        public GameObject prefab;
    }

    [System.Serializable]
    public struct TabButtonEntry
    {
        public ShopCategory category;
        public GameObject button;
    }
}