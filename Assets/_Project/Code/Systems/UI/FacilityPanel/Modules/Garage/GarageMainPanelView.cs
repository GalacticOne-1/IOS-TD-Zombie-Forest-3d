using System;
using Galactic1.UI.CharacterPreview;
using UnityEngine;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Garage
{
    public class GarageMainPanelView : MonoBehaviour
    {
        [Header("Preview")]
        [SerializeField] private RawImage vehiclePreview;

        [Header("Category")] 
        [SerializeField] private Transform categoryRoot;
        
        private Action<VehicleSlotType> _onCategorySelected;
        
        
        public void Bind(
            UIModulePreview preview,
            string prefabPath,
            UIPreviewConfig previewConfig,
            Func<VehicleSlotType, Sprite> iconProvider,
            Action<VehicleSlotType> onCategorySelected)
        {
            _onCategorySelected = onCategorySelected;
            
            // ошибка если пустой конфиг для превью
            if (previewConfig == null)
            {
                Debug.LogError("GarageMainPanelView: UIPreviewConfig is null");
            }
            preview.Show(vehiclePreview, AppConstants.PATH_ENTITIES + prefabPath, null, previewConfig);

            var l = categoryRoot.childCount;
            for (int i = 0; i < l; i++)
            {
                var btn = categoryRoot.GetChild(i).GetComponent<ModuleCategoryButton>();
                btn.Bind(
                    GetCategory(i),
                    OnCategoryClicked);
                
                if (i == 0) // пока активна только 1 кнопка
                    btn.SetIcon(iconProvider.Invoke(btn.Category));
            }
        }
        

        private void OnCategoryClicked(VehicleSlotType category)
        {
            _onCategorySelected?.Invoke(category);
        }

        public void UpdateSlot(VehicleSlotType category, Sprite icon)
        {
            var l = categoryRoot.childCount;

            for (int i = 0; i < l; i++)
            {
                var btn = categoryRoot.GetChild(i).GetComponent<ModuleCategoryButton>();

                if (btn.Category == category)
                {
                    btn.SetIcon(icon);
                    break;
                }
            }
        }


        public VehicleSlotType GetCategory(int i)
        {
            return i switch
            {
                1 => VehicleSlotType.Cargo,
                2 => VehicleSlotType.Weapon,
                3 => VehicleSlotType.Utility,
                _=>VehicleSlotType.None
            };
        }
    }
}