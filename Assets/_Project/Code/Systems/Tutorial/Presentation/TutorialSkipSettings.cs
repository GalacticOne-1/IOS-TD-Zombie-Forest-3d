
using Galactic1.Code.Systems.Tutorial.Runtime;
using Galactic1.Core;
using UnityEngine;

namespace Galactic1.Code.Systems.Tutorial.Presentation
{
    public class TutorialSkipSettings : MonoBehaviour
    {
        [SerializeField] private GameObject skipButton;
        [SerializeField] private GameObject lockPanel;


        private DIContainer _container;

        public void Setup(DIContainer container)
        {
            _container = container;
            var stateProvider = _container.Resolve<IGameStateProvider>();

            
            // === Hide
            if (PlayerPrefs.HasKey(stateProvider.NewGameKey))
            {
                lockPanel.SetActive(false);
                skipButton.SetActive(false);
                return;
            }
            
            // === Show
            
            PlayerPrefs.SetString(stateProvider.NewGameKey, "y");
            
            skipButton.SetActive(true);
            
            
            skipButton.RegisterButtonClick(Skip);
        }
        
        void Skip()
        {
            var stateProvider = _container.Resolve<IGameStateProvider>();
            PlayerPrefs.SetString(stateProvider.SkipTutorialKey, "y");
            
            lockPanel.SetActive(true);
            
            _container.Resolve<ITutorialService>().CompleteCurrentCampaign();
        }
    }
}