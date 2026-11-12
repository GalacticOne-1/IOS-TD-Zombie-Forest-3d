using Galactic1.Code.Systems.Tutorial.Runtime;
using Galactic1.Core;
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    public class TutorialSkipPopup : UIScreenPanel
    {
        [SerializeField] private GameObject skipButton;
        [SerializeField] private GameObject continueButton;
        [SerializeField] private GameObject skipPanel;

        /*
         *  Показываем опцию пропуска обучения один раз при старте обучения
         *  после панель всегда остается не активна
         */
        public override void Initialize(DIContainer container, UIScreenId id)
        {
            base.Initialize(container, id);

            var stateProvider = container.Resolve<IGameStateProvider>();

            
            // === Hide
            if (PlayerPrefs.HasKey(stateProvider.NewGameKey))
            {
                skipButton.SetActive(false);
                return;
            }
            
            // === Show
            
            PlayerPrefs.SetString(stateProvider.NewGameKey, "y");
            
            gameObject.SetActive(true);
            
            
            continueButton.RegisterButtonClick(() => gameObject.SetActive(false));
            skipButton.RegisterButtonClick(Skip);
        }

        public override void Remove()
        {
            base.Remove();
        }


        void Skip()
        {
            skipPanel.SetActive(false);
            
            var stateProvider = _container.Resolve<IGameStateProvider>();
            PlayerPrefs.SetString(stateProvider.SkipTutorialKey, "y");
            
            _container.Resolve<ITutorialService>().CompleteCurrentCampaign();
        }
        
    }
}