using Firebase.Analytics;

namespace Galactic1.Mobile
{
    public static class AnalyticsService
    {
        private static bool analiticMode = false;
        public static void SetAnalyticsMode(bool value) => analiticMode = value;
        
        
        
        
        public enum ERequestAd
        {
            EMPTY, Auto_Inter, After_Battle_Money, After_Battle_Loot, Player_Rank, 
            Daily_Reward, IAP_Free_Money,
            
            
            
            // -------------    ^ DEFAULT ^     -------------------
            
            
            Revive, CampMarket, WorldMapMarket, PostRaidReward,
        }
        

        public static void ADS(ERequestAd request)
        {
            if (!analiticMode) return;
            
            
            switch (request)
            {
                case ERequestAd.Auto_Inter:
                    FirebaseAnalytics.LogEvent("ad_auto_interstitial");
                    break;
                
                case ERequestAd.Daily_Reward:
                    FirebaseAnalytics.LogEvent("ad_daily_bonus");
                    break;
                
                case ERequestAd.After_Battle_Money:
                    FirebaseAnalytics.LogEvent("ad_reward_after_battle");
                    break;
                
                case ERequestAd.Player_Rank:
                    FirebaseAnalytics.LogEvent("ad_player_rank");
                    break;
                
                
                
                
                case ERequestAd.Revive:
                    FirebaseAnalytics.LogEvent("ad_revive");
                    break;
                case ERequestAd.CampMarket:
                    FirebaseAnalytics.LogEvent("ad_camp_bonus");
                    break;
                case ERequestAd.WorldMapMarket:
                    FirebaseAnalytics.LogEvent("ad_world_map_bonus");
                    break;
                case ERequestAd.PostRaidReward:
                    FirebaseAnalytics.LogEvent("ad_post_raid_reward");
                    break;
            }
        }
        
        
        
        
        
        
        public enum ERequestGameplay
        {
            Start_App, Game_Launch, 
            Tutorial_Start, Tutorial_Finish, Tutorial_Task,
            Reach_Game_Level, Reach_Player_Rank,
            
            // -------------    ^ DEFAULT ^     -------------------
            
            Repair_Defense, EVENT_DESTROY_CONVOY,
        }
        
        
        public static void Gameplay(ERequestGameplay request, int value = 0)
        {
            if (!analiticMode) return;
            
            switch (request)
            {
                case ERequestGameplay.Start_App:
                        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLogin);
                    break;
                case ERequestGameplay.Game_Launch:
                    //if (!ApplicationSetup.APP_LOAD)
                        //FirebaseAnalytics.LogEvent("game_launch");
                    break;
                
                case ERequestGameplay.Reach_Game_Level:
                    if (value <= 10 || value == 15 || value == 20 || value == 30)
                        FirebaseAnalytics.LogEvent($"reach_surv_day_{value}");
                    break;
                
                case ERequestGameplay.Reach_Player_Rank:
                    
                    break;
                
                
                
                case ERequestGameplay.Tutorial_Start:
                    FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventTutorialBegin);
                    break;
                case ERequestGameplay.Tutorial_Finish:
                    FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventTutorialComplete);
                    break;
                case ERequestGameplay.Tutorial_Task:
                    FirebaseAnalytics.LogEvent($"tutorial_task_{value}");
                    break;
                
                
                
                
                case ERequestGameplay.Repair_Defense:
                    FirebaseAnalytics.LogEvent("repair_defense_objects");
                    break;
                
                case ERequestGameplay.EVENT_DESTROY_CONVOY:
                    FirebaseAnalytics.LogEvent("EVENT_destroy_convoy");
                    break;
            }
        }
    }
    
}