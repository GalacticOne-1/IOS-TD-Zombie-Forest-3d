 using UnityEngine;

namespace Galactic1
{
    public class UIGameplayRootBinder : UIRootBinder
    {
        [SerializeField] private CheatPanelBinder _cheatPanelBinder;


        protected override void OnBind(UIRootViewModel rootViewModel)
        {
           base.OnBind(rootViewModel);

           var viewModel = rootViewModel as UIGameplayRootViewModel;
           
           _cheatPanelBinder.Bind(viewModel.CheatPanelViewModel);
        }
    }
}