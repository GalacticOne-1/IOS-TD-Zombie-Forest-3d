
namespace Galactic1.Code.Systems.Runtime.Building
{
    public interface ISceneAdapterFactory
    {
        (IFacilitySceneAdapter adapter, FacilityUpgradeSceneAdapter upgrade) Create(BaseCampFacilityRuntime runtime);
    }
}