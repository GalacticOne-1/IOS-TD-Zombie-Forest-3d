using System;
using Galactic1.Code.Systems.Construction.Configs;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Construction
{
    /// <summary>
    /// UI кнопка вкладки категории строительства.
    /// </summary>
    public class ConstructionTabButtonView : BaseUIButton
    {
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private Image iconImg;

        public ConstructionCategory Category { get; private set; }
        private Action<ConstructionCategory> _onClick;


        public void Bind(
            ConstructionCategory category,
            string title,
            Sprite icon,
            Action<ConstructionCategory> onClick)
        {
            Category = category;
            _onClick = onClick;

            labelText.text = title;
            iconImg.sprite = icon;

            gameObject.RegisterButtonClick(OnClick);
        }


        private void OnClick()
        {
            _onClick?.Invoke(Category);
        }
    }
    
    
}