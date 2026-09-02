using System;
using System.Collections.Generic;
using Galactic1.Code.Gameplay.Abilities;
using Galactic1.Code.Gameplay.Effect;
using Galactic1.Code.Inventory.Context;
using Galactic1.Code.Systems;
using Galactic1.Code.Systems.GameModes;
using Galactic1.Code.Systems.Raid;
using Galactic1.Code.Systems.Raid.Survivors;
using Galactic1.Code.UI.Interaction;
using Galactic1.Code.UI.Inventory;
using Galactic1.Code.UI.UnitCard;
using Galactic1.UI.Core;
using Galactic1.UI.Core.TabPanel;

namespace Galactic1.Core.UI.HUD
{
    public sealed class UnitCardBindingSystem : IDisposable
    {
        private InventoryManagementController _inventoryController;
        
        private readonly List<UnitCardPresenter> _presenters = new();

        public void Bind(
            SquadRuntime squad,
            List<UnitCardView> views,
            ItemUseService useService,
            AbilityUseCoordinator coordinator,
            AbilityTargetingHUD hud,
            SceneGameModeService gameModeService,
            UIStateController uiStateController,
            InventoryManagementController inventoryController,
            UIInputRouter inputRouter)
        {

            _inventoryController = inventoryController;
            var squadUICoordinator = new SquadUICoordinator(uiStateController);
            
            // передаем в мод
            var mode = gameModeService.Get<AbilityTargetingGameMode>(GameModeType.AbilityTargeting);
            mode.Initialize(squadUICoordinator);
            
            // HUD подписывается на события координатора
            hud.Bind(squadUICoordinator);
            

            List<IUnitRuntime> units = new();
            foreach (var u in squad.Units)
                units.Add(u);
            
            for (int i = 0; i < squad.Units.Count && i < views.Count; i++)
            {
                var presenter = new UnitCardPresenter(
                    i,
                    squad.Units[i],
                    views[i],
                    OnUnitClicked,
                    useService,
                    units,      // todo завязан на юнита для рейда, для лагеря доработать !!!
                    coordinator,
                    squadUICoordinator,
                    inputRouter
                );
                _presenters.Add(presenter);
            }
        }
        
        private void OnUnitClicked(int viewIndex, string unitId)
        {
            ServiceLocator.Current.Get<UIManager>().OpenScreen(
                UIScreenId.Inventory, 
                new TabPanelController.FlagInventory(),
                _ =>
                {
                    _inventoryController.Open(InventoryGameplayMode.Transport_SquadOnly);
                    _inventoryController.SelectUnitByIndex(viewIndex, unitId);
                });
        }
        
        

        public void Dispose()
        {
            foreach (var p in _presenters) 
                p.Dispose();
            _presenters.Clear();
        }
    }
}