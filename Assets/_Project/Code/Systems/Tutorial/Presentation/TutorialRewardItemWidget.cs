using Galactic1.Code.Gameplay.Tasks;
using Galactic1.Code.Systems.Tutorial.Runtime;
using Galactic1.Game.Meta.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    /// <summary>
    /// Read-only виджет одной награды в туторе. Сознательно не переиспользует
    /// InventorySlotView (drag/click/inventory-зависимости) — только геометрия/текст,
    /// по образцу визуальных конвенций InventorySlotView.Set(), без интерактивности.
    /// </summary>
    public sealed class TutorialRewardItemWidget : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private TMP_Text durabilityText;
        [SerializeField] private TMP_Text ammoText;

        public void Set(ScenarioTaskReward reward)
        {
            var item = reward.Item;
            if (item == null) return;

            icon.sprite = item.Header.icon;
            amountText.text = reward.Amount > 1 ? $"×{reward.Amount}" : "";

            var maxDurability = item.Physical.maxDurability;
            bool showDurability = item.Physical.usesDurability && maxDurability > 0;
            durabilityText.gameObject.SetActive(showDurability);
            if (showDurability)
            {
                var durability = reward.Durability == -1 ? maxDurability : reward.Durability;
                durabilityText.text = Mathf.CeilToInt((float)durability / maxDurability * 100) + "%";
            }

            bool hasMagazine = item.HasModule<WeaponModule>() && item.Weapon.Definition.magazineSize > 0;
            ammoText.gameObject.SetActive(hasMagazine);
            if (hasMagazine)
                ammoText.text = $"{reward.AmmoInMagazine}/{item.Weapon.Definition.magazineSize}";
        }
    }
}