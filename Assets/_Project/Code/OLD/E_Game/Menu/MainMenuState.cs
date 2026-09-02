
using Galactic1;
using UnityEngine;

namespace Galactic1
{
    public class MainMenuState1 : MainMenuStateScr
    {
        public override void Enter()
        {
            //ServiceLocator.Current.Get<ViewGameController>().ConstructViewModel.OpenWindow();
        }

        public override void Exit()
        {
            //ServiceLocator.Current.Get<ViewGameController>().ConstructViewModel.CloseWindow();
        }
    }
    
    public class MainMenuState2 : MainMenuStateScr
    {
        public override void Enter()
        {
            //ServiceLocator.Current.Get<ViewGameController>().InventoryViewModel.OpenWindow(InventoryModel.EMode.UNIT);
            //Object.FindAnyObjectByType<BackpackPanelController>().TogglePanel();
        }

        public override void Exit()
        {
            DLog.Alert("Exit menu 2", EDlogColor.ORANGE);
        }
    }
    
    public class MainMenuState3 : MainMenuStateScr
    {
        public override void Enter()
        {
            //ServiceLocator.Current.Get<ViewGameController>().CraftViewModel.OpenWindow();
        }

        public override void Exit()
        {
            
        }
    }
    
    public class MainMenuState4 : MainMenuStateScr
    {
        public override void Enter()
        {
            DLog.Alert("Enter menu 4");
        }

        public override void Exit()
        {
            DLog.Alert("Exit menu 4", EDlogColor.ORANGE);
        }
    }
    
    public class MainMenuState5 : MainMenuStateScr
    {
        public override void Enter()
        {
            DLog.Alert("Enter menu 5");
        }

        public override void Exit()
        {
            DLog.Alert("Exit menu 5", EDlogColor.ORANGE);
        }

        //public override void InFocus() => IAP.I.OpenWindow();
        
        //public override void OutFocus() => IAP.I.CloseWindow();
    }
}