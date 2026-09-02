using Galactic1.Code.GameDatabase.Registries;
using Galactic1.Code.Systems.GameLoop;
using Galactic1.Code.Systems.Runtime.Building;
using Galactic1.Code.UI.BuildingPanel;
using Galactic1.UI.Core;
using Galactic1.UI.Core.TabPanel;

namespace Galactic1.Code.UI.Buildings
{
    /// <summary>
    /// Открывает панель здания без наличия Scene Instance.
    /// Используется на карте мира, списках зданий и т.п.
    /// </summary>
    public sealed class RuntimeFacilityPanelController : IGameService
    {
        private readonly GameLoopContext _gameLoopContext;
        private FacilityPanelController _panel;

        public RuntimeFacilityPanelController(
            GameLoopContext gameLoopContext)
        {
            _gameLoopContext = gameLoopContext;

            EventBus<SceneActivateEvent>.Register(new EventBinding<SceneActivateEvent>(() =>
                _panel = ServiceLocator.Current.Get<FacilityPanelController>()));
        }

        public void HideTabButton()
        {
            // если сцена карта, то скрываем кнопку 
            EventBus<TabControllerReadyEvent>.Register(new EventBinding<TabControllerReadyEvent>(() =>
            {
                ServiceLocator.Current.Get<TabPanelController>().HideButton(UIScreenId.FacilityPanel);
            }));
        }

        public bool Open(RuntimeId configId)
        {
            var runtime = _gameLoopContext.GetFacilityByConfigId(configId);

            if (runtime == null)
                return false;

            _panel.OpenRuntime(runtime);
            return true;
        }

        public bool Open(BaseCampFacilityRuntime runtime)
        {
            if (runtime == null)
                return false;

            _panel.OpenRuntime(runtime);
            return true;
        }
    }
}