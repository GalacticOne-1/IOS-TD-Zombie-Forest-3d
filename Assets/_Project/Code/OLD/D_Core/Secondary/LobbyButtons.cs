using Galactic1;
using Galactic1.Mobile;
using UnityEngine;

namespace Galactic1
{
    public class LobbyButtons : MonoBehaviour, IGameService, ISceneActivator
    {
        
        /*
         *    Подисывает методы для разных кнопок в Lobby
         */

        [SerializeField] private CoreBtn openOptions, openOptions2, pause, speed;

        [SerializeField] private CoreBtn launchRewardedAD,
            launchReviveAD,
            buyRevive,
            launchGemsAD;



        [Header("SHOP")] 
        [SerializeField] private CoreBtn openShopCoin;
        [SerializeField] private CoreBtn openShopGems, 
            openShopBase;
        [SerializeField] private CoreBtn closeAlertAIPShop, 
            closeShop;

        
        [Header("GAME")] 
        [SerializeField] private CoreBtn bQuest;
        [SerializeField] private CoreBtn bAdventure;
        
        [SerializeField] private GameObject bAdBox;
        [SerializeField] private GameObject bInbox;
        

        [Header("STATE")]
        [SerializeField] private CoreBtn toBattle; 
        [SerializeField] private CoreBtn toLobby, 
            cancelBattle,
            revive,
            newGameGP;
        
        
        
        
        
        public void Activator()
        {
            // DEFAULT
            speed._event.AddListener(SpeedBattle.I.SetSpeed);
            //openOptions._event.AddListener(ServiceLocator.Current.Get<W_Options>().OpenPanel);
            //openOptions2._event.AddListener(ServiceLocator.Current.Get<W_Options>().OpenPanel);
            //pause._event.AddListener(W_Pause.I.Pause);
            //closeAlertAIPShop._event.AddListener(Monetization.HideShopAlert);      // закрытие панели ошибки магазина
            //closeShop._event.AddListener(Monetization.CloseShop); 
            // ADS
            //launchRewardedAD._event.AddListener(Monetization.LaunchAfterBattle); 
            //launchReviveAD._event.AddListener(Monetization.LaunchRevive); 
            //launchGemsAD._event.AddListener(Monetization.LaunchGems); 
            //buyRevive._event.AddListener(GAMEPLAY_old.BuyRevive); 
            //launchEnergyAD._event.AddListener(Monetization.LaunchEnergy); 
            //buyEnergy._event.AddListener(GAMEPLAY.BuyEnergy); 
            
            // STORE
            //openShopBase._event.AddListener(IAP.I.OpenWindow);
            //openShopGems._event.AddListener(IAP.I.OpenWindow);
            //openShopCoin._event.AddListener(IAP.I.OpenWindow);
            
            // ------------------ ^ NOT DELETE ^
            
            
            // economics
            //launchRepairKitAD._event.AddListener(Monetization.LaunchRepairKit); 
            //buyRepairKit._event.AddListener(GAMEPLAY.BuyRepairKit); 
            //getPowerfulStuff._event.AddListener(GAMEPLAY.GetPowerfulStuff); 
            
            // ------------
            
            //bAdBox.EventBtn_old(ServiceLocator.Current.Get<ViewGameController>().EquipmentADBoxViewModel.OpenWindow);
            //bInbox.EventBtn_old(ServiceLocator.Current.Get<ViewGameController>().InboxViewModel.OpenWindow);
            
            toBattle._event.AddListener(() => ServiceLocator.Current.Get<Bootstrap>().SetState(EGameState.LEVEL_PLAY));
            //toLobby._event.AddListener(() => Bootstrap.ToState(EGameState.LEVEL_EXIT));
            //cancelBattle._event.AddListener(() => GameManager.BTN_GAMEPLAY_DEFEAT_exit());
            //revive._event.AddListener(() => GameManager.BTN_GAMEPLAY_DEFEAT_revive());
            //newGameGP._event.AddListener(() => GameManager.BTN_GAMEPLAY_DEFEAT_newgame());
            
            //ServiceLocator.Current.Get<CampBattle>().BBattle.EventBtn(ServiceLocator.Current.Get<GameMachine>().Camp_Start);
            
            
            
            //  ***      MENU SCREEN        ***
            //bQuest._event.AddListener(() => ServiceLocator.Current.Get<ViewGameController>().QuestViewModel.OpenWindow());
            
            
        }
    }
}