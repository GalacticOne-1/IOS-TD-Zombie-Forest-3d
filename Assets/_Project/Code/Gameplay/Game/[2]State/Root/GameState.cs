using System.Collections.Generic;
using Galactic1.UI.Shop;
using Galactic1.Structs;
using Galactic1.Code.Core;

namespace Galactic1.Core
{
    /*
     *      Хранилище всех состояний в игре 
     *      ! синхронизировано с сохранением !
     */
    
    public class GameState
    {
        public ServerTime Server { get; set; }
        
        public CGameStateIAP IAP { get; set; }
        public List<IAPCardData> IAPCardData { get; set; }
        
        public CGameStateAD ADState { get; set; }
        public CGameStateDailyReward DailyReward { get; set; }
        public CGameStateDailyQuest DailyQuest { get; set; }
        public CGameStateReview Review { get; set; }
        
        // ********************         DEFAULT        ************************************************************
        
        
        
        
        
        
        public CGameStateTutorial Tutorial { get; set; }
       
        public CPlayerInventory[] Inbox { get; set; }
        
        
        // ********************         GAME        ************************************************************
        public GameLoopContextData GameLoopContext { get; set; }
        // public int CurrentDay;
        // public int RemainingHour; // 0–24
        // public ThreatSaveData ThreatData { get; set; }

        
        public int GlobalEntityId { get; set; }
        
        

        public List<BankResourceData> BankResources { get; set; }
        
        public List<PlayerData> PlayerUnits { get; set; }
        
        public List<WorldData> WorldsData { get; set; }
        
        

        public int CreateEntityId() => GlobalEntityId++;
    }
}