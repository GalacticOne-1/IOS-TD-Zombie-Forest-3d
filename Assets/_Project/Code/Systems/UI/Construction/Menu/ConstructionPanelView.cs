using System;
using System.Collections.Generic;
using Galactic1.Code.Systems.Construction.Configs;
using Galactic1.Game.Meta.Items;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Code.UI.Construction
{
    /// <summary>
    /// View панели строительства.
    /// Управляет вкладками и списком зданий.
    /// </summary>
    public class ConstructionPanelView : MonoBehaviour
    {
        [Header("Tabs")]
        [SerializeField] private RectTransform tabRoot;
        [SerializeField] private ConstructionTabButtonView tabPrefab;
        
        [Header("List")]
        [SerializeField] private FacilityListView listView;

        
        public RectTransform TabRoot => tabRoot;

        private List<ConstructionCategoryConfig> _categories;
        

        private readonly List<ConstructionTabButtonView> _tabs = new();

        private Action<ConstructionCategory> _onTabSelected;

        
        public void Bind(
            DIContainer container,
            UIStyleResolver styleResolver,
            List<FacilityModule> facilities,
            List<ConstructionCategoryConfig> categories,
            ConstructionCategory category,
            Action<FacilityModule> onSelected,
            Action<ConstructionCategory> onTabSelected)
        {

            _categories = categories;

            BuildTabs();

            listView.Build(
                container,
                styleResolver,
                facilities,
                categories,
                onSelected);

            _onTabSelected = onTabSelected;
            
            SelectTab(category);
        }
        
        private void BuildTabs()
        {
            ClearTabs();

            foreach (var category in _categories)
            {
                var tab = Instantiate(tabPrefab, tabRoot);

                tab.Bind(
                    category.Category,
                    category.Title,
                    category.Icon,
                    OnTabSelected);

                _tabs.Add(tab);
            }
        }

        private void OnTabSelected(ConstructionCategory category)
        {
            SelectTab(category);
        }

        private void SelectTab(ConstructionCategory category)
        {
            foreach (var tab in _tabs)
                tab.SetSelected(tab.Category == category);

            listView.Filter(category);
            
            _onTabSelected?.Invoke(category);
        }

        private void ClearTabs()
        {
            foreach (var tab in _tabs)
                Destroy(tab.gameObject);

            _tabs.Clear();
        }
    }
}