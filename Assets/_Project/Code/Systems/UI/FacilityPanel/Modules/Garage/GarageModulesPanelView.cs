using System;
using System.Collections.Generic;
using System.Linq;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Game.Meta.Items;
using Galactic1.Game.UI.Garage.DTO;
using Galactic1.Game.UI.Production;
using Galactic1.UI.CharacterPreview;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Garage
{
    /// <summary>
    /// Панель отображения модулей категории
    /// </summary>
    public class GarageModulesPanelView : MonoBehaviour
    {
        [Header("Actions")] 
        [SerializeField] private GameObject backButton;
        [SerializeField] private GameObject applyButton;
        [SerializeField] private GameObject buyButton;
        
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private ModuleCardView cardPrefab;
        
        [Header("Preview")]
        [SerializeField] private RawImage modulePreview;

        [Header("Details")] 
        [SerializeField] private ScrollRect statScroll;
        [SerializeField] private GarageDetailsView garageDetailsView;
        [SerializeField] private ScrollRect requiresItemScroll;
        [SerializeField] private ModuleRecipeRequireSlotView requireViewPrefab;

        
        public ScrollRect StatScroll => statScroll;
        private UIModulePreview _preview;
        

        private readonly List<ModuleCardView> cards = new();
        
        public event Action<RuntimeId> OnModuleSelected;
        private Action onApplyModule;
        private Action onBack;
        
        private ModuleRecipeRequireSlotView[] requireSlots;
        public GarageModuleDetailsDTO ModuleDto {get; private set;}
        
        private ModuleCardView _selectedCard;
        private bool _selectedUnlocked;
        private bool _selectedEquipped;
        bool isInitialized;
        
        
        
        
        public void BindBuy(Action buy)
        {
            buyButton.RegisterButtonClick(() => buy?.Invoke());
        }
        public void BindApply(Action apply)
        {
            onApplyModule = apply;
            applyButton.RegisterButtonClick(() => onApplyModule?.Invoke());
        }
        public void BindBack(Action back)
        {
            onBack = back;
            backButton.RegisterButtonClick(() => onBack?.Invoke());
        }
        
        
        private void Initialize()
        {
            if (isInitialized) return;
            isInitialized = true;
            

            // local pool
            requireSlots = new ModuleRecipeRequireSlotView[10];
            for (int i = 0; i < 10; i++)
            {
                requireSlots[i] = requireViewPrefab.gameObject
                    .CreateGO(requiresItemScroll.content).GetComponent<ModuleRecipeRequireSlotView>();
            }
        }
        

        public void Build(
            GarageModuleDetailsDTO dto,
            UIModulePreview preview,
            IReadOnlyCollection<ItemConfig> modules,
            IReadOnlyCollection<RuntimeId> unlockedModules,
            RuntimeId equippedModuleId)
        {
            Initialize();
            
            ModuleDto = dto;
            
            scrollRect.content.MakeHidden();
            Clear();
            
            _preview = preview;
        
            foreach (var module in modules)
            {
                bool unlocked = unlockedModules.Contains(module.Id);
                
                var card = Instantiate(cardPrefab, scrollRect.content);
        
                card.Bind(
                    module.Header.icon,
                    module.Header.titleLid,
                    module.Id,
                    module.Id == equippedModuleId,
                    unlocked);
        
                card.OnClicked += HandleCardClicked;
                cards.Add(card);
            }

            scrollRect.SetSizeContentLayoutGroup(true, null, true, true);
            scrollRect.ScrollRectResetV();
        }

        public void Unbind()
        {
            Clear();
        }

        public void SetModuleDetails(GarageModuleDetailsDTO dto)
        {
            ModuleDto = dto;
            
            garageDetailsView.ShowDetails(dto, this);

            _selectedUnlocked = dto.IsPurchased;
            _selectedEquipped = dto.IsEquipped;

            ShowPreview(dto.PrefabPath, dto.PreviewConfig);
            UpdateButtons();

            if (!_selectedUnlocked)
            {
                LoadRequirements();
            }
            else
            {
                requiresItemScroll.content.gameObject.SetActive(false);
            }
        }

        private void HandleCardClicked(RuntimeId moduleId)
        {
            SetSelected(moduleId);

            OnModuleSelected?.Invoke(moduleId);
        }

        public void SelectCard(RuntimeId moduleId)
        {
            foreach (var card in cards)
                if (card.ModuleId == moduleId)
                {
                    card.Click();
                    break;
                }
        }
        
        public void UpdateEquipped(RuntimeId equippedId)
        {
            foreach (var card in cards)
            {
                card.SetEquipped(card.ModuleId == equippedId);
            }
        }

        public void SetSelected(RuntimeId itemId)
        {
            foreach (var card in cards)
                card.SetSelected(card.ModuleId == itemId);
        }

        private void UpdateButtons()
        {
            applyButton.SetActive(_selectedUnlocked && !_selectedEquipped);
            buyButton.SetActive(!_selectedUnlocked);
        }

        private void ShowPreview(string prefab, UIPreviewConfig previewConfig)
        {
            if (prefab != null)
                _preview.Show(modulePreview, AppConstants.PATH_ENTITIES + prefab, null, previewConfig);
        }
        
        
        private void LoadRequirements()
        {
            if (ModuleDto.Requirements == null)
            {
                requiresItemScroll.content.gameObject.SetActive(false);
                return;
            }
            
            requiresItemScroll.content.gameObject.SetActive(true);
            
            
            var requirementsCount = ModuleDto.Requirements.Count;
            var l = requireSlots.Length;
            for (int i = 0; i < l; i++)
            {
                if (i >= requirementsCount)
                {
                    requireSlots[i].gameObject.SetActive(false);
                    continue;
                }

                requireSlots[i].gameObject.SetActive(true);
                var requireSlot = requireSlots[i];
                var requirement = ModuleDto.Requirements[i];

                requireSlot.Setup(
                    requirement.Id,
                    requirement.Item,
                    requirement.Icon,
                    requirement.OwnedAmount,
                    requirement.RequiredAmount,
                    requirement.IsEnough
                );
            }

            if (requirementsCount > l)
            {
                Debug.LogError($"Recipe requirement more slots -> {ModuleDto.Title}");
            }

            requiresItemScroll.SetSizeContentLayoutGroup(false, null, true, true);
            requiresItemScroll.ScrollRectResetH(0);

        }
        

        private void Clear()
        {
            foreach (var card in cards)
            {
                card.OnClicked -= HandleCardClicked;
                Destroy(card.gameObject);
            }
            
            garageDetailsView.Release();
            cards.Clear();
        }
    }
}