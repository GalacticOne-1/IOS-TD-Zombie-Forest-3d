using Galactic1.Game.UI.Stats.DTO;
using TMPro;
using UnityEngine;

namespace Galactic1.Game.UI.Stats
{
    public class ItemListFieldView : StatViewBase
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private Transform itemRoot;

        public override void Bind(StatDtoBase data)
        {
            base.Bind(data);
            
            if (data is ItemListViewDto stat)
            {
                StatUIBuilder.Apply(stat.StatStyleEntry, label, itemRoot, stat.ItemIds);
            }
        }

        public override void ResetView()
        {
            label.text = "";
            var l = itemRoot.childCount;
            for (int i = 0; i < l; i++)
                itemRoot.GetChild(i).gameObject.SetActive(false);
        }
    }
}