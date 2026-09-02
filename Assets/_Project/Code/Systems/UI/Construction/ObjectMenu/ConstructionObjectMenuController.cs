using UnityEngine;
using System;
using Galactic1.Code.Gameplay.Construction;
using Galactic1.Code.Gameplay.Construction.Repair;
using Galactic1.Code.Utils;
using TMPro;

namespace Galactic1.Code.UI.Construction
{
    /// <summary>
    /// Меню выбранного объекта (Move/Delete/Confirm/Cancel)
    /// </summary>
    public class ConstructionObjectMenuController : MonoBehaviour
    {
        [SerializeField] private ConstructionSubMenuMove movePanel;
        [SerializeField] private ConstructionSubMenuUpgrade upgradePanel;
        [SerializeField] private TMP_Text alertText;
        [SerializeField] private TMP_Text nameText;

        [SerializeField] private Vector3 offset = new(0, 2f, 0);

        
        private Camera _camera;
        private RectTransform _rect;
        private BuildableObject _target;



        public event Action OnSwitchMovePressed;
        public event Action OnRotatePressed;
        public event Action OnDeletePressed;
        public event Action OnConfirmPressed;
        public event Action OnCancelPressed;
        public event Action OnRepairPressed;



        
        
        public void Initialize(Camera camera)
        {
            _camera = camera;
            _rect = GetComponent<RectTransform>();
            Hide();
            
            // clearing
            EventBus<SceneClearEvent>.Register(new EventBinding<SceneClearEvent>(() => Destroy(gameObject)));
            
            // move menu
            movePanel.cancelButton.RegisterButtonClick(CancelPressed);
            movePanel.rotateButton.RegisterButtonClick(RotatePressed);
            movePanel.confirmButton.RegisterButtonClick(ConfirmPressed);
            
            // upgrade menu
            upgradePanel.switchMoveMenuButton.RegisterButtonClick(SwitchMovePressed);
            upgradePanel.deleteButton.RegisterButtonClick(DeletePressed);
            upgradePanel.repairButton.RegisterButtonClick(RepairPressed);
        }
        
        
        public void Show(BuildableObject target, EConstructionSubMenu menu)
        {
            _target = target;
            nameText.text = target.FacilityConfig.Item.Header.titleLid;
            UpdatePosition();

            
            switch (menu)
            {
                case EConstructionSubMenu.Move:
                    movePanel.gameObject.SetActive(true);
                    upgradePanel.gameObject.SetActive(false);
                    break;
                
                case EConstructionSubMenu.Upgrade:
                    movePanel.gameObject.SetActive(false);
                    upgradePanel.gameObject.SetActive(true);
                    break;
            }
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _target = null;
        }

        private void LateUpdate()
        {
            if (_target == null)
                return;

            UpdatePosition();
        }

        private void UpdatePosition()
        {
            Vector3 worldPos = _target.transform.position + offset;
            Vector3 screenPos = _camera.WorldToScreenPoint(worldPos);
            
            // if (screenPos.z < 0)
            // {
            //     Hide();
            //     return;
            // }
            
            _rect.position = screenPos;
        }
        
        
        public void ShowAlert(bool show, string resultMessage)
        {
            alertText.transform.parent.gameObject.SetActive(show);
            alertText.text = resultMessage;
        }

        public void SetConfirmButtonEnabled(bool enabled)
        {
            if (enabled)
                CanvasGroupUtility.Enable(movePanel.confirmButton.CMP_CG());
            else
                CanvasGroupUtility.Disable(movePanel.confirmButton.CMP_CG(), false);
            movePanel.confirmButton.CMP_Btn().SetInteractableOnly(enabled);
        }


        // UI Buttons

        public void SwitchMovePressed()
        {
            OnSwitchMovePressed?.Invoke();
        }
        public void RotatePressed()
        {
            OnRotatePressed?.Invoke();
        }

        public void DeletePressed()
        {
            OnDeletePressed?.Invoke();
        }

        public void ConfirmPressed()
        {
            OnConfirmPressed?.Invoke();
        }
        
        public void RepairPressed()
        {
            OnRepairPressed?.Invoke();
        }

        public void RefreshRepair(RepairRequirementResult result)
        {
            upgradePanel.repairButton.SetActive(result.IsRepairable && result.NeedsRepair);

            if (!result.IsRepairable)
                return;


            if (result.NeedsRepair)
                upgradePanel.repairRequirementList.Render(result.Entries);
        }
        

        public void CancelPressed()
        {
            OnCancelPressed?.Invoke();
        }

        
    }


    public enum EConstructionSubMenu { Move, Upgrade } 
}