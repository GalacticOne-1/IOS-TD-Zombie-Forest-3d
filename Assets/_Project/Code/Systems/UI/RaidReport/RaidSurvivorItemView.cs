
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.RaidReport
{
    /// <summary>
    /// UI-представление одного бойца в отчёте рейда.
    /// </summary>
    public class RaidSurvivorItemView : MonoBehaviour
    {
        [SerializeField] private RawImage portrait;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text statusText;

        /// <summary>
        /// Заполняет элемент данными бойца.
        /// </summary>
        public void Bind(RaidSurvivorResult result)
        {
            portrait.texture = result.RenderPortrait;
            nameText.text = result.Name;
            statusText.text = result.Status;
        }
    }
}