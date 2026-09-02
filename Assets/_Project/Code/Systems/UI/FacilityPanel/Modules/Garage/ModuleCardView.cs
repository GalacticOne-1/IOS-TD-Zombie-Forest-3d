using System;
using Galactic1.Code.GameDatabase.Registries;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Garage
{
    /// <summary>
    /// Карточка модуля транспорта
    /// </summary>
    public class ModuleCardView : ButtonUIRegular
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text title;
        [SerializeField] private GameObject selected;   // выбрано пользователем
        [SerializeField] private GameObject equipped;   // установлено на транспорт

        private RuntimeId _moduleId;
        public event Action<RuntimeId> OnClicked;
        
        public RuntimeId ModuleId => _moduleId;
        
        

        public void Bind(
            Sprite iconSprite,
            string name,
            RuntimeId moduleId,
            bool equip,
            bool unlocked)
        {
            icon.sprite = iconSprite;
            title.text = name;

            _moduleId = moduleId;

            SetEquipped(equip);
            SetSelected(false);

            gameObject.RegisterButtonClick(HandleClick);
        }
        
        /// <summary>
        /// Вызывается извне для переключения визуала выбора
        /// </summary>
        public void SetSelected(bool value)
        {
            selected.SetActive(value);
        }
        
        public void SetEquipped(bool value)
        {
            equipped.SetActive(value);
        }

        void HandleClick()
        {
            OnClicked?.Invoke(_moduleId);
        }

        public void Click() => HandleClick();
    }
}