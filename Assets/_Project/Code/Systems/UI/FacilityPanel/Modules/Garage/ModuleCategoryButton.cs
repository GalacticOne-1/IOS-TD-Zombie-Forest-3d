using Galactic1.UI.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Garage
{
    /// <summary>
    /// Кнопка категории модулей
    /// </summary>
    public class ModuleCategoryButton : ButtonUIRegular
    {
        [SerializeField] private GameObject lockedGO;
        [SerializeField] private Image iconImage;
        
        
        public VehicleSlotType Category { get; private set; }
        private System.Action<VehicleSlotType> _callback;

        public void Bind(
            VehicleSlotType category,
            System.Action<VehicleSlotType> callback)
        {
            
            if (category != VehicleSlotType.None)   // todo пока оставляем только машину
            {
                lockedGO.SetActive(true);
                gameObject.ButtonSetInteractable(false);
                return;
            }
            
            
            Category = category;
            _callback = callback;


            gameObject.RegisterButtonClick(OnClick);
        }

        private void OnClick()
        {
            _callback?.Invoke(Category);
        }
        
        
        public void SetIcon(Sprite icon)
        {
            if (iconImage != null)
                iconImage.sprite = icon;
        }
    }
}