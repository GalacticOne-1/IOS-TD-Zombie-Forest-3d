
using Galactic1.Code.Inventory.Context;
using Galactic1.Code.Systems.Squad;
using Galactic1.Configs;
using Galactic1.Core.Systems.GameLoopSession;
using Galactic1.UI.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Galactic1.Code.UI.Inventory
{
    /// <summary>
    /// UI state controller for mode buttons and contextual controls.
    /// Does NOT contain gameplay logic.
    /// Purely reflects InventoryManagementController state.
    /// </summary>
    public sealed class InventoryManagementPanelState : MonoBehaviour
    {
        [Header("Mode Buttons")] 
        [SerializeField] private GameObject rootButtons;
        [SerializeField] private GameObject campButton;
        [SerializeField] private GameObject logisticsButton;
        [SerializeField] private GameObject squadButton;


        [Header("Squad Extra")] 
        [SerializeField] private GameObject squadExtraRoot;
        [SerializeField] private GameObject squadExtraTransport;
        [SerializeField] private GameObject squadExtraBase;

        [SerializeField] private GameObject squadButtonsRoot;
        [SerializeField] private GameObject deleteUnit;
        [SerializeField] private GameObject squadToggleButton;
        [SerializeField] private TMP_Text squadCountText;


        [Header("Cargo Drone")] 
        [SerializeField] private GameObject droneButtonsRoot;
        [SerializeField] private GameObject lootButton;
        [SerializeField] private GameObject droneButton;


        
        
        
        
        private Sprite iconAdd, iconRemove;


        private InventoryManagementController _controller;
        private StrategicSquadSystem _strategicSquadSystem;
        

        public void Initialize(InventoryManagementController controller)
        {
            _controller = controller;
            var gameSession = ServiceLocator.Current.Get<GameSession>();
            var gameLoopContext = gameSession.GameLoopContext;
            
            // === main buttons
            campButton.RegisterButtonClick(() => SetMode(InventoryGameplayMode.Camp_AllUnits));
            squadButton.RegisterButtonClick(() => SetMode(InventoryGameplayMode.Transport_SquadOnly));
            logisticsButton.RegisterButtonClick(() => SetMode(InventoryGameplayMode.Camp_And_Transport));

            // === extra for squad
            squadExtraBase.RegisterButtonClick(() => SetMode(InventoryGameplayMode.Camp_SquadOnly));
            squadExtraTransport.RegisterButtonClick(() => SetMode(InventoryGameplayMode.Transport_SquadOnly));
            
            // === cargo drone
            lootButton.RegisterButtonClick(() => SetMode(InventoryGameplayMode.Transport_BufferLoot));
            droneButton.RegisterButtonClick(() => SetMode(InventoryGameplayMode.Transport_BufferDrone));

            _controller.OnSourcesChanged += RefreshUI;
            RefreshUI();


            // === squad selection
            _strategicSquadSystem = ServiceLocator.Current.Get<GameSession>().StrategicSquadSystem;
            squadToggleButton.RegisterButtonClick(() =>
            {
                var unitId = _controller.SelectedUnit.unitId;
                if (!string.IsNullOrEmpty(unitId))
                {
                    var unitRuntime = gameLoopContext.GetUnit(unitId);

                    if (_strategicSquadSystem.IsInSquad(unitId))
                        _strategicSquadSystem.RemoveUnit(unitRuntime);
                    else
                        _strategicSquadSystem.AddUnit(unitRuntime);
                }
            });
            
            // === кнопка для удаления юнита
            deleteUnit.RegisterButtonClick(() =>
            {
                var data = new ConfirmPopupData(
                    "Confirm Banish",
                    "Are you sure you want to banish the survivor?",
                    "Yes",
                    () =>
                    {
                        var unitId = _controller.SelectedUnit.unitId;
                        if (!string.IsNullOrEmpty(unitId))
                        {
                            // sound ...
                            gameLoopContext.DeleteUnitByPlayer(unitId);
                        }
                    },
                    onClose: () => { Debug.Log("Игрок отменил изгнание выжившего"); }
                );

                ServiceLocator.Current.Get<UIManager>().OpenPopup(UIScreenId.ConfirmPopup, data);
            });

            var style = ServiceLocator.Current.Get<ConfigProvider>().Get<UIStyleDatabase>().InventoryIcons;
            iconAdd = style.SquadAdd;
            iconRemove = style.SquadRemove;

            controller.OnUnitChanged += _ => RefreshSquadButtons(_, _strategicSquadSystem.IsInSquad(_));
            _strategicSquadSystem.OnSquadChanged += RefreshSquadButtons;
            RefreshSquadButtons(_controller.SelectedUnit.unitId, _strategicSquadSystem.IsInSquad(_controller.SelectedUnit.unitId));
        }

        /// <summary>
        /// Панель для режима инвентаря
        /// </summary>
        /// <param name="state"></param>
        public void ResolvePanelState(InventoryPanelState state)
        {
            rootButtons.SetActive(state == InventoryPanelState.CampFull);
            squadButtonsRoot.SetActive(state == InventoryPanelState.CampFull);
            
            // map report
            droneButtonsRoot.SetActive(state == InventoryPanelState.RaidReportLoot || state == InventoryPanelState.RaidReportDrone);
            lootButton.SetActive(state == InventoryPanelState.RaidReportDrone);
            droneButton.SetActive(state == InventoryPanelState.RaidReportLoot);
        }

        private void SetMode(InventoryGameplayMode mode)
        {
            _controller.Open(mode);
            RefreshUI();
        }

        private void RefreshUI()
        {
            var mode = _controller.CurrentMode;

            // --- Подсветка кнопок ---
            HighlightButton(campButton.GetChild(0).CMP_Image(), mode == InventoryGameplayMode.Camp_AllUnits);
            HighlightButton(logisticsButton.GetChild(0).CMP_Image(), mode == InventoryGameplayMode.Camp_And_Transport);
            
            
            bool squadMode = mode == InventoryGameplayMode.Transport_SquadOnly || 
                             mode == InventoryGameplayMode.Camp_SquadOnly;
            HighlightButton(squadButton.GetChild(0).CMP_Image(), squadMode);
            
            // --- Дополнительные кнопки Squad ---
            squadExtraRoot.SetActive(squadMode);
            HighlightButton(squadExtraTransport.GetChild(0).CMP_Image(), mode == InventoryGameplayMode.Transport_SquadOnly);
            HighlightButton(squadExtraBase.GetChild(0).CMP_Image(), mode == InventoryGameplayMode.Camp_SquadOnly);
        }
        
        void RefreshSquadButtons(string changedUnitId, bool isInSquad)
        {
            if (string.IsNullOrEmpty(_controller.SelectedUnit.unitId))
            {
                deleteUnit.SetActive(false);
                squadToggleButton.SetActive(false);
                return;
            }


            var mode = _controller.CurrentMode;
            bool allUnitsPanel = mode == InventoryGameplayMode.Camp_AllUnits;
            bool squadPanel = mode == InventoryGameplayMode.Transport_SquadOnly ||
                              mode == InventoryGameplayMode.Camp_SquadOnly;

            deleteUnit.SetActive(true);
            
            // Кнопка вообще показывается только там, где возможна работа с отрядом
            squadToggleButton.gameObject.SetActive(allUnitsPanel || squadPanel);

            // if (!squadToggleButton.gameObject.activeSelf)
            //     return;

            var squadStat= _strategicSquadSystem.GetSquadStat();
            squadCountText.text = $"{squadStat.Item1}/{squadStat.Item2}";

            // --- Иконка и функционал ---
            if (allUnitsPanel)
            {
                squadToggleButton.CMP_Image().sprite = isInSquad ? iconRemove : iconAdd;
                squadToggleButton.SetActive(true);
            }
            else if (squadPanel)
            {
                squadToggleButton.CMP_Image().sprite = iconRemove; // в отряде всегда remove
                squadToggleButton.SetActive(true);
            }
        }
        
        
        
        

        private static void HighlightButton(Image img, bool state)
        {
            if (img != null)
                img.enabled = state;
        }
    }
}
