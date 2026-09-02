using Galactic1.Code.Cameras;
using Galactic1.Code.Gameplay.Construction;
using Galactic1.Code.Systems.Interaction;
using Galactic1.UI.Core;

namespace Galactic1.Code.Systems
{
    /// <summary>
    /// Construction mode.
    /// Отвечает за состояние сцены.
    /// </summary>
    public class ConstructionGameMode : ISceneGameMode
    {
        public GameModeType ModeType => GameModeType.Construction;
        
        
        private readonly ConstructionModeController _constructionController;
        private readonly CameraController _camera;
        private readonly SceneInteractionBlocker _interactionBlocker;
        private readonly UIManager _uiManager;
        

        public ConstructionGameMode()
        {
            _constructionController = ServiceLocator.Current.Get<ConstructionModeController>();
            _camera = ServiceLocator.Current.Get<CameraController>();
            _interactionBlocker = ServiceLocator.Current.Get<SceneInteractionBlocker>();
            _uiManager = ServiceLocator.Current.Get<UIManager>();
        }

        // === Включает всю среду строительства
        public void Enter()
        {
            
            _camera.EnterConstructionMode(20f);
            _interactionBlocker.Enable();
            _uiManager.EnterConstructionMode();

            // включаем систему строительства
            _constructionController.EnterMode();
        }

        // === Возвращает сцену в normal state
        public void Exit()
        {
            
            // выключаем строительство
            _constructionController.ExitMode();
            
            _camera.ExitConstructionMode();
            _uiManager.ExitConstructionMode();
            _interactionBlocker.Disable();
        }
    }
}