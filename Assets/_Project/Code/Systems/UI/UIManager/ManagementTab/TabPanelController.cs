
using System;
using System.Collections.Generic;
using Galactic1.Code.Inventory.Context;
using Galactic1.Code.UI.Inventory;
using Galactic1.Core.Systems.GameSession;
using Galactic1.UI.Shop;
using UnityEngine;


namespace Galactic1.UI.Core.TabPanel
{
    /// Событие полной готовности контроллера
    public struct TabControllerReadyEvent : IEvent {}
    
    public sealed class TabPanelController : UIScreenPanel, IGameService
    {
        [SerializeField] private GameObject bClose;
        [SerializeField] private Transform tabButtonRoot;
        [SerializeField] private GameObject tabButtonPrefab;
        [SerializeField] private Transform contentRoot;

        private readonly List<TabEntry> tabs = new();
        private TabEntry activeTab;

        public Param EntryParam { get; set; }
        
        private Action<GameObject> currentOnShow;
        private bool isCamp;

        // Обновляем SetOnShow — вызывается явно:
        private void SetOnShow(Action<GameObject> onShow)
        {
            currentOnShow = onShow;
    
            // первая кнопка для быстрого возврата к открытому виджету
            if (isCamp && tabs.Count > 0)
                tabs[0].Button.gameObject.SetActive(currentOnShow != null);
        }
        
        
        // =========================================================
        // INIT
        // =========================================================

        // должен первым создаваться т.к другие панели его требуют
        public override void Initialize(DIContainer diContainer, UIScreenId id)
        {
            base.Initialize(diContainer, id);

            ServiceLocator.Current.Register(this);
            bClose.RegisterButtonClick(OnHide);

            isCamp = FindAnyObjectByType<CampSceneSessionManager>() != null;

            
            // BuildTabs() финализирует порядок и создаёт кнопки
            EventBus<SceneUIReadyEvent>.Register(new EventBinding<SceneUIReadyEvent>(() =>
            {
                BuildTabs();
                if (isCamp)
                    tabs[0].Button.gameObject.SetActive(false);
                
                EventBus<TabControllerReadyEvent>.Raise(new TabControllerReadyEvent());
            }));
        }

        public override void Remove()
        {
            ServiceLocator.Current.Unregister<TabPanelController>();
        }

        /// <summary>
        /// Регистрирует вкладку с явным порядком.
        /// order — чем меньше, тем левее/выше вкладка.
        /// Кнопки создаются в BuildTabs() после всех RegisterTab().
        /// </summary>
        public void RegisterTab(RegistryEntry entry)
        {
            entry.Panel.transform.SetParent(contentRoot, false);
            entry.Panel.gameObject.SetActive(false);

            tabs.Add(new TabEntry
            {
                HideTab = entry.HideTab,
                Label = entry.Label,
                Panel = entry.Panel,
                PanelId = entry.PanelId,
                Order = entry.Order,
                Icon = entry.Icon,
                OnAction = entry.OnAction
            });
        }

        public void HideButton(UIScreenId panelId)
        {
            foreach (var t in tabs)
            {
                if (t.PanelId == panelId)
                {
                    t.HideTab = true;
                    t.Button.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Финализирует регистрацию — сортирует по Order и создаёт кнопки.
        /// Вызывать ПОСЛЕ всех RegisterTab(), до первого OnShow().
        /// </summary>
        public void BuildTabs()
        {
            var style = ServiceLocator.Current.Get<UIStyleResolver>().ManagerUIStyleConfig;
            
            // Сортировка по порядку
            tabs.Sort((a, b) => a.Order.CompareTo(b.Order));

            // Создаём кнопки в правильном порядке
            foreach (var tab in tabs)
            {
                tab.Icon = style.GetTabIcon(tab.PanelId);
                tab.Button = CreateTabButton(tab);

                if (tab.HideTab)
                    tab.Button.gameObject.SetActive(false);
            }

            // Сбрасываем активную вкладку — BuildTabs может вызываться повторно
            activeTab = null;
        }

        // =========================================================
        // PUBLIC
        // =========================================================

        public override void OnHide()
        {
            base.OnHide();

            EntryParam?.OnClosed?.Invoke();
            EntryParam = null;
            
            SetOnShow(null);
            activeTab?.Panel.OnHide();
            activeTab = null;
            gameObject.SetActive(false);
        }

        public void SwitchTo(
            int index, 
            Action<GameObject> onShow = null, 
            object data = null)
        {
            if (index < 0 || index >= tabs.Count) return;
            SwitchTo(tabs[index], onShow, data);
        }

        // from UIManager
        public void SwitchTo(
            UIScreenId panelId, 
            Action<GameObject> onShow = null, 
            object data = null)
        {
            // определяем состояние боковых кнопок 
            tabButtonRoot.gameObject.SetActive(EntryParam == null || !EntryParam.HideTab);
            
            var entry = tabs.Find(t => t.PanelId == panelId);
            if (entry != null)
                SwitchTo(entry, onShow, data);
        }

        // =========================================================
        // PRIVATE
        // =========================================================

        private void SwitchTo(
            TabEntry entry, 
            Action<GameObject> onShow = null, 
            object data = null)
        {
            if (activeTab == entry) return;

            // если клик через вкладку
            if (onShow == null && entry.OnAction != null)
            {
                entry.OnAction();
                return;
            }

            var l = tabs.Count;
            for (int i = 0; i < l; i++)
            {
                tabs[i].Panel.OnHide();
                tabs[i].Panel.gameObject.SetActive(false);
                tabs[i].Button.SetSelected(false);
            }

            activeTab = entry;
            activeTab.Panel.gameObject.SetActive(true);
            
            if (entry.PanelId == UIScreenId.FacilityPanel)
            {
                // метод передается от оригинального открытия FacilityPanel
                if (onShow != null) 
                    SetOnShow(onShow);
                // здесь открывается FacilityPanel но через вкладку поэтому onShow пустой и мы восстанавливаем
                else
                    onShow = currentOnShow;
            }


            if (entry.PanelId == UIScreenId.Inventory &&
                activeTab.Panel is InventoryManagementWindow inventory &&
                data is not FlagInventory)
            {
                inventory.modeController.Open(InventoryGameplayMode.Camp_AllUnits);
            }
            // else if (entry.PanelId == UIScreenId.GameStore && data == null)
            // {
            //     _container.Resolve<GameStoreService>().ShowWindow();
            // }
            else
            {
                onShow?.Invoke(activeTab.Panel.gameObject);
                activeTab.Panel.OnShow(data);
            }
            
            activeTab.Button.SetSelected(true);
            gameObject.SetActive(true);
        }

        private BaseUIButton CreateTabButton(TabEntry entry)
        {
            var go = Instantiate(tabButtonPrefab, tabButtonRoot);
            var btn = go.GetComponent<BaseUIButton>();

            if (go.GetComponentInChildren<TMPro.TMP_Text>() is { } tmp)
                tmp.text = entry.Label;

            if (entry.Icon != null)
            {
                btn.gameObject.GetChild(0).CMP_Image().sprite = entry.Icon;
                btn.Initialize();
            }

            // Захватываем entry — не индекс, чтобы порядок не влиял
            btn.gameObject.RegisterButtonClick(() => SwitchTo(entry));
            return btn;
        }

        // =========================================================
        // NESTED
        // =========================================================

        
        public sealed class RegistryEntry
        {
            public bool HideTab;
            public string Label;
            public UIScreenPanel Panel;
            public UIScreenId PanelId;
            public int Order;
            public Sprite Icon;
            public Action OnAction;
        }
        private sealed class TabEntry
        {
            public bool HideTab;
            public string Label;
            public UIScreenPanel Panel;
            public UIScreenId PanelId;
            public int Order;
            public Sprite Icon;
            public BaseUIButton Button;
            public Action OnAction;
        }
        
        public class Param
        {
            public bool HideTab;
            public Action OnClosed;
        }
        
        
        // для открытия инвентаря через событие,
        // иначе открывается по дефолту -> Camp_AllUnits
        public struct FlagInventory {}
    }
}