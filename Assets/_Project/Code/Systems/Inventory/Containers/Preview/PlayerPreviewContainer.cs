using Galactic1.Core;
using Galactic1.Core.Systems.PlayerCreation;
using Galactic1.Gameplay.Player;
using Galactic1.Items;

namespace Galactic1.Code.UI.Inventory
{
    public class PlayerPreviewContainer : EquipmentPreviewContainer
    {
        protected override InventoryProxy InventoryProxy =>
            ServiceLocator.Current.Get<IGameStateProvider>().GameStateProxy.PlayerUnits[0].EquipmentProxy;

        private PlayerWeaponBuilder _weaponBuilder;
        
        protected override void Awake()
        {
            base.Awake();
            _weaponBuilder = new PlayerWeaponBuilder(GetComponent<IPlayerController>());
        }
        

        protected override WeaponBuilderBase GetWeaponBuilder() => _weaponBuilder;
    }
}