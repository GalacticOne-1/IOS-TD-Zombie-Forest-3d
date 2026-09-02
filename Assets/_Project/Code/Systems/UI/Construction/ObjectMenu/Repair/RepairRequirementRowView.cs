using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Galactic1.Code.Gameplay.Construction.Repair;
using Galactic1.Code.Utils;

namespace Galactic1.Code.UI.Construction.Repair
{
    /// <summary>
    /// Отображение одной строки требования ремонта.
    /// Только рендер — никаких расчётов.
    /// </summary>
    public class RepairRequirementRowView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text amountText;

        public void Bind(RepairRequirementEntry entry)
        {
            if (entry.Item != null)
            {
                icon.sprite = entry.Item.Header.icon;
            }

            amountText.text = TextUtils.FormatOwnedRequired(entry.Owned, entry.Required);
        }
    }
}