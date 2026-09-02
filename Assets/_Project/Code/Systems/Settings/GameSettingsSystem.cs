
using Galactic1.UI.Core;
using UnityEngine;

namespace Galactic1.Systems
{
    public class GameSettingsSystem
    {
        private readonly DIContainer _rootContainer;

        public GameSettingsSystem(DIContainer rootContainer)
        {
            _rootContainer = rootContainer;
        }

        public void ShowWindow()
        {
            // var resourcePath = $"{AppConstants.PATH_UI}Root/Canvas BasicSettings";
            // var prefab = Resources.Load<GameObject>(resourcePath);
            //
            // if (prefab != null)
            // {
            //     var instance = prefab.CreateGO(null);
            //     instance.GetComponent<SettingsUI>().Activator();
            // }
            //
            
            ServiceLocator.Current.Get<UIManager>().OpenScreen(UIScreenId.Settings);
        }
    }
}