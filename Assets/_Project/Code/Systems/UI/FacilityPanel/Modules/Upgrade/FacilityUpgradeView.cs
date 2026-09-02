using System;
using Galactic1.Game.UI.Buildings.DTO;
using Galactic1.Game.UI.Production;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Game.UI.Buildings
{
    /// <summary>
    /// UI блока улучшения здания.
    /// Показывает требования и кнопку Upgrade.
    /// </summary>
    public sealed class FacilityUpgradeView : MonoBehaviour
    {
        [Header("Header")] 
        [SerializeField] private TMP_Text upgradeText;
        [SerializeField] private TMP_Text tierDesText;
        [SerializeField] private TMP_Text curLvlText;
        [SerializeField] private TMP_Text nextLvlText;
        [SerializeField] private Image facilityImg;


        [Space] 
        [SerializeField] private GameObject alertBox;
        [SerializeField] private ScrollRect requiresItemScroll;
        [SerializeField] private RecipeRequireSlotView requireViewPrefab;

        [SerializeField] private GameObject upgradeButton;

        private RecipeRequireSlotView[] requireSlots;

        private bool initialized;

        public event Action OnUpgradeClicked;

        private void Initialize()
        {
            if (initialized) return;
            initialized = true;

            requireSlots = new RecipeRequireSlotView[10];

            for (int i = 0; i < 10; i++)
            {
                requireSlots[i] = requireViewPrefab
                    .gameObject
                    .CreateGO(requiresItemScroll.content)
                    .GetComponent<RecipeRequireSlotView>();
            }
        }

        public void Show(FacilityUpgradeDetailsDTO dto)
        {
            Initialize();

            upgradeText.text = $"Upgrading up to Lvl.{dto.NextLevel+1}";
            var r = dto.UsingRecipes ? "recipes" : "modules"; 
            tierDesText.text = $"Tier {dto.NextLevel+1} {r} will be available after the upgrade";
            
            facilityImg.sprite = dto.Icon;
            curLvlText.text = $"Lvl. {dto.CurrentLevel+1}";
            nextLvlText.text = $"Lvl. {dto.NextLevel+1}";

            LoadRequirements(dto);

            alertBox.SetActive(!dto.CanUpgrade);
            upgradeButton.ButtonSetInteractable(dto.CanUpgrade);
            upgradeButton.RegisterButtonClick(() => OnUpgradeClicked?.Invoke());
        }

        private void LoadRequirements(FacilityUpgradeDetailsDTO dto)
        {
            var requirements = dto.Requirements;

            for (int i = 0; i < requireSlots.Length; i++)
            {
                if (i >= requirements.Count)
                {
                    requireSlots[i].gameObject.SetActive(false);
                    continue;
                }

                var r = requirements[i];

                requireSlots[i].gameObject.SetActive(true);

                requireSlots[i].Setup(
                    r.ItemId,
                    r.Item,
                    r.Icon,
                    r.OwnedAmount,
                    r.RequiredAmount,
                    r.IsEnough
                );
            }

            requiresItemScroll.SetSizeContentLayoutGroup(false, null, true, true);
            requiresItemScroll.ScrollRectResetH(0);
        }
    }
}