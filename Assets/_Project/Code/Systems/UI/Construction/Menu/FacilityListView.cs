using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.Inventory.Services;
using Galactic1.Code.Systems.Construction.Configs;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.Game.Meta.Items;
using Galactic1.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Construction
{
    /// <summary>
    /// View списка зданий.
    /// Хранит карточки и фильтрует их по категориям.
    /// </summary>
    public class FacilityListView : MonoBehaviour
    {
        [SerializeField] private ConstructionFacilityCardView cardPrefab;
        [SerializeField] private ScrollRect scrollRect;

        private Dictionary<ConstructionCategory, ConstructionCategoryConfig> _categoryMap;
        private readonly List<ConstructionFacilityCardView> _cards = new();
        private readonly List<FacilityModule> _facilities = new();


        public void Build(
            DIContainer container,
            UIStyleResolver styleResolver,
            List<FacilityModule> facilities,
            List<ConstructionCategoryConfig> categories,
            Action<FacilityModule> onSelected)
        {
            Clear();

            _categoryMap = categories.ToDictionary(c => c.Category);

            var requirementService = container.Resolve<ConstructionRequirementService>();
            var gameLoopContext = container.Resolve<GameSession>().GameLoopContext;

            // Сортировка:
            // 1. Сначала доступные здания.
            // 2. Затем здания с исчерпанным лимитом.
            // 3. Внутри каждой группы — по Header.order.
            var sorted = facilities
                .Where(f => !f.Item.Classification.flag.HasFlag(ItemFlags.HideInConstruct))
                .OrderBy(f => IsLimitReached(f, gameLoopContext) ? 1 : 0)
                .ThenBy(f => f.Item.Header.order)
                .ToList();

            // _facilities заполняем из sorted — индексы совпадают с _cards
            _facilities.AddRange(sorted);

            foreach (var facility in sorted)
            {
                bool reached = IsLimitReached(facility, gameLoopContext);

                var card = Instantiate(cardPrefab, scrollRect.content);
                card.Bind(
                    requirementService,
                    facility,
                    styleResolver,
                    onSelected,
                    reached); // ← передаём флаг лимита в карточку

                _cards.Add(card);
            }


        }

        public void Filter(ConstructionCategory category)
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                bool visible = MatchCategory(_facilities[i], category);
                _cards[i].gameObject.SetActive(visible);
            }

            scrollRect.SetSizeContentLayoutGroup(false, null, true, true);
            scrollRect.ScrollRectResetH(0);
        }


        /// <summary>
        /// Проверяет исчерпан ли лимит постройки через GameLoopContext.
        /// </summary>
        private static bool IsLimitReached(FacilityModule facility, GameLoopContext context)
        {
            if (facility.BuildLimit == 0)
                return false;

            int built = context.GetFacilityCount(facility);
            return facility.IsLimitReached(built);
        }

        private bool MatchCategory(FacilityModule facility, ConstructionCategory category)
        {
            if (!_categoryMap.TryGetValue(category, out var config))
                return true;

            // foreach (var type in config.FacilityTypes)
            // {
            //     if (facility.FacilityType == type)
            //         return true;
            // }
            //
            // return false;
            
            return config.MatchCategory(facility);
        }

        public void Clear()
        {
            foreach (var card in _cards)
                DestroyImmediate(card.gameObject);

            _cards.Clear();
            _facilities.Clear();
        }
    }
}