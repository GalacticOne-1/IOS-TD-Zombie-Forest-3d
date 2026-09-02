
using System.Collections.Generic;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.UI.CharacterPreview;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Inventory
{
    public sealed class UnitScrollListPresenter : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private UnitBadgeView itemPrefab;
        

        private InventoryManagementController controller;
        
        private List<UnitBadgeView> itemViews = new();

        public void Initialize(InventoryManagementController controller)
        {
            this.controller = controller;
            controller.OnSelectionChanged += OnSelectionChanged;
            controller.OnUnitListChanged += Rebuild;
            Rebuild();
        }

        private void Rebuild()
        {
            var units = controller.VisibleUnits;
            var uc = units.Count;

            // Создаём элементы, если ещё нет
            while (itemViews.Count < uc)
            {
                var item = Instantiate(itemPrefab, contentRoot);
                itemViews.Add(item);
            }

            var squadSystem = ServiceLocator.Current.Get<GameSession>().StrategicSquadSystem;
            var portraitCache = ServiceLocator.Current.Get<CharacterPortraitCache>();
            
            var liv = itemViews.Count;

            // Привязываем данные
            for (int i = 0; i < uc; i++)
            {
                itemViews[i].Bind(
                    units[i],
                    i,
                    OnUnitClicked,
                    i == controller.SelectedUnit.viewIndex,
                    squadSystem,
                    portraitCache);
            }

            // Скрываем лишние
            for (int i = uc; i < liv; i++)
                itemViews[i].gameObject.SetActive(false);


            scrollRect.SetSizeContentLayoutGroup(false, contentRoot, true, true);
            scrollRect.ScrollRectResetH(0);
        }
        private void OnSelectionChanged(int selectedIndex, string _)
        {
            for (int i = 0; i < itemViews.Count; i++)
                itemViews[i].SetHighlight(i == selectedIndex);
        }

        private void OnUnitClicked(int viewIndex, string unitId)
            => controller.SelectUnitByIndex(viewIndex, unitId);
    }
}