
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Game.UI.Buildings.DTO;

namespace Galactic1.Code.UI.Buildings
{
    /// <summary>
    /// Отображает информацию о жителях.
    /// Используется жилыми отсеками.
    /// </summary>
    public class LivingModulePanelModule : FacilityPanelModule
    {
        public override bool IsSupported(FacilityDTO dto)
            => dto.Details.Type == FacilityType.LivingModule;

        public override void Bind(
            FacilityDTO dto, 
            object sceneAdapter = null,
            FacilityUpgradeSceneAdapter upgradeAdapter = null)
        {
            base.Bind(dto, sceneAdapter, upgradeAdapter);
        }
        public override void Rebind(FacilityDTO dto) {}

        public override void Unbind()
        {
            base.Unbind();
        }
    }
}